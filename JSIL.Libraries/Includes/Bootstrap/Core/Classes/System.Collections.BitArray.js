function BitArray(length) {
  this._bytes = new Uint8Array(Math.ceil(length / 8));
}

BitArray.prototype.get = function (i) {
  var _byte = Math.floor(i / 8);
  var mask = 1 << (((i / 8) - _byte) * 8);

  return (this._bytes[_byte] & mask) != 0;
}

BitArray.prototype.set = function (i, bool) {
  var _byte = Math.floor(i / 8);
  var mask = 1 << (((i / 8) - _byte) * 8);

  if (bool) {
    this._bytes[_byte] |= mask;
  } else {
    this._bytes[_byte] &= ~mask;
  }
}

JSIL.ImplementExternals("System.Collections.BitArray", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$.Int32], []),
    function _ctor(length) {
      this._length = length;
      this._bitarray = new BitArray(length);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Length",
    new JSIL.MethodSignature($.Int32, [], []),
    function get_Length() {
      return this._length;
    }
  );

  $.Method({ Static: false, Public: true }, "Get",
    new JSIL.MethodSignature($.Boolean, [$.Int32], []),
    function Get(index) {
      return this._bitarray.get(index);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Item",
    new JSIL.MethodSignature($.Boolean, [$.Int32], []),
    function get_Item(index) {
      return this._bitarray.get(index);
    }
  );

  $.Method({ Static: false, Public: true }, "Set",
    new JSIL.MethodSignature(null, [$.Int32, $.Boolean], []),
    function Set(index, bool) {
      return this._bitarray.set(index, bool);
    }
  );

  $.Method({ Static: false, Public: true }, "set_Item",
    new JSIL.MethodSignature(null, [$.Int32, $.Boolean], []),
    function set_Item(index, bool) {
      return this._bitarray.set(index, bool);
    }
  );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Collections.BitArray", true, [], function ($) {
  $.Property({ Public: true, Static: false }, "Method");
});
//? }