using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using JSIL.Compiler.Extensibility;
using JSIL.Internal;
using JSIL.Translator;
using Mono.Cecil;

namespace JSIL {
    class JavascriptAssemblyEmitter : IAssemblyEmitter {
        public readonly AssemblyTranslator  Translator;
        public readonly JavascriptFormatter Formatter;

        public JavascriptAssemblyEmitter (
            AssemblyTranslator assemblyTranslator,
            JavascriptFormatter formatter
        ) {
            Translator = assemblyTranslator;
            Formatter = formatter;
        }

        // HACK
        private TypeInfoProvider _TypeInfoProvider {
            get {
                return Translator._TypeInfoProvider;
            }
        }

        private Configuration Configuration {
            get {
                return Translator.Configuration;
            }
        }

        public void EmitHeader (bool stubbed, bool skeletons) {
            Formatter.Comment(AssemblyTranslator.GetHeaderText());
            Formatter.NewLine();

            Formatter.WriteRaw("'use strict';");
            Formatter.NewLine();

            if (stubbed) {
                if (skeletons) {
                    Formatter.Comment("Generating type skeletons");
                } else {
                    Formatter.Comment("Generating type stubs only");
                }
                Formatter.NewLine();
            }

            Formatter.DeclareAssembly();
            Formatter.NewLine();
        }

        public void EmitAssemblyEntryPoint (AssemblyDefinition assembly, MethodDefinition entryMethod, MethodSignature signature) {
            Formatter.WriteRaw("JSIL.SetEntryPoint");
            Formatter.LPar();

            Formatter.AssemblyReference(assembly);

            Formatter.Comma();

            var context = new TypeReferenceContext();
            Formatter.TypeReference(entryMethod.DeclaringType, context);

            Formatter.Comma();

            Formatter.Value(entryMethod.Name);

            Formatter.Comma();

            Formatter.MethodSignature(
                entryMethod, signature, context
            );

            Formatter.RPar();
            Formatter.Semicolon(true);

            Formatter.NewLine();
        }

        public IAstEmitter MakeAstEmitter (
            JSILIdentifier jsil, TypeSystem typeSystem, TypeInfoProvider typeInfoProvider, Configuration configuration
        ) {
            return new JavascriptAstEmitter(
                Formatter, jsil, typeSystem, typeInfoProvider, configuration
            );
        }

        private string PickGenericParameterName (GenericParameter gp) {
            var result = gp.Name;

            if ((gp.Attributes & GenericParameterAttributes.Covariant) == GenericParameterAttributes.Covariant)
                result = "out " + result;
            if ((gp.Attributes & GenericParameterAttributes.Contravariant) == GenericParameterAttributes.Contravariant)
                result = "in " + result;

            return result;
        }

        public void WriteGenericParameterNames (IEnumerable<GenericParameter> parameters) {
            Formatter.CommaSeparatedList(
                (from p in parameters
                 select PickGenericParameterName(p)), 
                null, ListValueType.Primitive
            );
        }

        public void EmitInterfaceDefinition (
            DecompilerContext context, IAstEmitter astEmitter, TypeDefinition iface
        ) {
            Formatter.Identifier("JSIL.MakeInterface", EscapingMode.None);
            Formatter.LPar();
            Formatter.NewLine();
            
            Formatter.Value(Util.DemangleCecilTypeName(iface.FullName));
            Formatter.Comma();

            Formatter.Value(iface.IsPublic);
            Formatter.Comma();

            Formatter.OpenBracket();
            WriteGenericParameterNames(iface.GenericParameters);
            Formatter.CloseBracket();

            Formatter.Comma();
            Formatter.OpenFunction(null, (f) =>
                f.Identifier("$")
            );

            var refContext = new TypeReferenceContext {
                EnclosingType = iface,
                DefiningType = iface
            };

            bool isFirst = true;
            foreach (var methodGroup in iface.Methods.GroupBy(md => md.Name)) {
                foreach (var m in methodGroup) {
                    if (Translator.ShouldSkipMember(m))
                        continue;

                    var methodInfo = _TypeInfoProvider.GetMethod(m);

                    if ((methodInfo == null) || methodInfo.IsIgnored)
                        continue;

                    Formatter.Identifier("$", EscapingMode.None);
                    Formatter.Dot();
                    Formatter.Identifier("Method", EscapingMode.None);
                    Formatter.LPar();

                    Formatter.WriteRaw("{}");
                    Formatter.Comma();

                    Formatter.Value(Util.EscapeIdentifier(m.Name, EscapingMode.String));
                    Formatter.Comma();

                    Formatter.MethodSignature(m, methodInfo.Signature, refContext);

                    Formatter.RPar();
                    Formatter.Semicolon(true);
                }
            }

            foreach (var p in iface.Properties) {
                var propertyInfo = _TypeInfoProvider.GetProperty(p);
                if ((propertyInfo != null) && propertyInfo.IsIgnored)
                    continue;

                Formatter.Identifier("$", EscapingMode.None);
                Formatter.Dot();
                Formatter.Identifier("Property", EscapingMode.None);
                Formatter.LPar();

                Formatter.WriteRaw("{}");
                Formatter.Comma();

                Formatter.Value(Util.EscapeIdentifier(p.Name, EscapingMode.String));

                Formatter.RPar();
                Formatter.Semicolon(true);
            }

            Formatter.CloseBrace(false);

            Formatter.Comma();

            refContext = new TypeReferenceContext {
                EnclosingType = iface.DeclaringType,
                DefiningType = iface
            };

            Formatter.OpenBracket();
            foreach (var i in iface.Interfaces) {
                if (!isFirst) {
                    Formatter.Comma();
                }

                Formatter.TypeReference(i, refContext);

                isFirst = false;
            }
            Formatter.CloseBracket();

            Formatter.RPar();

            TranslateCustomAttributes(context, iface, iface, astEmitter);

            Formatter.Semicolon();
            Formatter.NewLine();
        }

