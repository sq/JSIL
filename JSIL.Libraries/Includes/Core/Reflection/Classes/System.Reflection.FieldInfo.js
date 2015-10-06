JSIL.ImplementExternals(
  "System.Reflection.FieldInfo", function ($) {
    $.Method({ Static: false, Public: true }, "get_FieldType",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [])),
      function get_FieldType() {
        var result = this._cachedFieldType;

        if (typeof (result) === "undefined") {
          result = this._cachedFieldType = JSIL.ResolveTypeReference(
            this._data.fieldType, this._typeObject.__Context__
          )[1];
        }

        return result;
      }
    );

    $.Method({ Static: false, Public: true }, "get_IsInitOnly",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function get_IsInitOnly() {
        return this._descriptor.IsReadOnly;
      }
    );

    $.Method({ Static: false, Public: true, Virtual: true }, "GetRawConstantValue",
      new JSIL.MethodSignature($.Object, [], []),
      function GetRawConstantValue() {
        return this._data.constant;
      }
    );

    $.Method({ Static: false, Public: true, Virtual: true }, "GetValue",
      (new JSIL.MethodSignature($.Object, [$.Object], [])),
      function GetValue(obj) {
        if (this.IsStatic) {
          return this.DeclaringType.__PublicInterface__[this._descriptor.Name];
        }

        if (obj === null) {
          throw new System.Exception("Non-static field requires a target.");
        }

        if (!this.DeclaringType.IsAssignableFrom(obj.__ThisType__)) {
          throw new System.Exception("Field is not defined on the target object.");
        }

        return obj[this._descriptor.Name];
      }
    );

    var equalsImpl = function (lhs, rhs) {
      if (lhs === rhs)
        return true;

      return JSIL.ObjectEquals(lhs, rhs);
    };

    $.Method({ Static: true, Public: true }, "op_Equality",
      (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.FieldInfo"), $jsilcore.TypeRef("System.Reflection.FieldInfo")], [])),
      function op_Equality(left, right) {
        return equalsImpl(left, right);
      }
    );

    $.Method({ Static: true, Public: true }, "op_Inequality",
      (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Reflection.FieldInfo"), $jsilcore.TypeRef("System.Reflection.FieldInfo")], [])),
      function op_Inequality(left, right) {
        return !equalsImpl(left, right);
      }
    );

    $.Method({ Static: false, Public: true }, "get_IsLiteral",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function get_IsLiteral() {
        return false;
      }
    );

    $.Method({ Static: false, Public: true, Virtual: true }, "SetValue",
      (new JSIL.MethodSignature($.Object, [$.Object, $.Object], [])),
      function SetValue(obj, value) {
        var fieldType = this.get_FieldType();
        if (!fieldType.$Is(value))
          throw new System.ArgumentException("value");

        if (this.IsStatic) {
          this.DeclaringType.__PublicInterface__[this._descriptor.Name] = value;
          return;
        }

        if (obj === null) {
          throw new System.Exception("Non-static field requires a target.");
        }

        if (!this.DeclaringType.IsAssignableFrom(obj.__ThisType__)) {
          throw new System.Exception("Field is not defined on the target object.");
        }

        if (fieldType.IsValueType) {
          value = fieldType.__PublicInterface__.$Cast(value);
        }

        obj[this._descriptor.Name] = value;
      }
    );
  }
);

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.FieldInfo", true, [], function ($) {
  $.Property({ Public: true, Static: false }, "FieldType");
  $.Property({ Public: true, Static: false, Virtual: true }, "MemberType");
  $.Method({ Public: true, Static: false, Virtual: true }, "get_MemberType",
    JSIL.MethodSignature.Return($jsilcore.TypeRef("System.Reflection.MemberTypes")),
    function get_MemberType() {
      return $jsilcore.System.Reflection.MemberTypes.Field;
    }
  );
});