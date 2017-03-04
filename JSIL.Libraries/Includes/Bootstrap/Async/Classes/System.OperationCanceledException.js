JSIL.MakeType({
  BaseType: $jsilcore.TypeRef("System.SystemException"),
  Name: "System.OperationCanceledException",
  IsPublic: true,
  IsReferenceType: true,
  MaximumConstructorArguments: 2,
}, function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.ExternalMethod({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [], [])
  );
});