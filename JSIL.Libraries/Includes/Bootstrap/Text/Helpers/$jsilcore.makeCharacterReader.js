//? include("../Utils/$jsilcore.charCodeAt.js");

$jsilcore.makeCharacterReader = function (str) {
  var position = 0, length = str.length;
  var cca = $jsilcore.charCodeAt;

  var result = {
    read: function () {
      if (position >= length)
        return false;

      var nextChar = cca(str, position);
      position += 1;
      return nextChar;
    }
  };

  Object.defineProperty(result, "eof", {
    get: function () {
      return (position >= length);
    },
    configurable: true,
    enumerable: true
  });

  return result;
};