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
        var assembly = JSIL.GetAssembly(asm, true);
        if (assembly != null) {
            result.push(assembly.__Assembly__);
        }
    }
    return result;
}

function HLGetTypes(type, onlyInstantiable, includeGenericTypeDefinitions) {
    console.log("GetTypes called");
}


