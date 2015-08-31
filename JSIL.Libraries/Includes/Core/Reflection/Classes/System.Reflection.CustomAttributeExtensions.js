JSIL.ImplementExternals(
  "System.Reflection.CustomAttributeExtensions", function ($) {
      $.Method({ Static: true, Public: true }, "GetCustomAttribute",
        new JSIL.MethodSignature($jsilcore.TypeRef("System.Attribute"), [$jsilcore.TypeRef("System.Reflection.MemberInfo"), $jsilcore.TypeRef("System.Type"), $.Boolean], []),
        function GetCustomAttribute(element, attributeType, inherit) {
            var attributes = element.GetCustomAttributes(attributeType, inherit);
            if (attributes.length === 0) {
                return null;
            } else {
                return attributes[0];
            }
        }
      );
  }
);