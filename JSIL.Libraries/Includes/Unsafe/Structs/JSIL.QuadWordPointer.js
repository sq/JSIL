JSIL.MakeStruct("JSIL.Pointer", "JSIL.QuadWordPointer", true, [], function ($) {
    $.RawMethod(false, "get",
      function QuadWordPointer_Get() {
          return this.view[this.offsetInElements];
      }
    );

    $.RawMethod(false, "set",
      function QuadWordPointer_Set(value) {
          this.view[this.offsetInElements] = value;
      }
    );

    $.RawMethod(false, "getElement",
      function QuadWordPointer_GetElement(offsetInElements) {
          return this.view[(this.offsetInElements + offsetInElements) | 0];
      }
    );

    $.RawMethod(false, "setElement",
      function QuadWordPointer_SetElement(offsetInElements, value) {
          this.view[(this.offsetInElements + offsetInElements) | 0] = value;
      }
    );

    $.RawMethod(false, "getOffset",
      function QuadWordPointer_GetOffset(offsetInBytes) {
          return this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 3];
      }
    );

    $.RawMethod(false, "setOffset",
      function QuadWordPointer_SetOffset(offsetInBytes, value) {
          this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 3] = value;
      }
    );
});