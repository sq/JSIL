using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler.ILAst;
using JSIL.Ast;
using Mono.Cecil;

namespace JSIL {
    public class DynamicCallSiteInfoCollection {
        protected readonly Dictionary<string, DynamicCallSiteInfo> CallSites = new Dictionary<string, DynamicCallSiteInfo>();

        public bool Get (FieldReference storageField, out DynamicCallSiteInfo info) {
            return CallSites.TryGetValue(storageField.FullName, out info);
        }

        public DynamicCallSiteInfo InitializeCallSite (FieldReference storageField, string bindingType, JSExpression[] arguments) {
            DynamicCallSiteInfo callSiteInfo;

            switch (bindingType) {
                case "InvokeMember":
                    callSiteInfo = new DynamicCallSiteInfo.InvokeMember(arguments);
                break;
                default:
                    throw new NotImplementedException(String.Format("Call sites of type '{0}' not implemented.", bindingType));
            }

            return CallSites[storageField.FullName] = callSiteInfo;
        }
    }

    public abstract class DynamicCallSiteInfo {
        protected readonly JSExpression[] Arguments;

        protected DynamicCallSiteInfo (JSExpression[] arguments) {
            Arguments = arguments;
        }

        public class InvokeMember : DynamicCallSiteInfo {
            public InvokeMember (JSExpression[] arguments)
                : base (arguments) {
            }

            public JSExpression BinderFlags {
                get {
                    return Arguments[0];
                }
            }

            public JSExpression MemberName {
                get {
                    return Arguments[1];
                }
            }

            public JSExpression TypeArguments {
                get {
                    return Arguments[2];
                }
            }

            public JSExpression UsageContext {
                get {
                    return Arguments[3];
                }
            }

            public JSExpression ArgumentInfo {
                get {
                    return Arguments[4];
                }
            }
        }
    }
}
