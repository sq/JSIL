JSIL.ImplementExternals("System.Resources.ResourceManager", function ($) {
  $.RawMethod(false, "$fromBaseNameAndAssembly", function (baseName, assembly) {
    this._baseName = baseName;
    this._assembly = assembly;
    this._resourceCollection = {};
  });

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [$.String, $jsilcore.TypeRef("System.Reflection.Assembly")], [])),
    function _ctor(baseName, assembly) {
      this.$fromBaseNameAndAssembly(baseName, assembly);
    }
  );

  $.Method({ Static: false, Public: true }, ".ctor",
    (new JSIL.MethodSignature(null, [
          $.String, $jsilcore.TypeRef("System.Reflection.Assembly"),
          $jsilcore.TypeRef("System.Type")
    ], [])),
    function _ctor(baseName, assembly, usingResourceSet) {
      // FIXME: usingResourceSet
      this.$fromBaseNameAndAssembly(baseName, assembly);
    }
  );

  $.Method({ Static: false, Public: true }, "GetObject",
    (new JSIL.MethodSignature($.Object, [$.String], [])),
    function GetObject(name) {
      var set = this.GetResourceSet($jsilcore.getCurrentUICultureImpl(), false, true)
      return set.GetObject(name);
    }
  );

  $.Method({ Static: false, Public: true }, "GetObject",
    (new JSIL.MethodSignature($.Object, [$.String, $jsilcore.TypeRef("System.Globalization.CultureInfo")], [])),
    function GetObject(name, culture) {
      var set = this.GetResourceSet(culture, false, true)
      return set.GetObject(name);
    }
  );

  $.RawMethod(false, "$findResourcesForCulture", function (culture) {
    var key = this._baseName + "." + culture.get_TwoLetterISOLanguageName() + ".resj";
    if (JSIL.Host.doesAssetExist(key))
      return JSIL.Host.getAsset(key);

    key = this._baseName + ".resj";
    if (JSIL.Host.doesAssetExist(key))
      return JSIL.Host.getAsset(key);

    return null;
  });

  $.Method({ Static: false, Public: true }, "GetResourceSet",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.Resources.ResourceSet"), [
          $jsilcore.TypeRef("System.Globalization.CultureInfo"), $.Boolean,
          $.Boolean
    ], [])),
    function GetResourceSet(culture, createIfNotExists, tryParents) {
      if (!culture)
        culture = $jsilcore.getCurrentUICultureImpl();

      var resources = this.$findResourcesForCulture(culture);
      if (!resources)
        throw new System.Exception("No resources available for culture '" + culture.get_Name() + "'.");

      var result = this._resourceCollection[culture.get_Name()];
      if (!result) {
        var tSet = System.Resources.ResourceSet.__Type__;
        result = this._resourceCollection[culture.get_Name()] = JSIL.CreateInstanceOfType(
          tSet, "$fromResources", [this, resources]
        );
      }

      return result;
    }
  );

  $.Method({ Static: false, Public: true }, "GetStream",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.UnmanagedMemoryStream"), [$.String], [])),
    function GetStream(name) {
      var set = this.GetResourceSet($jsilcore.getCurrentUICultureImpl(), false, true)
      return set.GetStream(name);
    }
  );

  $.Method({ Static: false, Public: true }, "GetStream",
    (new JSIL.MethodSignature($jsilcore.TypeRef("System.IO.UnmanagedMemoryStream"), [$.String, $jsilcore.TypeRef("System.Globalization.CultureInfo")], [])),
    function GetStream(name, culture) {
      var set = this.GetResourceSet(culture, false, true)
      return set.GetStream(name);
    }
  );

  $.Method({ Static: false, Public: true }, "GetString",
    (new JSIL.MethodSignature($.String, [$.String], [])),
    function GetString(name) {
      var set = this.GetResourceSet($jsilcore.getCurrentUICultureImpl(), false, true)
      return set.GetString(name);
    }
  );

  $.Method({ Static: false, Public: true }, "GetString",
    (new JSIL.MethodSignature($.String, [$.String, $jsilcore.TypeRef("System.Globalization.CultureInfo")], [])),
    function GetString(name, culture) {
      var set = this.GetResourceSet(culture, false, true)
      return set.GetString(name);
    }
  );

});