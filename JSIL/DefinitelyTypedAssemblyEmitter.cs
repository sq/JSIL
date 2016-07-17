using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using ICSharpCode.Decompiler;
using JSIL.Ast;
using JSIL.Compiler.Extensibility;
using JSIL.Internal;
using JSIL.Transforms;
using JSIL.Translator;
using Mono.Cecil;
using AssemblyDefinition = Mono.Cecil.AssemblyDefinition;
using TypeDefinition = Mono.Cecil.TypeDefinition;
using TypeInfo = JSIL.Internal.TypeInfo;

namespace JSIL {

    public class DefinitelyTypedInternalsEmitter : DefinitelyTypedBaseEmitter {
        public readonly AssemblyTranslator Translator;

        public DefinitelyTypedInternalsEmitter(
            AssemblyTranslator assemblyTranslator,
            JavascriptFormatter formatter) : base(formatter) {
            Translator = assemblyTranslator;
        }

        public override void EmitHeader (bool stubbed, bool iife) {
            Formatter.WriteRaw("import {$private as $asmJsilCore, StaticType as $StaticType, Type as $Type, NullArg as $Null} from \"./JSIL.Core\"");
            Formatter.NewLine();
        }

        public override void EmitAssemblyReferences (string assemblyDeclarationReplacement, Dictionary<AssemblyManifest.Token, string> assemblies) {
            if (assemblies != null) {
                foreach (var referenceOverride in assemblies) {
                    if (!Translator.IsIgnoredAssembly(referenceOverride.Value)) {
                        Formatter.WriteRaw(string.Format("import {{$private as {0}}} from \"./{1}\"", referenceOverride.Key.IDString, referenceOverride.Value));
                        Formatter.Semicolon();
                    }
                }
            }

            Formatter.WriteRaw("export declare namespace $private");
            Formatter.OpenBrace();
        }

        public override void EmitFooter (bool iife) {
            Formatter.CloseBrace();
        }

        public override bool EmitTypeDeclarationHeader(DecompilerContext context, IAstEmitter astEmitter, TypeDefinition typedef, TypeInfo typeInfo)
        {
            if (!DefinitelyTypedUtilities.IsTypePublic(typedef))
            {
                return true;
            }

            Formatter.EmitInsideNamespace(typedef, false, isTopLevel => {
                Formatter.WriteRaw("namespace");
                Formatter.Space();
                Formatter.Identifier(DefinitelyTypedUtilities.GetClassName(typedef));
                Formatter.OpenBrace();
                EmitClassInstance(typedef);
                EmitClassInOutType(typedef);
                EmitClassStatic(typedef);
                EmitClassFactory(typedef);
                Formatter.CloseBrace();
            });

            Formatter.NewLine();

            return true;
        }

        private void EmitClassInstance (TypeDefinition typedef) {
            Formatter.WriteRaw("class");
            Formatter.Space();
            Formatter.WriteSelfReference(typedef, Facade.Instance);
            Formatter.Space();
            Formatter.OpenBrace();

            Formatter.WriteRaw("private __$brand_");
            Formatter.Identifier(typedef.FullName);
            Formatter.WriteRaw(" : any");
            Formatter.Semicolon();

            foreach (var genericParameter in DefinitelyTypedUtilities.BuildGenericParemetersMap(typedef.GenericParameters, null)) {
                Formatter.WriteRaw("private");
                Formatter.Space();
                Formatter.WriteRaw("__$" + genericParameter.Value + "_brand_");
                Formatter.Identifier(typedef.FullName);
                Formatter.Space();
                Formatter.WriteRaw(":");
                Formatter.Space();
                if (genericParameter.Key.IsCovariant) {
                    Formatter.Identifier(DefinitelyTypedUtilities.GetGenericParameterOutParameterName(genericParameter.Value));
                } else if (genericParameter.Key.IsContravariant) {
                    Formatter.Identifier(DefinitelyTypedUtilities.GetGenericParameterInParameterName(genericParameter.Value));
                } else {
                    Formatter.Identifier(DefinitelyTypedUtilities.GetGenericParameterInstanceParameterName(genericParameter.Value));
                }
                Formatter.Semicolon();
            }

            if (!typedef.IsInterface) {
                var instanceFields = typedef.Fields.Where(it => it.IsPublic && !it.IsStatic && !Translator.ShouldSkipMember(it)).OrderBy(md => md.Name);
                foreach (var instanceField in instanceFields)
                {
                    EmitField(instanceField);
                }

                // TODO: We need filter them to not hide fields
                var instanceProperties = typedef.Properties
                .Where(it => !Translator.ShouldSkipMember(it) && !it.HasParameters && (
                        (it.GetMethod != null && it.GetMethod.IsPublic &&!it.GetMethod.IsStatic && !Translator.ShouldSkipMember(it.GetMethod))
                     || (it.SetMethod != null && it.SetMethod.IsPublic && !it.SetMethod.IsStatic && !Translator.ShouldSkipMember(it.SetMethod))))
                     .OrderBy(md => md.Name);
                foreach (var instanceProperty in instanceProperties)
                {
                    EmitProperty(instanceProperty);
                }

                var instanceMethods = typedef.Methods.Where(it => it.IsPublic && !it.IsStatic && !it.IsConstructor && !Translator.ShouldSkipMember(it)).OrderBy(md => md.Name);
                foreach (var instanceMethod in instanceMethods) {
                    EmitMethod(instanceMethod);
                }
            }

            Formatter.CloseBrace();
        }

