"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

if (typeof ($jsilxna) === "undefined")
  throw new Error("JSIL.XNACore required");

JSIL.MakeClass($jsilcore.System.Object, "VirtualVolume", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (name) {
  	this.name = name;
  	this.rootDirectory = new VirtualDirectory(this);
  });

  $.RawMethod(false, "stripRoot", function (path) {
  	var indexOfRoot = path.indexOf(this.rootDirectory.path);
  	if (indexOfRoot === 0)
  		path = path.substr(this.rootDirectory.path.length);

  	return path;
  });  

  $.RawMethod(false, "createDirectory", function (path) {
  	path = this.stripRoot(path);

  	var pieces = path.split("/");

  	for (var i = 0, l = pieces.length; i < l; i++) {
  		var containingPath = pieces.slice(0, i).join("/") + "/";

  		var containingDirectory = this.rootDirectory.resolvePath(containingPath);
  		containingDirectory.createDirectory(pieces[i], true);
  	}

  	return this.rootDirectory.resolvePath(path);
  });

  $.RawMethod(false, "resolvePath", function (path, throwOnFail) {
  	path = this.stripRoot(path);

  	return this.rootDirectory.resolvePath(path, throwOnFail);
  });

  $.RawMethod(false, "toString", function () {
    return "<Virtual Storage Volume '" + this.name + "'>";
  });
});

JSIL.MakeClass($jsilcore.System.Object, "VirtualDirectory", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (volume, parent, name) {
  	this.volume = volume;
  	this.parent = parent || null;
  	this.name = parent ? name : "/";
  	this.path = parent ? parent.path + (name + "/") : "/";

  	this.directories = {};
  	this.files = {};
  });

  $.RawMethod(false, "getFile", function (name) {
  	var file = this.files[name.toLowerCase()];
  	if (!file)
  		return null;

  	return file;
  });

  $.RawMethod(false, "getDirectory", function (name) {
  	if ((name === ".") || (name === ""))
  		return this;
  	else if (name === "..")
  		return this.parent;

  	var directory = this.directories[name.toLowerCase()];
  	if (!directory)
  		return null;

  	return directory;
  });

  $.RawMethod(false, "createFile", function (name, allowExisting) {
  	var existingFile = this.getFile(name);
  	if (existingFile) {
  		if (allowExisting)
  			return existingFile;
  		else
  			throw new Error("A file named '" + name + "' already exists.");
  	}

  	return this.files[name.toLowerCase()] = new VirtualFile(this, name);
  });

  $.RawMethod(false, "createDirectory", function (name, allowExisting) {
  	var existingDirectory = this.getDirectory(name);
  	if (existingDirectory) {
  		if (allowExisting)
  			return existingDirectory;
  		else
  			throw new Error("A directory named '" + name + "' already exists.");
  	}

  	return this.directories[name.toLowerCase()] = new VirtualDirectory(this.volume, this, name);
  });

  $.RawMethod(false, "resolvePath", function (path, throwOnFail) {
  	var firstSlash = path.indexOf("/"), itemName, childPath;

  	if (typeof(throwOnFail) === "undefined")
  		throwOnFail = true;

  	if (firstSlash >= 0) {
  		itemName = path.substr(0, firstSlash);
  		childPath = path.substr(firstSlash);
  	} else {
  		itemName = path;
  		childPath = null;
  	}

  	var forceDirectory = childPath === "/";
  	if (forceDirectory) {
  		childPath = null;
  	} else if (childPath) {
  		while (childPath[0] === "/")
  			childPath = childPath.substr(1);
  	}

  	var result = this.getDirectory(itemName);
  	if (result === null) {
  		if (forceDirectory) {
  			if (throwOnFail)
  				throw new Error("No directory named '" + itemName + "' could be found in directory '" + this.path + "'.");
  			else
  				result = null;
		} else
  			result = this.getFile(itemName);
  	}

  	if (childPath) {
  		if (result) {
  			return result.resolvePath(childPath, throwOnFail);
  		} else {
  			if (throwOnFail)
  				throw new Error("No directory named '" + itemName + "' could be found in directory '" + this.path + "'.");
  			else
  				return null;
  		}
  	} else {
  		if (!result && throwOnFail)
			throw new Error("No file or directory named '" + itemName + "' could be found in directory '" + this.path + "'.");

  		return result;
  	}
  });

  $.RawMethod(false, "toString", function () {
    return "<Virtual Directory '" + this.path + "' in volume '" + this.volume.name + "'>";
  });
});

JSIL.MakeClass($jsilcore.System.Object, "VirtualFile", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (parent, name) {
 	this.parent = parent;
  	this.name = name;
  	this.path = parent.path + name;
  });

  $.RawMethod(false, "toString", function () {
    return "<Virtual File '" + this.path + "' in volume '" + this.parent.volume.name + "'>";
  });
});

