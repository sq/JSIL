if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.DeclareNamespace("System.Linq");
JSIL.DeclareNamespace("System.Linq.Expressions");

JSIL.MakeClass("System.Object", "JSIL.AbstractEnumerable", true, ["T"], function ($) {
  var T = new JSIL.GenericParameter("T", "JSIL.AbstractEnumerable");

  $.Method({Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [JSIL.AnyType, JSIL.AnyType, JSIL.AnyType]),
    function (getNextItem, reset, dispose) {
      if (arguments.length === 1) {
        this._getEnumerator = getNextItem;
      } else {
        this._getEnumerator = null;
        this._getNextItem = getNextItem;
        this._reset = reset;
        this._dispose = dispose;
      }
    }
  );

  $.Method({Static: false, Public: true }, "GetEnumerator",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerator`1", [T]), []),
    function () {
      if (this._getEnumerator !== null)
        return this._getEnumerator();
      else
        return new (JSIL.AbstractEnumerator.Of(this.T))(this._getNextItem, this._reset, this._dispose);
    }
  );

  $.ImplementInterfaces(
    /* 0 */ $jsilcore.TypeRef("System.Collections.IEnumerable"), 
    /* 1 */ $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [T])
  );
});

JSIL.ImplementExternals(
  "System.Linq.Enumerable", function ($) {
    $.Method({Static:true , Public:true }, "Any", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        var enumerator = JSIL.GetEnumerator(enumerable, T);

        var moveNext = System.Collections.IEnumerator.MoveNext;

        try {
          if (moveNext.Call(enumerator))
            return true;
        } finally {
          JSIL.Dispose(enumerator);
        }

        return false;
      }
    );

    $.Method({Static:true , Public:true }, "Any", 
      new JSIL.MethodSignature(
        $jsilcore.TypeRef("System.Boolean"), 
        [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $jsilcore.TypeRef("System.Func`2", ["!!0", $jsilcore.TypeRef("System.Boolean")])], 
        ["TSource"]
      ),
      function (T, enumerable, predicate) {
        var enumerator = JSIL.GetEnumerator(enumerable, T);
        
        var tIEnumerator = System.Collections.Generic.IEnumerator$b1.Of(T);
        var moveNext = System.Collections.IEnumerator.MoveNext;
        var get_Current = tIEnumerator.get_Current;
        
        try {
          while (moveNext.Call(enumerator)) {
            if (predicate(get_Current.Call(enumerator)))
              return true;
          }
        } finally {
          JSIL.Dispose(enumerator);
        }

        return false;
      }
    );

    $.Method({Static:true , Public:true }, "Count", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        var enumerator = JSIL.GetEnumerator(enumerable, T);

        var moveNext = System.Collections.IEnumerator.MoveNext;

        var result = 0;
        try {
          while (moveNext.Call(enumerator))
            result += 1;
        } finally {
          JSIL.Dispose(enumerator);
        }
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "First", 
      new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        var enumerator = JSIL.GetEnumerator(enumerable, T);

        var tIEnumerator = System.Collections.Generic.IEnumerator$b1.Of(T);
        var moveNext = System.Collections.IEnumerator.MoveNext;
        var get_Current = tIEnumerator.get_Current;

        try {
          if (moveNext.Call(enumerator))
            return get_Current.Call(enumerator);
        } finally {
          JSIL.Dispose(enumerator);
        }

        throw new System.Exception("Enumerable contains no items");
      }
    );

    $.Method({Static:true , Public:true }, "Select", 
      new JSIL.MethodSignature(
        $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!1"]), 
        [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $jsilcore.TypeRef("System.Func`2", ["!!0", "!!1"])], 
        ["TSource", "TResult"]
      ),
      function (TSource, TResult, enumerable, selector) {
        var state = {};

        var tIEnumerator = System.Collections.Generic.IEnumerator$b1.Of(TSource);
        var moveNext = System.Collections.IEnumerator.MoveNext;
        var get_Current = tIEnumerator.get_Current;

        return new (JSIL.AbstractEnumerable.Of(TResult))(
          function getNext (result) {
            var ok = moveNext.Call(state.enumerator);
            if (ok)
              result.set(selector(get_Current.Call(state.enumerator)));

            return ok;
          },
          function reset () {
            state.enumerator = JSIL.GetEnumerator(enumerable);
          },
          function dispose () {
            JSIL.Dispose(state.enumerator);
          }
        );
      }
    );
    
    $.Method({Static:true , Public:true }, "ToArray", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", ["!!0"]), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        return JSIL.EnumerableToArray(enumerable);
      }
    );

    $.Method({Static:true , Public:true }, "Contains", 
      (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), "!!0"], ["TSource"])), 
      function Contains$b1 (T, source, item) {
        var enumerator = JSIL.GetEnumerator(source, T);

        var tIEnumerator = System.Collections.Generic.IEnumerator$b1.Of(T);
        var moveNext = System.Collections.IEnumerator.MoveNext;
        var get_Current = tIEnumerator.get_Current;

        try {
          while (moveNext.Call(enumerator)) {
            if (JSIL.ObjectEquals(get_Current.Call(enumerator), item))
              return true;
          }
        } finally {
          JSIL.Dispose(enumerator);
        }

        return false;
      }
    );    

    $.Method({Static: true , Public: true }, "Cast",
      new JSIL.MethodSignature(
        $jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]),
        [$jsilcore.TypeRef("System.Collections.IEnumerable")],
        ["TResult"]
      ),
     function (TResult, enumerable) {
      var state = {};

      var moveNext = System.Collections.IEnumerator.MoveNext;
      var get_Current = System.Collections.IEnumerator.get_Current;

      return new (JSIL.AbstractEnumerable.Of(TResult))(
        function getNext (result) {
          var ok = moveNext.Call(state.enumerator);

          if (ok)
            result.set(TResult.$Cast(get_Current.Call(state.enumerator)));

          return ok;
        },
        function reset () {
          state.enumerator = JSIL.GetEnumerator(enumerable);
        },
        function dispose () {
          JSIL.Dispose(state.enumerator);
        }
      );
     });
    
    $.Method({Static: true , Public: true }, "ToList",
      new JSIL.MethodSignature(
       $jsilcore.TypeRef("System.Collections.Generic.List`1", ["!!0"]),
       [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])],
       ["TSource"]
    ),
    function (TSource, enumerable) {
      var constructor = new JSIL.ConstructorSignature($jsilcore.TypeRef("System.Collections.Generic.List`1", [TSource]), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", [TSource])]);
      return constructor.Construct(enumerable);
    });  

    $.Method({Static:true , Public:true }, "ElementAt", 
      new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $.Int32], ["TSource"]), 
      function ElementAt$b1 (TSource, source, index) {
        // FIXME: Wasteful
        var temp = JSIL.EnumerableToArray(source);
        return temp[index];
      }
    );

    $.Method({Static:true , Public:true }, "ElementAtOrDefault", 
      new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $.Int32], ["TSource"]), 
      function ElementAtOrDefault$b1 (TSource, source, index) {
        // FIXME: Wasteful
        var temp = JSIL.EnumerableToArray(source);
        if ((index >= 0) && (index <= temp.length))
          return temp[index];
        else
          return JSIL.DefaultValue(TSource);
      }
    );

    $.Method({Static:true , Public:true }, "OfType", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), [$jsilcore.TypeRef("System.Collections.IEnumerable")], ["TResult"]), 
      function OfType$b1 (TResult, source) {
        var state = {};

        var moveNext = System.Collections.IEnumerator.MoveNext;
        var get_Current = System.Collections.IEnumerator.get_Current;

        return new (JSIL.AbstractEnumerable.Of(TResult))(
          function getNext (result) {
            while (moveNext.Call(state.enumerator)) {
              var current = get_Current.Call(state.enumerator);

              if (TResult.$Is(current)) {
                result.set(current);
                return true;
              }
            }

            return false;
          },
          function reset () {
            state.enumerator = JSIL.GetEnumerator(source);
          },
          function dispose () {
            JSIL.Dispose(state.enumerator);
          }
        );
      }
    );    
  }
);

