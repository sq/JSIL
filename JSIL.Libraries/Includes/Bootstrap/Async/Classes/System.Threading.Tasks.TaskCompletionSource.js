JSIL.ImplementExternals("System.Threading.Tasks.TaskCompletionSource`1", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [], [])),
    function _ctor() {
      this.task = new (System.Threading.Tasks.Task$b1.Of(System.Threading.Tasks.TaskCompletionSource$b1.TResult.get(this)))();
    }
  );

  $.Method({ Static: false, Public: true }, "get_Task",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.TaskCompletionSource`1")]), [], [])),
    function get_Task() {
      return this.task;
    }
  );

  $.Method({ Static: false, Public: true }, "TrySetResult",
    (new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.TaskCompletionSource`1")], [])),
    function TrySetResult(result) {
      if (this.task.IsCompleted)
        return false;

      this.task.result = result;
      this.task.SetComplete();
      return true;
    }
  );

  $.Method({ Static: false, Public: true }, "TrySetCanceled",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function TrySetCanceled() {
      if (this.task.IsCompleted)
        return false;

      this.task.SetCancel();
      return true;
    }
  );

  $.Method({ Static: false, Public: true }, "TrySetException",
    (new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Exception")], [])),
    function TrySetException(exception) {
      if (this.task.IsCompleted)
        return false;

      this.task.SetException(exception);
      return true;
    }
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.Object"),
  Name: "System.Threading.Tasks.TaskCompletionSource`1",
  IsPublic: true,
  IsReferenceType: true,
  GenericParameters: ["TResult"],
  MaximumConstructorArguments: 2,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "get_Task",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.TaskCompletionSource`1")]), [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "TrySetCanceled",
    new JSIL.MethodSignature($.Boolean, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "TrySetException",
    new JSIL.MethodSignature($.Boolean, [$jsilcore.TypeRef("System.Exception")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "TrySetResult",
    new JSIL.MethodSignature($.Boolean, [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.TaskCompletionSource`1")], [])
  );

  $.Property({ Static: false, Public: true }, "Task", $jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.TaskCompletionSource`1")]));
});