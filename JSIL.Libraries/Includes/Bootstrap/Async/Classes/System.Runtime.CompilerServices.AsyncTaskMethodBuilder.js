JSIL.ImplementExternals("System.Runtime.CompilerServices.AsyncTaskMethodBuilder", function ($) {
  var $TaskCompletionSourceOfObject = function () {
    return ($TaskCompletionSourceOfObject = JSIL.Memoize($jsilcore.System.Threading.Tasks.TaskCompletionSource$b1.Of($jsilcore.System.Object)))();
  };

  var $TrySetExceptionSignature = function () {
    return ($TrySetExceptionSignature = JSIL.Memoize(new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.Exception")])))();
  };

  $.Method({ Static: false, Public: false }, ".ctor",
    (new JSIL.MethodSignature(null, [], [])),
    function _ctor() {
    }
  );

  $.Method({ Static: true, Public: true }, "Create",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Runtime.CompilerServices.AsyncTaskMethodBuilder"), [], [])),
    function Create() {
      return new $jsilcore.System.Runtime.CompilerServices.AsyncTaskMethodBuilder();
    }
  );

  $.Method({ Static: false, Public: true }, "AwaitOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"]),
    function AwaitOnCompleted(TAwaiter, TStateMachine, awaiter, stateMachine) {
      stateMachine = stateMachine.get();

      var completedInterfaceMethod = $jsilcore.System.Runtime.CompilerServices.INotifyCompletion.OnCompleted;
      completedInterfaceMethod.Call(awaiter.get(), null, $jsilcore.System.Action.New(stateMachine, stateMachine.MoveNext));
    }
  );

  $.Method({ Static: false, Public: true }, "AwaitUnsafeOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"]),
    function AwaitOnCompleted(TAwaiter, TStateMachine, awaiter, stateMachine) {
      stateMachine = stateMachine.get();

      var completedInterfaceMethod = $jsilcore.System.Runtime.CompilerServices.INotifyCompletion.OnCompleted;
      completedInterfaceMethod.Call(awaiter.get(), null, $jsilcore.System.Action.New(stateMachine, stateMachine.MoveNext));
    }
  );

  $.Method({ Static: false, Public: true }, "Start",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"])], ["TStateMachine"]),
    function AwaitOnCompleted(TStateMachine, stateMachine) {
      stateMachine.get().MoveNext();
    }
  );

  $.Method({ Static: false, Public: true }, "SetResult",
    new JSIL.MethodSignature(null, [], []),
    function SetResult() {
      return $TaskCompletionSourceOfObject().prototype.TrySetResult.call(this.get_TaskSource(), null);
    }
  );

  $.Method({ Static: false, Public: true }, "SetException",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Exception")], []),
    function SetException(exception) {
      JSIL.Host.warning(exception);
      $TrySetExceptionSignature().Call($TaskCompletionSourceOfObject().prototype, "TrySetException", null, this.get_TaskSource(), exception);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Task",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [], []),
    function get_Task() {
      return $TaskCompletionSourceOfObject().prototype.get_Task.call(this.get_TaskSource());
    }
  );

  $.RawMethod(false, "get_TaskSource",
    function get_TaskSource() {
      if (!this._taskSource) {
        this._taskSource = new ($TaskCompletionSourceOfObject())();
      }
      return this._taskSource;
    }
  );
});