        private void EmitClassInOutType (TypeDefinition typedef) {
            /*In*/
            Formatter.WriteRaw("type");
            Formatter.Space();
            Formatter.WriteSelfReference(typedef, Facade.TIn);

            Formatter.Space();
            Formatter.WriteRaw("=");
            Formatter.Space();

            Formatter.WriteSelfReference(typedef, Facade.Instance);

            if (typedef.IsClass && typedef.BaseType != null)
            {
                Formatter.Space();
                Formatter.WriteRaw("|");
                Formatter.Space();
                Formatter.WriteTypeReference(typedef.BaseType, typedef, JavascriptFormatterHelper.ReplaceMode.In, false);
            }

            var interfaces = typedef.Interfaces.Where(it => DefinitelyTypedUtilities.IsTypePublic(it) && !Translator.ShouldSkipMember(it));
            foreach (var iface in interfaces)
            {
                Formatter.Space();
                Formatter.WriteRaw("|");
                Formatter.Space();
                Formatter.WriteTypeReference(iface, typedef, JavascriptFormatterHelper.ReplaceMode.In, false);
            }

            Formatter.Semicolon();

            /*Out*/
            Formatter.WriteRaw("type");
            Formatter.Space();
            Formatter.WriteSelfReference(typedef, Facade.TOut);

            Formatter.Space();
            Formatter.WriteRaw("=");
            Formatter.Space();

            Formatter.WriteSelfReference(typedef, Facade.Instance);

            if (typedef.IsClass && typedef.BaseType != null) {
                Formatter.Space();
                Formatter.WriteRaw("&");
                Formatter.Space();
                Formatter.WriteTypeReference(typedef.BaseType, typedef, JavascriptFormatterHelper.ReplaceMode.Out, false);
            }

            foreach (var iface in interfaces) {
                Formatter.Space();
                Formatter.WriteRaw("&");
                Formatter.Space();
                Formatter.WriteTypeReference(iface, typedef, JavascriptFormatterHelper.ReplaceMode.Out, false);
            }

            Formatter.Semicolon();
        }

        private void EmitClassStatic (TypeDefinition typedef) {
            Formatter.WriteRaw("interface");
            Formatter.Space();
            Formatter.WriteSelfReference(typedef, Facade.Static);
            Formatter.Space();
            Formatter.WriteRaw("extends");
            Formatter.Space();
            Formatter.WriteRaw(typedef.IsAbstract && typedef.IsSealed ? "$StaticType" : "$Type");
            Formatter.WriteRaw("<");
            Formatter.WriteSelfReference(typedef, Facade.Instance);
            Formatter.Comma();
            Formatter.WriteSelfReference(typedef, Facade.TIn);
            Formatter.Comma();
            Formatter.WriteSelfReference(typedef, Facade.TOut);
            Formatter.WriteRaw(">");
            Formatter.Space();
            Formatter.OpenBrace();

            var staticFields = typedef.Fields.Where(it => it.IsPublic && it.IsStatic && !Translator.ShouldSkipMember(it)).OrderBy(md => md.Name);
            foreach (var staticField in staticFields)
            {
                EmitField(staticField);
            }

            // TODO: We need filter them to not hide fields
            var staticProperties = typedef.Properties
                .Where(it => !Translator.ShouldSkipMember(it) && !it.HasParameters && (
                        (it.GetMethod != null && it.GetMethod.IsPublic && it.GetMethod.IsStatic && !Translator.ShouldSkipMember(it.GetMethod))
                     || (it.SetMethod != null && it.SetMethod.IsPublic && it.SetMethod.IsStatic && !Translator.ShouldSkipMember(it.SetMethod))))
                     .OrderBy(md => md.Name);
            foreach (var staticProperty in staticProperties)
            {
                EmitProperty(staticProperty);
            }

            var constructors = typedef.Methods.Where(it => it.IsPublic && !it.IsStatic && it.IsConstructor && !Translator.ShouldSkipMember(it));
            var staticMethods = typedef.Methods.Where(it => it.IsPublic && it.IsStatic && !it.IsConstructor && !Translator.ShouldSkipMember(it)).OrderBy(it => it.Name);
            foreach (var method in constructors.Concat(staticMethods)) {
                EmitMethod(method);
            }

            if (typedef.IsInterface) {
                var instanceMethods = typedef.Methods.Where(it => it.IsPublic && !Translator.ShouldSkipMember(it)).GroupBy(method => method.Name).OrderBy(group => group.Key);
                foreach (var instanceMethodGroup in instanceMethods)
                {
                    EmitInterfaceMethodGroup(instanceMethodGroup.Key, instanceMethodGroup);
                }
            }

            Formatter.CloseBrace();
        }

