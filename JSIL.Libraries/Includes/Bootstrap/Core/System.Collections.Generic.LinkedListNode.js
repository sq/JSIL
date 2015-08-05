JSIL.ImplementExternals("System.Collections.Generic.LinkedListNode`1", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedListNode`1")], [])),
    function _ctor(value) {
      this._list = null;
      this._value = value;
      this._previous = null;
      this._next = null;
    }
  );

  $.Method({ Static: false, Public: false }, ".ctor",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.LinkedList`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedListNode`1")]), new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedListNode`1")], [])),
    function _ctor(list, value) {
      this._list = list;
      this._value = value;
      this._previous = null;
      this._next = null;
    }
  );

  $.Method({ Static: false, Public: true }, "get_List",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedList`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedListNode`1")]), [], [])),
    function get_List() {
      return this._list;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Next",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedListNode`1")]), [], [])),
    function get_Next() {
      return this._next;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Previous",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedListNode`1")]), [], [])),
    function get_Previous() {
      return this._previous;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Value",
    (new JSIL.MethodSignature(new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedListNode`1"), [], [])),
    function get_Value() {
      return this._value;
    }
  );

  $.Method({ Static: false, Public: true }, "set_Value",
    (new JSIL.MethodSignature(null, [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedListNode`1")], [])),
    function set_Value(value) {
      this._value = value;
    }
  );

});