JSIL.ImplementExternals("System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1", function ($) {
  var $TrySetExceptionSignature = function () {
    return ($TrySetExceptionSignature = JSIL.Memoize(new JSIL.MethodSignature($jsilcore.TypeRef("System.Boolean"), [$jsilcore.TypeRef("System.Exception")])))();
  };

  $.Method({ Static: false, Public: false }, ".ctor",
    (new JSIL.MethodSignature(null, [], [])),
    function _ctor() {
    }
  );

  $.Method({ Static: true, Public: true }, "Create",
    (new JSIL.MethodSignature($.Type, [], [])),
    function Create() {
      return new ($jsilcore.System.Runtime.CompilerServices.AsyncTaskMethodBuilder$b1.Of(this.TResult))();
    }
  );

  $.Method({ Static: false, Public: true }, "AwaitOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"]),
    function AwaitOnCompleted(TAwaiter, TStateMachine, awaiter, stateMachine) {
      stateMachine = stateMachine.get();

      var completedInterfaceMethod = $jsilcore.System.Runtime.CompilerServices.INotifyCompletion.OnCompleted;
      completedInterfaceMethod.Call(awaiter.get(), null, $jsilcore.System.Action.New(stateMachine, stateMachine.MoveNext));
    }
  );

  $.Method({ Static: false, Public: true }, "AwaitUnsafeOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"]),
    function AwaitOnCompleted(TAwaiter, TStateMachine, awaiter, stateMachine) {
      stateMachine = stateMachine.get();

      var completedInterfaceMethod = $jsilcore.System.Runtime.CompilerServices.INotifyCompletion.OnCompleted;
      completedInterfaceMethod.Call(awaiter.get(), null, $jsilcore.System.Action.New(stateMachine, stateMachine.MoveNext));
    }
  );

  $.Method({ Static: false, Public: true }, "Start",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"])], ["TStateMachine"]),
    function AwaitOnCompleted(TStateMachine, stateMachine) {
      stateMachine.get().MoveNext();
    }
  );

  $.Method({ Static: false, Public: true }, "SetResult",
    new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TResult", "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1")], []),
    function SetResult(result) {
      var taskCompletionSource = $jsilcore.System.Threading.Tasks.TaskCompletionSource$b1.Of(this.TResult);
      return taskCompletionSource.prototype.TrySetResult.call(this.get_TaskSource(), result);
    }
  );

  $.Method({ Static: false, Public: true }, "SetException",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Exception")], []),
    function SetException(exception) {
      JSIL.Host.warning(exception);
      var taskCompletionSource = $jsilcore.System.Threading.Tasks.TaskCompletionSource$b1.Of(this.TResult);
      $TrySetExceptionSignature().Call(taskCompletionSource.prototype, "TrySetException", null, this.get_TaskSource(), exception);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Task",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1")]), [], []),
    function get_Task() {
      var taskCompletionSource = $jsilcore.System.Threading.Tasks.TaskCompletionSource$b1.Of(this.TResult);
      return taskCompletionSource.prototype.get_Task.call(this.get_TaskSource());
    }
  );

  $.RawMethod(false, "get_TaskSource",
    function get_TaskSource() {
      if (!this._taskSource) {
        this._taskSource = new ($jsilcore.System.Threading.Tasks.TaskCompletionSource$b1.Of(this.TResult))();
      }
      return this._taskSource;
    }
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.ValueType"),
  Name: "System.Runtime.CompilerServices.AsyncTaskMethodBuilder",
  IsPublic: true,
  IsReferenceType: false,
  MaximumConstructorArguments: 0,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: true }, "AwaitOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"])
  );

  $.ExternalMethod({ Static: false, Public: true }, "AwaitUnsafeOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"])
  );

  $.ExternalMethod({ Static: true, Public: true }, "Create",
    new JSIL.MethodSignature($.Type, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "get_Task",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "SetException",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Exception")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "SetResult",
    new JSIL.MethodSignature(null, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "SetStateMachine",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Runtime.CompilerServices.IAsyncStateMachine")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "Start",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"])], ["TStateMachine"])
  );

  $.Property({ Static: false, Public: true }, "Task", $jsilcore.TypeRef("System.Threading.Tasks.Task"));
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.ValueType"),
  Name: "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1",
  IsPublic: true,
  IsReferenceType: false,
  GenericParameters: ["TResult"],
  MaximumConstructorArguments: 0,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: true }, "AwaitOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"])
  );

  $.ExternalMethod({ Static: false, Public: true }, "AwaitUnsafeOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"])
  );

  $.ExternalMethod({ Static: true, Public: true }, "Create",
    new JSIL.MethodSignature($.Type, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "get_Task",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1")]), [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "SetException",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Exception")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "SetResult",
    new JSIL.MethodSignature(null, [new JSIL.GenericParameter("TResult", "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "SetStateMachine",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Runtime.CompilerServices.IAsyncStateMachine")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "Start",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"])], ["TStateMachine"])
  );

  $.ExternalMethod({ Static: false, Public: false, Virtual: true }, "System.Runtime.CompilerServices.IAsyncMethodBuilder.PreBoxInitialization",
    new JSIL.MethodSignature(null, [], [])
  );

  $.Property({ Static: false, Public: true }, "Task", $jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1")]));
});