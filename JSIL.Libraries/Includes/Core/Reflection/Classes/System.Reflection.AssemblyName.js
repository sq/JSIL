(function AssemblyName$Members() {
  var $, $thisType;
  JSIL.MakeType({
    BaseType: $jsilcore.TypeRef("System.Object"),
    Name: "System.Reflection.AssemblyName",
    IsPublic: true,
    IsReferenceType: true,
    MaximumConstructorArguments: 2,
  }, function ($interfaceBuilder) {
    $ = $interfaceBuilder;

    $.ExternalMethod({ Static: false, Public: true }, ".ctor",
      JSIL.MethodSignature.Void
    )
    ;

    $.ExternalMethod({ Static: false, Public: true }, ".ctor",
      JSIL.MethodSignature.Action($.String)
    )
    ;

    $.ExternalMethod({ Static: false, Public: true }, "get_Flags",
      JSIL.MethodSignature.Return($asm02.TypeRef("System.Reflection.AssemblyNameFlags"))
    )
    ;

    $.ExternalMethod({ Static: false, Public: true }, "get_FullName",
      JSIL.MethodSignature.Return($.String)
    )
    ;

    $.ExternalMethod({ Static: false, Public: true }, "get_Name",
      JSIL.MethodSignature.Return($.String)
    )
    ;

    $.ExternalMethod({ Static: false, Public: true }, "get_Version",
      JSIL.MethodSignature.Return($asm02.TypeRef("System.Version"))
    )
    ;

    $.ExternalMethod({ Static: true, Public: true }, "GetAssemblyName",
      new JSIL.MethodSignature($.Type, [$.String])
    )
    ;

    $.ExternalMethod({ Static: false, Public: true }, "set_Flags",
      JSIL.MethodSignature.Action($asm02.TypeRef("System.Reflection.AssemblyNameFlags"))
    )
    ;

    $.ExternalMethod({ Static: false, Public: true }, "set_Name",
      JSIL.MethodSignature.Action($.String)
    )
    ;

    $.ExternalMethod({ Static: false, Public: true }, "set_Version",
      JSIL.MethodSignature.Action($asm02.TypeRef("System.Version"))
    )
    ;

    $.ExternalMethod({ Static: false, Public: true, Virtual: true }, "toString",
      JSIL.MethodSignature.Return($.String)
    )
    ;

    $.Property({ Static: false, Public: true }, "Name", $.String)
    ;

    $.Property({ Static: false, Public: true }, "Version", $asm02.TypeRef("System.Version"))
    ;

    $.Property({ Static: false, Public: true }, "Flags", $asm02.TypeRef("System.Reflection.AssemblyNameFlags"))
    ;

    $.Property({ Static: false, Public: true }, "FullName", $.String)
    ;

    $.ImplementInterfaces(
      /* 0 */ $asm02.TypeRef("System.Runtime.InteropServices._AssemblyName"),
      /* 1 */ $asm02.TypeRef("System.ICloneable"),
      /* 2 */ $asm02.TypeRef("System.Runtime.Serialization.ISerializable"),
      /* 3 */ $asm02.TypeRef("System.Runtime.Serialization.IDeserializationCallback")
    );

    return function (newThisType) { $thisType = newThisType; };
  });

})();

JSIL.ImplementExternals("System.Reflection.AssemblyName", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  $.Method({ Static: false, Public: true }, ".ctor",
    JSIL.MethodSignature.Void,
    function _ctor() {
      this.set_Name(null);
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    JSIL.MethodSignature.Action($.String),
    function _ctor(assemblyName) {
      this.set_Name(assemblyName);
    }
  );

  $.Method({ Static: false, Public: true }, "get_Flags",
    JSIL.MethodSignature.Return($jsilcore.TypeRef("System.Reflection.AssemblyNameFlags")),
    function get_Flags() {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "get_FullName",
    JSIL.MethodSignature.Return($.String),
    function get_FullName() {
      return this._FullName;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Name",
    JSIL.MethodSignature.Return($.String),
    function get_Name() {
      return this._Name;
    }
  );

  $.Method({ Static: false, Public: true }, "get_Version",
    JSIL.MethodSignature.Return($jsilcore.TypeRef("System.Version")),
    function get_Version() {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: true, Public: true }, "GetAssemblyName",
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Reflection.AssemblyName"), [$.String]),
    function GetAssemblyName(assemblyFile) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "set_Flags",
    JSIL.MethodSignature.Action($jsilcore.TypeRef("System.Reflection.AssemblyNameFlags")),
    function set_Flags(value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true }, "set_Name",
    JSIL.MethodSignature.Action($.String),
    function set_Name(value) {
      this._Name = value;
    }
  );

  $.Method({ Static: false, Public: true }, "set_Version",
    JSIL.MethodSignature.Action($jsilcore.TypeRef("System.Version")),
    function set_Version(value) {
      throw new Error('Not implemented');
    }
  );

  $.Method({ Static: false, Public: true, Virtual: true }, "toString",
    JSIL.MethodSignature.Return($.String),
    function toString() {
      throw new Error('Not implemented');
    }
  );

  ;
});