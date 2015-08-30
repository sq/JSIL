JSIL.MakeClass("JSIL.Reference", "JSIL.BoxedVariable", true, [], function ($) {
    $.RawMethod(false, ".ctor",
      function BoxedVariable_ctor(value) {
          this.$value = value;
      }
    );

    $.RawMethod(false, "get",
      function BoxedVariable_Get() {
          return this.$value;
      }
    );

    $.RawMethod(false, "set",
      function BoxedVariable_Set(value) {
          return this.$value = value;
      }
    );
});