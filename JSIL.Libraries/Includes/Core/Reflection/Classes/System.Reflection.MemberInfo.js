//? include("../Utils/$jsilcore.MemberInfoExternals.js");

JSIL.ImplementExternals(
  "System.Reflection.MemberInfo", $jsilcore.MemberInfoExternals
);

JSIL.MakeClass("System.Object", "System.Reflection.MemberInfo", true, [], function ($) {
  $.Property({ Public: true, Static: false, Virtual: true }, "DeclaringType");
  $.Property({ Public: true, Static: false, Virtual: true }, "Name");
  $.Property({ Public: true, Static: false, Virtual: true }, "IsPublic");
  $.Property({ Public: true, Static: false, Virtual: true }, "IsStatic");
  $.Property({ Public: true, Static: false, Virtual: true }, "IsSpecialName");
  $.Property({ Public: true, Static: false, Virtual: true }, "MemberType");

  $.ImplementInterfaces(
    $jsilcore.TypeRef("System.Reflection.ICustomAttributeProvider")
  );
});