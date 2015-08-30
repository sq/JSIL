JSIL.ImplementExternals("System.IO.FileStream", function ($) {
  $.RawMethod(false, "$fromVirtualFile", function (virtualFile, fileMode, autoFlush) {
    System.IO.Stream.prototype._ctor.call(this);

    this._fileName = virtualFile.path;
    this._buffer = JSIL.Array.Clone(virtualFile.readAllBytes());

    this._pos = 0;
    this._length = this._buffer.length;

    this._canRead = true;
    this._canWrite = true;

    this._onClose = function () {
      if (this._modified && this._buffer) {
        var resultBuffer = JSIL.Array.New(System.Byte, this._length);
        JSIL.Array.CopyTo(this._buffer, resultBuffer, 0);

        virtualFile.writeAllBytes(resultBuffer);

        if (autoFlush)
          virtualFile.volume.flush();

        this._buffer = null;
      }
    };

    this.$applyMode(fileMode);
  });
});