JSIL.MakeClass("System.Reflection.MethodInfo", "System.Reflection.RuntimeMethodInfo", false, [], function ($) {
  $.Method({ Static: false, Public: false }, "GetParentDefinition",
  (new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.RuntimeMethodInfo"), [], [])),
  function get_ReturnType() {
    if (!this._descriptor.Virtual || this._descriptor.Static) {
      return null;
    }

    var currentType = this.get_DeclaringType();
    while (true) {
      currentType = currentType.__BaseType__;
      if (!(currentType && currentType.GetType)) {
        return null;
      }
      var foundMethod = JSIL.GetMethodInfo(currentType.__PublicInterface__, this.get_Name(), this._data.signature, false, null);
      if (foundMethod != null) {
        return foundMethod;
      }
    }
  }
);
});