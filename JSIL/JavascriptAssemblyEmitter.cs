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
using JSIL.Transforms;
using JSIL.Translator;
using Mono.Cecil;

namespace JSIL {
    class JavascriptAssemblyEmitter : IAssemblyEmitter {
        public readonly AssemblyTranslator  Translator;
        public readonly JavascriptFormatter Formatter;
        private readonly IDictionary<AssemblyManifest.Token, string> _referenceOverrides;

        public JavascriptAssemblyEmitter (
            AssemblyTranslator assemblyTranslator,
            JavascriptFormatter formatter,
            IDictionary<AssemblyManifest.Token, string> referenceOverrides
        ) {
            Translator = assemblyTranslator;
            Formatter = formatter;
            _referenceOverrides = referenceOverrides;
        }

        // HACK
        private TypeInfoProvider TypeInfo {
            get {
                return Translator.TypeInfoProvider;
            }
        }

        private Configuration Configuration {
            get {
                return Translator.Configuration;
            }
        }

        public void EmitHeader (bool stubbed) {
            Formatter.Comment(AssemblyTranslator.GetHeaderText());
            Formatter.NewLine();

            Formatter.WriteRaw("'use strict';");
            Formatter.NewLine();

            if (stubbed) {
                Formatter.Comment("Generating type stubs only");
                Formatter.NewLine();
            }

            if (_referenceOverrides != null)
            {
                Formatter.LPar();
                Formatter.OpenFunction(string.Empty, null);
            }

            Formatter.DeclareAssembly();
            Formatter.NewLine();

            if (_referenceOverrides != null) {
                Formatter.WriteReferencesOverrides(_referenceOverrides);
            }
        }

        public void EmitFooter () {
            if (_referenceOverrides != null)
            {
                Formatter.CloseBrace();
                Formatter.RPar();
                Formatter.LPar();
                Formatter.RPar();
                Formatter.Semicolon();
            }
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
            var typeInfo = TypeInfo.GetTypeInformation(iface);

            Formatter.Identifier("JSIL.MakeInterface", EscapingMode.None);
            Formatter.LPar();
            Formatter.NewLine();
            
            Formatter.Value(Util.DemangleCecilTypeName(typeInfo.FullName));
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

                    var methodInfo = TypeInfo.GetMethod(m);

                    if ((methodInfo == null) || methodInfo.IsIgnored)
                        continue;

                    Formatter.Identifier("$", EscapingMode.None);
                    Formatter.Dot();
                    Formatter.Identifier("Method", EscapingMode.None);
                    Formatter.LPar();

                    var renamedName = methodInfo.Name;
                    var offset = renamedName.IndexOf("`");
                    if (offset > 0)
                        renamedName = renamedName.Substring(0, offset);

                    if (renamedName != m.Name) {
                        Formatter.WriteRaw("{OriginalName: ");
                        Formatter.Value(m.Name);
                        Formatter.WriteRaw("}");
                    } else {
                        Formatter.WriteRaw("{}");
                    }
                    Formatter.Comma();

                    Formatter.Value(Util.EscapeIdentifier(renamedName, EscapingMode.String));
                    Formatter.Comma();

                    Formatter.MethodSignature(m, methodInfo.Signature, refContext);

                    Formatter.RPar();
                    Formatter.Semicolon(true);
                }
            }

            foreach (var p in iface.Properties) {
                if (Translator.ShouldSkipMember(p))
                    continue;

                var propertyInfo = TypeInfo.GetProperty(p);
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
                if (Translator.ShouldSkipMember(i))
                {
                    continue;
                }
                if (!isFirst) {
                    Formatter.Comma();
                }

                Formatter.TypeReference(i, refContext);

                isFirst = false;
            }
            Formatter.CloseBracket();

            Formatter.RPar();

            EmitCustomAttributes(context, iface, iface, astEmitter);

