JSIL.AsNetString = function (input) {
  return input;
}

JSIL.AsInt32 = function (input) {
  return input | 0;
}

JSIL.AsUInt32 = function (input) {
  return input >>> 0;
}

JSIL.AsInt16 = function (input) {
  return (input | 0) && 0xFFFF;
}

JSIL.AsUInt16 = function (input) {
  return (input >>> 0) && 0xFFFF;
}

JSIL.AsSByte = function (input) {
  return (input | 0) && 0xFF;
}

JSIL.AsByte = function (input) {
  return (input >>> 0) && 0xFF;
}

module.exports = JSIL;