JSIL.ImplementExternals("Microsoft.Xna.Framework.Storage.StorageContainer", function ($) {

  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [
          $xnaasms.xnaStorage.TypeRef("Microsoft.Xna.Framework.Storage.StorageDevice"), $xnaasms.xna.TypeRef("Microsoft.Xna.Framework.PlayerIndex"), 
          $.String
        ], [])), 
    function _ctor (device, index, displayName) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "add_Disposing", 
    (new JSIL.MethodSignature(null, [$xnaasms.corlib.TypeRef("System.EventHandler`1", [$xnaasms.corlib.TypeRef("System.EventArgs")])], [])), 
    function add_Disposing (value) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "CreateDirectory", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function CreateDirectory (directory) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "CreateFile", 
    (new JSIL.MethodSignature($xnaasms.corlib.TypeRef("System.IO.Stream"), [$.String], [])), 
    function CreateFile (file) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:false}, "DeleteContainer", 
    (new JSIL.MethodSignature(null, [
          $xnaasms.xnaStorage.TypeRef("Microsoft.Xna.Framework.Storage.StorageDevice"), $xnaasms.xna.TypeRef("Microsoft.Xna.Framework.PlayerIndex"), 
          $.String, $.Boolean
        ], [])), 
    function DeleteContainer (device, index, displayName, closeContainerFirst) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "DeleteDirectory", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function DeleteDirectory (directory) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "DeleteFile", 
    (new JSIL.MethodSignature(null, [$.String], [])), 
    function DeleteFile (file) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "DirectoryExists", 
    (new JSIL.MethodSignature($.Boolean, [$.String], [])), 
    function DirectoryExists (directory) {
      // FIXME
      return false;
    }
  );

  $.Method({Static:false, Public:true }, "Dispose", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Dispose () {
      // FIXME
    }
  );

  $.Method({Static:false, Public:false}, "Dispose", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function Dispose (disposing) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:false}, "DisposeOverride", 
    (new JSIL.MethodSignature(null, [$.Boolean], [])), 
    function DisposeOverride (disposing) {
      // FIXME
    }
  );

  $.Method({Static:false, Public:true }, "FileExists", 
    (new JSIL.MethodSignature($.Boolean, [$.String], [])), 
    function FileExists (file) {
      // FIXME
      return false;
    }
  );

  $.Method({Static:false, Public:false}, "Finalize", 
    (new JSIL.MethodSignature(null, [], [])), 
    function Finalize () {
      // FIXME
    }
  );

  $.Method({Static:false, Public:false}, "FinishCreation", 
    (new JSIL.MethodSignature(null, [], [])), 
    function FinishCreation () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_DisplayName", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function get_DisplayName () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_IsDisposed", 
    (new JSIL.MethodSignature($.Boolean, [], [])), 
    function get_IsDisposed () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "get_StorageDevice", 
    (new JSIL.MethodSignature($xnaasms.xnaStorage.TypeRef("Microsoft.Xna.Framework.Storage.StorageDevice"), [], [])), 
    function get_StorageDevice () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:false}, "GetDeviceFolder", 
    (new JSIL.MethodSignature($.String, [$.UInt32], [])), 
    function GetDeviceFolder (deviceIndex) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:false}, "GetDirectoryForContainer", 
    (new JSIL.MethodSignature($xnaasms.corlib.TypeRef("System.IO.DirectoryInfo"), [
          $xnaasms.xnaStorage.TypeRef("Microsoft.Xna.Framework.Storage.StorageDevice"), $.String, 
          $xnaasms.xna.TypeRef("Microsoft.Xna.Framework.PlayerIndex")
        ], [])), 
    function GetDirectoryForContainer (device, displayName, playerIndex) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "GetDirectoryNames", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [], [])), 
    function GetDirectoryNames () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "GetDirectoryNames", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [$.String], [])), 
    function GetDirectoryNames (searchPattern) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "GetFileNames", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [], [])), 
    function GetFileNames () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "GetFileNames", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.String]), [$.String], [])), 
    function GetFileNames (searchPattern) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "GetFullPath", 
    (new JSIL.MethodSignature($.String, [$.String], [])), 
    function GetFullPath (path) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:false}, "GetGameTitle", 
    (new JSIL.MethodSignature($.String, [], [])), 
    function GetGameTitle () {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "OpenFile", 
    (new JSIL.MethodSignature($xnaasms.corlib.TypeRef("System.IO.Stream"), [$.String, $xnaasms.corlib.TypeRef("System.IO.FileMode")], [])), 
    function OpenFile (file, fileMode) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "OpenFile", 
    (new JSIL.MethodSignature($xnaasms.corlib.TypeRef("System.IO.Stream"), [
          $.String, $xnaasms.corlib.TypeRef("System.IO.FileMode"), 
          $xnaasms.corlib.TypeRef("System.IO.FileAccess")
        ], [])), 
    function OpenFile (file, fileMode, fileAccess) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "OpenFile", 
    (new JSIL.MethodSignature($xnaasms.corlib.TypeRef("System.IO.Stream"), [
          $.String, $xnaasms.corlib.TypeRef("System.IO.FileMode"), 
          $xnaasms.corlib.TypeRef("System.IO.FileAccess"), $xnaasms.corlib.TypeRef("System.IO.FileShare")
        ], [])), 
    function OpenFile (file, fileMode, fileAccess, fileShare) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:true }, "remove_Disposing", 
    (new JSIL.MethodSignature(null, [$xnaasms.corlib.TypeRef("System.EventHandler`1", [$xnaasms.corlib.TypeRef("System.EventArgs")])], [])), 
    function remove_Disposing (value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "ValidateArguments", 
    (new JSIL.MethodSignature($.String, [$.String, $.String], [])), 
    function ValidateArguments (path, argumentName) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:false, Public:false}, "VerifyNotDisposed", 
    (new JSIL.MethodSignature(null, [], [])), 
    function VerifyNotDisposed () {
      throw new Error('Not implemented');
    }
  );

});