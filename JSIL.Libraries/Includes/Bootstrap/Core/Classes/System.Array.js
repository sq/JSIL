//? include("../Utils/JSIL.$WrapIComparer.js");
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
    if (length < 2) {
      return;
    }

    if ((index !== 0) || (length !== array.length)) {
      var sortedArrayPart = Array.prototype.slice.call(array, index, index + length).sort(comparison);
      for (var i = 0; i < length; i++)
        array[i + index] = sortedArrayPart[i];
    } else {
      Array.prototype.sort.call(array, comparison);
    }
  };

  var reverseImpl = function (array, index, length) {
    if (length < 2) {
      return;
    }

    if ((index !== 0) || (length !== array.length)) {
      var reversedArrayPart = Array.prototype.slice.call(array, index, index + length).reverse();
      for (var i = 0; i < length; i++)
        array[i + index] = reversedArrayPart[i];
    } else {
      Array.prototype.reverse.call(array);
    }
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
  );

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
  );

  $.Method({ Static: true, Public: true }, "Reverse",
    new JSIL.MethodSignature(null, [
      $jsilcore.TypeRef("System.Array")
    ]),
    function Reverse(array) {
      reverseImpl(array, 0, array.length);
    }
  );

  $.Method({ Static: true, Public: true }, "Reverse",
    new JSIL.MethodSignature(null, [
      $jsilcore.TypeRef("System.Array"), $.Int32, $.Int32
    ]),
    function Reverse(array, index, length) {
      reverseImpl(array, index, length);
    }
  );

  $.Method({ Static: true, Public: true }, "Empty",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", ["!!0"]), [], ["T"]),
    function Empty(T) {
      return $jsilcore.System.Array_EmptyArray$b1.Of(T).Value;
    }
  );
});


JSIL.MakeStaticClass("System.Array+EmptyArray`1", false, ["T"], function($) {
  $.Field({ Static: true, Public: true, ReadOnly: true }, "Value", $jsilcore.TypeRef("System.Array", [$.GenericParameter("T")]));

  $.Method({ Static: true, Public: false }, ".cctor",
    JSIL.MethodSignature.Void,
    function EmptyArray$b1__cctor() {
      this.Value = JSIL.Array.New(this.T, 0);
    }
  );
});
