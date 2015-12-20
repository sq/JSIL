JSIL.MakeStaticClass("Microsoft.CSharp.RuntimeBinder.Binder", true, [], function($) {
  $.RawMethod(true, "BinaryOperation",
    function(flags, operation, context, argumentInfo) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.Add || operation === $jsilcore.System.Linq.Expressions.ExpressionType.AddChecked) {
        binder.Method = function (callSite, left, right) {
          return left+right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.And) {
        binder.Method = function (callSite, left, right) {
          return left & right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.AndAlso) {
        binder.Method = function (callSite, left, right) {
          return left && right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.Divide) {
        binder.Method = function (callSite, left, right) {
          return left / right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.Equal) {
        binder.Method = function (callSite, left, right) {
          return left == right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.ExclusiveOr) {
        binder.Method = function (callSite, left, right) {
          return left ^ right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.GreaterThan) {
        binder.Method = function (callSite, left, right) {
          return left > right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.GreaterThanOrEqual) {
        binder.Method = function (callSite, left, right) {
          return left >= right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.LeftShift) {
        binder.Method = function (callSite, left, right) {
          return left << right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.LessThan) {
        binder.Method = function (callSite, left, right) {
          return left < right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.LessThanOrEqual) {
        binder.Method = function (callSite, left, right) {
          return left <= right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.Modulo) {
        binder.Method = function (callSite, left, right) {
          return left % right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.Multiply || operation === $jsilcore.System.Linq.Expressions.ExpressionType.MultiplyChecked) {
        binder.Method = function (callSite, left, right) {
          return left * right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.NotEqual) {
        binder.Method = function (callSite, left, right) {
          return left != right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.Or) {
        binder.Method = function (callSite, left, right) {
          return left | right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.OrElse) {
        binder.Method = function (callSite, left, right) {
          return left || right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.RightShift) {
        binder.Method = function (callSite, left, right) {
          return left >> right;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.Subtract || operation === $jsilcore.System.Linq.Expressions.ExpressionType.SubtractChecked) {
        binder.Method = function (callSite, left, right) {
          return left - right;
        };
      } else {
        throw new Error("Binary operator is not supported.");
      }
      return binder;
    });

  $.RawMethod(true, "Convert",
    function(flags, type, context) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      binder.Method = function (callSite, target) {
        return type.__PublicInterface__.$Cast(target);
      };
      return binder;
    });

  $.RawMethod(true, "GetIndex",
    function(flags, context, argumentInfo) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      var isStaticCall = (argumentInfo[0].Flags & $jsilcore.Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.IsStaticType) > 0;
      binder.Method = function (callSite, target) {
        var realTarget = (isStaticCall ? target.__PublicInterface__ : target);
        if ("get_Item" in realTarget) {
          return realTarget["get_Item"].apply(realTarget, Array.prototype.slice.call(arguments, 2));
        } else {
          // TODO: Jagged arrays support
          if (arguments.length === 3) {
            return realTarget[arguments[2]];
          } else {
            throw new Error("Cannot use multi-dimensional indexer for object without indexed property.");
          }
        }
      };
      return binder;
    });

  $.RawMethod(true, "GetMember",
    function (flags, name, context, argumentInfo) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      var isStaticCall = (argumentInfo[0].Flags & $jsilcore.Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.IsStaticType) > 0;
      binder.Method = function (callSite, target) {
        var realTarget = (isStaticCall ? target.__PublicInterface__ : target);
        if (("get_" + name) in realTarget) {
          return realTarget["get_" + name]();
        } else {
          return realTarget[name];
        }
      };
      return binder;
    });

  $.RawMethod(true, "Invoke",
    function (flags, context, argumentInfo) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      binder.Method = function (callSite, target) {
        return target.apply(null, Array.prototype.slice.call(arguments, 2));
      };
      return binder;
    });

  $.RawMethod(true, "InvokeConstructor",
    function(flags, context, argumentInfo) {
      throw new Error("Not implemented");
    });

  $.RawMethod(true, "InvokeMember",
    function (flags, name, typeArguments, context, argumentInfo) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      var isStaticCall = (argumentInfo[0].Flags & $jsilcore.Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.IsStaticType) > 0;
      if (typeArguments !== null) {
        var useMemberName = name + "$b" + typeArguments.length;
        binder.Method = function (callSite, target) {
          var realTarget = (isStaticCall ? target.__PublicInterface__ : target);
          return realTarget[useMemberName].apply(realTarget, typeArguments).apply(realTarget, Array.prototype.slice.call(arguments, 2));
        };
      } else {
        binder.Method = function (callSite, target) {
          var realTarget = (isStaticCall ? target.__PublicInterface__ : target);
          return realTarget[name].apply(realTarget, Array.prototype.slice.call(arguments, 2));
        };
      }
      return binder;
    });

  $.RawMethod(true, "IsEvent",
    function(flags, name, context) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      binder.Method = function () {
        return false;
      };
      return binder;
    });

  $.RawMethod(true, "SetIndex",
    function(flags, context, argumentInfo) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      var isStaticCall = (argumentInfo[0].Flags & $jsilcore.Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.IsStaticType) > 0;
      binder.Method = function (callSite, target) {
        var realTarget = (isStaticCall ? target.__PublicInterface__ : target);
        if ("set_Item" in realTarget) {
          return realTarget["set_Item"].apply(realTarget, Array.prototype.slice.call(arguments, 2));
        } else {
          // TODO: Jagged arrays support
          if (arguments.length === 4) {
            realTarget[arguments[2]] = arguments[3];
          } else {
            throw new Error("Cannot use multi-dimensional indexer for object without indexed property.");
          }
        }
      };
      return binder;
    });

  $.RawMethod(true, "SetMember",
    function(flags, name, context, argumentInfo) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      var isStaticCall = (argumentInfo[0].Flags & $jsilcore.Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.IsStaticType) > 0;
      binder.Method = function(callSite, target, value) {
        var realTarget = (isStaticCall ? target.__PublicInterface__ : target);
        if (("set_" + name) in realTarget) {
          return realTarget["set_" + name](value);
        } else {
          realTarget[name] = value;
        }
      };
      return binder;
    });

  $.RawMethod(true, "UnaryOperation",
    function(flags, operation, context, argumentInfo) {
      var binder = new $jsilcore.System.Runtime.CompilerServices.CallSiteBinder();
      if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.UnaryPlus) {
        binder.Method = function(callSite, target) {
          return target;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.Negate || operation === $jsilcore.System.Linq.Expressions.ExpressionType.NegateChecked) {
        binder.Method = function (callSite, target) {
          return -target;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.Not) {
        binder.Method = function (callSite, target) {
          if (typeof(target) === "boolean") {
            return ~target;
          }
          return ~target;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.IsTrue) {
        binder.Method = function (callSite, target) {
          return target === true;
        };
      } else if (operation === $jsilcore.System.Linq.Expressions.ExpressionType.IsFalse) {
        binder.Method = function (callSite, target) {
          return target === false;
        };
      } else {
        throw new Error("Unary operator is not supported.");
      }
      return binder;
    });
});