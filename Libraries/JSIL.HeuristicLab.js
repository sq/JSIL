// JavaScript source code
"use strict";

if (typeof (JSIL) === "undefined")
    throw new Error("JSIL.Core is required");

if (!$jsilcore)
    throw new Error("JSIL.Core is required");

function get_CurrentDomain() {
    console.log("CurrentDomain called");
}

function GetAssemblies() {
    var result = [];
    for (var asm in JSIL.PrivateNamespaces) {
        // Only load HeuristicLab assemblies
        if (asm.indexOf("HeuristicLab") !== -1) {
            var assembly = JSIL.GetAssembly(asm, true);
            if (assembly != null) {
                result.push(assembly.__Assembly__);
            }
        }
    }
    return result;
}

function HLGetTypes(type, assembly, pOnlyInstantiable, pIncludeGenericTypeDefinitions) {
    var assemblyTypes = assembly.GetTypes();
    var onlyInstantiable = true;
    if (pOnlyInstantiable == null || typeof (pOnlyInstantiable) == "undefined")
        onlyInstantiable = true;
    else
        onlyInstantiable = Boolean(pOnlyInstantiable);
    var includeGenericTypeDefinitions = Boolean(pIncludeGenericTypeDefinitions);

    JSIL.InitializeType(type);
    var result = [];

    for (var k = 0; k < assemblyTypes.length; k++) {
        JSIL.InitializeType(assemblyTypes[k]);
        /*if (assemblyTypes[k].Name == "SequentialEngine") {
            console.log("SequentialEngine");
        }*/

        var t = HLBuildType(assemblyTypes[k], type);

        if (t != null) {
            
            
            if (HLIsSubTypeOf(t, type)) {
                if (!HLIsNonDiscoverable(t)) {
                    if (onlyInstantiable === false || (!t.IsAbstract && !t.IsInterface && !t.HasElementType)) {
                        if (includeGenericTypeDefinitions || !t.IsGenericTypeDefinition) {
                            result.push(t);
                        }
                    }
                }
            }
        }
    }

    /*if (result.length > 0) {
        console.log("-------------------")
        for (var i = 0; i < result.length; i++) {
            console.log("Selected type: " + result[i].Name)
        }
    }*/

    return result;
}

function HLIsNonDiscoverable(type) {
    if (type.__Attributes__.length != 0) {
        var attributes = type.__Attributes__;
        for (var i = 0; i < attributes.length; i++) {
            if (attributes[i].type.typeName == "HeuristicLab.PluginInfrastructure.NonDiscoverableTypeAttribute")
                return true;
        }
    }
    return false;
}

function HLBuildType(type, protoType) {
    if (type == null || protoType == null)
        return null;

    if (!type.IsGenericTypeDefinition) return type;
    if (protoType.IsGenericTypeDefinition) return type;
    if (!protoType.IsGenericType) return type;

    var typeGenericArguments = type.GetGenericArguments();
    var protoTypeGenericArguments = protoType.GetGenericArguments();
    if (typeGenericArguments.length != protoTypeGenericArguments.length) return null;

    for (var i = 0; i < typeGenericArguments.length; i++) {
        var typeGenericArgument = typeGenericArguments[i];
        var protoTypeGenericArgument = protoTypeGenericArguments[i];

        //check class contraint on generic type parameter 
        if (typeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            if (!protoTypeGenericArgument.IsClass && !protoTypeGenericArgument.IsInterface && !protoType.IsArray) return null;

        //check default constructor constraint on generic type parameter 
        if (typeGenericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            if (!protoTypeGenericArgument.IsValueType && protoTypeGenericArgument.GetConstructor(Type.EmptyTypes) == null) return null;

        //check type restrictions on generic type parameter
        var constraints = typeGenericArgument.GetGenericParameterConstraints();
        for (var c = 0; c < constraints.length; i++) {
            if (!constraints[c].IsAssignableFrom(protoTypeGenericArgument)) return null;
        }
    }
    try {
        return type.MakeGenericType(protoTypeGenericArguments);
    } catch (err) {
        return null;
    }
}

function HLIsSubTypeOf(subType, baseType) {
    if (baseType.IsAssignableFrom(subType)) return true;
    if (!baseType.IsGenericType) return false;

    if (HLRecursiveCheckGenericTypes(baseType, subType)) return true;

    var interfaces = subType.GetInterfaces();
    for (var i = 0; i < interfaces.length; i++) {
        if (interfaces[i].IsGenericType) {
            if (baseType.CheckGenericTypes(interfaces[i])) return true;
        }
    }

    return false;
}

function HLRecursiveCheckGenericTypes(baseType, subType) {
    JSIL.InitializeType(subType);
    if (!baseType.IsGenericType) return false;
    if (!subType.IsGenericType) return false;
    if (baseType.CheckGenericTypes(subType)) return true;
    if (subType.BaseType == null) return false;

    return RecursiveCheckGenericTypes(baseType, subType.BaseType);
}



