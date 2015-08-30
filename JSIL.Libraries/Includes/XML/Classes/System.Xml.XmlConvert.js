JSIL.ImplementExternals("System.Xml.XmlConvert", function ($) {

    $.Method({ Static: true, Public: true }, "ToDouble",
      (new JSIL.MethodSignature($.Double, [$.String], [])),
      function ToDouble(s) {
          return parseFloat(s);
      }
    );

    $.Method({ Static: true, Public: true }, "ToSingle",
      (new JSIL.MethodSignature($.Single, [$.String], [])),
      function ToSingle(s) {
          return parseFloat(s);
      }
    );

    $.Method({ Static: true, Public: true }, "ToInt16",
      (new JSIL.MethodSignature($.Int16, [$.String], [])),
      function ToInt16(s) {
          var i = parseInt(s, 10);
          if (isNaN(i))
              throw new Error("Invalid integer");

          return i;
      }
    );

    $.Method({ Static: true, Public: true }, "ToInt32",
      (new JSIL.MethodSignature($.Int32, [$.String], [])),
      function ToInt32(s) {
          var i = parseInt(s, 10);
          if (isNaN(i))
              throw new Error("Invalid integer");

          return i;
      }
    );

    $.Method({ Static: true, Public: true }, "ToInt64",
      (new JSIL.MethodSignature($.Int64, [$.String], [])),
      function ToInt64(s) {
          var i = parseInt(s, 10);
          if (isNaN(i))
              throw new Error("Invalid integer");

          return i;
      }
    );

    $.Method({ Static: true, Public: true }, "ToUInt16",
      (new JSIL.MethodSignature($.UInt16, [$.String], [])),
      function ToUInt16(s) {
          var i = parseInt(s, 10);
          if (isNaN(i) || i < 0)
              throw new Error("Invalid unsigned integer");

          return i;
      }
    );

    $.Method({ Static: true, Public: true }, "ToUInt32",
      (new JSIL.MethodSignature($.UInt32, [$.String], [])),
      function ToUInt32(s) {
          var i = parseInt(s, 10);
          if (isNaN(i) || i < 0)
              throw new Error("Invalid unsigned integer");

          return i;
      }
    );

    $.Method({ Static: true, Public: true }, "ToUInt64",
      (new JSIL.MethodSignature($.UInt64, [$.String], [])),
      function ToUInt64(s) {
          var i = parseInt(s, 10);
          if (isNaN(i) || i < 0)
              throw new Error("Invalid unsigned integer");

          return i;
      }
    );

    $.Method({ Static: true, Public: true }, "ToBoolean",
      (new JSIL.MethodSignature($.Boolean, [$.String], [])),
      function ToBoolean(s) {
          var text = s.toLowerCase().trim();
          if (text == "true")
              return true;
          else if (text == "false")
              return false;

          var i = parseInt(s, 10);
          if (isNaN(i))
              throw new Error("Invalid boolean");

          return (i != 0);
      }
    );

    $.Method({ Static: true, Public: true }, "ToByte",
      (new JSIL.MethodSignature($.Byte, [$.String], [])),
      function ToByte(s) {
          var i = parseInt(s, 10);
          if (isNaN(i) || i < 0 || i > 255)
              throw new Error("Invalid byte");

          return i;
      }
    );
});