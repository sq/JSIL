JSIL.ImplementExternals(
  "System.Reflection.MethodBase", function ($) {
    $.RawMethod(false, "InitResolvedSignature",
      function InitResolvedSignature() {
        if (this.resolvedSignature === undefined) {
          this._data.resolvedSignature = this._data.signature.Resolve($jsilcore.$MemberInfoGetName(this));
          if (this._data.signature.genericArgumentValues !== undefined) {
            this._data.resolvedSignature = this._data.resolvedSignature.ResolvePositionalGenericParameters(this._data.signature.genericArgumentValues)
          }
        }
      }
    );

    $.Method({ Static: false, Public: false }, "GetParameterTypes",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Type")]), [], [])),
      function GetParameterTypes() {
        return $jsilcore.$MethodGetParameterTypes(this);
      }
    );

    $.Method({ Static: false, Public: true }, "toString",
      new JSIL.MethodSignature($.String, [], []),
      function () {
        // FIXME: Types are encoded as long names, not short names, which is incompatible with .NET
        // i.e. 'System.Int32 Foo()' instead of 'Int32 Foo()'
        return this._data.signature.toString(this.Name);
      }
    );

    $.Method({ Public: true, Static: false }, "get_IsConstructor",
      new JSIL.MethodSignature($.Boolean, []),
      function get_IsConstructor() {
        return $jsilcore.System.Reflection.ConstructorInfo.$Is(this) && !this.get_IsStatic();
      }
    );
  }
);

JSIL.MakeClass("System.Reflection.MemberInfo", "System.Reflection.MethodBase", true, [], function ($) {
  $.Property({ Public: true, Static: false, Virtual: true }, "IsGenericMethod");
  $.Property({ Public: true, Static: false, Virtual: true }, "IsGenericMethodDefinition");
  $.Property({ Public: true, Static: false, Virtual: true }, "ContainsGenericParameters");
});