        private void EmitClassFactory (TypeDefinition typedef) {
            if (typedef.GenericParameters.Count > 0) {
                Formatter.WriteRaw("interface");
                Formatter.Space();
                Formatter.WriteSelfReference(typedef, Facade.Factory);
                Formatter.Space();
                Formatter.OpenBrace();
                Formatter.Identifier("Of");
                Formatter.WriteGenericMethodSignatureWithoutResultType(typedef.GenericParameters, null);
                Formatter.Space();
                Formatter.WriteRaw(":");
                Formatter.Space();
                Formatter.WriteSelfReference(typedef, Facade.Static);
                Formatter.CloseBrace();
            } else {
                Formatter.WriteRaw("type");
                Formatter.Space();
                Formatter.WriteSelfReference(typedef, Facade.Factory);
                Formatter.Space();
                Formatter.WriteRaw("=");
                Formatter.Space();
                Formatter.WriteSelfReference(typedef, Facade.Static);
                Formatter.Semicolon();
            }
        }

        private void EmitField (FieldDefinition field) {
            Formatter.Identifier(field.Name);
            Formatter.Space();
            Formatter.WriteRaw(":");
            Formatter.Space();
            Formatter.WriteTypeReference(field.FieldType, null, JavascriptFormatterHelper.ReplaceMode.Out);
            Formatter.Semicolon();
        }

        private void EmitProperty(PropertyDefinition property)
        {
            Formatter.Identifier(property.Name);
            Formatter.Space();
            Formatter.WriteRaw(":");
            Formatter.Space();
            Formatter.WriteTypeReference(property.PropertyType, null, JavascriptFormatterHelper.ReplaceMode.Out);
            Formatter.Semicolon();
        }

        private void EmitMethod (MethodDefinition method) {
            if (method.IsConstructor) {
                Formatter.WriteRaw("new");
            } else {
                Formatter.Identifier(method.Name);
                if (method.GenericParameters.Count > 0) {
                    Formatter.WriteGenericMethodSignatureWithoutResultType(method.GenericParameters, method.DeclaringType.GenericParameters);
                    Formatter.Space();
                    Formatter.WriteRaw(":");
                    Formatter.Space();
                }
            }

            Formatter.WriteRaw("(");
            Formatter.CommaSeparatedList(method.Parameters, item => {
                Formatter.Identifier(item.Name);
                Formatter.Space();
                Formatter.WriteRaw(":");
                Formatter.Space();
                Formatter.WriteTypeReference(item.ParameterType, null, JavascriptFormatterHelper.ReplaceMode.Instance);
            });
            Formatter.WriteRaw(")");
            Formatter.Space();
            Formatter.WriteRaw(method.GenericParameters.Count > 0 ? "=>" : ":");
            Formatter.Space();

            if (!method.IsConstructor) {
                Formatter.WriteTypeReference(method.ReturnType, null, JavascriptFormatterHelper.ReplaceMode.Out);
            } else {
                Formatter.WriteSelfReference(method.DeclaringType, Facade.TOut);
            }

            Formatter.Semicolon();
        }

        private void EmitInterfaceMethodGroup (string name, IEnumerable<MethodDefinition> methods) {
            Formatter.Identifier(name);
            Formatter.Space();
            Formatter.WriteRaw(":");
            Formatter.Space();
            Formatter.OpenBrace();
            foreach (var methodDefinition in methods) {
                EmitInterfaceMethod(methodDefinition);
            }
            Formatter.CloseBrace();
        }