JSIL.MakeStaticClass("System.Linq.Enumerable", true, [], function ($) {
  $.ExternalMethod({Static:true , Public:true }, "Any", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"])
  );

  $.ExternalMethod({Static:true , Public:true }, "Any", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $jsilcore.TypeRef("System.Func`2", ["!!0", $jsilcore.TypeRef("System.Boolean")])], ["TSource"])
  );

  $.ExternalMethod({Static:true , Public:true }, "Count", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"])
  );

  $.ExternalMethod({Static:true , Public:true }, "First", 
    new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"])
  );

  $.ExternalMethod({Static:true , Public:true }, "Select", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!1"]), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"]), $jsilcore.TypeRef("System.Func`2", ["!!0", "!!1"])], ["TSource", "TResult"])
  );

  $.ExternalMethod({Static:true , Public:true }, "ToArray", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", ["!!0"]), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"])
  );
});


JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Linq.Expressions.Expression", true, [], function ($) {
  var $thisType = $.publicInterface;

  $.ExternalMethod({Static:true , Public:true }, "Constant", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object], []))
  );

  $.ExternalMethod({Static:true , Public:true }, "Constant", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object, $jsilcore.TypeRef("System.Type")], []))
  );

  $.ExternalMethod({Static:true , Public:true }, "Lambda", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.Expression`1", ["!!0"]), [$jsilcore.TypeRef("System.Linq.Expressions.Expression"), $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Linq.Expressions.ParameterExpression")])], ["TDelegate"]))
  );
});

JSIL.ImplementExternals("System.Linq.Expressions.Expression", function ($) {
  $.Method({Static:true , Public:true }, "Constant", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object], [])), 
    function Constant (value) {
      return new System.Linq.Expressions.ConstantExpression(value);
    }
  );

  $.Method({Static:true , Public:true }, "Constant", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object, $jsilcore.TypeRef("System.Type")], [])), 
    function Constant (value, type) {
      return System.Linq.Expressions.ConstantExpression.Make(value, type);
    }
  );

  $.Method({Static:true , Public:true }, "Lambda", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.Expression`1", ["!!0"]), [$jsilcore.TypeRef("System.Linq.Expressions.Expression"), $jsilcore.TypeRef("System.Array", [$jsilcore.TypeRef("System.Linq.Expressions.ParameterExpression")])], ["TDelegate"])), 
    function Lambda$b1 (TDelegate, body, parameters) {
      var name = null;
      var tailCall = false;
      return new ( System.Linq.Expressions.Expression$b1.Of(TDelegate) )(body, name, tailCall, parameters);
    }
  );
});

