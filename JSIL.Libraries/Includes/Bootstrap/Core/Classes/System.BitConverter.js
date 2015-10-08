$jsilcore.SerializationScratchBuffers = null;

$jsilcore.GetSerializationScratchBuffers = function () {
  if (!$jsilcore.SerializationScratchBuffers) {
    var uint8 = new Uint8Array(32);
    var buffer = uint8.buffer;

    $jsilcore.SerializationScratchBuffers = {
      uint8: uint8,
      uint16: new Uint16Array(buffer),
      uint32: new Uint32Array(buffer),
      int8: new Int8Array(buffer),
      int16: new Int16Array(buffer),
      int32: new Int32Array(buffer),
      float32: new Float32Array(buffer),
      float64: new Float64Array(buffer),
      slice: function (byteCount) {
        byteCount = byteCount | 0;

        var result = new Uint8Array(byteCount);
        for (var i = 0; i < byteCount; i++)
          result[i] = uint8[i];

        return result;
      },
      fillFrom: function (bytes, offset, count) {
        offset = offset | 0;
        count = count | 0;

        if (!bytes)
          JSIL.RuntimeError("bytes cannot be null");

        for (var i = 0; i < count; i++)
          uint8[i] = bytes[offset + i];
      }
    };
  }

  return $jsilcore.SerializationScratchBuffers;
};


$jsilcore.BytesFromBoolean = function (value) {
  return [value ? 1 : 0];
};


$jsilcore.BytesFromSingle = function (value) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.float32[0] = value;
  return bufs.slice(4);
};

$jsilcore.BytesFromDouble = function (value) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.float64[0] = value;
  return bufs.slice(8);
};

$jsilcore.BytesFromInt16 = function (value) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.int16[0] = value;
  return bufs.slice(2);
};

$jsilcore.BytesFromInt32 = function (value) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.int32[0] = value;
  return bufs.slice(4);
};

$jsilcore.BytesFromInt64 = function (value) {
  return [
    (value.a >> 0) & 0xFF,
    (value.a >> 8) & 0xFF,
    (value.a >> 16) & 0xFF,
    (value.b >> 0) & 0xFF,
    (value.b >> 8) & 0xFF,
    (value.b >> 16) & 0xFF,
    (value.c >> 0) & 0xFF,
    (value.c >> 8) & 0xFF
  ];
};

// FIXME: Are these unsigned versions right?

$jsilcore.BytesFromUInt16 = function (value) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.uint16[0] = value;
  return bufs.slice(2);
};

$jsilcore.BytesFromUInt32 = function (value) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.uint32[0] = value;
  return bufs.slice(4);
};

$jsilcore.BytesFromUInt64 = function (value) {
  return [
    (value.a >>> 0) & 0xFF,
    (value.a >>> 8) & 0xFF,
    (value.a >>> 16) & 0xFF,
    (value.b >>> 0) & 0xFF,
    (value.b >>> 8) & 0xFF,
    (value.b >>> 16) & 0xFF,
    (value.c >>> 0) & 0xFF,
    (value.c >>> 8) & 0xFF
  ];
};


$jsilcore.BytesToBoolean = function (bytes, offset) {
  return bytes[offset] !== 0;
};

$jsilcore.BytesToInt16 = function (bytes, offset) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.fillFrom(bytes, offset, 2);
  return bufs.int16[0];
};

$jsilcore.BytesToInt32 = function (bytes, offset) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.fillFrom(bytes, offset, 4);
  return bufs.int32[0];
};

$jsilcore.BytesToInt64 = function (bytes, offset) {
  return $jsilcore.System.Int64.FromBytes(bytes, offset);
};

$jsilcore.BytesToUInt16 = function (bytes, offset) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.fillFrom(bytes, offset, 2);
  return bufs.uint16[0];
};

$jsilcore.BytesToUInt32 = function (bytes, offset) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.fillFrom(bytes, offset, 4);
  return bufs.uint32[0];
};

$jsilcore.BytesToUInt64 = function (bytes, offset) {
  return $jsilcore.System.UInt64.FromBytes(bytes, offset);
};

