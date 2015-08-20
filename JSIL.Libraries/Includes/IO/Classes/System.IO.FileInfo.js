JSIL.ImplementExternals("System.IO.FileInfo", function ($) {

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function _ctor(fileName) {
          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot)
              this.$fromNodeAndPath(storageRoot.resolvePath(fileName, false), storageRoot.normalizePath(fileName));
          else
              this.$fromNodeAndPath(null, fileName);
      }
    );

    $.Method({ Static: false, Public: true }, "AppendText",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.StreamWriter"), [], [])),
      function AppendText() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "CopyTo",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileInfo"), [$.String], [])),
      function CopyTo(destFileName) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "CopyTo",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileInfo"), [$.String, $.Boolean], [])),
      function CopyTo(destFileName, overwrite) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Create",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [], [])),
      function Create() {
          return System.IO.File.Create(this._path);
      }
    );

    $.Method({ Static: false, Public: true }, "CreateText",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.StreamWriter"), [], [])),
      function CreateText() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Decrypt",
      (JSIL.MethodSignature.Void),
      function Decrypt() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Delete",
      (JSIL.MethodSignature.Void),
      function Delete() {
          System.IO.File.Delete(this._path);
      }
    );

    $.Method({ Static: false, Public: true }, "Encrypt",
      (JSIL.MethodSignature.Void),
      function Encrypt() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "get_Directory",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.DirectoryInfo"), [], [])),
      function get_Directory() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "get_DirectoryName",
      (new JSIL.MethodSignature($.String, [], [])),
      function get_DirectoryName() {
          throw new Error('Not implemented');
      }
    );

    $.InheritBaseMethod("get_Exists", JSIL.MethodSignature.Return($.Boolean));

    $.Method({ Static: false, Public: true }, "get_IsReadOnly",
      (new JSIL.MethodSignature($.Boolean, [], [])),
      function get_IsReadOnly() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "get_Length",
      (new JSIL.MethodSignature($.Int64, [], [])),
      function get_Length() {
          throw new Error('Not implemented');
      }
    );

    $.InheritBaseMethod("get_Name", JSIL.MethodSignature.Return($.String));

    $.Method({ Static: false, Public: true }, "GetAccessControl",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Security.AccessControl.FileSecurity"), [], [])),
      function GetAccessControl() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetAccessControl",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Security.AccessControl.FileSecurity"), [$jsilcore.TypeRef("System.Security.AccessControl.AccessControlSections")], [])),
      function GetAccessControl(includeSections) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: false }, "GetDisplayPath",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      function GetDisplayPath(originalPath) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "MoveTo",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function MoveTo(destFileName) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Open",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [$jsilcore.TypeRef("System.IO.FileMode")], [])),
      function Open(mode) {
          return System.IO.File.Open(this._path, mode);
      }
    );

    $.Method({ Static: false, Public: true }, "Open",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [$jsilcore.TypeRef("System.IO.FileMode"), $jsilcore.TypeRef("System.IO.FileAccess")], [])),
      function Open(mode, access) {
          // FIXME: access
          return System.IO.File.Open(this._path, mode);
      }
    );

    $.Method({ Static: false, Public: true }, "Open",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [
            $jsilcore.TypeRef("System.IO.FileMode"), $jsilcore.TypeRef("System.IO.FileAccess"),
            $jsilcore.TypeRef("System.IO.FileShare")
      ], [])),
      function Open(mode, access, share) {
          // FIXME: access, share
          return System.IO.File.Open(this._path, mode);
      }
    );

    $.Method({ Static: false, Public: true }, "OpenRead",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [], [])),
      function OpenRead() {
          return System.IO.File.OpenRead(this._path);
      }
    );

    $.Method({ Static: false, Public: true }, "OpenText",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.StreamReader"), [], [])),
      function OpenText() {
          return System.IO.File.OpenText(this._path);
      }
    );

    $.Method({ Static: false, Public: true }, "OpenWrite",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileStream"), [], [])),
      function OpenWrite() {
          return System.IO.File.OpenWrite(this._path);
      }
    );

    $.Method({ Static: false, Public: true }, "Replace",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileInfo"), [$.String, $.String], [])),
      function Replace(destinationFileName, destinationBackupFileName) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "Replace",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.FileInfo"), [
            $.String, $.String,
            $.Boolean
      ], [])),
      function Replace(destinationFileName, destinationBackupFileName, ignoreMetadataErrors) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "set_IsReadOnly",
      (new JSIL.MethodSignature(null, [$.Boolean], [])),
      function set_IsReadOnly(value) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "SetAccessControl",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Security.AccessControl.FileSecurity")], [])),
      function SetAccessControl(fileSecurity) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "toString",
      (new JSIL.MethodSignature($.String, [], [])),
      function toString() {
          return "<FileInfo " + this._path + ">";
      }
    );

});