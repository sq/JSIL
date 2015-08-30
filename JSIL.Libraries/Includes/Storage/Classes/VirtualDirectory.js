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

        Object.defineProperty(
          this, "type", {
              get: function () {
                  return inode.type;
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
            this.inode.metadata.created = JSIL.Host.getFileTime();

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

    $.RawMethod(false, "unlink", function () {
        // FIXME: Call .unlink() on child directories/files instead of relying on unlinkInode.
        // Right now this will leak file bytes for child files.

        delete this.parent.directories[this.name.toLowerCase()];

        this.volume.unlinkInode(this.inode);
        this.volume.flush();
    });

    $.RawMethod(false, "createFile", function (name, allowExisting) {
        var existingFile = this.getFile(name);
        if (existingFile) {
            if (allowExisting)
                return existingFile;
            else
                throw new Error("A file named '" + name + "' already exists.");
        }

        if (this.volume.readOnly)
            throw new Error("The volume is read-only.");

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

        if (this.volume.readOnly)
            throw new Error("The volume is read-only.");

        return new VirtualDirectory(
          this.volume, this, this.volume.makeInode(this.inode, "directory", name)
        );
    });

    $.RawMethod(false, "createJunction", function (name, targetObject, allowExisting) {
        var existingDirectory = this.getDirectory(name);
        if (existingDirectory) {
            if (allowExisting)
                return existingDirectory;
            else
                throw new Error("A directory named '" + name + "' already exists.");
        }

        if ((typeof (targetObject) !== "object") || (targetObject.type !== "directory"))
            throw new Error("Target for junction must be a directory object");

        if (this.volume.readOnly)
            throw new Error("The volume is read-only.");

        return new VirtualJunction(
          this.volume, this, name, targetObject
        );
    });

    $.RawMethod(false, "resolvePath", function (path, throwOnFail) {
        if (typeof (throwOnFail) === "undefined")
            throwOnFail = true;

        if (path === null) {
            if (throwOnFail)
                throw new Error("path was null");
            else
                return null;
        }

        var firstSlash = path.indexOf("/"), itemName, childPath;

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

    $.RawMethod(false, "enumerate", function (nodeType, searchPattern) {
        var result = [];
        var predicate = function (fn) { return true; };

        if (searchPattern) {
            var starRegex = /\*/g;
            var questionMarkRegex = /\?/g;
            var dotRegex = /\./g

            var regexText = searchPattern
              .replace(dotRegex, "\\.")
              .replace(starRegex, "(.*)")
              .replace(questionMarkRegex, ".");

            var regex = new RegExp(regexText, "i");

            predicate = function (fn) {
                return regex.test(fn);
            };
        }

        if (nodeType !== "directory") {
            for (var k in this.files) {
                if (predicate(k))
                    result.push(this.files[k]);
            }
        }

        if (nodeType !== "file") {
            for (var k in this.directories) {
                if ((nodeType === "junction") && (this.directories[k].type !== "junction"))
                    continue;

                if (predicate(k))
                    result.push(this.directories[k])
            }
        }

        return result;
    });

    $.RawMethod(false, "toString", function () {
        return "<Virtual Directory '" + this.path + "' in volume '" + this.volume.name + "'>";
    });
});