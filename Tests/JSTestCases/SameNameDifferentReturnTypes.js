var $asm = JSIL.DeclareAssembly("Test");

JSIL.MakeStaticClass("Program", true, [], function ($) {
    $.Method({Static:true , Public:true }, "Main", 
        new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.String])], []), 
        function Main (args) {
            try {
                var inst = new T();
                var x = inst.Method();
            } catch (exc) {
                if (String(exc).indexOf("name and argument list") >= 0) {
                    print(exc);
                    return;
                } else {
                    throw exc;
                }
            }

            throw new Error("Should have failed");
        }
    );
});

JSIL.MakeType({
        BaseType: $jsilcore.TypeRef("System.Object"), 
        Name: "T", 
        IsPublic: true, 
        IsReferenceType: true, 
    }, function ($) {
        $.Method(
            { Public: true, Static: false},
            "Method",
            new JSIL.MethodSignature($.String, null, []),
            function () {
                return "String Method";
            }
        );

        $.Method(
            { Public: true, Static: false},
            "Method",
            new JSIL.MethodSignature($.Int32, null, []),
            function () {
                return "Int32 Method";
            }
        );
    }
);