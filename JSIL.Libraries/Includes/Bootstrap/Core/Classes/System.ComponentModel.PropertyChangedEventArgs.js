JSIL.ImplementExternals("System.ComponentModel.PropertyChangedEventArgs", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
	new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.String")], []),
	function (propertyName) {
	  this.propertyName = propertyName;
	}
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "get_PropertyName",
	new JSIL.MethodSignature($.String, [], []),
	function () {
	  return this.propertyName;
	}
  );
});