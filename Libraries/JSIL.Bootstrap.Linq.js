if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core is required");

if (!$jsilcore)  
  throw new Error("JSIL.Core is required");

JSIL.DeclareNamespace("System.Linq");
JSIL.DeclareNamespace("System.Linq.Expressions");

JSIL.MakeClass("System.Object", "JSIL.AbstractEnumerable", true, [], function ($) {
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
    new JSIL.MethodSignature("System.Collections.IEnumerator", []),
    function () {
      if (this._getEnumerator !== null)
        return this._getEnumerator();
      else
        return new JSIL.AbstractEnumerator(this._getNextItem, this._reset, this._dispose);
    }
  );

  $.ImplementInterfaces(
    System.Collections.IEnumerable, System.Collections.Generic.IEnumerable$b1
  );
});

JSIL.ImplementExternals(
  "System.Linq.Enumerable", function ($) {
    $.Method({Static:true , Public:true }, "Any", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        var enumerator = JSIL.GetEnumerator(enumerable);

        try {
          if (enumerator.IEnumerator_MoveNext())
            return true;
        } finally {
          enumerator.IDisposable_Dispose();
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
        var enumerator = JSIL.GetEnumerator(enumerable);
        
        try {
          while (enumerator.IEnumerator_MoveNext()) {
            if (predicate(enumerator.IEnumerator_Current))
              return true;
          }
        } finally {
          enumerator.IDisposable_Dispose();
        }

        return false;
      }
    );

    $.Method({Static:true , Public:true }, "Count", 
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Int32"), [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        var e = JSIL.GetEnumerator(enumerable);
        var result = 0;
        try {
          while (e.IEnumerator_MoveNext())
            result += 1;
        } finally {
          e.IDisposable_Dispose();
        }
        return result;
      }
    );

    $.Method({Static:true , Public:true }, "First", 
      new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("System.Collections.Generic.IEnumerable`1", ["!!0"])], ["TSource"]),
      function (T, enumerable) {
        var enumerator = JSIL.GetEnumerator(enumerable);
        try {
          if (enumerator.IEnumerator_MoveNext())
            return enumerator.IEnumerator_Current;
        } finally {
          enumerator.IDisposable_Dispose();
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

        return new JSIL.AbstractEnumerable(
          function getNext (result) {
            var ok = state.enumerator.IEnumerator_MoveNext();
            if (ok)
              result.set(selector(state.enumerator.IEnumerator_Current));

            return ok;
          },
          function reset () {
            state.enumerator = JSIL.GetEnumerator(enumerable);
          },
          function dispose () {
            state.enumerator.IDisposable_Dispose();
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
      function Contains$b1 (TSource, source, item) {
        var enumerator = JSIL.GetEnumerator(source);

        try {
          while (enumerator.IEnumerator_MoveNext()) {
            if (JSIL.ObjectEquals(enumerator.IEnumerator_Current, item))
              return true;
          }
        } finally {
          enumerator.IDisposable_Dispose();
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

      return new JSIL.AbstractEnumerable(
        function getNext (result) {
          var ok = state.enumerator.IEnumerator_MoveNext();

          if (ok)
            result.set(TResult.$Cast(state.enumerator.IEnumerator_Current));

          return ok;
        },
        function reset () {
          state.enumerator = JSIL.GetEnumerator(enumerable);
        },
        function dispose () {
          state.enumerator.IDisposable_Dispose();
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