        private void EmitInterfaceMethod (MethodDefinition method) {
            Formatter.Identifier("Call");

            if (method.GenericParameters.Count > 0) {
                Formatter.WriteGenericArgumentsIfNeed(method.GenericParameters, method.DeclaringType.GenericParameters);
            }

            Formatter.WriteRaw("(");
            Formatter.Identifier("thisArg");
            Formatter.Space();
            Formatter.WriteRaw(":");
            Formatter.Space();
            Formatter.WriteSelfReference(method.DeclaringType, Facade.Instance);
            Formatter.Comma();

            if (method.GenericParameters.Count > 0) {
                Formatter.Identifier("genericArgs");
                Formatter.Space();
                Formatter.WriteRaw(":");
                Formatter.Space();
                Formatter.OpenBracket();
                Formatter.CommaSeparatedList(DefinitelyTypedUtilities.BuildGenericParemetersMap(method.GenericParameters, method.DeclaringType.GenericParameters), pair => {
                    Formatter.WriteRaw("$Type");
                    Formatter.WriteGenericArgumentsIfNeed(new[] { DefinitelyTypedUtilities.GetGenericParameterInstanceParameterName(pair.Value), DefinitelyTypedUtilities.GetGenericParameterInParameterName(pair.Value), DefinitelyTypedUtilities.GetGenericParameterOutParameterName(pair.Value) });
                });
                Formatter.CloseBracket();
            } else {
                Formatter.WriteRaw("nullArg");
                Formatter.Space();
                Formatter.WriteRaw(":");
                Formatter.Space();
                Formatter.Identifier("$Null");
            }

            if (method.Parameters.Count > 0) {
                Formatter.Comma();

                Formatter.CommaSeparatedList(method.Parameters, item => {
                    Formatter.Identifier(item.Name);
                    Formatter.Space();
                    Formatter.WriteRaw(":");
                    Formatter.Space();
                    Formatter.WriteTypeReference(item.ParameterType, null, JavascriptFormatterHelper.ReplaceMode.Instance);
                });
            }

            Formatter.WriteRaw(")");
            Formatter.Space();
            Formatter.WriteRaw(":");
            Formatter.Space();
            Formatter.WriteTypeReference(method.ReturnType, null, JavascriptFormatterHelper.ReplaceMode.Out);
            Formatter.Semicolon();
        }
    }

    public class DefinitelyTypedExportEmitter : DefinitelyTypedBaseEmitter
    {
        public readonly AssemblyTranslator Translator;

        public DefinitelyTypedExportEmitter(
            AssemblyTranslator assemblyTranslator,
            JavascriptFormatter formatter) : base(formatter)
        {
            Translator = assemblyTranslator;
        }

        public override void EmitHeader(bool stubbed, bool iife)
        {
            Formatter.WriteRaw(string.Format("import {{$private as $this}} from \"./internals/{0}\"", Formatter.Assembly.FullName));
            Formatter.Semicolon();
        }

        public override bool EmitTypeDeclarationHeader(DecompilerContext context, IAstEmitter astEmitter, TypeDefinition typedef, TypeInfo typeInfo) {
            if (!DefinitelyTypedUtilities.IsTypePublic(typedef))
            {
                return true;
            }

            Formatter.EmitInsideNamespace(typedef, true, isTopLevel => {
                if (isTopLevel)
                {
                    Formatter.WriteRaw("export");
                    Formatter.Space();
                    Formatter.WriteRaw("declare");
                    Formatter.Space();
                }

                Formatter.WriteRaw("let");
                Formatter.Space();

                Formatter.Identifier(DefinitelyTypedUtilities.GetClassName(typedef));
                Formatter.Space();
                Formatter.WriteRaw(":");
                Formatter.Space();

                Formatter.Identifier("$this");
                Formatter.Dot();

                foreach (var part in DefinitelyTypedUtilities.GetFullNamespace(typedef))
                {
                    Formatter.Identifier(part);
                    Formatter.Dot();
                }

                Formatter.Identifier(DefinitelyTypedUtilities.GetClassName(typedef));
                Formatter.Dot();
                Formatter.Identifier("Factory");
                Formatter.Semicolon();
            });

            return true;
        }
    }

    public class DefinitelyTypedModuleEmitter : DefinitelyTypedBaseEmitter
    {
        public readonly AssemblyTranslator Translator;

        public DefinitelyTypedModuleEmitter(
            AssemblyTranslator assemblyTranslator,
            JavascriptFormatter formatter) : base(formatter)
        {
            Translator = assemblyTranslator;
        }

        public override void EmitHeader(bool stubbed, bool iife)
        {
            Formatter.WriteRaw(string.Format("module.exports = JSIL.GetAssembly(\"{0}\");", Formatter.Assembly.FullName));
            Formatter.Semicolon();
        }
    }

    public enum Facade {
        Instance,
        TIn,
        TOut,
        Static,
        Factory
    }

    static class JavascriptFormatterHelper {
        public enum ReplaceMode {
            Instance,
            In,
            Out
        }

        private static Dictionary<string, string> _rawTypes = new Dictionary<string, string> {
            {"System.Void", "void"},
        };

