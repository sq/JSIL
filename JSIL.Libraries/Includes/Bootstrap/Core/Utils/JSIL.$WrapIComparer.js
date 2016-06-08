/*? if (!('JSIL_$WrapIComparer' in __out)) { __out.JSIL_$WrapIComparer = true; */
JSIL.$WrapIComparer = function (T, comparer) {
  var compare;
  if (T !== null) {
    var tComparer = System.Collections.Generic.IComparer$b1.Of(T);
    compare = tComparer.$Methods.Compare.InterfaceMethod;
  } else {
    compare = System.Collections.IComparer.$Methods.Compare.InterfaceMethod;
  }

  return function (lhs, rhs) {
    return compare.Call(comparer, null, lhs, rhs);
  };
};
/*? }*/