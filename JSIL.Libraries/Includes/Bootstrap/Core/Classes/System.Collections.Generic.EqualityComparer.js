(function EqualityComparer$b1$Members() {
  var $S00 = function() {
    return ($S00 = JSIL.Memoize(new JSIL.ConstructorSignature($jsilcore.TypeRef("System.ArgumentOutOfRangeException"), [$jsilcore.TypeRef("System.String")])))();
  };

  JSIL.ImplementExternals("System.Collections.Generic.EqualityComparer`1", function($) {
    $.Method({ Static: false, Public: false }, ".ctor",
      JSIL.MethodSignature.Void,
      function EqualityComparer$b1__ctor() {}
    );

    $.Method({ Static: true, Public: false }, "CreateComparer",
      new JSIL.MethodSignature($.Type, null),
      function EqualityComparer$b1_CreateComparer() {
        return new ($jsilcore.JSIL.ObjectEqualityComparer$b1.Of(this.T))();
      }
    );

    $.Method({ Static: true, Public: true }, "get_Default",
      new JSIL.MethodSignature($.Type, null),
      function EqualityComparer$b1_get_Default() {
        var equalityComparer = this.__Type__.__PublicInterface__.defaultComparer;
        if (!equalityComparer) {
          equalityComparer = this.__Type__.__PublicInterface__.CreateComparer();
          this.__Type__.__PublicInterface__.defaultComparer = equalityComparer;
        }
        return equalityComparer;
      }
    );

    $.Method({ Static: false, Public: false, Virtual: true }, "System.Collections.IEqualityComparer.Equals",
      new JSIL.MethodSignature($.Boolean, [$.Object, $.Object]),
      function EqualityComparer$b1_System_Collections_IEqualityComparer_Equals(x, y) {
        var $s00 = new JSIL.MethodSignature($jsilcore.System.Boolean, [this.T, this.T]);
        if (x === y) {
          var result = true;
        } else if (!((x !== null) && (y !== null))) {
          result = false;
        } else {
          if ((this.T.$As(x) === null) || !this.T.$Is(y)) {
            throw $S00().Construct("Invalid type of some arguments");
          }
          result = $s00.CallVirtual("Equals", null, this, JSIL.CloneParameter(this.T, this.T.$Cast(x)), JSIL.CloneParameter(this.T, this.T.$Cast(y)));
        }
        return result;
      }).Overrides($jsilcore.TypeRef("System.Collections.IEqualityComparer"), "Equals");

    $.Method({ Static: false, Public: false, Virtual: true }, "System.Collections.IEqualityComparer.GetHashCode",
      new JSIL.MethodSignature($.Int32, [$.Object]),
      function EqualityComparer$b1_System_Collections_IEqualityComparer_GetHashCode(obj) {
        if (obj === null) {
          var result = 0;
        } else {
          if (!this.T.$Is(obj)) {
            throw $S00().Construct("Invalid argument type");
          }
          result = this.GetHashCode(JSIL.CloneParameter(this.T, this.T.$Cast(obj)));
        }
        return result;
      }
    ).Overrides($jsilcore.TypeRef("System.Collections.IEqualityComparer"), "GetHashCode");
  });

//? if ('GENERATE_STUBS' in  __out) {
  JSIL.MakeType({
    BaseType: $jsilcore.TypeRef("System.Object"),
    Name: "System.Collections.Generic.EqualityComparer`1",
    IsPublic: true,
    IsReferenceType: true,
    GenericParameters: ["T"],
    MaximumConstructorArguments: 0,
  }, function($interfaceBuilder) {
    var $ = $interfaceBuilder;

    $.ExternalMethod({ Static: false, Public: false }, ".ctor",
      JSIL.MethodSignature.Void
    );

    $.ExternalMethod({ Static: true, Public: false }, "CreateComparer",
      new JSIL.MethodSignature($.Type, null)
    );

    $.ExternalMethod({ Static: true, Public: true }, "get_Default",
      new JSIL.MethodSignature($.Type, null)
    );

    $.ExternalMethod({ Static: false, Public: false, Virtual: true }, "System.Collections.IEqualityComparer.Equals",
      new JSIL.MethodSignature($.Boolean, [$.Object, $.Object])
    ).Overrides($jsilcore.TypeRef("System.Collections.IEqualityComparer"), "Equals");

    $.ExternalMethod({ Static: false, Public: false, Virtual: true }, "System.Collections.IEqualityComparer.GetHashCode",
      new JSIL.MethodSignature($.Int32, [$.Object])
    ).Overrides($jsilcore.TypeRef("System.Collections.IEqualityComparer"), "GetHashCode");

    $.Field({ Static: true, Public: false }, "defaultComparer", $jsilcore.TypeRef("EqualityComparer`1"));
    $.GenericProperty({ Static: true, Public: true }, "Default", $.Type);

    $.ImplementInterfaces(
      $jsilcore.TypeRef("System.Collections.IEqualityComparer"),
      $jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [$.GenericParameter("T")])
    );
  });
//? }
})();

(function ObjectEqualityComparer$b1$Members() {
  var $, $thisType;
  var $T00 = function() {
    return ($T00 = JSIL.Memoize($jsilcore.System.Boolean))();
  };
  var $T01 = function() {
    return ($T01 = JSIL.Memoize($jsilcore.System.Object))();
  };
  var $T02 = function() {
    return ($T02 = JSIL.Memoize($jsilcore.System.Int32))();
  };

  JSIL.MakeType({
    BaseType: $jsilcore.TypeRef("System.Collections.Generic.EqualityComparer`1", [new JSIL.GenericParameter("T", "ObjectEqualityComparer`1")]),
    Name: "JSIL.ObjectEqualityComparer`1",
    IsPublic: false,
    IsReferenceType: true,
    GenericParameters: ["T"],
    MaximumConstructorArguments: 0,
  }, function($interfaceBuilder) {
    $ = $interfaceBuilder;

    $.Method({ Static: false, Public: true }, ".ctor",
      JSIL.MethodSignature.Void,
      function ObjectEqualityComparer$b1__ctor() {
        $jsilcore.System.Collections.Generic.EqualityComparer$b1.Of($thisType.T.get(this)).prototype._ctor.call(this);
      }
    );

    $.Method({ Static: false, Public: true, Virtual: true }, "Equals",
      new JSIL.MethodSignature($.Boolean, [$.GenericParameter("T"), $.GenericParameter("T")]),
      function ObjectEqualityComparer$b1_Equals$00(x, y) {
        if (x !== null) {
          var result = ((y !== null) &&
          (JSIL.ObjectEquals(x, y)));
        } else {
          result = (y === null);
        }
        return result;
      }
    ).Overrides($jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [$.GenericParameter("T")]), "Equals");

    $.Method({ Static: false, Public: true, Virtual: true }, "GetHashCode",
      new JSIL.MethodSignature($.Int32, [$.GenericParameter("T")]),
      function ObjectEqualityComparer$b1_GetHashCode(obj) {
        if (obj === null) {
          var result = 0;
        } else {
          result = (JSIL.ObjectHashCode(obj, true));
        }
        return result;
      }
    ).Overrides($jsilcore.TypeRef("System.Collections.Generic.IEqualityComparer`1", [$.GenericParameter("T")]), "GetHashCode");

    $.ImplementInterfaces(
    );

    return function(newThisType) { $thisType = newThisType; };
  });

})();