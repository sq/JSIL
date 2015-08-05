// HACK: Nasty compatibility shim for JS Error <-> C# Exception
Error.prototype.get_Message = function () {
    return String(this);
};

Error.prototype.get_StackTrace = function () {
    return this.stack || "";
};