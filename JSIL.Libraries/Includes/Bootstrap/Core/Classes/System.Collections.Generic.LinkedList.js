JSIL.ImplementExternals("System.Collections.Generic.LinkedList`1", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    (JSIL.MethodSignature.Void),
    function _ctor() {
      this.Clear();
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")])], [])),
    function _ctor(collection) {
      this.Clear();

      throw new Error('Not implemented');
    }
  );

  var makeNode = function (self, value) {
    var tNode = System.Collections.Generic.LinkedListNode$b1.Of(self.T).__Type__;
    return JSIL.CreateInstanceOfType(tNode, "_ctor", [self, value]);
  };

  var addIntoEmptyImpl = function (self, node) {
    if ((!self._head) && (!self._tail)) {
      node._list = self;
      self._head = self._tail = node;
      self._count = 1;
      return true;
    }

    return false;
  }

  var addBeforeImpl = function (self, beforeNode, node) {
    if (addIntoEmptyImpl(self, node))
      return;

    node._list = self;
    node._next = beforeNode;

    if (beforeNode)
      beforeNode._previous = node;

    if (self._head === beforeNode)
      self._head = node;

    self._count += 1;
  };

  var addAfterImpl = function (self, afterNode, node) {
    if (addIntoEmptyImpl(self, node))
      return;

    node._list = self;
    node._previous = afterNode;

    if (afterNode)
      afterNode._next = node;

    if (self._tail === afterNode)
      self._tail = node;

    self._count += 1;
  };

  var addFirstImpl = function (self, node) {
    addBeforeImpl(self, self._head, node);
  };

  var addLastImpl = function (self, node) {
    addAfterImpl(self, self._tail, node);
  };

  $.Method({ Static: false, Public: true }, "AddAfter",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), [$jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")], [])),
    function AddAfter(node, value) {
      var newNode = makeNode(self, value);
      addAfterImpl(this, node, newNode);
      return newNode;
    }
  );

  $.Method({ Static: false, Public: true }, "AddAfter",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), $jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")])], [])),
    function AddAfter(node, newNode) {
      addAfterImpl(this, node, newNode);
    }
  );

  $.Method({ Static: false, Public: true }, "AddBefore",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), [$jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")], [])),
    function AddBefore(node, value) {
      var newNode = makeNode(self, value);
      addBeforeImpl(this, node, newNode);
      return newNode;
    }
  );

  $.Method({ Static: false, Public: true }, "AddBefore",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), $jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")])], [])),
    function AddBefore(node, newNode) {
      addBeforeImpl(this, node, newNode);
    }
  );

  $.Method({ Static: false, Public: true }, "AddFirst",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")], [])),
    function AddFirst(value) {
      var node = makeNode(this, value);
      addFirstImpl(this, node);
      return node;
    }
  );

  $.Method({ Static: false, Public: true }, "AddFirst",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")])], [])),
    function AddFirst(node) {
      addFirstImpl(this, node);
    }
  );

  $.Method({ Static: false, Public: true }, "AddLast",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")], [])),
    function AddLast(value) {
      var node = makeNode(this, value);
      addLastImpl(this, node);
      return node;
    }
  );

  $.Method({ Static: false, Public: true }, "AddLast",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")])], [])),
    function AddLast(node) {
      addLastImpl(this, node);
    }
  );

  $.Method({ Static: false, Public: true }, "Clear",
    (JSIL.MethodSignature.Void),
    function Clear() {
      this._head = null;
      this._tail = null;
      this._count = 0;
    }
  );

  $.Method({ Static: false, Public: true }, "Contains",
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")], [])),
    function Contains(value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "CopyTo",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), $.Int32], [])),
    function CopyTo(array, index) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "Find",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")], [])),
    function Find(value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "FindLast",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")], [])),
    function FindLast(value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "get_Count",
    (new JSIL.MethodSignature($.Int32, [], [])),
    function get_Count() {
      return this._count;
    }
  );

  $.Method({ Static: false, Public: true }, "get_First",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), [], [])),
    function get_First() {
      return this._head;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Last",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), [], [])),
    function get_Last() {
      return this._tail;
    }
  );

  $.Method({ Static: false, Public: true }, "GetEnumerator",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.LinkedList`1+Enumerator", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")]), [], [])),
    function GetEnumerator() {
      throw new Error('Not implemented');
    }
  );

  $.RawMethod(false, "$removeNode", function Remove_Node(node) {
    if (node._list !== this)
      JSIL.RuntimeError("Node is not a member of this list");

    var previous = node._previous || null;
    var next = node._next || null;

    if (previous)
      previous._next = next;
    if (next)
      next._previous = previous;

    if (this._head === node)
      this._head = next;
    else if (this._tail === node)
      this._tail = previous;

    node._list = null;
    node._count -= 1;
  });

  $.Method({ Static: false, Public: true }, "Remove",
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")], [])),
    function Remove(value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "Remove",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Collections.Generic.LinkedListNode`1", [new JSIL.GenericParameter("T", "System.Collections.Generic.LinkedList`1")])], [])),
    function Remove(node) {
      this.$removeNode(node);
    }
  );

  $.Method({ Static: false, Public: true }, "RemoveFirst",
    (JSIL.MethodSignature.Void),
    function RemoveFirst() {
      this.$removeNode(this._head);
    }
  );

  $.Method({ Static: false, Public: true }, "RemoveLast",
    (JSIL.MethodSignature.Void),
    function RemoveLast() {
      this.$removeNode(this._tail);
    }
  );
});