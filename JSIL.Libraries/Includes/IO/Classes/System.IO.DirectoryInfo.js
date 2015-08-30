JSIL.ImplementExternals("System.IO.DirectoryInfo", function ($) {

    $.Method({ Static: false, Public: true }, ".ctor",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function _ctor(path) {
          var storageRoot = JSIL.Host.getStorageRoot();

          if (storageRoot)
              this.$fromNodeAndPath(storageRoot.resolvePath(path, false), storageRoot.normalizePath(path));
          else
              this.$fromNodeAndPath(null, path);
      }
    );

    $.Method({ Static: false, Public: true }, "Create",
      (JSIL.MethodSignature.Void),
      function Create() {
          System.IO.Directory.CreateDirectory(this._path);
      }
    );

    $.Method({ Static: false, Public: true }, "Create",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Security.AccessControl.DirectorySecurity")], [])),
      function Create(directorySecurity) {
          // FIXME: directorySecurity
          System.IO.Directory.CreateDirectory(this._path);
      }
    );

    $.Method({ Static: false, Public: true }, "EnumerateDirectories",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.IO.DirectoryInfo")]), [], [])),
      function EnumerateDirectories() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "EnumerateDirectories",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.IO.DirectoryInfo")]), [$.String], [])),
      function EnumerateDirectories(searchPattern) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "EnumerateDirectories",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.IO.DirectoryInfo")]), [$.String, $jsilcore.TypeRef("System.IO.SearchOption")], [])),
      function EnumerateDirectories(searchPattern, searchOption) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "EnumerateFiles",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.IO.FileInfo")]), [], [])),
      function EnumerateFiles() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "EnumerateFiles",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.IO.FileInfo")]), [$.String], [])),
      function EnumerateFiles(searchPattern) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "EnumerateFiles",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.IO.FileInfo")]), [$.String, $jsilcore.TypeRef("System.IO.SearchOption")], [])),
      function EnumerateFiles(searchPattern, searchOption) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "EnumerateFileSystemInfos",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.IO.FileSystemInfo")]), [], [])),
      function EnumerateFileSystemInfos() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "EnumerateFileSystemInfos",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.IO.FileSystemInfo")]), [$.String], [])),
      function EnumerateFileSystemInfos(searchPattern) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "EnumerateFileSystemInfos",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [$jsilcore.TypeRef("System.IO.FileSystemInfo")]), [$.String, $jsilcore.TypeRef("System.IO.SearchOption")], [])),
      function EnumerateFileSystemInfos(searchPattern, searchOption) {
          throw new Error('Not implemented');
      }
    );

    $.InheritBaseMethod("get_Exists", JSIL.MethodSignature.Return($.Boolean));
    $.InheritBaseMethod("get_Name", JSIL.MethodSignature.Return($.String));

    $.Method({ Static: false, Public: true }, "get_Parent",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.DirectoryInfo"), [], [])),
      function get_Parent() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "get_Root",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.DirectoryInfo"), [], [])),
      function get_Root() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetAccessControl",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Security.AccessControl.DirectorySecurity"), [], [])),
      function GetAccessControl() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetAccessControl",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Security.AccessControl.DirectorySecurity"), [$jsilcore.TypeRef("System.Security.AccessControl.AccessControlSections")], [])),
      function GetAccessControl(includeSections) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetDirectories",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.IO.DirectoryInfo")]), [], [])),
      function GetDirectories() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetDirectories",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.IO.DirectoryInfo")]), [$.String], [])),
      function GetDirectories(searchPattern) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetDirectories",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.IO.DirectoryInfo")]), [$.String, $jsilcore.TypeRef("System.IO.SearchOption")], [])),
      function GetDirectories(searchPattern, searchOption) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: false }, "GetDirName",
      (new JSIL.MethodSignature($.String, [$.String], [])),
      function GetDirName(fullPath) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: false }, "GetDisplayName",
      (new JSIL.MethodSignature($.String, [$.String, $.String], [])),
      function GetDisplayName(originalPath, fullPath) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetFiles",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.IO.FileInfo")]), [$.String], [])),
      function GetFiles(searchPattern) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetFiles",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.IO.FileInfo")]), [$.String, $jsilcore.TypeRef("System.IO.SearchOption")], [])),
      function GetFiles(searchPattern, searchOption) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetFiles",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.IO.FileInfo")]), [], [])),
      function GetFiles() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetFileSystemInfos",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.IO.FileSystemInfo")]), [$.String], [])),
      function GetFileSystemInfos(searchPattern) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetFileSystemInfos",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.IO.FileSystemInfo")]), [$.String, $jsilcore.TypeRef("System.IO.SearchOption")], [])),
      function GetFileSystemInfos(searchPattern, searchOption) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "GetFileSystemInfos",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.IO.FileSystemInfo")]), [], [])),
      function GetFileSystemInfos() {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "MoveTo",
      (new JSIL.MethodSignature(null, [$.String], [])),
      function MoveTo(destDirName) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "SetAccessControl",
      (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Security.AccessControl.DirectorySecurity")], [])),
      function SetAccessControl(directorySecurity) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: false, Public: true }, "toString",
      (new JSIL.MethodSignature($.String, [], [])),
      function toString() {
          return "<DirectoryInfo " + this._path + ">";
      }
    );

});