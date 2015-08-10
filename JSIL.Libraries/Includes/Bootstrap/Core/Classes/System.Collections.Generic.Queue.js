//? include("../Utils/$jsilcore.InitResizableArray.js");
JSIL.ImplementExternals("System.Collections.Generic.Queue`1", function ($) {
  var system = JSIL.GetAssembly("System", true);

  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      $jsilcore.InitResizableArray(this, this.T, 16);
      this._size = 0;
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.Int32], [])),
    function _ctor(capacity) {
      $jsilcore.InitResizableArray(this, this.T, capacity);
      this._size = 0;
    }
  );

  $.Method({ Static: false, Public: true }, "Clear",
    (JSIL.MethodSignature.Void),
    function Clear() {
      this._items.length = this._size = 0;
    }
  );

  $.Method({ Static: false, Public: true }, "Dequeue",
    (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.Queue`1"), [], [])),
    function Dequeue() {
      var result = this._items.shift();
      this._size -= 1;
      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "Enqueue",
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Generic.Queue`1")], [])),
    function Enqueue(item) {
      this._items.push(item);
      this._size += 1;
    }
  );

  $.Method({ Static: false, Public: true }, "Contains",
    new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.Queue`1")], []),
    function Contains(value) {
      return JSIL.Array.IndexOf(this._items, 0, this._items.length, value) >= 0;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Count",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Count() {
      return this._size;
    }
  );

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    (new JSIL.MethodSignature(system.TypeRef("System.Collections.Generic.Queue`1+Enumerator", [new JSIL.GenericParameter("T", "System.Collections.Generic.Queue`1")]), [], [])),
    function GetEnumerator() {
      return this.$GetEnumerator();
    }
  );

});