JSIL.ImplementExternals("System.Runtime.CompilerServices.TaskAwaiter", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Threading.Tasks.Task")], []),
      function TaskAwaiter__ctor(task) {
        // TODO: Check MemberwiseClone here. We don't define _task as field. Is it work?
        this._task = task;
      }
  );

  $.Method({ Static: false, Public: true }, "get_IsCompleted",
    new JSIL.MethodSignature($.Boolean, [], []),
    function MyTaskAwaiter_get_IsCompleted() {
      return this._task.get_IsCompleted();
    }
  );

  $.Method({ Static: false, Public: true }, "GetResult",
    new JSIL.MethodSignature(null, [], []),
    function GetResult() {
      if (!this._task.get_IsCompleted()) {
        throw new JSIL.ConstructorSignature($jsilcore.TypeRef("System.Exception"), [$jsilcore.TypeRef("System.String")]).Construct("TaskNotCompleted");
      }
      var taskException = this._task.get_Exception();
      if (taskException !== null) {
        throw taskException.get_InnerException();
      }
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "OnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Action")], []),
    function MyTaskAwaiter_OnCompleted(continuation) {
      var continueSignature = new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [$jsilcore.TypeRef("System.Action`1", [$jsilcore.TypeRef("System.Threading.Tasks.Task")])], []);
      continueSignature.CallVirtual("ContinueWith", null, this._task, function (task) { continuation() });
    }
  );
});

JSIL.ImplementExternals("System.Runtime.CompilerServices.TaskAwaiter`1", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Runtime.CompilerServices.TaskAwaiter`1")])], []),
      function TaskAwaiter__ctor(task) {
        this._task = task;
      }
  );

  $.Method({ Static: false, Public: true }, "get_IsCompleted",
    new JSIL.MethodSignature($.Boolean, [], []),
    function MyTaskAwaiter_get_IsCompleted() {
      return this._task.get_IsCompleted();
    }
  );

  $.Method({ Static: false, Public: true }, "GetResult",
    new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Runtime.CompilerServices.TaskAwaiter`1"), [], []),
    function GetResult() {
      if (!this._task.get_IsCompleted()) {
        throw new JSIL.ConstructorSignature($jsilcore.TypeRef("System.Exception"), [$jsilcore.TypeRef("System.String")]).Construct("TaskNotCompleted");
      }
      var taskException = this._task.get_Exception();
      if (taskException !== null) {
        throw taskException.get_InnerException();
      }
      return this._task.get_Result();
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "OnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Action")], []),
    function MyTaskAwaiter_OnCompleted(continuation) {
      var continueSignature = new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [$jsilcore.TypeRef("System.Action`1", [$jsilcore.TypeRef("System.Threading.Tasks.Task")])], []);
      continueSignature.CallVirtual("ContinueWith", null, this._task, function (task) { continuation() });
    }
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.ValueType"),
  Name: "System.Runtime.CompilerServices.TaskAwaiter",
  IsPublic: true,
  IsReferenceType: false,
  MaximumConstructorArguments: 1,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: false }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Threading.Tasks.Task")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "get_IsCompleted",
    new JSIL.MethodSignature($.Boolean, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "GetResult",
    new JSIL.MethodSignature(null, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true, Virtual: true }, "OnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Action")], [])
  );

  $.Property({ Static: false, Public: true }, "IsCompleted", $.Boolean);

  $.ImplementInterfaces(
    /* 0 */ $jsilcore.TypeRef("System.Runtime.CompilerServices.INotifyCompletion")
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.ValueType"),
  Name: "System.Runtime.CompilerServices.TaskAwaiter`1",
  IsPublic: true,
  IsReferenceType: false,
  GenericParameters: ["TResult"],
  MaximumConstructorArguments: 1,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: false }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Runtime.CompilerServices.TaskAwaiter`1")])], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "get_IsCompleted",
    new JSIL.MethodSignature($.Boolean, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "GetResult",
    new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Runtime.CompilerServices.TaskAwaiter`1"), [], [])
  );

  $.ExternalMethod({ Static: false, Public: true, Virtual: true }, "OnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Action")], [])
  );

  $.Property({ Static: false, Public: true }, "IsCompleted", $.Boolean);

  $.ImplementInterfaces(
    /* 0 */ $jsilcore.TypeRef("System.Runtime.CompilerServices.INotifyCompletion")
  );
});