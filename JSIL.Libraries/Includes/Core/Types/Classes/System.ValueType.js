JSIL.MakeClass("System.Object", "System.ValueType", true, [], function ($) {
  var makeComparerCore = function(typeObject, context, body) {
    var fields = JSIL.GetFieldList(typeObject);

    if (context.prototype.__CompareMembers__) {
      context.comparer = context.prototype.__CompareMembers__;
      body.push("  return context.comparer(lhs, rhs);");
    } else {
      for (var i = 0; i < fields.length; i++) {
        var field = fields[i];
        var fieldType = field.type;

        if (fieldType.__IsNumeric__ || fieldType.__IsEnum__) {
          body.push("  if (" + JSIL.FormatMemberAccess("lhs", field.name) + " !== " + JSIL.FormatMemberAccess("rhs", field.name) + ")");
        } else {
          body.push("  if (!JSIL.ObjectEquals(" + JSIL.FormatMemberAccess("lhs", field.name) + ", " + JSIL.FormatMemberAccess("rhs", field.name) + "))");
        }

        body.push("    return false;");
      }

      body.push("  return true;");
    }
  };

  var makeStructComparer = function (typeObject, publicInterface) {
    var prototype = publicInterface.prototype;
    var context = {
      prototype: prototype
    };

    var body = [];

    makeComparerCore(typeObject, context, body);

    return JSIL.CreateNamedFunction(
      typeObject.__FullName__ + ".StructComparer",
      ["lhs", "rhs"],
      body.join("\r\n")
    );
  };

  var structEquals = function Struct_Equals(lhs, rhs) {
    if (lhs === rhs)
      return true;

    if ((rhs === null) || (rhs === undefined))
      return false;

    var thisType = lhs.__ThisType__;
    var comparer = thisType.__Comparer__;
    if (comparer === $jsilcore.FunctionNotInitialized)
      comparer = thisType.__Comparer__ = makeStructComparer(thisType, thisType.__PublicInterface__);

    return comparer(lhs, rhs);
  };

  var makeGetHashCode = function (typeObject, publicInterface) {
    var body = [];

    var fields = JSIL.GetFieldList(typeObject);
    body.push("  var hash = 17;");
    for (var i = 0; i < fields.length; i++) {
      var field = fields[i];
      var fieldAccess = JSIL.FormatMemberAccess("thisReference", field.name);
      body.push("  hash =( Math.imul(hash * 23) + ((" + fieldAccess + " === null ? 17 : JSIL.ObjectHashCode(" + fieldAccess + ", true, $jsilcore.System.Object)) | 0)) | 0;");
    }
    body.push("  return hash;");

    return JSIL.CreateNamedFunction(
      typeObject.__FullName__ + ".GetHashCode",
      ["thisReference"],
      body.join("\r\n")
    );
  };

  var structGetHashCode = function Struct_GetHashCode(thisReference) {
    var thisType = thisReference.__ThisType__;
    var getHashCode = thisType.__GetHashCode__;
    if (getHashCode === $jsilcore.FunctionNotInitialized)
      getHashCode = thisType.__GetHashCode__ = makeGetHashCode(thisType, thisType.__PublicInterface__);

    return getHashCode(thisReference);
  };

  $.Method({ Static: false, Public: true }, "Object.Equals",
    new JSIL.MethodSignature($.Boolean, [System.Object]),
    function(rhs) {
      return structEquals(this, rhs);
    }
  );

  $.Method({ Static: false, Public: true }, "GetHashCode",
    new JSIL.MethodSignature($.Int32, []),
    function() {
      return structGetHashCode(this);
    }
  );
});