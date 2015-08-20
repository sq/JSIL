JSIL.MakeClass("JSIL.Reference", "JSIL.ArrayElementReference", true, [], function ($) {
    $.RawMethod(false, ".ctor",
      function ArrayElementReference_ctor(array, index) {
          this.array = array;
          this.index = index | 0;
      }
    );

    $.RawMethod(false, "get",
      function ArrayElementReference_Get() {
          return this.array[this.index];
      }
    );

    $.RawMethod(false, "set",
      function ArrayElementReference_Set(value) {
          return this.array[this.index] = value;
      }
    );

    $.RawMethod(false, "retarget",
      function ArrayElementReference_Retarget(array, index) {
          this.array = array;
          this.index = index | 0;
          return this;
      }
    );
});