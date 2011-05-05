using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using Microsoft.CSharp.RuntimeBinder;
using Mono.Cecil;

namespace JSIL {
    internal class DynamicCallSiteInfoCollection {
        protected readonly Dictionary<string, DynamicCallSiteInfo> CallSites = new Dictionary<string, DynamicCallSiteInfo>();
        protected readonly Dictionary<string, FieldReference> Aliases = new Dictionary<string, FieldReference>();

        public bool Get (ILVariable localVariable, out DynamicCallSiteInfo info) {
            FieldReference storageField;

            if (Aliases.TryGetValue(localVariable.Name, out storageField))
                return Get(storageField, out info);

            info = null;
            return false;
        }

        public bool Get (FieldReference storageField, out DynamicCallSiteInfo info) {
            return CallSites.TryGetValue(storageField.FullName, out info);
        }

        public DynamicCallSiteInfo InitializeCallSite (FieldReference storageField, string bindingType, TypeReference targetType, JSExpression[] arguments) {
            DynamicCallSiteInfo callSiteInfo;

            switch (bindingType) {
                case "InvokeMember":
                    callSiteInfo = new DynamicCallSiteInfo.InvokeMember(targetType, arguments);
                break;
                default:
                    throw new NotImplementedException(String.Format("Call sites of type '{0}' not implemented.", bindingType));
            }

            return CallSites[storageField.FullName] = callSiteInfo;
        }

        public void SetAlias (ILVariable variable, FieldReference fieldReference) {
            Aliases[variable.Name] = fieldReference;
        }
    }

    internal abstract class DynamicCallSiteInfo {
        protected readonly JSExpression[] Arguments;

        protected DynamicCallSiteInfo (JSExpression[] arguments) {
            Arguments = arguments;
        }

        public abstract JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments);

        public class InvokeMember : DynamicCallSiteInfo {
            public readonly TypeReference ReturnType;

            public InvokeMember (TypeReference targetType, JSExpression[] arguments)
                : base (arguments) {

                var targetTypeName = targetType.FullName;
                var git = targetType as GenericInstanceType;

                if (targetTypeName.StartsWith("System.Action`")) {
                    ReturnType = null;
                } else if (targetTypeName.StartsWith("System.Func`")) {
                    ReturnType = git.GenericArguments[git.GenericArguments.Count - 1];
                } else
                    throw new NotImplementedException("This type of call site target is not implemented");

                if ((BinderFlags & CSharpBinderFlags.ResultDiscarded) == CSharpBinderFlags.ResultDiscarded)
                    ReturnType = null;
            }

            public CSharpBinderFlags BinderFlags {
                get {
                    return (CSharpBinderFlags)((JSEnumLiteral)Arguments[0]).Value;
                }
            }

            public string MemberName {
                get {
                    return ((JSStringLiteral)Arguments[1]).Value;
                }
            }

            public JSExpression TypeArguments {
                get {
                    return Arguments[2];
                }
            }

            public TypeReference UsageContext {
                get {
                    return ((JSType)Arguments[3]).Type;
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[4];
                }
            }

            public override JSExpression Translate (ILBlockTranslator translator, JSExpression[] arguments) {
                // The first argument is always the callsite, so we ignore it
                var thisArgument = arguments[1];

                var returnType = ReturnType;
                if (returnType == null)
                    returnType = translator.TypeSystem.Void;

                return new JSInvocationExpression(
                    JSDotExpression.New(
                        thisArgument,
                        new JSIdentifier(MemberName, returnType)
                    ),
                    arguments.Skip(2).ToArray()
                );
            }
        }
    }
}
