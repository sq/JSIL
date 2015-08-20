JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.MarshalByRefObject", true, [], function ($) {
    $.Field({ Static: false, Public: false }, "__identity", $.Object);

    $.Property({ Static: false, Public: false, Virtual: true }, "Identity");
});