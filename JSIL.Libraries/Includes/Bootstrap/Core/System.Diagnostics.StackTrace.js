JSIL.ImplementExternals("System.Diagnostics.StackTrace", function ($) {
  var mscorlib = JSIL.GetCorlib();

  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      this.CaptureStackTrace(0, false, null, null);
    }
  );

  $.Method({ Static: false, Public: false }, "CaptureStackTrace",
    (new JSIL.MethodSignature(null, [
          $.Int32, $.Boolean,
          mscorlib.TypeRef("System.Threading.Thread"), mscorlib.TypeRef("System.Exception")
    ], [])),
    function CaptureStackTrace(iSkip, fNeedFileInfo, targetThread, e) {
      // FIXME
      this.frames = [];
    }
  );

  $.Method({ Static: false, Public: true }, "GetFrame",
    (new JSIL.MethodSignature(mscorlib.TypeRef("System.Diagnostics.StackFrame"), [$.Int32], [])),
    function GetFrame(index) {
      // FIXME
      return new System.Diagnostics.StackFrame();
    }
  );

});