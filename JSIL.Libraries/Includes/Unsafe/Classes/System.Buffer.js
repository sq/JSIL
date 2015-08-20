JSIL.ImplementExternals("System.Buffer", function ($interfaceBuilder) {
    var $ = $interfaceBuilder;

    $.Method({ Static: true, Public: true }, "BlockCopy",
      new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("System.Array"), $.Int32,
          $jsilcore.TypeRef("System.Array"), $.Int32,
          $.Int32
      ], []),
      function BlockCopy(src, srcOffset, dst, dstOffset, count) {
          var srcBuffer = JSIL.GetArrayBuffer(src);
          srcOffset += JSIL.GetArrayByteOffset(src);

          var dstBuffer = JSIL.GetArrayBuffer(dst);
          dstOffset += JSIL.GetArrayByteOffset(dst);

          var srcView = new Uint8Array(srcBuffer, srcOffset, count);
          var dstView = new Uint8Array(dstBuffer, dstOffset, count);

          dstView.set(srcView);
      }
    );

    $.Method({ Static: true, Public: true }, "ByteLength",
      new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Array")], []),
      function ByteLength(array) {
          return JSIL.GetArrayByteLength(array);
      }
    );

    $.Method({ Static: true, Public: true }, "GetByte",
      new JSIL.MethodSignature($.Byte, [$jsilcore.TypeRef("System.Array"), $.Int32], []),
      function GetByte(array, index) {
          var buffer = JSIL.GetArrayBuffer(array);
          index += JSIL.GetArrayByteOffset(array);

          var view = new Uint8Array(buffer, index, 1);
          return view[0];
      }
    );

});