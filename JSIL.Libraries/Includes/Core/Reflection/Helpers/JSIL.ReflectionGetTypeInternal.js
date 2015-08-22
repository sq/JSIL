JSIL.ReflectionGetTypeInternal = function (thisAssembly, name, throwOnFail, ignoreCase) {
  var parsed = JSIL.ParseTypeName(name);

  var result = JSIL.GetTypeInternal(parsed, thisAssembly, false);

  // HACK: Emulate fallback to global namespace search.
  if (!result) {
    result = JSIL.GetTypeInternal(parsed, JSIL.GlobalNamespace, false);
  }

  if (!result) {
    if (throwOnFail)
      throw new System.TypeLoadException("The type '" + name + "' could not be found in the assembly '" + thisAssembly.toString() + "' or in the global namespace.");
    else
      return null;
  }

  return result;
};