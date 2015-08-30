var $bytestream = function ($) {
  $.Method({ Static: false, Public: true }, "Read",
    (new JSIL.MethodSignature($.Int32, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32
    ], [])),
    function Read(buffer, offset, count) {
      var startPos = this._pos | 0;
      var endPos = (this._pos + count) | 0;

      if (endPos >= this._length) {
        endPos = this._length - 1;
        count = endPos - startPos + 1;
      }

      if ((startPos < 0) || (startPos >= this._length))
        return 0;

      offset = (offset | 0);
      count = (count | 0);

      var sourceBuffer = this._buffer;
      for (var i = 0; i < count; i = (i + 1) | 0) {
        var destIndex = (offset + i) | 0;
        var sourceIndex = (startPos + i) | 0;

        /*
        if ((destIndex < 0) || (destIndex >= buffer.length))
          throw new Error("Destination index out of range: " + destIndex);
        if ((sourceIndex < 0) || (sourceIndex >= sourceBuffer.length))
          throw new Error("Source index out of range: " + sourceIndex);
        */

        buffer[destIndex] = sourceBuffer[sourceIndex];
      }

      this._pos += count;

      return count;
    }
  );

  $.Method({ Static: false, Public: true }, "$PeekByte",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function PeekByte() {
      if (this._pos >= this._length)
        return -1;

      return this._buffer[this._pos];
    }
  );

  $.Method({ Static: false, Public: true }, "get_Position",
    (new JSIL.MethodSignature($.Int64, [], [])),
    function get_Position() {
      return $jsilcore.System.Int64.FromInt32(this._pos);
    }
  );

  $.Method({ Static: false, Public: true }, "set_Position",
    (new JSIL.MethodSignature(null, [$.Int64], [])),
    function set_Position(value) {
      this._pos = value.ToInt32();
    }
  );

  $.Method({ Static: false, Public: true }, "get_Length",
    (new JSIL.MethodSignature($.Int64, [], [])),
    function get_Length() {
      return $jsilcore.System.Int64.FromInt32(this._length);
    }
  );

  $.Method({ Static: false, Public: true }, "get_CanSeek",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function get_CanSeek() {
      return true;
    }
  );

  $.Method({ Static: false, Public: true }, "get_CanRead",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function get_CanRead() {
      return true;
    }
  );

  $.Method({ Static: false, Public: true }, "Seek",
    (new JSIL.MethodSignature($.Int64, [$.Int64, $jsilcore.TypeRef("System.IO.SeekOrigin")], [])),
    function Seek(offset, origin) {
      switch (origin) {
        case System.IO.SeekOrigin.Begin:
          this._pos = offset.ToInt32();
          break;
        case System.IO.SeekOrigin.Current:
          this._pos += offset.ToInt32();
          break;
        case System.IO.SeekOrigin.End:
          this._pos = this._buffer.length + offset.ToInt32();
          break;
      }

      return $jsilcore.System.Int64.FromInt32(this._pos);
    }
  );

  $.Method({ Static: false, Public: true }, "Write",
    (new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32,
          $.Int32
    ], [])),
    function Write(buffer, offset, count) {
      var newPosition = this._pos + count;

      if (newPosition > this._length)
        this._length = newPosition;

      for (var i = 0; i < count; i++)
        this._buffer[this._pos + i] = buffer[offset + i];

      this._pos = newPosition;

      this._modified = true;
    }
  );

  $.Method({ Static: false, Public: true }, "WriteByte",
    (new JSIL.MethodSignature(null, [$.Byte], [])),
    function WriteByte(value) {
      if (this._pos >= this._length)
        this._length += 1;

      this._buffer[this._pos] = value;
      this._pos += 1;

      this._modified = true;
    }
  );

};