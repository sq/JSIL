JSIL.ImplementExternals("System.IO.Directory", function ($) {
    $.Method({ Static: true, Public: true }, "Exists",
      new JSIL.MethodSignature($.Boolean, [$.String], []),
      function (filename) {
          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot) {
              var resolved = storageRoot.resolvePath(filename, false);
              return (resolved && resolved.type !== "file");
          }

          return false;
      }
    );

    $.Method({ Static: true, Public: true }, "CreateDirectory",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.DirectoryInfo"), [$.String], [])),
      function CreateDirectory(path) {
          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot) {
              var node = storageRoot.createDirectory(path);
              var tInfo = $jsilcore.System.IO.DirectoryInfo.__Type__;

              var result = JSIL.CreateInstanceOfType(tInfo, "$fromNodeAndPath", [node, path]);
              return result;
          } else {
              throw new Error('Storage root not available');
          }
      }
    );

    $.Method({ Static: true, Public: true }, "GetDirectories",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [$.String], [])),
      function GetDirectories(path) {
          return System.IO.Directory.GetDirectories(path, "*", null);
      }
    );

    $.Method({ Static: true, Public: true }, "GetDirectories",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [$.String, $.String], [])),
      function GetDirectories(path, searchPattern) {
          return System.IO.Directory.GetDirectories(path, searchPattern, null);
      }
    );

    $.Method({ Static: true, Public: true }, "GetDirectories",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [
            $.String, $.String,
            $jsilcore.TypeRef("System.IO.SearchOption")
      ], [])),
      function GetDirectories(path, searchPattern, searchOption) {
          // FIXME: searchOption
          if (Number(searchOption))
              throw new Error("Recursive search not implemented");

          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot) {
              var searchPath = storageRoot.resolvePath(path, true);

              return searchPath.enumerate("directory", searchPattern).map(function (node) { return node.path; });
          } else {
              throw new Error('Storage root not available');
          }
      }
    );

    $.Method({ Static: true, Public: true }, "GetFiles",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [$.String], [])),
      function GetFiles(path) {
          return System.IO.Directory.GetFiles(path, "*", null);
      }
    );

    $.Method({ Static: true, Public: true }, "GetFiles",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [$.String, $.String], [])),
      function GetFiles(path, searchPattern) {
          return System.IO.Directory.GetFiles(path, searchPattern, null);
      }
    );

    $.Method({ Static: true, Public: true }, "GetFiles",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [
            $.String, $.String,
            $jsilcore.TypeRef("System.IO.SearchOption")
      ], [])),
      function GetFiles(path, searchPattern, searchOption) {
          // FIXME: searchOption
          if (Number(searchOption))
              throw new Error("Recursive search not implemented");

          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot) {
              var searchPath = storageRoot.resolvePath(path, true);

              return searchPath.enumerate("file", searchPattern).map(function (node) { return node.path; });
          } else {
              throw new Error('Storage root not available');
          }
      }
    );

});