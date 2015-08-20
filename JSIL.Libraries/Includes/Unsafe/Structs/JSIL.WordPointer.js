
JSIL.MakeStruct("JSIL.Pointer", "JSIL.WordPointer", true, [], function ($) {
    $.RawMethod(false, "get",
      function WordPointer_Get() {
          return this.view[this.offsetInElements];
      }
    );

    $.RawMethod(false, "set",
      function WordPointer_Set(value) {
          this.view[this.offsetInElements] = value;
      }
    );

    $.RawMethod(false, "getElement",
      function WordPointer_GetElement(offsetInElements) {
          return this.view[(this.offsetInElements + offsetInElements) | 0];
      }
    );

    $.RawMethod(false, "setElement",
      function WordPointer_SetElement(offsetInElements, value) {
          this.view[(this.offsetInElements + offsetInElements) | 0] = value;
      }
    );

    $.RawMethod(false, "getOffset",
      function WordPointer_GetOffset(offsetInBytes) {
          return this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 1];
      }
    );

    $.RawMethod(false, "setOffset",
      function WordPointer_SetOffset(offsetInBytes, value) {
          this.view[((this.offsetInBytes + offsetInBytes) | 0) >> 1] = value;
      }
    );
});