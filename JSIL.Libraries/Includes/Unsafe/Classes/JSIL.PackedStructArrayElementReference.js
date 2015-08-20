JSIL.MakeClass("JSIL.Reference", "JSIL.PackedStructArrayElementReference", true, [], function ($) {
    $.RawMethod(false, ".ctor",
      function PackedStructArrayElementReference_ctor(array, index) {
          this.array = array;
          this.index = index | 0;
      }
    );

    $.RawMethod(false, "get",
      function PackedStructArrayElementReference_Get() {
          return this.array.get_Item(this.index);
      }
    );

    $.RawMethod(false, "set",
      function PackedStructArrayElementReference_Set(value) {
          return this.array.set_Item(this.index, value);
      }
    );

    $.RawMethod(false, "retarget",
      function PackedStructArrayElementReference_Retarget(array, index) {
          this.array = array;
          this.index = index | 0;
          return this;
      }
    );
});