$jsilcore.BytesToSingle = function (bytes, offset) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.fillFrom(bytes, offset, 4);
  return bufs.float32[0];
};

$jsilcore.BytesToDouble = function (bytes, offset) {
  var bufs = $jsilcore.GetSerializationScratchBuffers();
  bufs.fillFrom(bytes, offset, 8);
  return bufs.float64[0];
};

JSIL.ImplementExternals("System.BitConverter", function ($) {
  $.Method({ Static: true, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Boolean], [])),
    $jsilcore.BytesFromBoolean
  );

  $.Method({ Static: true, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Int16], [])),
    $jsilcore.BytesFromInt16
  );

  $.Method({ Static: true, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Int32], [])),
    $jsilcore.BytesFromInt32
  );

  $.Method({ Static: true, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Int64], [])),
    $jsilcore.BytesFromInt64
  );

  $.Method({ Static: true, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.UInt16], [])),
    $jsilcore.BytesFromUInt16
  );

  $.Method({ Static: true, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.UInt32], [])),
    $jsilcore.BytesFromUInt32
  );

  $.Method({ Static: true, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.UInt64], [])),
    $jsilcore.BytesFromUInt64
  );

  $.Method({ Static: true, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Single], [])),
    $jsilcore.BytesFromSingle
  );

  $.Method({ Static: true, Public: true }, "GetBytes",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.Double], [])),
    $jsilcore.BytesFromDouble
  );

  /*

  $.Method({Static:true , Public:false}, "GetHexValue", 
    (new JSIL.MethodSignature($.Char, [$.Int32], [])), 
    function GetHexValue (i) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "Int64BitsToDouble", 
    (new JSIL.MethodSignature($.Double, [$.Int64], [])), 
    function Int64BitsToDouble (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "ToChar", 
    (new JSIL.MethodSignature($.Char, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    function ToChar (value, startIndex) {
      throw new Error('Not implemented');
    }
  );

  */

  $.Method({ Static: true, Public: true }, "ToBoolean",
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])),
    $jsilcore.BytesToBoolean
  );

  $.Method({ Static: true, Public: true }, "ToInt16",
    (new JSIL.MethodSignature($.Int16, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])),
    $jsilcore.BytesToInt16
  );

  $.Method({ Static: true, Public: true }, "ToInt32",
    (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])),
    $jsilcore.BytesToInt32
  );

  $.Method({ Static: true, Public: true }, "ToInt64",
    (new JSIL.MethodSignature($.Int64, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])),
    $jsilcore.BytesToInt64
  );

  /*

  $.Method({Static:true , Public:true }, "ToString", 
    (new JSIL.MethodSignature($.String, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, 
          $.Int32
        ], [])), 
    function ToString (value, startIndex, length) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "ToString", 
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte])], [])), 
    function ToString (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "ToString", 
    (new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])), 
    function ToString (value, startIndex) {
      throw new Error('Not implemented');
    }
  );

  */

  $.Method({ Static: true, Public: true }, "ToUInt16",
    (new JSIL.MethodSignature($.UInt16, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])),
    $jsilcore.BytesToUInt16
  );

  $.Method({ Static: true, Public: true }, "ToUInt32",
    (new JSIL.MethodSignature($.UInt32, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])),
    $jsilcore.BytesToUInt32
  );

  $.Method({ Static: true, Public: true }, "ToUInt64",
    (new JSIL.MethodSignature($.UInt64, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])),
    $jsilcore.BytesToUInt64
  );

  $.Method({ Static: true, Public: true }, "ToSingle",
    (new JSIL.MethodSignature($.Single, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])),
    $jsilcore.BytesToSingle
  );

  $.Method({ Static: true, Public: true }, "ToDouble",
    (new JSIL.MethodSignature($.Double, [$jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32], [])),
    $jsilcore.BytesToDouble
  );

  $.Method({ Static: true, Public: true }, "DoubleToInt64Bits",
    (new JSIL.MethodSignature($.Int64, [$.Double], [])),
    function DoubleToInt64Bits(double) {
      return $jsilcore.BytesToInt64($jsilcore.BytesFromDouble(double), 0);
    }
  );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStaticClass("System.BitConverter", true, [], function ($) {
});
//? }