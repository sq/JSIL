(function JsObjectHelpers$Members() {
  var $, $thisType;

  function JsObjectHelpers_As$b1(T, obj) {
    return T.$As(obj);
  };

  function JsObjectHelpers_AssumeType$b1(T, obj) {
    return obj;
  };

  function JsObjectHelpers_Call(target, key) {
    return target[key]();
  };

  function JsObjectHelpers_Call$b1(TArg1, target, key, arg1) {
    return target[key](arg1);
  };

  function JsObjectHelpers_Call$b2(TArg1, TArg2, target, key, arg1, arg2) {
    return target[key](arg1, arg2);
  };

  function JsObjectHelpers_Call$b3(TArg1, TArg2, TArg3, target, key, arg1, arg2, arg3) {
    return target[key](arg1, arg2, arg3);
  };

  function JsObjectHelpers_Call$b4(TArg1, TArg2, TArg3, TArg4, target, key, arg1, arg2, arg3, arg4) {
    return target[key](arg1, arg2, arg3, arg4);
  };

  function JsObjectHelpers_Call$b5(TArg1, TArg2, TArg3, TArg4, TArg5, target, key, arg1, arg2, arg3, arg4, arg5) {
    return target[key](arg1, arg2, arg3, arg4, arg5);
  };

  function JsObjectHelpers_Call$b6(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, target, key, arg1, arg2, arg3, arg4, arg5, arg6) {
    return target[key](arg1, arg2, arg3, arg4, arg5, arg6);
  };

  function JsObjectHelpers_Call$b7(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, target, key, arg1, arg2, arg3, arg4, arg5, arg6, arg7) {
    return target[key](arg1, arg2, arg3, arg4, arg5, arg6, arg7);
  };

  function JsObjectHelpers_Call$b8(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, target, key, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
    return target[key](arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
  };

  function JsObjectHelpers_Cast$b1(T, obj) {
    return T.$Cast(obj);
  };

  function JsObjectHelpers_Delete(target, key) {
    delete target[key];
  };

  function JsObjectHelpers_Get(target, key) {
    return target[key];
  };

  function JsObjectHelpers_In(target, key) {
    return key in target;
  };

  function JsObjectHelpers_Invoke(target) {
    return target();
  };

  function JsObjectHelpers_Invoke$b1(TArg1, target, arg1) {
    return target(arg1);
  };

  function JsObjectHelpers_Invoke$b2(TArg1, TArg2, target, arg1, arg2) {
    return target(arg1, arg2);
  };

  function JsObjectHelpers_Invoke$b3(TArg1, TArg2, TArg3, target, arg1, arg2, arg3) {
    return target(arg1, arg2, arg3);
  };

  function JsObjectHelpers_Invoke$b4(TArg1, TArg2, TArg3, TArg4, target, arg1, arg2, arg3, arg4) {
    return target(arg1, arg2, arg3, arg4);
  };

  function JsObjectHelpers_Invoke$b5(TArg1, TArg2, TArg3, TArg4, TArg5, target, arg1, arg2, arg3, arg4, arg5) {
    return target(arg1, arg2, arg3, arg4, arg5);
  };

  function JsObjectHelpers_Invoke$b6(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, target, arg1, arg2, arg3, arg4, arg5, arg6) {
    return target(arg1, arg2, arg3, arg4, arg5, arg6);
  };

  function JsObjectHelpers_Invoke$b7(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, target, arg1, arg2, arg3, arg4, arg5, arg6, arg7) {
    return target(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
  };

  function JsObjectHelpers_Invoke$b8(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, target, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
    return target(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
  };

  function JsObjectHelpers_Is$b1(T, obj) {
    return T.$Is(obj);
  };

  function JsObjectHelpers_New(target) {
    return new target();
  };

  function JsObjectHelpers_New$b1(TArg1, target, arg1) {
    return new target(arg1);
  };

  function JsObjectHelpers_New$b2(TArg1, TArg2, target, arg1, arg2) {
    return new target(arg1, arg2);
  };

  function JsObjectHelpers_New$b3(TArg1, TArg2, TArg3, target, arg1, arg2, arg3) {
    return new target(arg1, arg2, arg3);
  };

  function JsObjectHelpers_New$b4(TArg1, TArg2, TArg3, TArg4, target, arg1, arg2, arg3, arg4) {
    return new target(arg1, arg2, arg3, arg4);
  };

  function JsObjectHelpers_New$b5(TArg1, TArg2, TArg3, TArg4, TArg5, target, arg1, arg2, arg3, arg4, arg5) {
    return new target(arg1, arg2, arg3, arg4, arg5);
  };

  function JsObjectHelpers_New$b6(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, target, arg1, arg2, arg3, arg4, arg5, arg6) {
    return new target(arg1, arg2, arg3, arg4, arg5, arg6);
  };

  function JsObjectHelpers_New$b7(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, target, arg1, arg2, arg3, arg4, arg5, arg6, arg7) {
    return new target(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
  };

  function JsObjectHelpers_New$b8(TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, target, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
    return new target(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
  };

  function JsObjectHelpers_Set$b1(TValue, target, key, value) {
    target[key] = value;
  };

  JSIL.MakeStaticClass("JSIL.JsObjectHelpers", true, [], function ($ib) {
    $ = $ib;

    $.Method({ Static: true, Public: true }, "Adapt",
      new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("JSIL.JsObject")], ["T"]),
      JsObjectHelpers_Adapt$b1
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "As",
      new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("JSIL.JsObject")], ["T"]),
      JsObjectHelpers_As$b1
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "AssumeType",
      new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("JSIL.JsObject")], ["T"]),
      JsObjectHelpers_AssumeType$b1
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Call",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [$jsilcore.TypeRef("JSIL.JsObject+JsFunction"), $.String]),
      JsObjectHelpers_Call
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Call",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), $.String,
          "!!0"
      ], ["TArg1"]),
      JsObjectHelpers_Call$b1
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Call",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), $.String,
          "!!0", "!!1"
      ], ["TArg1", "TArg2"]),
      JsObjectHelpers_Call$b2
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Call",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), $.String,
          "!!0", "!!1",
          "!!2"
      ], ["TArg1", "TArg2", "TArg3"]),
      JsObjectHelpers_Call$b3
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Call",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), $.String,
          "!!0", "!!1",
          "!!2", "!!3"
      ], ["TArg1", "TArg2", "TArg3", "TArg4"]),
      JsObjectHelpers_Call$b4
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Call",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), $.String,
          "!!0", "!!1",
          "!!2", "!!3",
          "!!4"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5"
      ]),
      JsObjectHelpers_Call$b5
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Call",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), $.String,
          "!!0", "!!1",
          "!!2", "!!3",
          "!!4", "!!5"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5", "TArg6"
      ]),
      JsObjectHelpers_Call$b6
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Call",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), $.String,
          "!!0", "!!1",
          "!!2", "!!3",
          "!!4", "!!5",
          "!!6"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5", "TArg6", "TArg7"
      ]),
      JsObjectHelpers_Call$b7
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Call",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), $.String,
          "!!0", "!!1",
          "!!2", "!!3",
          "!!4", "!!5",
          "!!6", "!!7"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5", "TArg6", "TArg7", "TArg8"
      ]),
      JsObjectHelpers_Call$b8
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Cast",
      new JSIL.MethodSignature("!!0", [$jsilcore.TypeRef("JSIL.JsObject")], ["T"]),
      JsObjectHelpers_Cast$b1
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Delete",
      new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.JsObject"), $.String]),
      JsObjectHelpers_Delete
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Get",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [$jsilcore.TypeRef("JSIL.JsObject"), $.String]),
      JsObjectHelpers_Get
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "In",
      new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("JSIL.JsObject"), $.String]),
      JsObjectHelpers_In
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Invoke",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [$jsilcore.TypeRef("JSIL.JsObject+JsFunction")]),
      JsObjectHelpers_Invoke
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Invoke",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [$jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0"], ["TArg1"]),
      JsObjectHelpers_Invoke$b1
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Invoke",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1"
      ], ["TArg1", "TArg2"]),
      JsObjectHelpers_Invoke$b2
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Invoke",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2"
      ], ["TArg1", "TArg2", "TArg3"]),
      JsObjectHelpers_Invoke$b3
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Invoke",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3"
      ], ["TArg1", "TArg2", "TArg3", "TArg4"]),
      JsObjectHelpers_Invoke$b4
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Invoke",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3", "!!4"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5"
      ]),
      JsObjectHelpers_Invoke$b5
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Invoke",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3", "!!4",
          "!!5"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5", "TArg6"
      ]),
      JsObjectHelpers_Invoke$b6
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Invoke",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3", "!!4",
          "!!5", "!!6"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5", "TArg6", "TArg7"
      ]),
      JsObjectHelpers_Invoke$b7
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Invoke",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3", "!!4",
          "!!5", "!!6",
          "!!7"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5", "TArg6", "TArg7", "TArg8"
      ]),
      JsObjectHelpers_Invoke$b8
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Is",
      new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("JSIL.JsObject")], ["T"]),
      JsObjectHelpers_Is$b1
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "New",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [$jsilcore.TypeRef("JSIL.JsObject+JsFunction")]),
      JsObjectHelpers_New
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "New",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [$jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0"], ["TArg1"]),
      JsObjectHelpers_New$b1
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "New",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1"
      ], ["TArg1", "TArg2"]),
      JsObjectHelpers_New$b2
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "New",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2"
      ], ["TArg1", "TArg2", "TArg3"]),
      JsObjectHelpers_New$b3
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "New",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3"
      ], ["TArg1", "TArg2", "TArg3", "TArg4"]),
      JsObjectHelpers_New$b4
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "New",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3", "!!4"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5"
      ]),
      JsObjectHelpers_New$b5
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "New",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3", "!!4",
          "!!5"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5", "TArg6"
      ]),
      JsObjectHelpers_New$b6
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "New",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3", "!!4",
          "!!5", "!!6"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5", "TArg6", "TArg7"
      ]),
      JsObjectHelpers_New$b7
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "New",
      new JSIL.MethodSignature($jsilcore.TypeRef("JSIL.JsObject"), [
          $jsilcore.TypeRef("JSIL.JsObject+JsFunction"), "!!0",
          "!!1", "!!2",
          "!!3", "!!4",
          "!!5", "!!6",
          "!!7"
      ], [
          "TArg1", "TArg2", "TArg3", "TArg4",
          "TArg5", "TArg6", "TArg7", "TArg8"
      ]),
      JsObjectHelpers_New$b8
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

    $.Method({ Static: true, Public: true }, "Set",
      new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("JSIL.JsObject"), $.String,
          "!!0"
      ], ["TValue"]),
      JsObjectHelpers_Set$b1
    )
      .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));


    return function (newThisType) { $thisType = newThisType; };
  })
    .Attribute($jsilcore.TypeRef("System.Runtime.CompilerServices.ExtensionAttribute"));

})();