JSIL.MakeClass($jsilcore.System.Object, "VirtualJunction", true, [], function ($) {
    $.RawMethod(false, ".ctor", function (volume, parent, name, targetObject) {
        this.volume = volume;
        this.parent = parent;
        this.target = targetObject;
        this.inode = null;
        this.name = name;

        JSIL.SetValueProperty(
          this, "path",
          parent ?
            parent.path + (this.name + "/") :
            this.name
        );

        if (parent)
            parent.directories[this.name.toLowerCase()] = this;
    });

    $.RawMethod(false, "getFile", function (name) {
        return this.target.getFile(name);
    });

    $.RawMethod(false, "getDirectory", function (name) {
        return this.target.getDirectory(name);
    });

    $.RawMethod(false, "createFile", function (name, allowExisting) {
        return this.target.createFile(name, allowExisting);
    });

    $.RawMethod(false, "createDirectory", function (name, allowExisting) {
        return this.target.createDirectory(name, allowExisting);
    });

    $.RawMethod(false, "resolvePath", function (path, throwOnFail) {
        return this.target.resolvePath(path, throwOnFail);
    });

    $.RawMethod(false, "enumerate", function (nodeType, searchPattern) {
        return this.target.enumerate(nodeType, searchPattern);
    });

    $.RawMethod(false, "toString", function () {
        return "<Virtual Junction '" + this.path + "' in volume '" + this.volume.name + "' pointing to '" + this.target.path + "'>";
    });
});