        private static List<string> _coreTypes = new List<string> {
            "System.String",
            "System.Byte",
            "System.SByte",
            "System.Int16",
            "System.UInt16",
            "System.Int32",
            "System.UInt32",
            "System.Single",
            "System.Double",
            "System.Boolean",
            "System.Char",
            "System.Object"
        };

        private static void TRSuffix (this JavascriptFormatter formatter, ReplaceMode replaceMode) {
            switch (replaceMode) {
                case ReplaceMode.In:
                    formatter.Dot();
                    formatter.Identifier("TIn");
                    break;
                case ReplaceMode.Out:
                    formatter.Dot();
                    formatter.Identifier("TOut");
                    break;
                case ReplaceMode.Instance:
                    formatter.Dot();
                    formatter.Identifier("Instance");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("replaceMode");
            }
        }

        public static bool WriteTypeReference (this JavascriptFormatter formatter, TypeReference typeReference, TypeDefinition context, ReplaceMode replaceMode, bool useStandartSubstitution = true) {
            if (typeReference is ArrayType) {
                var arrayType = (ArrayType) typeReference;
                formatter.WriteRaw("$asmJsilCore.System.");
                formatter.Identifier(arrayType.IsVector ? "Vector" : "Array");
                formatter.TRSuffix(replaceMode);
                formatter.WriteRaw("<");
                formatter.WriteTypeReference(arrayType.ElementType, context, ReplaceMode.Instance);
                formatter.Comma();
                formatter.WriteTypeReference(arrayType.ElementType, context, ReplaceMode.In);
                formatter.Comma();
                formatter.WriteTypeReference(arrayType.ElementType, context, ReplaceMode.Out);
                if (!arrayType.IsVector) {
                    formatter.Comma();
                    formatter.WriteRaw("\"");
                    formatter.Value(arrayType.Dimensions.Count.ToString(CultureInfo.InvariantCulture));
                    formatter.WriteRaw("\"");
                }
                formatter.WriteRaw(">");
            } else if (typeReference is ByReferenceType) {
                var byRefType = (ByReferenceType) typeReference;
                formatter.WriteRaw("$asmJsilCore.JSIL.Reference");
                formatter.TRSuffix(replaceMode);
                formatter.WriteRaw("<");
                formatter.WriteTypeReference(byRefType.ElementType, context, ReplaceMode.Instance);
                formatter.Comma();
                formatter.WriteTypeReference(byRefType.ElementType, context, ReplaceMode.In);
                formatter.Comma();
                formatter.WriteTypeReference(byRefType.ElementType, context, ReplaceMode.Out);
                formatter.WriteRaw(">");
            } else if (typeReference is GenericParameter) {
                var gp = (GenericParameter) typeReference;
                DefinitelyTypedUtilities.GenericParemetersKeyedCollection map;
                if (gp.Owner is TypeDefinition) {
                    map = DefinitelyTypedUtilities.BuildGenericParemetersMap(((TypeDefinition) gp.Owner).GenericParameters, null);
                } else if (gp.Owner is MethodDefinition) {
                    map = DefinitelyTypedUtilities.BuildGenericParemetersMap(((MethodDefinition) gp.Owner).GenericParameters, ((MethodDefinition) gp.Owner).DeclaringType.GenericParameters);
                } else {
                    throw new Exception("Unexpected generic parameter owner");
                }

                var name = map[gp].Value;

                switch (replaceMode) {
                    case ReplaceMode.Instance:
                        formatter.Identifier(DefinitelyTypedUtilities.GetGenericParameterInstanceParameterName(name));
                        break;
                    case ReplaceMode.Out:
                        formatter.Identifier(DefinitelyTypedUtilities.GetGenericParameterOutParameterName(name));
                        break;
                    case ReplaceMode.In:
                        formatter.Identifier(DefinitelyTypedUtilities.GetGenericParameterInParameterName(name));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("replaceMode", replaceMode, null);
                }
            } else if (typeReference is GenericInstanceType) {
                var genericType = (GenericInstanceType) typeReference;
                if (formatter.WriteTypeReference(genericType.ElementType, context, replaceMode)) /*TODO*/ {
                    formatter.WriteRaw("<");
                    formatter.CommaSeparatedList(genericType.GenericArguments, genericArgument => {
                        formatter.WriteTypeReference(genericArgument, context, ReplaceMode.Instance);
                        formatter.Comma();
                        formatter.WriteTypeReference(genericArgument, context, ReplaceMode.In);
                        formatter.Comma();
                        formatter.WriteTypeReference(genericArgument, context, ReplaceMode.Out);
                    });
                    formatter.WriteRaw(">");
                }
            } else if (typeReference is PointerType || typeReference is OptionalModifierType || typeReference is RequiredModifierType || typeReference is PinnedType || typeReference is SentinelType || typeReference is FunctionPointerType
                || (!_coreTypes.Contains(typeReference.FullName) && formatter.TypeInfo.Get(typeReference).IsSuppressDeclaration)) {
                formatter.WriteRaw("Object"); // TODO!
                return false;
            } else {
                string rawType;
                if (useStandartSubstitution && _rawTypes.TryGetValue(typeReference.FullName, out rawType)) {
                    formatter.WriteRaw(rawType);
                } else {
                    var definition = typeReference.Resolve();
                    var targetAssembly = JavascriptFormatter.GetContainingAssemblyName(typeReference);
                    string assemblyRef;

                    if (_coreTypes.Contains(definition.FullName)) {
                        assemblyRef = "$asmJsilCore";
                    } else if (targetAssembly == formatter.Assembly.FullName) {
                        assemblyRef = string.Empty;
                    } else {
                        assemblyRef = formatter.Manifest.Entries.FirstOrDefault(item => item.Value == targetAssembly).Key;
                    }
                    if (definition != null && assemblyRef != null) {
                        if (assemblyRef != string.Empty) {
                            formatter.Identifier(assemblyRef);
                            formatter.Dot();
                        } else {
                            formatter.Identifier("$private");
                            formatter.Dot();
                        }

                        foreach (var part in DefinitelyTypedUtilities.GetFullNamespace(definition)) {
                            formatter.Identifier(part);
                            formatter.Dot();
                        }

                        formatter.Identifier(DefinitelyTypedUtilities.GetClassName(definition));

                        /* Hack to solve ciruclar refrence in generics. 
                         * It could be improved, by we really need generic variance support or support of:
                         type T = something & I<T> (see Microsoft/TypeScript#6230)
                         */
                        var fixedMode = (replaceMode != ReplaceMode.Instance && context == definition) ? ReplaceMode.Instance : replaceMode;
                        formatter.TRSuffix(fixedMode);
                    }
                    else {
                        //TODO: We was unable to resolve assembly. Think about JSIL Proxies
                        formatter.WriteRaw("Object");
                        return false;
                    }
                }
            }
            return true;
        }

