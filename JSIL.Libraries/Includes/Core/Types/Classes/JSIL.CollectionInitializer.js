JSIL.MakeClass("System.Object", "JSIL.CollectionInitializer", true, [], function ($) {
    $.RawMethod(false, ".ctor",
      function () {
          this.values = Array.prototype.slice.call(arguments);
      }
    );

    $.RawMethod(false, "Apply",
      function (previousValue) {
          JSIL.ApplyCollectionInitializer(previousValue, this.values);

          return previousValue;
      }
    );
});