        private void TranslateCustomAttributes (
            DecompilerContext context, 
            TypeReference declaringType,
            ICustomAttributeProvider member, 
            IAstEmitter astEmitter, 
            bool standalone = true
        ) {
            astEmitter.ReferenceContext.Push();
            try {
                astEmitter.ReferenceContext.EnclosingType = null;
                astEmitter.ReferenceContext.DefiningType = null;

                if (standalone)
                    Formatter.Indent();

                bool isFirst = true;

                foreach (var attribute in member.CustomAttributes) {
                    if (Translator.ShouldSkipMember(attribute.AttributeType))
                        continue;

                    if (!isFirst || standalone)
                        Formatter.NewLine();
                        
                    Formatter.Dot();
                    Formatter.Identifier("Attribute");
                    Formatter.LPar();
                    Formatter.TypeReference(attribute.AttributeType, astEmitter.ReferenceContext);

                    var constructorArgs = attribute.ConstructorArguments.ToArray();
                    if (constructorArgs.Length > 0) {
                        Formatter.Comma();

                        Formatter.WriteRaw("function () { return ");
                        Formatter.OpenBracket(false);
                        // FIXME: Get rid of this gross cast
                        ((JavascriptAstEmitter)astEmitter).CommaSeparatedList(
                            (from ca in constructorArgs
                             select Translator.TranslateAttributeConstructorArgument(
                                astEmitter.TypeSystem, declaringType, ca
                             ))
                        );
                        Formatter.CloseBracket(false);
                        Formatter.WriteRaw("; }");
                    }

                    Formatter.RPar();

                    isFirst = false;
                }

                if (standalone)
                    Formatter.Unindent();
            } finally {
                astEmitter.ReferenceContext.Pop();
            }
        }

        private void TranslateParameterAttributes (
            DecompilerContext context,
            TypeReference declaringType,
            MethodDefinition method,
            IAstEmitter astEmitter,
            IAssemblyEmitter assemblyEmitter
        ) {
            Formatter.Indent();

            foreach (var parameter in method.Parameters) {
                if (!parameter.HasCustomAttributes && !Configuration.CodeGenerator.EmitAllParameterNames.GetValueOrDefault(false))
                    continue;

                Formatter.NewLine();
                Formatter.Dot();
                Formatter.Identifier("Parameter");
                Formatter.LPar();

                Formatter.Value(parameter.Index);
                Formatter.Comma();

                Formatter.Value(parameter.Name);
                Formatter.Comma();

                Formatter.OpenFunction(null, (jf) => jf.Identifier("_"));

                Formatter.Identifier("_");

                TranslateCustomAttributes(context, declaringType, parameter, astEmitter, false);

                Formatter.NewLine();

                Formatter.CloseBrace(false);

                Formatter.RPar();
            }

            Formatter.Unindent();
        }

