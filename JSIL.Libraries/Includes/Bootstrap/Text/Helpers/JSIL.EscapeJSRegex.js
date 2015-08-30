JSIL.EscapeJSRegex = function (regexText) {
  return regexText.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&");
};