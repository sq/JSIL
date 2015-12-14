JSIL.ImplementExternals("System.Threading.Tasks.Task", function ($) {
  var $AggregateExceptionConstructorSignature = function () {
    return ($AggregateExceptionConstructorSignature = JSIL.Memoize(new JSIL.ConstructorSignature($jsilcore.TypeRef("System.AggregateException"), [$jsilcore.TypeRef("System.String"), $jsilcore.TypeRef("System.Exception")])))();
  };

  // TODO: Find solution to remove closure
  var createTaskCommon = function (self) {
    self.status = System.Threading.Tasks.TaskStatus.Created;
    self.action = null;
    self.exception = null;

    self.ContinueExecution = function () {
      // TODO: init continuationActions with null on ctor
      if (this.continuationActions !== undefined) {
        for (var i in this.continuationActions) {
          this.continuationActions[i](this);
        }
      }
    }

    self.SetComplete = function () {
      this.status = System.Threading.Tasks.TaskStatus.RanToCompletion;
      this.ContinueExecution();
    }

    self.SetCancel = function () {
      this.status = System.Threading.Tasks.TaskStatus.Canceled;
      this.ContinueExecution();
    }

    self.SetException = function (exception) {
      this.status = System.Threading.Tasks.TaskStatus.Faulted;
      this.exception = $AggregateExceptionConstructorSignature().Construct("One or more errors occured.", exception);
      this.ContinueExecution();
    }

    self.RunTask = function () {
      if (this.action !== null) {
        try {
          this.action();
          this.SetComplete();
        } catch (e) {
          this.SetException(e);
        }
      }
    }
  }

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Action")], [])),
    function _ctor(action) {
      createTaskCommon(this);
      this.action = action;
    }
  );

  $.Method({ Static: false, Public: false }, ".ctor",
    (new JSIL.MethodSignature(null, [], [])),
    function _ctor() {
      createTaskCommon(this);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Status",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.TaskStatus"), [], [])),
    function get_Status() {
      return this.status;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Exception",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.AggregateException"), [], [])),
    function get_Exception() {
      return this.exception;
    }
  );

  $.Method({ Static: false, Public: true }, "get_IsCompleted",
    (new JSIL.MethodSignature($.Boolean, [], [])),
    function get_IsCompleted() {
      return (this.status == System.Threading.Tasks.TaskStatus.RanToCompletion
            || this.status == System.Threading.Tasks.TaskStatus.Canceled
            || this.status == System.Threading.Tasks.TaskStatus.Faulted);
    }
  );

  $.Method({ Static: false, Public: true }, "ContinueWith",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [$jsilcore.TypeRef("System.Action`1", [$jsilcore.TypeRef("System.Threading.Tasks.Task")])], [])),
    function ContinueWith(continuationAction) {
      if (this.get_IsCompleted()) {
        continuationAction(this);
        return;
      }

      if (this.continuationActions === undefined) {
        this.continuationActions = [];
      }

      this.continuationActions.push(continuationAction);
    }
  );

  $.Method({ Static: true, Public: true }, "get_Factory",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.TaskFactory"), [], [])),
    function get_Factory() {
      // TODO: Think about caching factory 
      return new System.Threading.Tasks.TaskFactory();
    }
  );

  $.Method({ Static: false, Public: true }, "GetAwaiter",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Runtime.CompilerServices.TaskAwaiter"), [], [])),
    function GetAwaiter() {
      return new $jsilcore.System.Runtime.CompilerServices.TaskAwaiter(this);
    }
  );

  $.Method({ Static: true, Public: true }, "Delay",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [$.Int32], [])),
    function Delay(dueTime) {
      var tcs = new (System.Threading.Tasks.TaskCompletionSource$b1.Of(System.Boolean))();
      setTimeout(function () { tcs.TrySetResult(true); }, dueTime);
      return tcs.Task;
    }
  );
});

