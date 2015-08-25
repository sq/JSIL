using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler;
using JSIL.Ast;
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
            IAstEmitter astEmitter
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

            if (!Translator.ShouldTranslateMethodBody(
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
                    Formatter.WriteRaw("$.MakeEventAccessors");
                    Formatter.LPar();
                    Formatter.NewLine();
                    Formatter.MemberDescriptor(method.IsPublic, method.IsStatic, method.IsVirtual, false);
                    Formatter.Comma();
                    Formatter.Value(methodInfo.DeclaringEvent.Name);
                    Formatter.Comma();
                    Formatter.NewLine();
                    Formatter.TypeReference(methodInfo.DeclaringEvent.ReturnType, astEmitter.ReferenceContext);
                    Formatter.NewLine();
                    Formatter.RPar();
                    Formatter.Semicolon();
                    Formatter.NewLine();
                }

                return;
            }

            JSFunctionExpression function = Translator.GetFunctionBodyForMethod(
                isExternal, methodInfo
            );

            astEmitter.ReferenceContext.EnclosingType = method.DeclaringType;
            astEmitter.ReferenceContext.EnclosingMethod = null;

            Formatter.NewLine();

            astEmitter.ReferenceContext.Push();
            astEmitter.ReferenceContext.DefiningMethod = methodRef;

            try {
                dollar.WriteTo(Formatter);
                Formatter.Dot();
                if (methodInfo.IsPInvoke)
                    // FIXME: Write out dll name from DllImport
                    // FIXME: Write out alternate method name if provided
                    Formatter.Identifier("PInvokeMethod", EscapingMode.None);
                else if (isExternal && !Configuration.GenerateSkeletonsForStubbedAssemblies.GetValueOrDefault(false))
                    Formatter.Identifier("ExternalMethod", EscapingMode.None);
                else
                    Formatter.Identifier("Method", EscapingMode.None);
                Formatter.LPar();

                // FIXME: Include IsVirtual?
                Formatter.MemberDescriptor(method.IsPublic, method.IsStatic, method.IsVirtual, false);

                Formatter.Comma();
                Formatter.Value(Util.EscapeIdentifier(methodInfo.GetName(true), EscapingMode.String));

                Formatter.Comma();
                Formatter.NewLine();

                Formatter.MethodSignature(methodRef, methodInfo.Signature, astEmitter.ReferenceContext);

                if (methodInfo.IsPInvoke && method.HasPInvokeInfo) {
                    Formatter.Comma();
                    Formatter.NewLine();
                    EmitPInvokeInfo(
                        methodRef, method, astEmitter
                    );
                } else if (!isExternal) {
                    Formatter.Comma();
                    Formatter.NewLine();

                    if (function != null) {
                        Formatter.WriteRaw(Util.EscapeIdentifier(function.DisplayName));
                    } else {
                        Formatter.Identifier("JSIL.UntranslatableFunction", EscapingMode.None);
                        Formatter.LPar();
                        Formatter.Value(method.FullName);
                        Formatter.RPar();
                    }
                } else if (makeSkeleton) {
                    Formatter.Comma();
                    Formatter.NewLine();

                    Formatter.OpenFunction(
                        methodInfo.Name,
                        (o) => Formatter.WriteParameterList(
                            (from gpn in methodInfo.GenericParameterNames
                             select
                                 new JSParameter(gpn, methodRef.Module.TypeSystem.Object, methodRef))
                            .Concat(from p in methodInfo.Parameters
                                    select
                                        new JSParameter(p.Name, p.ParameterType, methodRef))
                        )
                    );

                    Formatter.WriteRaw("throw new Error('Not implemented');");
                    Formatter.NewLine();

                    Formatter.CloseBrace(false);
                }

                Formatter.NewLine();
                Formatter.RPar();

                astEmitter.ReferenceContext.AttributesMethod = methodRef;

                EmitOverrides(context, methodInfo.DeclaringType, method, methodInfo, astEmitter);

                if (!makeSkeleton)
                    TranslateCustomAttributes(context, method.DeclaringType, method, astEmitter);

                if (!makeSkeleton)
                    TranslateParameterAttributes(context, method.DeclaringType, method, astEmitter);

                Formatter.Semicolon();
            } finally {
                astEmitter.ReferenceContext.Pop();
            }
        }

        protected void EmitOverrides (
            DecompilerContext context, TypeInfo typeInfo,
            MethodDefinition method, MethodInfo methodInfo,
            IAstEmitter astEmitter
        ) {
            astEmitter.ReferenceContext.Push();
            try {
                astEmitter.ReferenceContext.EnclosingType = null;
                astEmitter.ReferenceContext.DefiningType = null;

                Formatter.Indent();

                foreach (var @override in methodInfo.Overrides) {
                    Formatter.NewLine();
                    Formatter.Dot();
                    Formatter.Identifier("Overrides");
                    Formatter.LPar();

                    Formatter.TypeReference(@override.InterfaceType, astEmitter.ReferenceContext);

                    Formatter.Comma();
                    Formatter.Value(@override.MemberIdentifier.Name);

                    Formatter.RPar();
                }

                Formatter.Unindent();
            } finally {
                astEmitter.ReferenceContext.Pop();
            }
        }

        private void EmitPInvokeInfo (
            MethodReference methodRef, MethodDefinition method, 
            IAstEmitter astEmitter
        ) {
            var pii = method.PInvokeInfo;

            Formatter.OpenBrace();

            if (pii != null) {
                Formatter.WriteRaw("Module: ");
                Formatter.Value(pii.Module.Name);
                Formatter.Comma();
                Formatter.NewLine();

                if (pii.IsCharSetAuto) {
                    Formatter.WriteRaw("CharSet: 'auto',");
                    Formatter.NewLine();
                } else if (pii.IsCharSetUnicode) {
                    Formatter.WriteRaw("CharSet: 'unicode',");
                    Formatter.NewLine();
                } else if (pii.IsCharSetAnsi) {
                    Formatter.WriteRaw("CharSet: 'ansi',");
                    Formatter.NewLine();
                }

                if ((pii.EntryPoint != null) && (pii.EntryPoint != method.Name)) {
                    Formatter.WriteRaw("EntryPoint: ");
                    Formatter.Value(pii.EntryPoint);
                    Formatter.Comma();
                    Formatter.NewLine();
                }
            }

            bool isArgsDictOpen = false;

            foreach (var p in method.Parameters) {
                if (p.HasMarshalInfo) {
                    if (!isArgsDictOpen) {
                        isArgsDictOpen = true;
                        Formatter.WriteRaw("Parameters: ");
                        Formatter.OpenBracket(true);
                    } else {
                        Formatter.Comma();
                        Formatter.NewLine();
                    }

                    EmitMarshalInfo(
                        methodRef, method,
                        p.Attributes, p.MarshalInfo, 
                        astEmitter
                    );
                } else if (isArgsDictOpen) {
                    Formatter.WriteRaw(", null");
                    Formatter.NewLine();
                }
            }

            if (isArgsDictOpen)
                Formatter.CloseBracket(true);

            if (method.MethodReturnType.HasMarshalInfo) {
                if (isArgsDictOpen)
                    Formatter.Comma();

                Formatter.WriteRaw("Result: ");

                EmitMarshalInfo(
                    methodRef, method,
                    method.MethodReturnType.Attributes, method.MethodReturnType.MarshalInfo, 
                    astEmitter
                );
                Formatter.NewLine();
            }

            Formatter.CloseBrace(false);
        }

        private void EmitMarshalInfo (
            MethodReference methodRef, MethodDefinition method,
            Mono.Cecil.ParameterAttributes attributes, MarshalInfo mi, 
            IAstEmitter astEmitter
        ) {
            Formatter.OpenBrace();

            if (mi.NativeType == NativeType.CustomMarshaler) {
                var cmi = (CustomMarshalInfo)mi;

                Formatter.WriteRaw("CustomMarshaler: ");
                Formatter.TypeReference(cmi.ManagedType, astEmitter.ReferenceContext);

                if (cmi.Cookie != null) {
                    Formatter.Comma();
                    Formatter.WriteRaw("Cookie: ");
                    Formatter.Value(cmi.Cookie);
                }
            } else {
                Formatter.WriteRaw("NativeType: ");
                Formatter.Value(mi.NativeType.ToString());
            }

            if (attributes.HasFlag(Mono.Cecil.ParameterAttributes.Out)) {
                Formatter.Comma();
                Formatter.NewLine();
                Formatter.WriteRaw("Out: true");
                Formatter.NewLine();
            } else {
                Formatter.NewLine();
            }

            Formatter.CloseBrace(false);
        }

        protected void EmitEnum (DecompilerContext context, IAssemblyEmitter assemblyEmitter, TypeDefinition enm, IAstEmitter astEmitter) {
            var typeInformation = _TypeInfoProvider.GetTypeInformation(enm);

            if (typeInformation == null)
                throw new InvalidDataException(String.Format(
                    "No type information for enum '{0}'!",
                    enm.FullName
                ));

            Formatter.Identifier("JSIL.MakeEnum", EscapingMode.None);
            Formatter.LPar();
            Formatter.NewLine();

            Formatter.OpenBrace();

            Formatter.WriteRaw("FullName: ");
            Formatter.Value(Util.DemangleCecilTypeName(typeInformation.FullName));
            Formatter.Comma();
            Formatter.NewLine();

            Formatter.WriteRaw("BaseType: ");
            // FIXME: Will this work on Mono?
            Formatter.TypeReference(enm.Fields.First(f => f.Name == "value__").FieldType, astEmitter.ReferenceContext);
            Formatter.Comma();
            Formatter.NewLine();

            Formatter.WriteRaw("IsPublic: ");
            Formatter.Value(enm.IsPublic);
            Formatter.Comma();
            Formatter.NewLine();

            Formatter.WriteRaw("IsFlags: ");
            Formatter.Value(typeInformation.IsFlagsEnum);
            Formatter.Comma();
            Formatter.NewLine();

            Formatter.CloseBrace(false);
            Formatter.Comma();
            Formatter.NewLine();

            Formatter.OpenBrace();

            foreach (var em in typeInformation.EnumMembers.Values.OrderBy((em) => em.Value)) {
                Formatter.Identifier(em.Name);
                Formatter.WriteRaw(": ");
                Formatter.Value(em.Value);
                Formatter.Comma();
                Formatter.NewLine();
            }

            Formatter.CloseBrace();

            Formatter.RPar();
            Formatter.Semicolon();
            Formatter.NewLine();
        }

        protected void EmitDelegate (DecompilerContext context, IAssemblyEmitter assemblyEmitter, TypeDefinition del, TypeInfo typeInfo, IAstEmitter astEmitter) {
            Formatter.Identifier("JSIL.MakeDelegate", EscapingMode.None);
            Formatter.LPar();

            Formatter.Value(Util.DemangleCecilTypeName(del.FullName));
            Formatter.Comma();

            Formatter.Value(del.IsPublic);

            Formatter.Comma();
            Formatter.OpenBracket();
            if (del.HasGenericParameters)
                WriteGenericParameterNames(del.GenericParameters);
            Formatter.CloseBracket();

            var invokeMethod = del.Methods.FirstOrDefault(method => method.Name == "Invoke");
            if (invokeMethod != null)
            {
                Formatter.Comma();
                Formatter.NewLine();

                astEmitter.ReferenceContext.Push();
                astEmitter.ReferenceContext.DefiningType = del;
                try
                {
                    Formatter.MethodSignature(invokeMethod,
                                           typeInfo.MethodSignatures.GetOrCreateFor("Invoke").First(),
                                           astEmitter.ReferenceContext);

                }
                finally
                {
                    astEmitter.ReferenceContext.Pop();
                }

                if (
                    invokeMethod.HasPInvokeInfo || 
                    invokeMethod.MethodReturnType.HasMarshalInfo ||
                    invokeMethod.Parameters.Any(p => p.HasMarshalInfo)
                ) {
                    Formatter.Comma();
                    EmitPInvokeInfo(invokeMethod, invokeMethod, astEmitter);
                    Formatter.NewLine();
                }
            }

            Formatter.RPar();
            Formatter.Semicolon();
            Formatter.NewLine();
        }

        public void DeclareTypeAlias (TypeDefinition typedef) {
            Formatter.WriteRaw("JSIL.MakeTypeAlias");
            Formatter.LPar();

            Formatter.WriteRaw("$jsilcore");
            Formatter.Comma();

            Formatter.Value(Util.DemangleCecilTypeName(typedef.FullName));

            Formatter.RPar();
            Formatter.Semicolon();
            Formatter.NewLine();
        }
    }
}
