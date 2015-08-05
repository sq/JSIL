JSIL.EnumerableToArray = function (enumerable, elementType) {
  var e = JSIL.GetEnumerator(enumerable, elementType);
  var result = [];

  var moveNext = $jsilcore.System.Collections.IEnumerator.MoveNext;
  var getCurrent = $jsilcore.System.Collections.IEnumerator.get_Current;

  try {
    while (moveNext.Call(e))
      result.push(getCurrent.Call(e));
  } finally {
    JSIL.Dispose(e);
  }

  return result;
};