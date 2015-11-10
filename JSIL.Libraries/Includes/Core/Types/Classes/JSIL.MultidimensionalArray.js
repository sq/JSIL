JSIL.MakeStaticClass("JSIL.MultidimensionalArray", true, [], function ($) {
  $.RawMethod(true, "New",
    function (type, dimensions, initializer) {
      var arrayType = new $jsilcore.System.Array.Of(type, JSIL.ArrayDimensionParameter(dimensions.length / 2));
      var ctorArgs = [];
      for (var i = 0; i < dimensions.length; i++) {
        ctorArgs.push($jsilcore.TypeRef("System.Int32"));
      }
      var ctorSignature = new JSIL.ConstructorSignature(arrayType, ctorArgs);
      var createdArray = ctorSignature.Construct.apply(ctorSignature, dimensions);
      if (JSIL.IsArray(initializer)) {
        JSIL.Array.ShallowCopy(createdArray.Items, initializer);
      }

      return createdArray;
    }
  );
});