JSIL.ImplementExternals("System.Threading.Monitor", function ($) {
  var enterImpl = function (obj) {
    var current = (obj.__LockCount__ || 0);
    if (current >= 1)
      JSIL.Host.warning("Warning: lock recursion " + obj);

    obj.__LockCount__ = current + 1;

    return true;
  };

  $.Method({ Static: true, Public: true }, "Enter",
    (new JSIL.MethodSignature(null, [$.Object], [])),
    function Enter(obj) {
      enterImpl(obj);
    }
  );

  $.Method({ Static: true, Public: true }, "Enter",
    (new JSIL.MethodSignature(null, [$.Object, $jsilcore.TypeRef("JSIL.Reference", [$.Boolean])], [])),
    function Enter(obj, /* ref */ lockTaken) {
      lockTaken.set(enterImpl(obj));
    }
  );

  $.Method({ Static: true, Public: true }, "Exit",
    (new JSIL.MethodSignature(null, [$.Object], [])),
    function Exit(obj) {
      var current = (obj.__LockCount__ || 0);
      if (current <= 0)
        JSIL.Host.warning("Warning: unlocking an object that is not locked " + obj);

      obj.__LockCount__ = current - 1;
    }
  );

});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeStaticClass("System.Threading.Monitor", true, []);
//? }