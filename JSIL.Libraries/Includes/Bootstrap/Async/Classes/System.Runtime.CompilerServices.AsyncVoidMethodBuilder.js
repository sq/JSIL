JSIL.ImplementExternals("System.Runtime.CompilerServices.AsyncVoidMethodBuilder", function ($) {
  $.Method({ Static: false, Public: false }, ".ctor",
    (new JSIL.MethodSignature(null, [], [])),
    function _ctor() {
    }
  );

  $.Method({ Static: true, Public: true }, "Create",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Runtime.CompilerServices.AsyncVoidMethodBuilder"), [], [])),
    function Create() {
      return new $jsilcore.System.Runtime.CompilerServices.AsyncVoidMethodBuilder();
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
    }
  );

  $.Method({ Static: false, Public: true }, "SetException",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Exception")], []),
    function SetException(exception) {
      JSIL.Host.warning(exception);
    }
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.ValueType"),
  Name: "System.Runtime.CompilerServices.AsyncVoidMethodBuilder",
  IsPublic: true,
  IsReferenceType: false,
  MaximumConstructorArguments: 1,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: false }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Threading.SynchronizationContext")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "AwaitOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"])
  );

  $.ExternalMethod({ Static: false, Public: true }, "AwaitUnsafeOnCompleted",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", ["!!0"]), $jsilcore.TypeRef("JSIL.Reference", ["!!1"])], ["TAwaiter", "TStateMachine"])
  );

  $.ExternalMethod({ Static: true, Public: true }, "Create",
    new JSIL.MethodSignature($.Type, [], [])
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
});