JSIL.ImplementExternals("System.Threading.Tasks.Task`1", function ($) {
  var createTaskCommon = function (self) {
    self.$function = null;
    self.RunTask = function () {
      if (this.$function !== null) {
        try {
          this.result = this.$function();
          this.SetComplete();
        } catch (e) {
          this.SetException(e);
        }
      }
    }
  }

  $.Method({ Static: false, Public: true }, "ContinueWith",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [$jsilcore.TypeRef("System.Action`1", [$.Type])], []),
    function ContinueWith(continuationAction) {
      if (this.continuationActions === undefined) {
        this.continuationActions = [];
      }

      this.continuationActions.push(continuationAction);
    }
  );

  $.Method({ Static: false, Public: false }, ".ctor",
    (new JSIL.MethodSignature(null, [], [])),
    function _ctor() {
      System.Threading.Tasks.Task.prototype._ctor.call(this);
      createTaskCommon(this);
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Func`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1")])], [])),
    function _ctor($function) {
      System.Threading.Tasks.Task.prototype._ctor.call(this);
      createTaskCommon(this);
      this.$function = $function;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Result",
    (new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1"), [], [])),
    function get_Result() {
      var taskException = this.get_Exception();
      if (taskException !== null) {
        throw taskException;
      }
      return this.result;
    }
  );

  $.Method({ Static: true, Public: true }, "get_Factory",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.TaskFactory`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1")]), [], [])),
    function get_Factory() {
      return new (System.Threading.Tasks.TaskFactory$b1.Of(System.Threading.Tasks.Task$b1.TResult.get(this)))();
    }
  );

  $.Method({ Static: false, Public: true }, "GetAwaiter",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Runtime.CompilerServices.TaskAwaiter`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1")]), [], [])),
    function GetAwaiter() {
      return new ($jsilcore.System.Runtime.CompilerServices.TaskAwaiter$b1.Of(this.TResult))(this);
    }
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.Object"),
  Name: "System.Threading.Tasks.Task",
  IsPublic: true,
  IsReferenceType: true,
  MaximumConstructorArguments: 8,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: false }, ".ctor",
    new JSIL.MethodSignature(null, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Action")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "ContinueWith",
    new JSIL.MethodSignature($.Type, [$jsilcore.TypeRef("System.Action`1", [$.Type])], [])
  );

  $.ExternalMethod({ Static: true, Public: true }, "Delay",
    new JSIL.MethodSignature($.Type, [$.Int32], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "get_Exception",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.AggregateException"), [], [])
  );

  $.ExternalMethod({ Static: true, Public: true }, "get_Factory",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.TaskFactory"), [], [])
  );

  $.ExternalMethod({ Static: false, Public: true, Virtual: true }, "get_IsCompleted",
    new JSIL.MethodSignature($.Boolean, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "get_Status",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.TaskStatus"), [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "GetAwaiter",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Runtime.CompilerServices.TaskAwaiter"), [], [])
  );

  $.Property({ Static: false, Public: true }, "Exception", $jsilcore.TypeRef("System.AggregateException"));

  $.Property({ Static: false, Public: true, Virtual: true }, "IsCompleted", $.Boolean);

  $.Property({ Static: true, Public: true }, "Factory", $jsilcore.TypeRef("System.Threading.Tasks.TaskFactory"));

  $.Property({ Static: false, Public: true }, "Status", $jsilcore.TypeRef("System.Threading.Tasks.TaskStatus"));
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.Threading.Tasks.Task"),
  Name: "System.Threading.Tasks.Task`1",
  IsPublic: true,
  IsReferenceType: true,
  GenericParameters: ["TResult"],
  MaximumConstructorArguments: 8,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: false }, ".ctor",
    new JSIL.MethodSignature(null, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Func`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1")])], [])
  );

  $.ExternalMethod({ Static: true, Public: true }, "get_Factory",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.TaskFactory`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1")]), [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "get_Result",
    new JSIL.MethodSignature(new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1"), [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "GetAwaiter",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Runtime.CompilerServices.TaskAwaiter`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1")]), [], [])
  );

  $.Property({ Static: false, Public: true }, "Result", new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1"));

  $.GenericProperty({ Static: true, Public: true }, "Factory", $jsilcore.TypeRef("System.Threading.Tasks.TaskFactory`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.Task`1")]));
});