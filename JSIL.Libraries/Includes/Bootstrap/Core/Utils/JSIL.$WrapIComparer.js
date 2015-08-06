/*? if (!('JSIL_$WrapIComparer' in __out)) { __out.JSIL_$WrapIComparer = true; */
JSIL.$WrapIComparer = function (T, comparer) {
  var compare;
  if (T !== null) {
    var tComparer = System.Collections.Generic.IComparer$b1.Of(T);
    compare = tComparer.Compare;
  } else {
    compare = System.Collections.IComparer.Compare;
  }

  return function (lhs, rhs) {
    return compare.Call(comparer, null, lhs, rhs);
  };
};
/*? }*/