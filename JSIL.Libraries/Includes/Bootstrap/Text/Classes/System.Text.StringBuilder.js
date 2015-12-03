JSIL.ImplementExternals("System.Text.StringBuilder", function ($) {

  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      this._str = "";
      this._capacity = 0;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Int32], [])),
    function _ctor(capacity) {
      this._str = "";
      this._capacity = capacity;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.String], [])),
    function _ctor(value) {
      this._str = value;
      this._capacity = value.length;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.String, $.Int32], [])),
    function _ctor(value, capacity) {
      this._str = value;
      this._capacity = capacity;
    }
  );

  var appendString = function (self, str, startIndex, length, copies) {
    if (arguments.length === 2) {
      startIndex = 0;
      length = str.length;
      copies = 1;
    }

    if ((startIndex !== 0) || (length !== str.length)) {
      for (var i = 0; i < copies; i++) {
        self._str += str.substr(startIndex, length);
      }

    } else {
      for (var i = 0; i < copies; i++) {
        self._str += str;
      }

    }

    self._capacity = Math.max(self._capacity, self._str.length);
  };

  var appendNumber = function (self, num) {
    self._str += String(num);

    self._capacity = Math.max(self._capacity, self._str.length);
  };

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Char, $.Int32], [])),
    function Append(value, repeatCount) {
      appendString(this, value, 0, value.length, repeatCount);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [
          $jsilcore.TypeRef("System.Array", [$.Char]), $.Int32,
          $.Int32
    ], [])),
    function Append(value, startIndex, charCount) {
      for (var i = 0; i < charCount; i++)
        this._str += value[startIndex + i];

      this._capacity = Math.max(this._capacity, this._str.length);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Object], [])),
    function Append(value) {
      var string = value.toString();
      appendString(this, string, 0, string.length, 1);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.String], [])),
    function Append(value) {
      appendString(this, value, 0, value.length, 1);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [
          $.String, $.Int32,
          $.Int32
    ], [])),
    function Append(value, startIndex, count) {
      appendString(this, value, startIndex, count, 1);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Boolean], [])),
    function Append(value) {
      this._str += (value ? "True" : "False");
      this._capacity = Math.max(this._capacity, this._str.length);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.SByte], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Byte], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Char], [])),
    function Append(value) {
      appendString(this, value, 0, value.length, 1);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Int16], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Int32], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Int64], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Single], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.Double], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.UInt16], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.UInt32], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$.UInt64], [])),
    function Append(value) {
      appendNumber(this, value);
    }
  );

  $.Method({ Static: false, Public: true }, "Append",
    (new JSIL.MethodSignature($.Type, [$jsilcore.TypeRef("System.Array", [$.Char])], [])),
    function Append(value) {
      for (var i = 0; i < value.length; i++)
        this._str += value[i];

      this._capacity = Math.max(this._capacity, this._str.length);
    }
  );

  $.Method({ Static: false, Public: true }, "AppendLine",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Text.StringBuilder"), [], [])),
    function AppendLine() {
      appendString(this, "\r\n", 0, 2, 1);
      return this;
    }
  );

  $.Method({ Static: false, Public: true }, "AppendLine",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Text.StringBuilder"), [$.String], [])),
    function AppendLine(value) {
      appendString(this, value, 0, value.length, 1);
      appendString(this, "\r\n", 0, 2, 1);
      return this;
    }
  );

  $.Method({ Static: false, Public: true }, "AppendFormat",
    (new JSIL.MethodSignature($.Type, [$.String, $.Object], [])),
    function AppendFormat(format, arg0) {
      appendString(this, System.String.Format(format, [arg0]));
      return this;
    }
  );

  $.Method({ Static: false, Public: true }, "AppendFormat",
    (new JSIL.MethodSignature($.Type, [
          $.String, $.Object,
          $.Object
    ], [])),
    function AppendFormat(format, arg0, arg1) {
      appendString(this, System.String.Format(format, [arg0, arg1]));
      return this;
    }
  );

  $.Method({ Static: false, Public: true }, "AppendFormat",
    (new JSIL.MethodSignature($.Type, [
          $.String, $.Object,
          $.Object, $.Object
    ], [])),
    function AppendFormat(format, arg0, arg1, arg2) {
      appendString(this, System.String.Format(format, [arg0, arg1, arg2]));
      return this;
    }
  );

  $.Method({ Static: false, Public: true }, "AppendFormat",
    (new JSIL.MethodSignature($.Type, [$.String, $jsilcore.TypeRef("System.Array", [$.Object])], [])),
    function AppendFormat(format, args) {
      appendString(this, System.String.Format(format, args));
      return this;
    }
  );

  $.Method({ Static: false, Public: true }, "Clear",
    (new JSIL.MethodSignature($.Type, [], [])),
    function Clear() {
      this._str = "";
    }
  );

  $.Method({ Static: false, Public: true }, "get_Length",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Length() {
      return this._str.length;
    }
  );

  $.Method({ Static: false, Public: true }, "set_Capacity",
    (new JSIL.MethodSignature(null, [$.Int32], [])),
    function set_Capacity(value) {
      // FIXME: What happens if value is lower than the length of the current contents?
      this._capacity = Math.max(value | 0, this._str.length);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Capacity",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Capacity() {
      return this._capacity;
    }
  );

  $.Method({ Static: false, Public: true }, "Remove",
    (new JSIL.MethodSignature($.Type, [$.Int32, $.Int32], [])),
    function Remove(startIndex, length) {
      this._str = this._str.substr(0, startIndex) + this._str.substring(startIndex + length, length);
      return this;
    }
  );

  var replace = function (self, oldText, newText, startIndex, count) {
    var prefix = self._str.substr(0, startIndex);
    var suffix = self._str.substr(startIndex + count);
    var region = self._str.substr(startIndex, count);
    var result = prefix + region.split(oldText).join(newText) + suffix;
    self._str = result;
    self._capacity = Math.max(self._capacity, self._str.length);
    return self;
  };

  $.Method({ Static: false, Public: true }, "Replace",
    (new JSIL.MethodSignature($.Type, [$.String, $.String], [])),
    function Replace(oldValue, newValue) {
      return replace(this, oldValue, newValue, 0, this._str.length);
    }
  );

  $.Method({ Static: false, Public: true }, "Replace",
    (new JSIL.MethodSignature($.Type, [
          $.String, $.String,
          $.Int32, $.Int32
    ], [])),
    function Replace(oldValue, newValue, startIndex, count) {
      return replace(this, oldValue, newValue, startIndex, count);
    }
  );

  $.Method({ Static: false, Public: true }, "Replace",
    (new JSIL.MethodSignature($.Type, [$.Char, $.Char], [])),
    function Replace(oldChar, newChar) {
      return replace(this, oldChar, newChar, 0, this._str.length);
    }
  );

  $.Method({ Static: false, Public: true }, "Replace",
    (new JSIL.MethodSignature($.Type, [
          $.Char, $.Char,
          $.Int32, $.Int32
    ], [])),
    function Replace(oldChar, newChar, startIndex, count) {
      return replace(this, oldChar, newChar, startIndex, count);
    }
  );

  var insert = function (self, string, startIndex, count) {
    while (startIndex > self._str.length - 1 && self._str.length < self._capacity) {
      self._str += "\0";
    }

    var suffix = self._str.substr(startIndex);
    self._str = self._str.substr(0, startIndex);
    for (var i = 0; i < count; i++) {
      self._str += string;
    }
    self._str += suffix;
    self._capacity = Math.max(self._capacity, self._str.length);
    return self;
  };

  $.Method({ Static: false, Public: true }, "Insert",
    (new JSIL.MethodSignature($.Type, [$.Int32, $.String], [])),
    function Insert(index, value) {
      return insert(this, value, index, 1);
    }
  );

  $.Method({ Static: false, Public: true }, "Insert",
    (new JSIL.MethodSignature($.Type, [$.Int32, $.String, $.Int32], [])),
    function Insert(index, value, count) {
      return insert(this, value, index, count);
    }
  );

  $.Method({ Static: false, Public: true }, "set_Length",
    (new JSIL.MethodSignature(null, [$.Int32], [])),
    function set_Length(value) {
      var delta = value - this._str.length;

      if (delta < 0) {
        this._str = this._str.substr(0, value);
      } else if (delta > 0) {
        var ch = new Array(delta);
        for (var i = 0; i < delta; i++)
          ch[i] = '\0';

        this._str += JSIL.StringFromByteArray(ch);
      }
    }
  );

  $.Method({ Static: false, Public: true }, "get_Chars",
    (new JSIL.MethodSignature($.Char, [$.Int32], [])),
    function get_Chars(i) {
      return this._str[i];
    }
  );

  $.Method({ Static: false, Public: true }, "set_Chars",
    (new JSIL.MethodSignature(null, [$.Int32, $.Char], [])),
    function set_Chars(i, value) {
      while (i > this._str.length - 1) {
        this._str += "\0";
      }
      this._str =
        this._str.substr(0, i) +
        value +
        this._str.substr(i + 1);
    }
  );

  $.Method({ Static: false, Public: true }, "toString",
    (new JSIL.MethodSignature($.String, [], [])),
    function toString() {
      return this._str;
    }
  );
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Text.StringBuilder", true, [], function ($) {
});