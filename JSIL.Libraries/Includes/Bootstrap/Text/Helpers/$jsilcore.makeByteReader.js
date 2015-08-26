$jsilcore.makeByteReader = function (bytes, index, count) {
  var position = (typeof (index) === "number") ? index : 0;
  var endpoint;

  if (typeof (count) === "number")
    endpoint = (position + count);
  else
    endpoint = (bytes.length - position);

  var result = {
    read: function () {
      if (position >= endpoint)
        return false;

      var nextByte = bytes[position];
      position += 1;
      return nextByte;
    }
  };

  Object.defineProperty(result, "eof", {
    get: function () {
      return (position >= endpoint);
    },
    configurable: true,
    enumerable: true
  });

  return result;
};