// #include_once "Bootstrap/Core/Utils/JSIL.$WrapIComparer.js"

JSIL.ImplementExternals("System.Array", function ($) {
  var copyImpl = function (sourceArray, sourceIndex, destinationArray, destinationIndex, length) {
    if (length < 0)
      throw new System.ArgumentException("length");
    if (sourceIndex < 0)
      throw new System.ArgumentException("sourceIndex");
    if (destinationIndex < 0)
      throw new System.ArgumentException("destinationIndex");

    var maxLength = Math.min(
      (sourceArray.length - sourceIndex) | 0,
      (destinationArray.length - destinationIndex) | 0
    );
    if (length > maxLength)
      throw new System.ArgumentException("length");

    length = length | 0;

    if (
      (sourceArray === destinationArray) &&
      (destinationIndex === sourceIndex)
    )
      return;

    if (
      (sourceArray === destinationArray) &&
      (destinationIndex < (sourceIndex + length)) &&
      (destinationIndex > sourceIndex)
    ) {
      for (var i = length - 1; i >= 0; i = (i - 1) | 0) {
        destinationArray[i + destinationIndex] = sourceArray[i + sourceIndex];
      }
    } else {
      for (var i = 0; i < length; i = (i + 1) | 0) {
        destinationArray[i + destinationIndex] = sourceArray[i + sourceIndex];
      }
    }
  };

  var sortImpl = function (array, index, length, comparison) {
    if ((index !== 0) || (length !== array.length))
      JSIL.RuntimeError("Sorting a subset of an array is not implemented");

    Array.prototype.sort.call(array, comparison);
  };

  $.Method({ Static: true, Public: true }, "Copy",
    new JSIL.MethodSignature(null, [
        $jsilcore.TypeRef("System.Array"), $jsilcore.TypeRef("System.Array"),
        $.Int32
    ], []),
    function Copy(sourceArray, destinationArray, length) {
      copyImpl(sourceArray, 0, destinationArray, 0, length);
    }
  );

  $.Method({ Static: true, Public: true }, "Copy",
    new JSIL.MethodSignature(null, [
        $jsilcore.TypeRef("System.Array"), $.Int32,
        $jsilcore.TypeRef("System.Array"), $.Int32,
        $.Int32
    ], []),
    function Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length) {
      copyImpl(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
    }
  );

  $.Method({ Static: true, Public: true }, "Sort",
    JSIL.MethodSignature.Action($jsilcore.TypeRef("System.Array")),
    function Sort(array) {
      sortImpl(array, 0, array.length, JSIL.CompareValues);
    }
  )

  $.Method({ Static: true, Public: true }, "Sort",
    new JSIL.MethodSignature(null, [
        $jsilcore.TypeRef("System.Array"), $.Int32,
        $.Int32
    ]),
    function Sort(array, index, length) {
      sortImpl(array, index, length, JSIL.CompareValues);
    }
  )

  $.Method({ Static: true, Public: true }, "Sort",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array"), $jsilcore.TypeRef("System.Collections.IComparer")]),
    function Sort(array, comparer) {
      sortImpl(array, 0, array.length, JSIL.$WrapIComparer(null, comparer));
    }
  )

  $.Method({ Static: true, Public: true }, "Sort",
    new JSIL.MethodSignature(null, [
        $jsilcore.TypeRef("System.Array"), $.Int32,
        $.Int32, $jsilcore.TypeRef("System.Collections.IComparer")
    ]),
    function Sort(array, index, length, comparer) {
      sortImpl(array, index, length, JSIL.$WrapIComparer(null, comparer));
    }
  )

  $.Method({ Static: true, Public: true }, "Sort",
    new JSIL.MethodSignature(null, [
        $jsilcore.TypeRef("System.Array", ["!!0"]), $.Int32,
        $.Int32
    ], ["T"]),
    function Sort$b1(T, array, index, length) {
      sortImpl(array, index, length, JSIL.CompareValues);
    }
  )

  $.Method({ Static: true, Public: true }, "Sort",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", ["!!0"]), $jsilcore.TypeRef("System.Collections.Generic.IComparer`1", ["!!0"])], ["T"]),
    function Sort$b1(T, array, comparer) {
      sortImpl(array, 0, array.length, JSIL.$WrapIComparer(T, comparer));
    }
  )

  $.Method({ Static: true, Public: true }, "Sort",
    new JSIL.MethodSignature(null, [
        $jsilcore.TypeRef("System.Array", ["!!0"]), $.Int32,
        $.Int32, $jsilcore.TypeRef("System.Collections.Generic.IComparer`1", ["!!0"])
    ], ["T"]),
    function Sort$b1(T, array, index, length, comparer) {
      sortImpl(array, index, length, JSIL.$WrapIComparer(T, comparer));
    }
  );

  $.Method({ Static: true, Public: true }, "Sort",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", ["!!0"]), $jsilcore.TypeRef("System.Comparison`1", ["!!0"])], ["T"]),
    function Sort$b1(T, array, comparison) {
      sortImpl(array, 0, array.length, comparison);
    }
  )
});