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

        Object.defineProperty(
          this, "type", {
              get: function () {
                  return inode.type;
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
            this.inode.metadata.created = JSIL.Host.getFileTime();

        parent.files[this.name.toLowerCase()] = this;
    });

    $.RawMethod(false, "unlink", function () {
        delete this.parent.files[this.name.toLowerCase()];
        this.volume.unlinkInode(this.inode);
        this.volume.deleteFileBytes(this.path);
        this.volume.flush();
    });

    $.RawMethod(false, "readAllBytes", function () {
        var bytes = this.volume.getFileBytes(this.path);

        this.inode.metadata.lastRead = JSIL.Host.getFileTime();

        if (!bytes)
            return JSIL.Array.New(System.Byte, this.inode.metadata.length || 0);

        return bytes;
    });

    $.RawMethod(false, "writeAllBytes", function (bytes) {
        this.volume.setFileBytes(this.path, bytes);

        this.inode.metadata.lastWritten = JSIL.Host.getFileTime();
        this.inode.metadata.length = bytes.length;
    });

    $.RawMethod(false, "toString", function () {
        return "<Virtual File '" + this.path + "' in volume '" + this.volume.name + "'>";
    });
});