        public static void CommaSeparatedList<T> (this JavascriptFormatter formatter, IEnumerable<T> list, Action<T> process) {
            bool first = true;
            foreach (var item in list) {
                if (first) {
                    first = false;
                } else {
                    formatter.Comma();
                }

                process(item);
            }
        }

        public static void WriteSelfReference (this JavascriptFormatter formatter, TypeDefinition typeDefinition, Facade facade) {
            switch (facade) {
                case Facade.Instance:
                    formatter.Identifier("Instance");
                    formatter.WriteGenericArgumentsIfNeed(typeDefinition.GenericParameters, null);
                    break;
                case Facade.TIn:
                    formatter.Identifier("TIn");
                    formatter.WriteGenericArgumentsIfNeed(typeDefinition.GenericParameters, null);
                    break;
                case Facade.TOut:
                    formatter.Identifier("TOut");
                    formatter.WriteGenericArgumentsIfNeed(typeDefinition.GenericParameters, null);
                    break;
                case Facade.Static:
                    formatter.Identifier("Static");
                    formatter.WriteGenericArgumentsIfNeed(typeDefinition.GenericParameters, null);
                    break;
                case Facade.Factory:
                    formatter.Identifier("Factory");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("facade", facade, null);
            }
        }

        public static void WriteGenericMethodSignatureWithoutResultType (this JavascriptFormatter formatter, IEnumerable<GenericParameter> args, IEnumerable<GenericParameter> additionalArgsForNameCalculation) {
            formatter.WriteGenericArgumentsIfNeed(args, additionalArgsForNameCalculation);
            formatter.WriteRaw("(");

            formatter.CommaSeparatedList(DefinitelyTypedUtilities.BuildGenericParemetersMap(args, additionalArgsForNameCalculation), pair => {
                formatter.Identifier("__" + pair.Value);
                formatter.Space();
                formatter.WriteRaw(":");
                formatter.Space();
                formatter.WriteRaw("$Type");
                formatter.WriteGenericArgumentsIfNeed(new[] { DefinitelyTypedUtilities.GetGenericParameterInstanceParameterName(pair.Value), DefinitelyTypedUtilities.GetGenericParameterInParameterName(pair.Value), DefinitelyTypedUtilities.GetGenericParameterOutParameterName(pair.Value)});
            });
            formatter.WriteRaw(")");
        }

