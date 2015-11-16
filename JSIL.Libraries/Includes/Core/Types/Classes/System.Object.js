JSIL.ImplementExternals("System.Object", function ($) {
    $.RawMethod(true, "CheckType",
      function (value) {
          var type = typeof (value);
          return value !== null && (type === "object" || type === "number" || type === "string" || type === "boolean");
      }
    );

    // FIXME: Remove this once the expressions stuff doesn't rely on it anymore
    $.RawMethod(false, "__Initialize__",
      function (initializer) {
          var isInitializer = function (v) {
              return (typeof (v) === "object") && (v !== null) &&
                (
                  (Object.getPrototypeOf(v) === JSIL.CollectionInitializer.prototype) ||
                  (Object.getPrototypeOf(v) === JSIL.ObjectInitializer.prototype)
                );
          };

          if (JSIL.IsArray(initializer)) {
              JSIL.ApplyCollectionInitializer(this, initializer);
              return this;
          } else if (isInitializer(initializer)) {
              initializer.Apply(this);
              return this;
          }

          for (var key in initializer) {
              if (!initializer.hasOwnProperty(key))
                  continue;

              var value = initializer[key];

              if (isInitializer(value)) {
                  this[key] = value.Apply(this[key]);
              } else {
                  this[key] = value;
              }
          }

          return this;
      }
    );


    $.Method({ Static: false, Public: true }, "GetType",
      new JSIL.MethodSignature($jsilcore.TypeRef("System.Type"), [], [], $jsilcore),
      function Object_GetType() {
          return this.__ThisType__;
      }
    );

    $.Method({ Static: false, Public: true }, "Object.Equals",
      new JSIL.MethodSignature($.Boolean, [$.Object], [], $jsilcore),
      function Object_Equals(rhs) {
          return this === rhs;
      }
    );

    $.Method({ Static: false, Public: true }, "GetHashCode",
      (new JSIL.MethodSignature($.Int32, [], [])),
      function Object_GetHashCode() {
          return JSIL.HashCodeInternal(this);
      }
    );

    // HACK: Prevent infinite recursion
    var currentMemberwiseCloneInvocation = null;

    $.Method({ Static: false, Public: false }, "MemberwiseClone",
      new JSIL.MethodSignature($.Object, [], [], $jsilcore),
      function Object_MemberwiseClone() {
          var result = null;

          // HACK: Handle Object.MemberwiseClone direct invocation
          if (currentMemberwiseCloneInvocation === this.MemberwiseClone) {
              result = new System.Object();
          } else {
              currentMemberwiseCloneInvocation = this.MemberwiseClone;
              try {
                  result = this.MemberwiseClone();
              } finally {
                  currentMemberwiseCloneInvocation = null;
              }
          }

          return result;
      }
    );

    $.Method({ Static: false, Public: true }, ".ctor",
      new JSIL.MethodSignature(null, []),
      function Object__ctor() {
      }
    );

    $.Method({ Static: false, Public: true }, "toString",
      new JSIL.MethodSignature($.String, [], [], $jsilcore),
      function Object_ToString() {
          return JSIL.GetTypeName(this);
      }
    );

    $.Method({ Static: true, Public: true }, "ReferenceEquals",
      (new JSIL.MethodSignature($.Boolean, [$.Object, $.Object], [])),
      function ReferenceEquals(objA, objB) {
          return objA === objB;
      }
    );

});

JSIL.MakeClass(Object, "System.Object", true, [], function ($) {
    $jsilcore.SystemObjectInitialized = true;
});