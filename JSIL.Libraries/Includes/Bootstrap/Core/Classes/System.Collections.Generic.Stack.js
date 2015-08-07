//? include("../Utils/$jsilcore.InitResizableArray.js");
JSIL.ImplementExternals("System.Collections.Generic.Stack`1", function ($) {
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

  $.Method({ Static: false, Public: true }, "get_Count",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Count() {
      return this._size;
    }
  );

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    (new JSIL.MethodSignature(system.TypeRef("System.Collections.Generic.Stack`1+Enumerator", [new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1")]), [], [])),
    function GetEnumerator() {
      return this.$GetEnumerator();
    }
  );

  $.Method({ Static: false, Public: true }, "Peek",
    (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1"), [], [])),
    function Peek() {
      if (this._size <= 0)
        throw new System.InvalidOperationException("Stack is empty");

      return this._items[this._size - 1];
    }
  );

  $.Method({ Static: false, Public: true }, "Pop",
    (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1"), [], [])),
    function Pop() {
      var result = this._items.pop();
      this._size -= 1;

      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "Push",
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1")], [])),
    function Push(item) {
      this._items.push(item)
      this._size += 1;
    }
  );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Collections.Generic.Stack`1", true, ["T"], function ($) {
  $.Property({ Public: true, Static: false }, "Count");

  $.ImplementInterfaces(
		$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.Stack`1")]),
    "System.Collections.IEnumerable"
	);
});
//? }