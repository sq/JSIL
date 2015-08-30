/*? if (!('$jsilcore_fromCharCode' in __out)) { __out.$jsilcore_fromCharCode = true; */
$jsilcore.fromCharCode = function fixedFromCharCode(codePt) {
  // https://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/String/fromCharCode
  if (codePt > 0xFFFF) {
    codePt -= 0x10000;
    return String.fromCharCode(0xD800 + (codePt >> 10), 0xDC00 + (codePt & 0x3FF));
  } else {
    return String.fromCharCode(codePt);
  }
};
/*? }*/