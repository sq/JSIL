JSIL.MakeClass("System.Object", "JSIL.ObjectInitializer", true, [], function ($) {
    $.RawMethod(false, ".ctor",
      function (newInstance, initializer) {
          this.hasInstance = (newInstance !== null);
          this.instance = newInstance;
          this.initializer = initializer;
      }
    );

    $.RawMethod(false, "Apply",
      function (previousValue) {
          var result = this.hasInstance ? this.instance : previousValue;

          if (result)
              result.__Initialize__(this.initializer);
          else
              JSIL.Host.warning("Object initializer applied to null/undefined!");

          return result;
      }
    );
});