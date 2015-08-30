JSIL.ImplementExternals("System.IO.Path", function ($) {
    var combineImpl = function () {
        return Array.prototype.slice.call(arguments).join("\\");
    };

    $.Method({ Static: true, Public: true }, "Combine",
      new JSIL.MethodSignature($.String, [$.String, $.String], []),
      combineImpl
    );

    $.Method({ Static: true, Public: true }, "Combine",
      new JSIL.MethodSignature($.String, [
          $.String, $.String,
          $.String
      ], []),
      combineImpl
    );

    $.Method({ Static: true, Public: true }, "Combine",
      new JSIL.MethodSignature($.String, [
          $.String, $.String,
          $.String, $.String
      ], []),
      combineImpl
    );

    $.Method({ Static: true, Public: true }, "Combine",
      new JSIL.MethodSignature($.String, [$jsilcore.TypeRef("System.Array", [$.String])], []),
      function Combine(pathSegments) {
          return pathSegments.join("\\");
      }
    );

    $.Method({ Static: true, Public: true }, "GetInvalidPathChars",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Char]), [], [])),
      function GetInvalidPathChars() {
          var result = ['"', "<", ">", "|", "*", "?"];

          for (var i = 0; i < 31; i++)
              result.push(String.fromCharCode(i));

          return result;
      }
    );

    $.Method({ Static: true, Public: true }, "GetInvalidFileNameChars",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Char]), [], [])),
      function GetInvalidFileNameChars() {
          var result = System.IO.Path.GetInvalidPathChars();
          result.push("\\");
          result.push("/");
          result.push(":");

          return result;
      }
    );

    $.Method({ Static: true, Public: true }, "GetExtension",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      function GetExtension(path) {
          var index = path.lastIndexOf(".");
          if (index >= 0) {
              return path.substr(index);
          }

          return "";
      }
    );

    $.Method({ Static: true, Public: true }, "GetDirectoryName",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      function GetDirectoryName(path) {
          var index = Math.max(path.lastIndexOf("\\"), path.lastIndexOf("/"));
          if (index >= 0) {
              return path.substr(0, index);
          }

          return "";
      }
    );

    $.Method({ Static: true, Public: true }, "GetFullPath",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      function GetFullPath(path) {
          // FIXME
          return path;
      }
    );

    $.Method({ Static: true, Public: true }, "GetFileName",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      function GetFileName(path) {
          var index = Math.max(path.lastIndexOf("\\"), path.lastIndexOf("/"));
          if (index >= 0) {
              return path.substr(index + 1);
          }

          return path;
      }
    );

    $.Method({ Static: true, Public: true }, "GetFileNameWithoutExtension",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      function GetFileNameWithoutExtension(path) {
          var index = Math.max(path.lastIndexOf("\\"), path.lastIndexOf("/"));
          if (index >= 0) {
              path = path.substr(index + 1);
          }

          index = path.indexOf(".");
          if (index >= 0)
              path = path.substr(0, index);

          return path;
      }
    );

    $.Method({ Static: true, Public: true }, "IsPathRooted",
      (new JSIL.MethodSignature($.Boolean, [$.String], [])),
      function GetFileName(path) {
          return path.match(/^(\w:|\\)/);
      }
    );
});