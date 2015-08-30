JSIL.ImplementExternals("System.Text.RegularExpressions.Regex", function ($) {
  var system = JSIL.GetAssembly("System", true);

  var makeRegex = function (pattern, options) {
    var tRegexOptions = system.System.Text.RegularExpressions.RegexOptions;
    if ((options & tRegexOptions.ECMAScript) === 0) {
      JSIL.RuntimeError("Non-ECMAScript regexes are not currently supported.");
    }

    var flags = "g";

    if ((options & tRegexOptions.IgnoreCase) !== 0) {
      flags += "i";
    }

    if ((options & tRegexOptions.Multiline) !== 0) {
      flags += "m";
    }

    return new RegExp(pattern, flags);
  };

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.String], [])),
    function _ctor(pattern) {
      this._regex = makeRegex(pattern, System.Text.RegularExpressions.RegexOptions.None);
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.String, system.TypeRef("System.Text.RegularExpressions.RegexOptions")], [])),
    function _ctor(pattern, options) {
      this._regex = makeRegex(pattern, options);
    }
  );

  $.Method({ Static: false, Public: true }, "Matches",
    (new JSIL.MethodSignature(system.TypeRef("System.Text.RegularExpressions.MatchCollection"), [$.String], [])),
    function Matches(input) {
      var matchObjects = [];
      var tMatch = system.System.Text.RegularExpressions.Match.__Type__;
      var tGroup = system.System.Text.RegularExpressions.Group.__Type__;
      var tGroupCollection = system.System.Text.RegularExpressions.GroupCollection.__Type__;

      var current = null;
      while ((current = this._regex.exec(input)) !== null) {
        var groupObjects = [];
        for (var i = 0, l = current.length; i < l; i++) {
          var groupObject = JSIL.CreateInstanceOfType(
            tGroup, "$internalCtor", [
              current[i],
              (current[i] !== null) && (current[i].length > 0)
            ]
          );
          groupObjects.push(groupObject);
        }

        var groupCollection = JSIL.CreateInstanceOfType(
          tGroupCollection, "$internalCtor", [groupObjects]
        );

        var matchObject = JSIL.CreateInstanceOfType(
          tMatch, "$internalCtor", [
            current[0], groupCollection
          ]
        );
        matchObjects.push(matchObject);
      }

      var result = JSIL.CreateInstanceOfType(
        System.Text.RegularExpressions.MatchCollection.__Type__,
        "$internalCtor", [matchObjects]
      );

      return result;
    }
  );

  $.Method({ Static: true, Public: true }, "Replace",
    (new JSIL.MethodSignature($.String, [
          $.String, $.String,
          $.String, system.TypeRef("System.Text.RegularExpressions.RegexOptions")
    ], [])),
    function Replace(input, pattern, replacement, options) {
      var re = makeRegex(pattern, options);

      return input.replace(re, replacement);
    }
  );

  $.Method({ Static: true, Public: true }, "Replace",
    (new JSIL.MethodSignature($.String, [
          $.String, $.String, $.String], [])),
    function Replace(input, pattern, replacement) {
      var re = makeRegex(pattern, System.Text.RegularExpressions.RegexOptions.ECMAScript);

      return input.replace(re, replacement);
    }
  );

  $.Method({ Static: false, Public: true }, "Replace",
    (new JSIL.MethodSignature($.String, [$.String, $.String], [])),
    function Replace(input, replacement) {
      return input.replace(this._regex, replacement);
    }
  );

  $.Method({ Static: false, Public: true }, "IsMatch",
    (new JSIL.MethodSignature($.Boolean, [$.String], [])),
    function IsMatch(input) {
      var matchCount = 0;

      var current = null;
      // Have to exec() until done because JS RegExp is stateful for some stupid reason
      while ((current = this._regex.exec(input)) !== null) {
        matchCount += 1;
      }

      return (matchCount > 0);
    }
  );
});

JSIL.MakeClass($jsilcore.TypeRef("System.Object"), "System.Text.RegularExpressions.Regex", true, [], function ($) {
  var $thisType = $.publicInterface;
});