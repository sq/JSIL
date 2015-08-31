//? include("../Utils/$jsilcore.InitResizableArray.js");
JSIL.ImplementExternals("System.Collections.Concurrent.ConcurrentQueue`1", function ($) {
    $.Method({ Static: false, Public: true }, ".ctor",
      (JSIL.MethodSignature.Void),
      function _ctor() {
          $jsilcore.InitResizableArray(this, this.T, 16);
          this._size = 0;
      }
    );

    $.Method({ Static: false, Public: true }, "Clear",
      (JSIL.MethodSignature.Void),
      function Clear() {
          this._items.length = this._size = 0;
      }
    );

    $.Method({ Static: false, Public: true }, "TryDequeue",
      (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("JSIL.Reference", [new JSIL.GenericParameter("T", "System.Collections.Concurrent.ConcurrentQueue`1")])], [])),
      function Dequeue(result) {
          if (this._size > 0) {
              var item = this._items.shift();
              this._size -= 1;
              result.set(item);
              return true;
          } else {
              result.set(JSIL.DefaultValue(T));
              return false;
          }
      }
    );

    $.Method({ Static: false, Public: true }, "Enqueue",
      (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Concurrent.ConcurrentQueue`1")], [])),
      function Enqueue(item) {
          this._items.push(item);
          this._size += 1;
      }
    );

    $.Method({ Static: false, Public: true }, "ToArray",
      (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Concurrent.ConcurrentQueue`1"), [], [])),
      function ToArray(item) {
          return this._items.slice(0, this._size);
      }
    );

    $.Method({ Static: false, Public: true }, "get_Count",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function get_Count() {
          return this._size;
      }
    );

    $.Method({ Static: false, Public: true }, "GetEnumerator",
      (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [new JSIL.GenericParameter("T", "System.Collections.Concurrent.ConcurrentQueue`1")]), [], [])),
      function GetEnumerator() {
          return this.$GetEnumerator();
      }
    )
      .Overrides("System.Collections.Generic.IEnumerable`1", "GetEnumerator");

});