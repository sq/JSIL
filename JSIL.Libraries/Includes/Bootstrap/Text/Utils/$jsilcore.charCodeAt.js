/*? if (!('$jsilcore_charCodeAt' in __out)) { __out.$jsilcore_charCodeAt = true; */
$jsilcore.charCodeAt = function fixedCharCodeAt(str, idx) {
  // https://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/String/charCodeAt

  idx = idx || 0;
  var code = str.charCodeAt(idx);
  var hi, low;

  if (0xD800 <= code && code <= 0xDBFF) {
    // High surrogate (could change last hex to 0xDB7F to treat high private surrogates as single characters)  
    hi = code;
    low = str.charCodeAt(idx + 1);
    if (isNaN(low))
      JSIL.RuntimeError("High surrogate not followed by low surrogate");

    return ((hi - 0xD800) * 0x400) + (low - 0xDC00) + 0x10000;
  }

  if (0xDC00 <= code && code <= 0xDFFF) {
    // Low surrogate  
    // We return false to allow loops to skip this iteration since should have already handled high surrogate above in the previous iteration  
    return false;
  }

  return code;
};
/*? }*/