        public void EmitMethodDefinition (
            DecompilerContext context, MethodReference methodRef, MethodDefinition method,
            IAstEmitter astEmitter, bool stubbed,
            JSRawOutputIdentifier dollar, MethodInfo methodInfo = null
        ) {
            if (methodInfo == null)
                methodInfo = _TypeInfoProvider.GetMemberInformation<Internal.MethodInfo>(method);

            bool isExternal, isReplaced, methodIsProxied;

            if (!ShouldTranslateMethodBody(
                method, methodInfo, stubbed,
                out isExternal, out isReplaced, out methodIsProxied
            ))
                return;

            var makeSkeleton = stubbed && isExternal && Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false);

            if (
                makeSkeleton &&
                method.IsPrivate &&
                method.IsCompilerGenerated()
            )
                return;

            if (
                makeSkeleton &&
                (methodInfo.DeclaringEvent != null) &&
                Configuration.CodeGenerator.AutoGenerateEventAccessorsInSkeletons.GetValueOrDefault(true)
            ) {
                if (method.Name.StartsWith("add_")) {
                    output.WriteRaw("$.MakeEventAccessors");
                    output.LPar();
                    output.NewLine();
                    output.MemberDescriptor(method.IsPublic, method.IsStatic, method.IsVirtual, false);
                    output.Comma();
                    output.Value(methodInfo.DeclaringEvent.Name);
                    output.Comma();
                    output.NewLine();
                    output.TypeReference(methodInfo.DeclaringEvent.ReturnType, astEmitter.ReferenceContext);
                    output.NewLine();
                    output.RPar();
                    output.Semicolon();
                    output.NewLine();
                }

                return;
            }

            JSFunctionExpression function = GetFunctionBodyForMethod(
                isExternal, methodInfo
            );

            astEmitter.ReferenceContext.EnclosingType = method.DeclaringType;
            astEmitter.ReferenceContext.EnclosingMethod = null;

            output.NewLine();

            astEmitter.ReferenceContext.Push();
            astEmitter.ReferenceContext.DefiningMethod = methodRef;

            try {
                dollar.WriteTo(output);
                output.Dot();
                if (methodInfo.IsPInvoke)
                    // FIXME: Write out dll name from DllImport
                    // FIXME: Write out alternate method name if provided
                    output.Identifier("PInvokeMethod", EscapingMode.None);
                else if (isExternal && !Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false))
                    output.Identifier("ExternalMethod", EscapingMode.None);
                else
                    output.Identifier("Method", EscapingMode.None);
                output.LPar();

                // FIXME: Include IsVirtual?
                output.MemberDescriptor(method.IsPublic, method.IsStatic, method.IsVirtual, false);

                output.Comma();
                output.Value(Util.EscapeIdentifier(methodInfo.GetName(true), EscapingMode.String));

                output.Comma();
                output.NewLine();

                output.MethodSignature(methodRef, methodInfo.Signature, astEmitter.ReferenceContext);

                if (methodInfo.IsPInvoke && method.HasPInvokeInfo) {
                    output.Comma();
                    output.NewLine();
                    TranslatePInvokeInfo(
                        methodRef, method, astEmitter, assemblyEmitter
                    );
                } else if (!isExternal) {
                    output.Comma();
                    output.NewLine();

                    if (function != null) {
                        output.WriteRaw(Util.EscapeIdentifier(function.DisplayName));
                    } else {
                        output.Identifier("JSIL.UntranslatableFunction", EscapingMode.None);
                        output.LPar();
                        output.Value(method.FullName);
                        output.RPar();
                    }
                } else if (makeSkeleton) {
                    output.Comma();
                    output.NewLine();

                    output.OpenFunction(
                        methodInfo.Name,
                        (o) => output.WriteParameterList(
                            (from gpn in methodInfo.GenericParameterNames
                             select
                                 new JSParameter(gpn, methodRef.Module.TypeSystem.Object, methodRef))
                            .Concat(from p in methodInfo.Parameters
                                    select
                                        new JSParameter(p.Name, p.ParameterType, methodRef))
                        )
                    );

                    output.WriteRaw("throw new Error('Not implemented');");
                    output.NewLine();

                    output.CloseBrace(false);
                }

                output.NewLine();
                output.RPar();

                astEmitter.ReferenceContext.AttributesMethod = methodRef;

                TranslateOverrides(context, methodInfo.DeclaringType, method, methodInfo, astEmitter, assemblyEmitter);

                if (!makeSkeleton)
                    TranslateCustomAttributes(context, method.DeclaringType, method, astEmitter, assemblyEmitter);

                if (!makeSkeleton)
                    TranslateParameterAttributes(context, method.DeclaringType, method, astEmitter, assemblyEmitter);

                output.Semicolon();
            } finally {
                astEmitter.ReferenceContext.Pop();
            }
        }
    }
}
