
JSIL.ImplementExternals("System.Threading.Tasks.TaskFactory", function ($) {
  $.Method({ Static: false, Public: true }, "StartNew",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [$jsilcore.TypeRef("System.Action")], [])),
    function StartNew(action) {
      var task = new System.Threading.Tasks.Task(action);
      task.RunTask();
      return task;
    }
  );

  $.Method({ Static: false, Public: true }, "StartNew",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task`1", ["!!0"]), [$jsilcore.TypeRef("System.Func`1", ["!!0"])], ["TResult"])),
    function StartNew$b1(TResult, $function) {
      var task = new (System.Threading.Tasks.Task$b1.Of(TResult.__PublicInterface__))();
      task.$function = $function;
      task.RunTask();
      return task;
    }
  );
});

JSIL.ImplementExternals("System.Threading.Tasks.TaskFactory`1", function ($) {
  $.Method({ Static: false, Public: true }, "StartNew",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.TaskFactory`1")]), [$jsilcore.TypeRef("System.Func`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.TaskFactory`1")])], [])),
    function StartNew($function) {
      var task = new (System.Threading.Tasks.Task$b1.Of(System.Threading.Tasks.TaskFactory$b1.TResult.get(this)))($function);
      task.RunTask();
      return task;
    }
  );
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.Object"),
  Name: "System.Threading.Tasks.TaskFactory",
  IsPublic: true,
  IsReferenceType: true,
  MaximumConstructorArguments: 4,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "StartNew",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [$jsilcore.TypeRef("System.Action")], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "StartNew",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task`1", ["!!0"]), [$jsilcore.TypeRef("System.Func`1", ["!!0"])], ["TResult"])
  )
});

JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.Object"),
  Name: "System.Threading.Tasks.TaskFactory`1",
  IsPublic: true,
  IsReferenceType: true,
  GenericParameters: ["TResult"],
  MaximumConstructorArguments: 4,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [], [])
  );

  $.ExternalMethod({ Static: false, Public: true }, "StartNew",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.TaskFactory`1")]), [$jsilcore.TypeRef("System.Func`1", [new JSIL.GenericParameter("TResult", "System.Threading.Tasks.TaskFactory`1")])], [])
  );
});