JSIL.MakeClass($jsilcore.System.Object, "VirtualVolume", true, [], function ($) {
    $.RawMethod(false, ".ctor", function (name, rootPath, inodes) {
        this.inodes = [];
        this.name = name;
        this.readOnly = false;

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
            if (!inode) {
                this.inodes.push(null);
                continue;
            }

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
            if (!inode)
                continue;

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
        if (path === null)
            return null;

        var backslashRe = /\\/g;

        path = path.replace(backslashRe, "/");

        if (this.rootDirectory) {
            var indexOfRoot = path.indexOf(this.rootDirectory.path);
            if (indexOfRoot === 0)
                path = path.substr(this.rootDirectory.path.length);
        }

        return path;
    });

    $.RawMethod(false, "unlinkInode", function (inode) {
        var toRemove = [inode.index];

        while (toRemove.length > 0) {
            var toRemoveNext = [];

            for (var i = 0, l = this.inodes.length; i < l; i++) {
                if (toRemove.indexOf(i) >= 0) {
                    this.inodes[i] = null;
                } else {
                    var current = this.inodes[i];

                    if (current) {
                        if (toRemove.indexOf(current.parent) >= 0)
                            toRemoveNext.push(i);
                    }
                }
            }

            toRemove = toRemoveNext;
        }
    });

    $.RawMethod(false, "enumerate", function (nodeType, searchPattern) {
        return this.rootDirectory.enumerate(nodeType, searchPattern);
    });

    $.RawMethod(false, "enumerateFilesRecursive", function (searchPattern) {
        var result = [];

        var step = function (directory) {
            var subdirs = directory.enumerate("directory");
            for (var i = 0; i < subdirs.length; i++)
                step(subdirs[i]);

            var files = directory.enumerate("file", searchPattern);

            for (var i = 0; i < files.length; i++)
                result.push(files[i]);
        };

        step(this.rootDirectory);

        return result;
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

    $.RawMethod(false, "createJunction", function (path, targetObject, allowExisting) {
        path = this.normalizePath(path);

        while (path[path.length - 1] === "/")
            path = path.substr(0, path.length - 1);

        var pieces = path.split("/"), containingDirectory = null, containingPath = null;

        for (var i = 0, l = pieces.length - 1; i < l; i++) {
            containingPath = pieces.slice(0, i).join("/") + "/";

            containingDirectory = this.rootDirectory.resolvePath(containingPath);
            containingDirectory.createDirectory(pieces[i], true);
        }

        containingPath = pieces.slice(0, pieces.length - 1).join("/");
        containingDirectory = this.rootDirectory.resolvePath(containingPath);
        return containingDirectory.createJunction(pieces[pieces.length - 1], targetObject, allowExisting);
    });

    $.RawMethod(false, "createFile", function (path, allowExisting, createParentDirectory) {
        path = this.normalizePath(path);

        var lastSlash = path.lastIndexOf("/"), parentDirectory, fileName;
        if (lastSlash >= 0) {
            if (createParentDirectory)
                parentDirectory = this.createDirectory(path.substr(0, lastSlash));
            else
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

    $.RawMethod(false, "deleteFileBytes", function (name) {
        throw new Error("Not implemented");
    });

    $.RawMethod(false, "getFileBytes", function (name) {
        throw new Error("Not implemented");
    });

    $.RawMethod(false, "setFileBytes", function (name, value) {
        throw new Error("Not implemented");
    });

    $.RawMethod(false, "toString", function () {
        return "<Virtual Storage Volume '" + this.name + "'>";
    });
});