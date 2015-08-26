JSIL.JoinEnumerable = function (separator, values) {
  return JSIL.JoinStrings(separator, JSIL.EnumerableToArray(values));
};