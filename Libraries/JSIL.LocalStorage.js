"use strict";

if (typeof (JSIL) === "undefined")
  throw new Error("JSIL.Core required");

if (typeof ($jsilstorage) === "undefined")
  throw new Error("JSIL.Storage required");

var $jsillocalstorage = JSIL.DeclareAssembly("JSIL.LocalStorage");

JSIL.MakeClass($jsilstorage.TypeRef("VirtualVolume"), "LocalStorageVolume", true, [], function ($) {
  $.RawMethod(false, ".ctor", function (name, rootPath) {
    var existingInodes = this.readInodes(name);

    VirtualVolume.prototype._ctor.call(
      this, name, rootPath, existingInodes
    );
  });

  var getKey = function (name) {
    return "storage_" + name;
  };

  $.RawMethod(false, "readInodes", function (name) {
    var key = getKey(name) + "_inodes";

    var json = localStorage.getItem(key);
    if (json)
      return JSON.parse(json);
    else
      return null;
  });

  $.RawMethod(false, "writeInodes", function (name) {
    var key = getKey(name) + "_inodes";

    localStorage.setItem(key, JSON.stringify(this.inodes));
  });

  $.RawMethod(false, "flush", function () {
    this.writeInodes(this.name);
  });

  $.RawMethod(false, "deleteFileBytes", function (name) {
    var key = getKey(this.name) + "_blobs_" + name;

    localStorage.removeItem(key);

    JSIL.Host.logWriteLine("Deleted '" + name + "' from local storage.");
  });

  $.RawMethod(false, "getFileBytes", function (name) {
    var key = getKey(this.name) + "_blobs_" + name;

    var json = localStorage.getItem(key);

    if (json) {
      JSIL.Host.logWriteLine("Loaded '" + name + "' from local storage.");

      var array = JSON.parse(json);
      if (window.Uint8Array) {
        return new Uint8Array(array);
      } else {
        return array;
      }
    } else {
      JSIL.Host.logWriteLine("Could not find '" + name + "' in local storage.");
      return null;
    }
  });

  $.RawMethod(false, "setFileBytes", function (name, value) {    
    var key = getKey(this.name) + "_blobs_" + name;

    // JSON.stringify turns typed arrays into dictionaries :|

    var json = "[";
    for (var i = 0; i < value.length; i++) {
      if (i !== 0)
        json += ",";

      json += value[i];
    }
    json += "]";

    localStorage.setItem(key, json);

    JSIL.Host.logWriteLine("Saved '" + name + "' to local storage.");
  });

  $.RawMethod(false, "toString", function () {
    return "<LocalStorage Volume '" + this.name + "'>";
  });
});

JSIL.LocalStorage = {};
JSIL.LocalStorage.Initialize = function (appName) {
  var volume = null;

  JSIL.RegisterStorageProvider({
    getVolumes: function () {
      if (!volume)
        volume = new LocalStorageVolume(appName, "/");

      return [volume];
    }
  });
};