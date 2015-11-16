$jsilcore.$tArrayEnumerator = null;

JSIL.MakeArrayEnumerator = function (array, elementType) {
  var tArrayEnumerator;

  if (!elementType) {
    if ($jsilcore.$tArrayEnumerator === null)
      $jsilcore.$tArrayEnumerator = JSIL.ArrayEnumerator.Of(System.Object);

    tArrayEnumerator = $jsilcore.$tArrayEnumerator;
  } else {
    tArrayEnumerator = JSIL.ArrayEnumerator.Of(elementType);
  }

  return new tArrayEnumerator(array, -1);
};

JSIL.GetEnumerator = function (enumerable, elementType, fallbackMethodInvoke) {
  if ((typeof (enumerable) === "undefined") || (enumerable === null))
    JSIL.RuntimeError("Enumerable is null or undefined");

  var tIEnumerable = $jsilcore.System.Collections.IEnumerable;
  var tIEnumerable$b1 = null;

  if (!elementType)
    elementType = $jsilcore.System.Object.__Type__;
  else
    tIEnumerable$b1 = $jsilcore.System.Collections.Generic.IEnumerable$b1.Of(elementType);

  var result = null;
  if (JSIL.IsArray(enumerable))
    result = JSIL.MakeArrayEnumerator(enumerable, elementType);
  else if (enumerable.__IsArray__)
    result = JSIL.MakeArrayEnumerator(enumerable.Items, elementType);
  else if (typeof (enumerable) === "string")
    result = JSIL.MakeArrayEnumerator(enumerable, elementType);
  else if ((fallbackMethodInvoke !== true) && tIEnumerable$b1 && tIEnumerable$b1.$Is(enumerable))
    result = tIEnumerable$b1.GetEnumerator.Call(enumerable);
  else if ((fallbackMethodInvoke !== true) && tIEnumerable.$Is(enumerable))
    result = tIEnumerable.GetEnumerator.Call(enumerable);
  else if ((fallbackMethodInvoke !== true) && (typeof (enumerable.GetEnumerator) === "function"))
    // HACK: This is gross.
    result = enumerable.GetEnumerator();
  else
    JSIL.RuntimeError("Value is not enumerable");

  if (!result)
    JSIL.RuntimeError("Value's GetEnumerator method did not return an enumerable.");

  return result;
};