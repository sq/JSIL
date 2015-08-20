JSIL.MakeStruct("JSIL.Pointer", "JSIL.BytePointer", true, [], function ($) {
    $.RawMethod(false, "get",
      function BytePointer_Get() {
          return this.view[this.offsetInBytes];
      }
    );

    $.RawMethod(false, "set",
      function BytePointer_Set(value) {
          this.view[this.offsetInBytes] = value;
      }
    );

    $.RawMethod(false, "getElement",
      function BytePointer_GetElement(offsetInElements) {
          return this.view[(this.offsetInBytes + offsetInElements) | 0];
      }
    );

    $.RawMethod(false, "setElement",
      function BytePointer_SetElement(offsetInElements, value) {
          this.view[(this.offsetInBytes + offsetInElements) | 0] = value;
      }
    );

    $.RawMethod(false, "getOffset",
      function BytePointer_GetOffset(offsetInBytes) {
          return this.view[(this.offsetInBytes + offsetInBytes) | 0];
      }
    );

    $.RawMethod(false, "setOffset",
      function BytePointer_SetOffset(offsetInBytes, value) {
          this.view[(this.offsetInBytes + offsetInBytes) | 0] = value;
      }
    );
});