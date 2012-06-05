"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

var $jsilstorage = JSIL.DeclareAssembly("JSIL.Storage");

JSIL.MakeClass($jsilcore.System.Object, "VirtualVolume", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (name, rootPath, inodes) {
    this.inodes = [];
    this.name = name;

    rootPath = this.normalizePath(rootPath);

    if (rootPath[rootPath.length - 1] !== "/")
      rootPath += "/";

    if (JSIL.IsArray(inodes)) {
      this.initFromInodes(rootPath, inodes);
    } else {
      this.rootDirectory = new VirtualDirectory(
        this, null, this.makeInode(null, "directory", rootPath)
      );
    }
  });

  $.RawMethod(false, "initFromInodes", function (rootPath, inodes) {
    // Create local copies of all the source inodes

    for (var i = 0, l = inodes.length; i < l; i++) {
      var inode = inodes[i], resultInode;

      if (typeof (inode.parent) === "number") {
        resultInode = this.makeInode(this.inodes[inode.parent], inode.type, inode.name);
      } else {
        resultInode = this.makeInode(null, inode.type, inode.name);
      }

      if (inode.metadata) {
        for (var k in inode.metadata) {
          if (!inode.metadata.hasOwnProperty(k))
            continue;

          resultInode.metadata[k] = inode.metadata[k];
        }
      }
    }

    this.inodes[0].name = rootPath;

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
      name: name,
      metadata: {}
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

  $.RawMethod(false, "normalizePath", function (path) {
    var backslashRe = /\\/g;

    path = path.replace(backslashRe, "/");

    if (this.rootDirectory) {
      var indexOfRoot = path.indexOf(this.rootDirectory.path);
      if (indexOfRoot === 0)
        path = path.substr(this.rootDirectory.path.length);
    }

    return path;
  });  

  $.RawMethod(false, "createDirectory", function (path) {
    path = this.normalizePath(path);

    var pieces = path.split("/");

    for (var i = 0, l = pieces.length; i < l; i++) {
      var containingPath = pieces.slice(0, i).join("/") + "/";

      var containingDirectory = this.rootDirectory.resolvePath(containingPath);
      containingDirectory.createDirectory(pieces[i], true);
    }

    return this.rootDirectory.resolvePath(path);
  });

  $.RawMethod(false, "createFile", function (path, allowExisting) {
    path = this.normalizePath(path);

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
    path = this.normalizePath(path);

    return this.rootDirectory.resolvePath(path, throwOnFail);
  });

  $.RawMethod(false, "flush", function () {    
    throw new Error("Not implemented");
  });

  $.RawMethod(false, "getBlob", function (name) {
    throw new Error("Not implemented");
  });

  $.RawMethod(false, "setBlob", function (name, value) {    
    throw new Error("Not implemented");
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
      parent ? 
        parent.path + (this.name + "/") : 
        this.name
    );

    Object.defineProperty(
      this.inode, "object", {
        value: this,
        enumerable: false,
        configurable: false,
        writable: false
      }
    );

    if (!this.inode.metadata.created)
      this.inode.metadata.created = Date.now();

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

    Object.defineProperty(
      this, "volume", {
        get: function () {
          return this.parent.volume;
        },
        enumerable: false,
        configurable: false
      }
    );

    if (!this.inode.metadata.created)
      this.inode.metadata.created = Date.now();

    parent.files[this.name.toLowerCase()] = this;
  });

  $.RawMethod(false, "readAllBytes", function () {
    var bytes = this.volume.getBlob(this.path);

    this.inode.metadata.lastRead = Date.now();

    if (!bytes)
      return new Array(this.inode.metadata.length || 0);

    return bytes;
  });

  $.RawMethod(false, "writeAllBytes", function (buffer, count) {
    var bytes = Array.prototype.slice.call(buffer, 0, count);

    this.volume.setBlob(this.path, bytes);

    this.inode.metadata.lastWritten = Date.now();
    this.inode.metadata.length = count;
  });

  $.RawMethod(false, "toString", function () {
    return "<Virtual File '" + this.path + "' in volume '" + this.volume.name + "'>";
  });
});

JSIL.ImplementExternals("System.IO.FileStream", function ($) {
  $.RawMethod(false, "$fromVirtualFile", function (virtualFile) {
    System.IO.Stream.prototype._ctor.call(this);

    this._fileName = virtualFile.path;
    this._buffer = virtualFile.readAllBytes();

    this._pos = 0;
    this._length = this._buffer.length;

    this._onClose = function () {
      if (this._modified)
        virtualFile.writeAllBytes(this._buffer, this._length);
    };
  });
});

$jsilstorage.providers = [];

JSIL.RegisterStorageProvider = function (provider) {
  $jsilstorage.providers.push(provider);
};

JSIL.GetStorageVolumes = function () {
  var result = [];

  for (var i = 0, l = $jsilstorage.providers.length; i < l; i++) {
    var provider = $jsilstorage.providers[i];

    var volumes = provider.getVolumes();

    for (var j = 0, m = volumes.length; j < m; j++)
      result.push(volumes[j]);
  }

  return result;
};