        public static void WriteGenericArgumentsIfNeed (this JavascriptFormatter formatter, IEnumerable<GenericParameter> args, IEnumerable<GenericParameter> additionalArgsForNameCalculation) {
            formatter.WriteGenericArgumentsIfNeed(
                DefinitelyTypedUtilities.BuildGenericParemetersMap(args, additionalArgsForNameCalculation)
                    .SelectMany(pair => new[] { DefinitelyTypedUtilities.GetGenericParameterInstanceParameterName(pair.Value), DefinitelyTypedUtilities.GetGenericParameterInParameterName(pair.Value), DefinitelyTypedUtilities.GetGenericParameterOutParameterName(pair.Value)}));
        }

        public static void WriteGenericArgumentsIfNeed (this JavascriptFormatter formatter, IEnumerable<string> genericParameterNames) {
            var items = genericParameterNames.ToList();
            if (items.Count > 0) {
                formatter.WriteRaw("<");
                formatter.CommaSeparatedList(items, item => { formatter.Identifier(item); });
                formatter.WriteRaw(">");
            }
        }

        public static void EmitInsideNamespace(this JavascriptFormatter formatter, TypeDefinition typedef, bool isTopLevel, Action<bool> inner)
        {
            var fullNamespace = DefinitelyTypedUtilities.GetFullNamespace(typedef);

            foreach (var part in fullNamespace)
            {
                if (isTopLevel)
                {
                    formatter.WriteRaw("export");
                    formatter.Space();
                    formatter.WriteRaw("declare");
                    formatter.Space();
                    isTopLevel = false;
                }

                formatter.WriteRaw("namespace");
                formatter.Space();
                formatter.Identifier(part);
                formatter.Space();
                formatter.OpenBrace();
            }

            inner(isTopLevel);

            foreach (var part in fullNamespace)
            {
                formatter.CloseBrace();
            }
        }

    }

    public static class DefinitelyTypedUtilities {
        public class GenericParemetersKeyedCollection : KeyedCollection<GenericParameter, KeyValuePair<GenericParameter, string>> {
            protected override GenericParameter GetKeyForItem (KeyValuePair<GenericParameter, string> item) {
                return item.Key;
            }
        }

        public static GenericParemetersKeyedCollection BuildGenericParemetersMap (IEnumerable<GenericParameter> genericParameters, IEnumerable<GenericParameter> useForNameCalculations) {
            var usedNames = new HashSet<string>();
            var map = new GenericParemetersKeyedCollection();

            if (useForNameCalculations != null) {
                foreach (var genericParameter in useForNameCalculations) {
                    var name = SelectFreeName(genericParameter.Name, usedNames);
                    usedNames.Add(name);
                }
            }

            foreach (var genericParameter in genericParameters) {
                var name = SelectFreeName(genericParameter.Name, usedNames);
                usedNames.Add(name);
                map.Add(new KeyValuePair<GenericParameter, string>(genericParameter, name));
            }
            return map;
        }

        public static IEnumerable<string> GetFullNamespace (TypeDefinition typeReference) {
            var declaringType = typeReference;
            while (declaringType.DeclaringType != null) {
                declaringType = declaringType.DeclaringType;
            }

            foreach (var part in declaringType.Namespace.Split('.').Where(it => !string.IsNullOrWhiteSpace(it))) {
                yield return part;
            }
        }

        public static string GetClassName (TypeDefinition typeReference) {
            var declaringType = typeReference;
            var listOfOuters = new List<string>();
            do {
                listOfOuters.Add(declaringType.Name);
                declaringType = declaringType.DeclaringType;
            } while (declaringType != null);
            listOfOuters.Reverse();

            return string.Join("_", listOfOuters);
        }

        public static string GetGenericParameterInstanceParameterName(string parameterName)
        {
            return "$T_" + parameterName;
        }

        public static string GetGenericParameterInParameterName (string parameterName) {
            return "$In_" + parameterName;
        }

        public static string GetGenericParameterOutParameterName (string parameterName) {
            return "$Out_" + parameterName;
        }

        public static bool IsTypePublic (TypeReference typeReference) {
            var typeDef = typeReference.Resolve();
            while (typeDef != null) {
                if (typeDef.IsPublic) {
                    return true;
                }
                if (!typeDef.IsNestedPublic) {
                    return false;
                }
                typeDef = typeDef.DeclaringType;
            }
            return false;
        }

        private static string SelectFreeName (string original, HashSet<string> usedNames) {
            var name = original;
            var i = 1;
            while (usedNames.Contains(name)) {
                name = original + "_" + i.ToString(CultureInfo.InvariantCulture);
            }
            return name;
        }
    }

    public abstract class DefinitelyTypedBaseEmitter : IAssemblyEmitter {
        public readonly JavascriptFormatter Formatter;

