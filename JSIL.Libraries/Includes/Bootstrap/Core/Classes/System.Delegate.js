$jsilcore.$GetInvocationList = function (delegate) {
  if (delegate === null) {
    return [];
  } else if (typeof (delegate.__delegates__) !== "undefined") {
    return delegate.__delegates__;
  } else if (typeof (delegate) === "function") {
    return [delegate];
  } else {
    return null;
  }
};
$jsilcore.$CompareSinglecastDelegate = function (lhs, rhs) {
  if (lhs.__object__ !== rhs.__object__)
    return false;

  if (lhs.get_Method() !== rhs.get_Method())
    return false;

  return true;
};
$jsilcore.$CompareMulticastDelegate = function (lhs, rhs) {
  var lhsInvocationList = $jsilcore.$GetInvocationList(lhs);
  var rhsInvocationList = $jsilcore.$GetInvocationList(rhs);

  if (lhsInvocationList.length !== rhsInvocationList.length)
    return false;

  for (var i = 0, l = lhsInvocationList.length; i < l; i++) {
    if (!$jsilcore.$AreDelegatesEqual(lhsInvocationList[i], rhsInvocationList[i]))
      return false;
  }

  return true;
};
$jsilcore.$AreDelegatesEqual = function (lhs, rhs) {
  if (lhs === rhs)
    return true;

  var singleMethod, otherMethod;
  if (!lhs.__isMulticast__)
    return $jsilcore.$CompareSinglecastDelegate(lhs, rhs);
  else if (!rhs.__isMulticast__)
    return $jsilcore.$CompareSinglecastDelegate(rhs, lhs);
  else
    return $jsilcore.$CompareMulticastDelegate(lhs, rhs);
};
$jsilcore.$CombineDelegates = function (lhs, rhs) {
  if (rhs === null) {
    return lhs;
  } else if (lhs === null) {
    return rhs;
  }

  var newList = Array.prototype.slice.call($jsilcore.$GetInvocationList(lhs));
  newList.push.apply(newList, $jsilcore.$GetInvocationList(rhs));
  var result = JSIL.MulticastDelegate.New(newList);
  return result;
};
$jsilcore.$RemoveDelegate = function (lhs, rhs) {
  if (rhs === null)
    return lhs;
  if (lhs === null)
    return null;

  var newList = Array.prototype.slice.call($jsilcore.$GetInvocationList(lhs));
  var rightList = $jsilcore.$GetInvocationList(rhs);

  if (newList.length >= rightList.length) {
    for (var i = newList.length - rightList.length; i >= 0; i--) {
      var equal = true;
      for (var j = 0; j < rightList.length; j++) {
        if (!$jsilcore.$AreDelegatesEqual(newList[i + j], rightList[j])) {
          equal = false;
          break;
        }
      }
      if (equal) {
        newList.splice(i, rightList.length);
        break;
      }
    }
  }

  if (newList.length == 0)
    return null;
  else if (newList.length == 1)
    return newList[0];
  else
    return JSIL.MulticastDelegate.New(newList);
};

JSIL.ImplementExternals("System.Delegate", function ($) {
  var tDelegate = $jsilcore.TypeRef("System.Delegate");

  $.Method({ Static: false, Public: true }, "GetInvocationList",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [tDelegate]), [], [])),
    function GetInvocationList() {
      return [this];
    }
  );

  $.Method({ Static: true, Public: true }, "CreateDelegate",
    (new JSIL.MethodSignature(tDelegate, [
          $jsilcore.TypeRef("System.Type"), $.Object,
          $jsilcore.TypeRef("System.Reflection.MethodInfo")
    ], [])),
    function CreateDelegate(delegateType, firstArgument, method) {
      var delegatePublicInterface = delegateType.__PublicInterface__;
      if (typeof (delegatePublicInterface.New) !== "function")
        JSIL.Host.abort(new Error("Invalid delegate type"));

      if (!method.IsStatic && firstArgument == null && delegateType.GetMethod("Invoke").GetParameters().length == method.GetParameters().length + 1) {
        //TODO: Check that first delegate Invoke parameter is assignable to method.DeclaringType
        return delegatePublicInterface.New(
          null,
          function () {
            var realFirstAgument = arguments[0];
            var impl = JSIL.$GetMethodImplementation(method, firstArgument);

            if (!impl) {
              throw new System.Exception("Method has no implementation");
            } else if (typeof (impl) === "function") {
              var f = delegatePublicInterface.New(realFirstAgument, impl, function () { return null; });
              return f.apply(null, Array.prototype.slice.call(arguments, 1));
            } else {
              throw new Error("Unexpected JSIL.$GetMethodImplementation result");
            }
          },
          function () { return method; });
      }

      var impl = JSIL.$GetMethodImplementation(method, firstArgument);

      if (!impl) {
        throw new System.Exception("Method has no implementation");
      } else if (typeof (impl) === "function") {
        return delegatePublicInterface.New(firstArgument, impl, function () { return method; });
      } else {
        throw new Error("Unexpected JSIL.$GetMethodImplementation result");
      }
    }
  );

  $.Method({ Static: true, Public: true }, "op_Equality",
    (new JSIL.MethodSignature($.Boolean, [tDelegate, tDelegate], [])),
    $jsilcore.$AreDelegatesEqual
  );

  $.Method({ Static: true, Public: true }, "op_Inequality",
    (new JSIL.MethodSignature($.Boolean, [tDelegate, tDelegate], [])),
    function op_Inequality(d1, d2) {
      return !$jsilcore.$AreDelegatesEqual(d1, d2);
    }
  );

  $.Method({ Static: true, Public: true }, "Combine",
    (new JSIL.MethodSignature(tDelegate, [tDelegate, tDelegate], [])),
    $jsilcore.$CombineDelegates
  );

  $.Method({ Static: true, Public: true }, "Remove",
    (new JSIL.MethodSignature(tDelegate, [tDelegate, tDelegate], [])),
    $jsilcore.$RemoveDelegate
  );

  $.Method({ Static: false, Public: true }, "get_Method",
    (new JSIL.MethodSignature.Return($jsilcore.TypeRef("System.Reflection.MethodInfo"))),
    function get_Method() {
      if (this.__isMulticast__) {
        return this.get_Method();
      }
      if (!this.__isMethodInfoResolved__) {
        var methodInfo = this.__methodInfoResolver__();
        // TODO: find better solution for resolving MethodInfo in class by MethodInfo in base class.
        // Currently it will not find proper derived implementation MethodInfo for virtual method and interface methods.
        JSIL.SetValueProperty(this, "__methodInfo__", methodInfo);
        this.__isMethodInfoResolved__ = true;
      }
      return this.__methodInfo__;
    }
  );
});

JSIL.MakeClass("System.Object", "System.Delegate", true, [], function ($) {
  $.Property({ Public: true, Static: false }, "Method");
});