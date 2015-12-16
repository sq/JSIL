JSIL.MakeClass("System.Object", "Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo", true, [], function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [], []),
    function () {
    }
  );

  $.Method({ Static: true, Public: true }, "Create",
    new JSIL.MethodSignature($.Type, [$jsilcore.TypeRef("Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags"), $.String], []),
    function CSharpArgumentInfo_Create(flags, name) {
      var info = new $jsilcore.Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo();
      info.Flags = flags;
      info.Name = name;
      return info;
    }
  );

  $.Field({ Public: false, Static: false }, "Flags", $jsilcore.TypeRef("Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags"));
  $.Field({ Public: false, Static: false }, "Name", $.String);

});