
JSIL.MakeStruct("JSIL.Pointer", "JSIL.NullPointer", true, [], function ($) {
    $.SetValue("__IsNull__", true);

    function NullPointer_ctor(elementType) {
        if (arguments.length !== 1)
            JSIL.RuntimeError("NullPointer ctor expects (elementType)");

        this.memoryRange = null;
        this.view = null;
        this.offsetInBytes = 0;
        this.shift = 0;
        this.offsetInElements = 0;
        this.elementType = elementType;
    };

    $.RawMethod(false, ".ctor", NullPointer_ctor);

    $.RawMethod(false, "toString",
      function NullPointer_ToString() {
          return "<null ptr>";
      }
    );
});