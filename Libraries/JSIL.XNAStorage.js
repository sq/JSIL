"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

if (typeof ($jsilxna) === "undefined")
  throw new Error("JSIL.XNACore required");

JSIL.MakeClass($jsilcore.System.Object, "VirtualVolume", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (name, inodes) {
  	this.inodes = [];
  	this.name = name;

  	if (inodes && (inodes.length > 0)) {
  		this.initFromInodes(inodes);
  	} else {
  		this.rootDirectory = new VirtualDirectory(
  			this, null, this.makeInode(null, "directory", "/")
		);
  	}
  });

  $.RawMethod(false, "initFromInodes", function (inodes) {
	// Create local copies of all the source inodes

  	for (var i = 0, l = inodes.length; i < l; i++) {
  		var inode = inodes[i], resultInode;

  		if (typeof (inode.parent) === "number") {
  			resultInode = this.makeInode(this.inodes[inode.parent], inode.type, inode.name);
  		} else {
  			resultInode = this.makeInode(null, inode.type, inode.name);
  		}

		if (inode.metadata)
  			resultInode.metadata = inode.metadata;

  		if (inode.ref)
  			resultInode.ref = inode.ref;
  	}

  	this.rootDirectory = new VirtualDirectory(
  		this, null, this.inodes[0]
	);

  	for (var i = 1, l = inodes.length; i < l; i++) {
  		var inode = this.inodes[i];
  		var parentInode = this.inodes[inode.parent];

  		switch (inode.type) {
  			case "directory":
  				new VirtualDirectory(
  					this, parentInode.object, inode
				);

  				break;

  			case "file":
  				new VirtualFile(
  					parentInode.object, inode
				);

  				break;
  		}
  	}
  });

  $.RawMethod(false, "makeInode", function (parent, type, name) {
  	var inode = {
		type: type,
		name: name
  	};

  	if (parent) {
	  	Object.defineProperty(
	  		inode, "parent", {
	  			value: parent.index,
	  			configurable: false,
	  			enumerable: true,
	  			writable: false
	  		}
		);
  	}

  	Object.defineProperty(
  		inode, "index", {
  			value: this.inodes.length,
  			configurable: false,
  			enumerable: false,
  			writable: false
  		}
	);

  	this.inodes.push(inode);

  	return inode;
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

  $.RawMethod(false, "createFile", function (path, allowExisting) {
  	path = this.stripRoot(path);

  	var lastSlash = path.lastIndexOf("/"), parentDirectory, fileName;
  	if (lastSlash >= 0) {
  		parentDirectory = this.rootDirectory.resolvePath(path.substr(0, lastSlash), true);
  		fileName = path.substr(lastSlash + 1);
  	} else {
  		parentDirectory = this.rootDirectory;
  		fileName = path;
  	}

	return parentDirectory.createFile(fileName, allowExisting);
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
  $.RawMethod(false, ".ctor", function (volume, parent, inode) {
  	if (inode.type !== "directory")
  		throw new Error("Inode is not a directory");

  	this.volume = volume;
 	this.parent = parent;
  	this.inode = inode;

  	JSIL.SetValueProperty(
  		this, "directories", {}
	);
	JSIL.SetValueProperty(
		this, "files", {}
	);

  	Object.defineProperty(
  		this, "name", {
  			configurable: false,
  			get: function () {
  				return inode.name;
  			}
  		}
	);

	JSIL.SetValueProperty(
		this, "path", 
		parent ? parent.path + (this.name + "/") : "/"
	);

	Object.defineProperty(
		this.inode, "object", {
			value: this,
			enumerable: false,
			configurable: false,
			writable: false
		}
	);

  	if (parent)
  		parent.directories[this.name.toLowerCase()] = this;
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

  	return new VirtualFile(
  		this, this.volume.makeInode(this.inode, "file", name)
	);
  });

  $.RawMethod(false, "createDirectory", function (name, allowExisting) {
  	var existingDirectory = this.getDirectory(name);
  	if (existingDirectory) {
  		if (allowExisting)
  			return existingDirectory;
  		else
  			throw new Error("A directory named '" + name + "' already exists.");
  	}

  	return new VirtualDirectory(
  		this.volume, this, this.volume.makeInode(this.inode, "directory", name)
	);
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
  $.RawMethod(false, ".ctor", function (parent, inode) {
  	if (inode.type !== "file")
  		throw new Error("Inode is not a file");

 	this.parent = parent;
  	this.inode = inode;

  	Object.defineProperty(
  		this, "name", {
  			configurable: false,
  			get: function () {
  				return inode.name;
  			}
  		}
	);

	JSIL.SetValueProperty(
		this, "path", parent.path + this.name
	);

	Object.defineProperty(
		this.inode, "object", {
			value: this,
			enumerable: false,
			configurable: false,
			writable: false
		}
	);

	parent.files[this.name.toLowerCase()] = this;
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