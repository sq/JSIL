JSIL.ImplementExternals("System.IO.File", function ($) {
    $.Method({ Static: true, Public: true }, "Exists",
      new JSIL.MethodSignature($.Boolean, [$.String], []),
      function (filename) {
          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot) {
              var resolved = storageRoot.resolvePath(filename, false);

              if (resolved && resolved.type === "file")
                  return true;
          }

          return JSIL.Host.doesAssetExist(filename, true);
      }
    );

    $.Method({ Static: true, Public: true }, "Open",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [$.String, $jsilcore.TypeRef("System.IO.FileMode"), $jsilcore.TypeRef("System.IO.FileAccess")], [])),
      function OpenRead(path, mode, access) {
          return new System.IO.FileStream(path, mode, access);
      }
    );

    $.Method({ Static: true, Public: true }, "OpenRead",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [$.String], [])),
      function OpenRead(path) {
          return new System.IO.FileStream(path, System.IO.FileMode.Open);
      }
    );

    $.Method({ Static: true, Public: true }, "OpenWrite",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [$.String], [])),
      function OpenWrite(path) {
          return new System.IO.FileStream(path, System.IO.FileMode.OpenOrCreate);
      }
    );

    $.Method({ Static: true, Public: true }, "Create",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [$.String], [])),
      function Create(path) {
          return new System.IO.FileStream(path, System.IO.FileMode.Create);
      }
    );

    $.Method({ Static: true, Public: true }, "AppendText",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.StreamWriter"), [$.String], [])),
      function AppendText(path) {
          return new System.IO.StreamWriter(path, true);
      }
    );

    $.Method({ Static: true, Public: true }, "CreateText",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.StreamWriter"), [$.String], [])),
      function CreateText(path) {
          return new System.IO.StreamWriter(path, false);
      }
    );

    $.Method({ Static: true, Public: true }, "OpenText",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.StreamReader"), [$.String], [])),
      function OpenText(path) {
          return new System.IO.StreamReader(path, false);
      }
    );

    var copyFileImpl = function Copy(from, to, overwrite) {
        var storageRoot = JSIL.Host.getStorageRoot();

        if (storageRoot) {
            var fromResolved = storageRoot.resolvePath(from, false);

            if (!fromResolved || fromResolved.type !== "file")
                throw new System.IO.FileNotFoundException(from);

            var toResolved = storageRoot.createFile(to, overwrite);
            if (!toResolved)
                throw new System.Exception("Could not create file '" + to + "'");

            toResolved.writeAllBytes(fromResolved.readAllBytes());
        } else {
            throw new System.NotImplementedException("No storage root available");
        }
    };

    $.Method({ Static: true, Public: true }, "Copy",
      (new JSIL.MethodSignature(null, [$.String, $.String], [])),
      copyFileImpl
    );

    $.Method({ Static: true, Public: true }, "Copy",
      (new JSIL.MethodSignature(null, [$.String, $.String, $.Boolean], [])),
      copyFileImpl
    );

    $.Method({ Static: true, Public: true }, "Delete",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function Delete(path) {
          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot) {
              var resolved = storageRoot.resolvePath(path, false);

              if (resolved && resolved.type === "file")
                  resolved.unlink();
          }
      }
    );

    $.Method({ Static: true, Public: true }, "ReadAllText",
      new JSIL.MethodSignature($.String, [$.String], []),
      function ReadAllText(filename) {
          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot) {
              var resolved = storageRoot.resolvePath(filename, false);

              if (resolved && resolved.type === "file")
                  return JSIL.StringFromByteArray(resolved.readAllBytes());
              else
                  throw new System.IO.FileNotFoundException(filename);
          }

          throw new System.NotImplementedException("No storage root available");
      }
    );

    $.Method({ Static: true, Public: true }, "ReadAllBytes",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Byte]), [$.String], []),
      function ReadAllBytes(filename) {
          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot) {
              var resolved = storageRoot.resolvePath(filename, false);

              if (resolved && resolved.type === "file")
                  return resolved.readAllBytes();
              else
                  throw new System.IO.FileNotFoundException(filename);
          }

          throw new System.NotImplementedException("No storage root available");
      }
    );
});