            Formatter.Semicolon();
            Formatter.NewLine();
        }

        public JSExpression TranslateAttributeConstructorArgument (
            TypeSystem typeSystem, TypeReference context, CustomAttributeArgument ca
        ) {
            if (ca.Value == null) {
                return JSLiteral.Null(ca.Type);
            } else if (ca.Value is CustomAttributeArgument) {
                // :|
                return TranslateAttributeConstructorArgument(
                    typeSystem, context, (CustomAttributeArgument)ca.Value
                );
            } else if (ca.Value is CustomAttributeArgument[]) {
                // Issue #141. WTF.
                var valueArray = (CustomAttributeArgument[])ca.Value;
                return new JSArrayExpression(typeSystem.Object, 
                    (from value in valueArray select TranslateAttributeConstructorArgument(
                        typeSystem, context, value
                    )).ToArray()
                );
            } else if (ca.Type.FullName == "System.Type") {
                return new JSTypeOfExpression((TypeReference)ca.Value);
            } else if (TypeUtil.IsEnum(ca.Type)) {
                var longValue = Convert.ToInt64(ca.Value);
                var result = JSEnumLiteral.TryCreate(
                    TypeInfo.GetExisting(ca.Type),
                    longValue
                );
                if (result != null)
                    return result;
                else
                    return JSLiteral.New(longValue);
            } else {
                try {
                    return JSLiteral.New(ca.Value as dynamic);
                } catch (Exception) {
                    throw new NotImplementedException(String.Format("Attribute arguments of type '{0}' are not implemented.", ca.Type.FullName));
                }
            }
        }

        public void EmitCustomAttributes (
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

                    // Don't emit Meta attributes into the output JS
                    if (attribute.AttributeType.Namespace == "JSIL.Meta")
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
                             select TranslateAttributeConstructorArgument(
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

        private void EmitParameterAttributes (
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

                EmitCustomAttributes(context, declaringType, parameter, astEmitter, false);

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
                methodInfo = TypeInfo.GetMemberInformation<Internal.MethodInfo>(method);

            bool isExternal, isReplaced, methodIsProxied;

            if (!Translator.ShouldTranslateMethodBody(
                method, methodInfo, stubbed,
                out isExternal, out isReplaced, out methodIsProxied
            ))
                return;

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
                }

                Formatter.NewLine();
                Formatter.RPar();

                astEmitter.ReferenceContext.AttributesMethod = methodRef;

                EmitOverrides(context, methodInfo.DeclaringType, method, methodInfo, astEmitter);

                EmitCustomAttributes(context, method.DeclaringType, method, astEmitter);

                EmitParameterAttributes(context, method.DeclaringType, method, astEmitter);

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

        protected void EmitEnum (DecompilerContext context, TypeDefinition enm, IAstEmitter astEmitter) {
            var typeInformation = TypeInfo.GetTypeInformation(enm);

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

        protected void EmitDelegate (DecompilerContext context, TypeDefinition del, TypeInfo typeInfo, IAstEmitter astEmitter) {
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

        public void EmitTypeAlias (TypeDefinition typedef) {
            Formatter.WriteRaw("JSIL.MakeTypeAlias");
            Formatter.LPar();

            Formatter.WriteRaw("$jsilcore");
            Formatter.Comma();

            Formatter.Value(Util.DemangleCecilTypeName(typedef.FullName));

            Formatter.RPar();
            Formatter.Semicolon();
            Formatter.NewLine();
        }

        public bool EmitTypeDeclarationHeader (DecompilerContext context, IAstEmitter astEmitter, TypeDefinition typedef, TypeInfo typeInfo) {
            Formatter.DeclareNamespace(typedef.Namespace);

            if (typeInfo.IsExternal) {
                Formatter.Identifier("JSIL.MakeExternalType", EscapingMode.None);
                Formatter.LPar();

                Formatter.Value(Util.DemangleCecilTypeName(typeInfo.FullName));
                Formatter.Comma();
                Formatter.Value(typedef.IsPublic);

                Formatter.RPar();
                Formatter.Semicolon();
                Formatter.NewLine();
                return false;
            } else if (typedef.IsInterface) {
                Formatter.Comment("interface {0}", Util.DemangleCecilTypeName(typedef.FullName));
                Formatter.NewLine();
                Formatter.NewLine();

                EmitInterfaceDefinition(context, astEmitter, typedef);
                return false;
            } else if (typedef.IsEnum) {
                Formatter.Comment("enum {0}", Util.DemangleCecilTypeName(typedef.FullName));
                Formatter.NewLine();
                Formatter.NewLine();

                EmitEnum(context, typedef, astEmitter);
                return false;
            } else if (typeInfo.IsDelegate) {
                Formatter.Comment("delegate {0}", Util.DemangleCecilTypeName(typedef.FullName));
                Formatter.NewLine();
                Formatter.NewLine();

                EmitDelegate(context, typedef, typeInfo, astEmitter);
                return false;
            }

            return true;
        }

        public void EmitPrimitiveDefinition (
            DecompilerContext context, TypeDefinition typedef, bool stubbed, JSRawOutputIdentifier dollar
        ) {
            bool isIntegral = false;
            bool isNumeric = false;

            switch (typedef.FullName) {
                case "System.Boolean":
                    isIntegral = true;
                    isNumeric = true;
                    break;
                case "System.Char":
                    isIntegral = true;
                    isNumeric = true;
                    break;
                case "System.Byte":
                case "System.SByte":
                case "System.UInt16":
                case "System.Int16":
                case "System.UInt32":
                case "System.Int32":
                case "System.UInt64":
                case "System.Int64":
                    isIntegral = true;
                    isNumeric = true;
                    break;
                case "System.Single":
                case "System.Double":
                case "System.Decimal":
                    isIntegral = false;
                    isNumeric = true;
                    break;
            }

            var setValue = (Action<string, bool>)((name, value) => {
                dollar.WriteTo(Formatter);
                Formatter.Dot();
                Formatter.Identifier("SetValue", EscapingMode.None);
                Formatter.LPar();
                Formatter.Value(name);
                Formatter.Comma();
                Formatter.Value(value);
                Formatter.RPar();
                Formatter.Semicolon(true);
            });

            setValue("__IsIntegral__", isIntegral);
            setValue("__IsNumeric__", isNumeric);
        }

        // HACK
        public void EmitSpacer () {
            Formatter.NewLine();
        }

        // HACK
        public void EmitSemicolon () {
            Formatter.Semicolon(false);
        }

        public void EmitProxyComment (string name) {
            Formatter.Comment("Proxied method implementation from {0}", name);
            Formatter.NewLine();
        }

        public void EmitProperty (
            DecompilerContext context, IAstEmitter astEmitter,
            PropertyDefinition property, JSRawOutputIdentifier dollar
        ) {
            if (Translator.ShouldSkipMember(property))
                return;

            var propertyInfo = TypeInfo.GetMemberInformation<Internal.PropertyInfo>(property);
            if ((propertyInfo == null) || propertyInfo.IsIgnored)
                return;

            var isStatic = (property.SetMethod ?? property.GetMethod).IsStatic;

            Formatter.NewLine();

            dollar.WriteTo(Formatter);
            Formatter.Dot();

            if (propertyInfo.IsExternal)
                Formatter.Identifier("ExternalProperty", EscapingMode.None);
            else if (property.DeclaringType.HasGenericParameters && isStatic)
                Formatter.Identifier("GenericProperty", EscapingMode.None);
            else
                Formatter.Identifier("Property", EscapingMode.None);

            Formatter.LPar();

            Formatter.MemberDescriptor(propertyInfo.IsPublic, propertyInfo.IsStatic, propertyInfo.IsVirtual);

            Formatter.Comma();

            Formatter.Value(Util.EscapeIdentifier(propertyInfo.Name, EscapingMode.String));

            Formatter.Comma();
            Formatter.TypeReference(property.PropertyType, astEmitter.ReferenceContext);

            Formatter.RPar();

            EmitCustomAttributes(context, property.DeclaringType, property, astEmitter);

            Formatter.Semicolon();
        }

        public void EmitEvent (
            DecompilerContext context, IAstEmitter astEmitter, 
            EventDefinition @event, JSRawOutputIdentifier dollar
        ) {
            if (Translator.ShouldSkipMember(@event))
                return;

            var eventInfo = TypeInfo.GetMemberInformation<Internal.EventInfo>(@event);
            if ((eventInfo == null) || eventInfo.IsIgnored)
                return;

            var isStatic = (@event.AddMethod ?? @event.RemoveMethod).IsStatic;

            Formatter.NewLine();

            dollar.WriteTo(Formatter);
            Formatter.Dot();

            if (eventInfo.IsExternal)
                Formatter.Identifier("ExternalEvent", EscapingMode.None);
            else if (@event.DeclaringType.HasGenericParameters && isStatic)
                Formatter.Identifier("GenericEvent", EscapingMode.None);
            else
                Formatter.Identifier("Event", EscapingMode.None);

            Formatter.LPar();

            Formatter.MemberDescriptor(eventInfo.IsPublic, eventInfo.IsStatic, eventInfo.IsVirtual);

            Formatter.Comma();

            Formatter.Value(Util.EscapeIdentifier(eventInfo.Name, EscapingMode.String));

            Formatter.Comma();
            Formatter.TypeReference(@event.EventType, astEmitter.ReferenceContext);

            Formatter.RPar();

            EmitCustomAttributes(context, @event.DeclaringType, @event, astEmitter);

            Formatter.Semicolon();
        }

        public void BeginEmitTypeDeclaration (TypeDefinition typedef) {
            Formatter.Comment("{0} {1}", typedef.IsValueType ? "struct" : "class", Util.DemangleCecilTypeName(typedef.FullName));
            Formatter.NewLine();
            Formatter.NewLine();

            Formatter.WriteRaw("(function {0}$Members () {{", Util.EscapeIdentifier(typedef.Name));
            Formatter.Indent();
            Formatter.NewLine();

            Formatter.WriteRaw("var $, $thisType");
            Formatter.Semicolon(true);
        }

        public void BeginEmitTypeDefinition (
            IAstEmitter astEmitter, 
            TypeDefinition typedef, TypeInfo typeInfo,
            TypeReference baseClass
        ) {
            Formatter.NewLine();

            bool isStaticClass = typedef.IsAbstract && typedef.IsSealed;

            if (isStaticClass) {
                Formatter.Identifier("JSIL.MakeStaticClass", EscapingMode.None);
                Formatter.LPar();

                Formatter.Value(Util.DemangleCecilTypeName(typeInfo.FullName));
                Formatter.Comma();
                Formatter.Value(typedef.IsPublic);

                Formatter.Comma();
                Formatter.OpenBracket();
                if (typedef.HasGenericParameters)
                    WriteGenericParameterNames(typedef.GenericParameters);
                Formatter.CloseBracket();

            } else {
                Formatter.Identifier("JSIL.MakeType", EscapingMode.None);

                Formatter.LPar();
                Formatter.OpenBrace();

                Formatter.WriteRaw("BaseType: ");

                if (baseClass == null) {
                    if (typedef.FullName != "System.Object") {
                        throw new InvalidDataException(String.Format(
                            "Type '{0}' has no base class and isn't System.Object.",
                            typedef.FullName
                        ));
                    }

                    Formatter.Identifier("$jsilcore");
                    Formatter.Dot();
                    Formatter.Identifier("TypeRef");
                    Formatter.LPar();
                    Formatter.Value("System.Object");
                    Formatter.RPar();
                } else if (typedef.FullName == "System.ValueType") {
                    Formatter.Identifier("$jsilcore");
                    Formatter.Dot();
                    Formatter.Identifier("TypeRef");
                    Formatter.LPar();
                    Formatter.Value("System.ValueType");
                    Formatter.RPar();
                } else {
                    Formatter.TypeReference(baseClass, astEmitter.ReferenceContext);
                }

                Formatter.Comma();
                Formatter.NewLine();

                Formatter.WriteRaw("Name: ");
                Formatter.Value(Util.DemangleCecilTypeName(typeInfo.FullName));
                Formatter.Comma();
                Formatter.NewLine();

                Formatter.WriteRaw("IsPublic: ");
                Formatter.Value(typedef.IsPublic);
                Formatter.Comma();
                Formatter.NewLine();

                Formatter.WriteRaw("IsReferenceType: ");
                Formatter.Value(!typedef.IsValueType);
                Formatter.Comma();
                Formatter.NewLine();

                if (typedef.HasGenericParameters) {
                    Formatter.WriteRaw("GenericParameters: ");
                    Formatter.OpenBracket();
                    WriteGenericParameterNames(typedef.GenericParameters);
                    Formatter.CloseBracket();
                    Formatter.Comma();
                    Formatter.NewLine();
                }

                var constructors = typedef.Methods.Where((m) => m.IsConstructor).ToList();
                if ((constructors.Count != 0) || typedef.IsValueType) {
                    Formatter.WriteRaw("MaximumConstructorArguments: ");

                    if (typedef.IsValueType && (constructors.Count == 0))
                        Formatter.Value(0);
                    else
                        Formatter.Value(constructors.Max((m) => m.Parameters.Count));

                    Formatter.Comma();
                    Formatter.NewLine();
                }

                if (typedef.IsExplicitLayout) {
                    Formatter.WriteRaw("ExplicitLayout: true");
                    Formatter.Comma();
                    Formatter.NewLine();
                } else if (typedef.IsSequentialLayout) {
                    Formatter.WriteRaw("SequentialLayout: true");
                    Formatter.Comma();
                    Formatter.NewLine();
                }

                if (typedef.HasLayoutInfo) {
                    if (typedef.PackingSize != 0) {
                        Formatter.WriteRaw("Pack: ");
                        Formatter.Value(typedef.PackingSize);
                        Formatter.Comma();
                        Formatter.NewLine();
                    }

                    if (typedef.ClassSize != 0) {
                        Formatter.WriteRaw("SizeBytes: ");
                        Formatter.Value(typedef.ClassSize);
                        Formatter.Comma();
                        Formatter.NewLine();
                    }
                }

                Formatter.CloseBrace(false);
            }

            // Hack to force the indent level for type definitions to be 1 instead of 2.
            Formatter.Unindent();

            Formatter.Comma();
            Formatter.OpenFunction(null, (f) => 
                f.Identifier("$ib")
            );

            Formatter.WriteRaw("$ = $ib;");
            Formatter.NewLine();

            astEmitter.ReferenceContext.Push();
            astEmitter.ReferenceContext.EnclosingType = typedef;
        }

        public void EndEmitTypeDefinition (IAstEmitter astEmitter, DecompilerContext context, TypeDefinition typedef) {
            Formatter.NewLine();
            Formatter.NewLine();
            Formatter.WriteRaw("return function (newThisType) { $thisType = newThisType; }");
            Formatter.Semicolon(false);

            Formatter.NewLine();

            Formatter.CloseBrace(false);

            // Hack to force the indent level for type definitions to be 1 instead of 2.
            Formatter.Indent();

            Formatter.RPar();

            EmitCustomAttributes(context, typedef.DeclaringType, typedef, astEmitter);

            Formatter.Semicolon();
            Formatter.NewLine();

            astEmitter.ReferenceContext.Pop();

            Formatter.Unindent();
            Formatter.WriteRaw("})();");
            Formatter.NewLine();
            Formatter.NewLine();
        }

        public void EmitInterfaceList (
            TypeInfo typeInfo, 
            IAstEmitter astEmitter,
            JSRawOutputIdentifier dollar
        ) {
            var interfaces = typeInfo.AllInterfacesRecursive;
            if (interfaces.Count <= 0)
                return;

            Formatter.NewLine();

            dollar.WriteTo(Formatter);
            Formatter.Dot();
            Formatter.Identifier("ImplementInterfaces", EscapingMode.None);
            Formatter.LPar();

            bool firstInterface = true;

            for (var i = 0; i < interfaces.Count; i++) {
                var elt = interfaces.Array[interfaces.Offset + i];
                if (elt.ImplementingType != typeInfo)
                    continue;
                if (elt.ImplementedInterface.Info.IsIgnored)
                    continue;
                if (Translator.ShouldSkipMember(elt.ImplementedInterface.Reference))
                    continue;

                var @interface = elt.ImplementedInterface.Reference;

                if (firstInterface)
                    firstInterface = false;
                else
                    Formatter.Comma();

                Formatter.NewLine();

                Formatter.Comment("{0}", i);
                Formatter.TypeReference(@interface, astEmitter.ReferenceContext);
            }

            Formatter.NewLine();
            Formatter.RPar();
            Formatter.Semicolon(true);
        }

        private void EmitFieldOrConstant (
            DecompilerContext context, IAstEmitter astEmitter, 
            FieldDefinition field, JSRawOutputIdentifier dollar, 
            JSExpression defaultValue, bool isConstant
        ) {
            var fieldInfo = TypeInfo.GetMemberInformation<Internal.FieldInfo>(field);
            if ((fieldInfo == null) || fieldInfo.IsIgnored)
                return;

            Formatter.NewLine();

            dollar.WriteTo(Formatter);
            Formatter.Dot();
            Formatter.Identifier(
                isConstant 
                    ? "Constant"
                    : "Field", EscapingMode.None
            );
            Formatter.LPar();

            Formatter.MemberDescriptor(
                field.IsPublic, fieldInfo.IsStatic,
                isReadonly: field.IsInitOnly,
                offset: field.DeclaringType.IsExplicitLayout 
                    ? (int?)field.Offset 
                    : null
            );

            Formatter.Comma();

            var fieldName = Util.EscapeIdentifier(fieldInfo.Name, EscapingMode.MemberIdentifier);

            Formatter.Value(fieldName);

            Formatter.Comma();

            Formatter.TypeReference(fieldInfo.FieldType, astEmitter.ReferenceContext);

            if (defaultValue != null) {
                Formatter.Comma();
                astEmitter.Emit(defaultValue);
            }

            Formatter.RPar();

            EmitCustomAttributes(context, field.DeclaringType, field, astEmitter);

            Formatter.Semicolon();
        }

        public void EmitCachedValues (IAstEmitter astEmitter, TypeExpressionCacher typeCacher, SignatureCacher signatureCacher, BaseMethodCacher baseMethodCacher) {
            var cts = typeCacher.CachedTypes.Values.OrderBy((ct) => ct.Index).ToArray();
            if (cts.Length > 0) {
                foreach (var ct in cts) {
                    Formatter.WriteRaw("var $T{0:X2} = function () ", ct.Index);
                    Formatter.OpenBrace();
                    Formatter.WriteRaw("return ($T{0:X2} = JSIL.Memoize(", ct.Index);
                    Formatter.Identifier(ct.Type, astEmitter.ReferenceContext, false);
                    Formatter.WriteRaw(")) ()");
                    Formatter.Semicolon(true);
                    Formatter.CloseBrace(false);
                    Formatter.Semicolon(true);
                }
            }

            var css = signatureCacher.Global.Signatures.OrderBy((cs) => cs.Value).ToArray();
            if (css.Length > 0) {
                foreach (var cs in css) {
                    Formatter.WriteRaw("var $S{0:X2} = function () ", cs.Value);
                    Formatter.OpenBrace();
                    Formatter.WriteRaw("return ($S{0:X2} = JSIL.Memoize(", cs.Value);
                    Formatter.Signature(cs.Key.Method, cs.Key.Signature, astEmitter.ReferenceContext, cs.Key.IsConstructor, false);
                    Formatter.WriteRaw(")) ()");
                    Formatter.Semicolon(true);
                    Formatter.CloseBrace(false);
                    Formatter.Semicolon(true);
                }
            }

            var bms = baseMethodCacher.CachedMethods.Values.OrderBy((ct) => ct.Index).ToArray();
            if (bms.Length > 0) {
                foreach (var bm in bms) {
                    Formatter.WriteRaw("var $BM{0:X2} = function () ", bm.Index);
                    Formatter.OpenBrace();
                    Formatter.WriteRaw("return ($BM{0:X2} = JSIL.Memoize(", bm.Index);
                    Formatter.WriteRaw("Function.call.bind(");
                    Formatter.Identifier(bm.Method.Reference, astEmitter.ReferenceContext, true);
                    Formatter.WriteRaw("))) ()");
                    Formatter.Semicolon(true);
                    Formatter.CloseBrace(false);
                    Formatter.Semicolon(true);
                }
            }

            var cims = signatureCacher.Global.InterfaceMembers.OrderBy((cim) => cim.Value).ToArray();
            if (cims.Length > 0) {
                foreach (var cim in cims) {
                    Formatter.WriteRaw("var $IM{0:X2} = function () ", cim.Value);
                    Formatter.OpenBrace();
                    Formatter.WriteRaw("return ($IM{0:X2} = JSIL.Memoize(", cim.Value);
                    Formatter.Identifier(cim.Key.InterfaceType, astEmitter.ReferenceContext, false);
                    Formatter.Dot();
                    Formatter.Identifier(cim.Key.InterfaceMember, EscapingMode.MemberIdentifier);
                    Formatter.WriteRaw(")) ()");
                    Formatter.Semicolon(true);
                    Formatter.CloseBrace(false);
                    Formatter.Semicolon(true);
                }
            }

            if ((cts.Length > 0) || (css.Length > 0))
                Formatter.NewLine();
        }

        public void EmitFunctionBody (IAstEmitter astEmitter, MethodDefinition method, JSFunctionExpression function) {
            astEmitter.ReferenceContext.Push();
            astEmitter.ReferenceContext.EnclosingMethod = method;

            try {
                astEmitter.Emit(function);
            } catch (Exception exc) {
                throw new Exception("Error occurred while generating javascript for method '" + method.FullName + "'.", exc);
            } finally {
                astEmitter.ReferenceContext.Pop();
            }

            EmitSemicolon();
            EmitSpacer();
        }

        public void EmitField (DecompilerContext context, IAstEmitter astEmitter, FieldDefinition field, JSRawOutputIdentifier dollar, JSExpression defaultValue) {
            EmitFieldOrConstant(context, astEmitter, field, dollar, defaultValue, false);
        }

        public void EmitConstant (DecompilerContext context, IAstEmitter astEmitter, FieldDefinition field, JSRawOutputIdentifier dollar, JSExpression value) {
            EmitFieldOrConstant(context, astEmitter, field, dollar, value, true);
        }
    }
}