JSIL.MakeClass($jsilcore.TypeRef("System.Linq.Expressions.Expression"), "System.Linq.Expressions.ConstantExpression", true, [], function ($) {
  var $thisType = $.publicInterface;

  $.ExternalMethod({Static:true , Public:false}, "Make", 
    (new JSIL.MethodSignature($.Type, [$.Object, $jsilcore.TypeRef("System.Type")], []))
  );
});

JSIL.ImplementExternals("System.Linq.Expressions.ConstantExpression", function ($) {
  $.Method({Static:false, Public:false}, ".ctor", 
    (new JSIL.MethodSignature(null, [$.Object], [])), 
    function _ctor (value) {
      this._value = value;
    }
  );

  $.Method({Static:true , Public:false}, "Make", 
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Linq.Expressions.ConstantExpression"), [$.Object, $jsilcore.TypeRef("System.Type")], [])), 
    function Make (value, type) {
      return new System.Linq.Expressions.ConstantExpression(value);
    }
  );

});

JSIL.MakeClass($jsilcore.TypeRef("System.Linq.Expressions.Expression"), "System.Linq.Expressions.ParameterExpression", true, [], function ($) {
  var $thisType = $.publicInterface;
});

JSIL.MakeClass($jsilcore.TypeRef("System.Linq.Expressions.Expression"), "System.Linq.Expressions.LambdaExpression", true, [], function ($) {
  var $thisType = $.publicInterface;
});

JSIL.MakeClass($jsilcore.TypeRef("System.Linq.Expressions.LambdaExpression"), "System.Linq.Expressions.Expression`1", true, ["TDelegate"], function ($) {
  var $thisType = $.publicInterface;
});

JSIL.ImplementExternals("System.Linq.Expressions.Expression`1", function ($) {
});