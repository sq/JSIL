var $asm = JSIL.DeclareAssembly("Test");

function assertMatches (x, y) {
    if (x !== y)
        throw new Error("Expected '" + y + "', got '" + x + "'");
    else
        print("Matched '" + y + "'");
}

JSIL.MakeStaticClass("Program", true, [], function ($) {
    $.Method({Static:true , Public:true }, "Main", 
        new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.String])], []), 
        function Main (args) {
            var inst = new T();
            var x = inst.Method();
            var y = I1.Method.Call(inst);
            var z = I2.Method.Call(inst);

            assertMatches(x, "T.Method");
            assertMatches(y, "I1.Method");
            assertMatches(z, "I2.Method");
        }
    );
});

JSIL.MakeInterface("I1", true, [], function ($) {
    $.Method({}, "Method", new JSIL.MethodSignature($.String, null, []));
});

JSIL.MakeInterface("I2", true, [], function ($) {
    $.Method({}, "Method", new JSIL.MethodSignature($.String, null, []));
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
                return "T.Method";
            }
        );

        $.Method(
            { Public: true, Static: false},
            null,
            new JSIL.MethodSignature($.String, null, []),
            function () {
                return "I1.Method";
            }
        )
            .Overrides("I1", "Method");

        $.Method(
            { Public: true, Static: false},
            null,
            new JSIL.MethodSignature($.String, null, []),
            function () {
                return "I2.Method";
            }
        )
            .Overrides("I2", "Method");

        $.ImplementInterfaces(
            "I1",
            "I2"
        );
    }
);