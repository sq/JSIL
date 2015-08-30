JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.Exception"),
  Name: "System.AggregateException",
  IsPublic: true,
  IsReferenceType: true,
  MaximumConstructorArguments: 2,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.Exception")], [])
  );
});