        public DefinitelyTypedBaseEmitter(JavascriptFormatter formatter)
        {
            Formatter = formatter;
        }

        public virtual void EmitHeader (bool stubbed, bool iife) {
        }

        public virtual void EmitFooter (bool iife) {
        }

        public virtual void EmitAssemblyEntryPoint (AssemblyDefinition assembly, MethodDefinition entryMethod, MethodSignature signature) {
        }

        public virtual IAstEmitter MakeAstEmitter(JSILIdentifier jsil, TypeSystem typeSystem, TypeInfoProvider typeInfoProvider, Configuration configuration)
        {
            return new DefinitelyTypedEmptyAstEmitter(
                Formatter, jsil, typeSystem, typeInfoProvider, configuration
                );
        }

        public virtual void EmitTypeAlias (TypeDefinition typedef) {
        }

        public virtual bool EmitTypeDeclarationHeader (DecompilerContext context, IAstEmitter astEmitter, TypeDefinition typedef, TypeInfo typeInfo) {
            return false;
        }

        public virtual void EmitCustomAttributes (DecompilerContext context, TypeReference declaringType, ICustomAttributeProvider member, IAstEmitter astEmitter, bool standalone = true) {
        }

        public virtual void EmitMethodDefinition (DecompilerContext context, MethodReference methodRef, MethodDefinition method, IAstEmitter astEmitter, bool stubbed, JSRawOutputIdentifier dollar, MethodInfo methodInfo = null) {
        }

        public virtual void EmitSpacer () {
        }

        public virtual void EmitSemicolon () {
        }

        public virtual void EmitProxyComment (string fullName) {
        }

        public virtual void EmitEvent (DecompilerContext context, IAstEmitter astEmitter, EventDefinition @event, JSRawOutputIdentifier dollar) {
        }

        public virtual void EmitProperty (DecompilerContext context, IAstEmitter astEmitter, PropertyDefinition property, JSRawOutputIdentifier dollar) {
        }

        public virtual void EmitField (DecompilerContext context, IAstEmitter astEmitter, FieldDefinition field, JSRawOutputIdentifier dollar, JSExpression defaultValue) {
        }

        public virtual void EmitConstant (DecompilerContext context, IAstEmitter astEmitter, FieldDefinition field, JSRawOutputIdentifier dollar, JSExpression value) {
        }

        public virtual void EmitPrimitiveDefinition (DecompilerContext context, TypeDefinition typedef, bool stubbed, JSRawOutputIdentifier dollar) {
        }

        public virtual void BeginEmitTypeDeclaration (TypeDefinition typedef) {
        }

        public virtual void BeginEmitTypeDefinition (IAstEmitter astEmitter, TypeDefinition typedef, TypeInfo typeInfo, TypeReference baseClass) {
        }

        public virtual void EndEmitTypeDefinition (IAstEmitter astEmitter, DecompilerContext context, TypeDefinition typedef) {
        }

        public virtual void EmitInterfaceList (TypeInfo typeInfo, IAstEmitter astEmitter, JSRawOutputIdentifier dollar) {
        }

        public virtual void EmitCachedValues (IAstEmitter astEmitter, TypeExpressionCacher typeCacher, SignatureCacher signatureCacher, BaseMethodCacher baseMethodCacher) {
        }

        public virtual void EmitFunctionBody (IAstEmitter astEmitter, MethodDefinition method, JSFunctionExpression function) {
        }

        public virtual void EmitAssemblyReferences (string assemblyDeclarationReplacement, Dictionary<AssemblyManifest.Token, string> assemblies) {
        }

        private class DefinitelyTypedEmptyAstEmitter : IAstEmitter
        {

            public DefinitelyTypedEmptyAstEmitter(
                JavascriptFormatter output, JSILIdentifier jsil,
                TypeSystem typeSystem, ITypeInfoSource typeInfo,
                Configuration configuration
                )
            {
                //Configuration = configuration;
                //Output = output;
                //JSIL = jsil;
                TypeSystem = typeSystem;
                //TypeInfo = typeInfo;

                //IncludeTypeParens.Push(false);
                //PassByRefStack.Push(false);
                //OverflowCheckStack.Push(false);

                //VisitNestedFunctions = true;

                /*if (output.SourceMapBuilder != null)
                {
                    BeforeNodeProcessed += AddSourceMapInfo;
                    //AfterNodeProcessed += AddSourceMapInfoEnd;
                }*/
                ReferenceContext = new TypeReferenceContext();
            }

            public TypeSystem TypeSystem { get; private set; }
            public TypeReferenceContext ReferenceContext { get; private set; }
            public SignatureCacher SignatureCacher { get; set; }
            public void Emit(JSNode node) { }
        }
    }
}