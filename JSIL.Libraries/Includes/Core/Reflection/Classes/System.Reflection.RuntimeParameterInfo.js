JSIL.MakeClass("System.Reflection.ParameterInfo", "System.Reflection.RuntimeParameterInfo", false, [], function ($) {
  $.RawMethod(false, "$fromArgumentTypeAndPosition", function (argumentType, position) {
    this.argumentType = argumentType;
    this.position = position;
    this._name = null;
    this.__Attributes__ = [];
  });

  $.RawMethod(false, "$populateWithParameterInfo", function (parameterInfo) {
    this._name = parameterInfo.name || null;

    if (parameterInfo.attributes) {
      var mb = new JSIL.MemberBuilder(null);
      parameterInfo.attributes(mb);
      this.__Attributes__ = mb.attributes;
    }
  });
});