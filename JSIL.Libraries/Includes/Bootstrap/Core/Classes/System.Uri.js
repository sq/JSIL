// modified from http://snipplr.com/view/6889/regular-expressions-for-uri-validationparsing/
var regexUri = /^([a-z][a-z0-9+.-]*):(?:\/\/((?:(?=((?:[a-z0-9-._~!$&'()*+,;=:]|%[0-9A-F]{2})*))(\3)@)?(?=(\[[0-9A-F:.]{2,}\]|(?:[a-z0-9-._~!$&'()*+,;=]|%[0-9A-F]{2})*))\5(?::(?=(\d*))\6)?)(\/(?=((?:[a-z0-9-._~!$&'()*+,;=:@\/]|%[0-9A-F]{2})*))\8)?|(\/?(?!\/)(?=((?:[a-z0-9-._~!$&'()*+,;=:@\/]|%[0-9A-F]{2})*))\10)?)(?:\?(?=((?:[a-z0-9-._~!$&'()*+,;=:@\/?]|%[0-9A-F]{2})*))\11)?(?:#(?=((?:[a-z0-9-._~!$&'()*+,;=:@\/?]|%[0-9A-F]{2})*))\12)?$/i;

function parseURI(uriString) {
  uriString = uriString.replace(/\\/g, '/');
  var uri = {};
  var match = uriString.match(regexUri);
  if (match === null) {
    // it's not a uri
    uri.path = uriString;
  } else {
    uri.scheme = match[1] || match[6];
    uri.userinfo = match[2]
    uri.host = match[3]
    uri.port = match[4]
    uri.path = match[5] || match[7]
    uri.query = match[8]
    uri.fragment = match[9]
  }
  return uri;
}

function pathSplit(s) {
  return s.split(/\//g).filter(function (x) { return x.length > 0 });
}

JSIL.ImplementExternals("System.Uri", function ($) {
  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$.String], []),
    function _ctor(uriString) {
      var uri = parseURI(uriString);
      this._scheme = uri.scheme;
      this._path = pathSplit(uri.path);
    }
  );

  // Join a path onto the end of an existing uri
  $.Method({ Static: false, Public: true }, ".ctor",
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Uri"), $.String], []),
    function _ctor(baseUri, relativeUriStr) {
      this._scheme = baseUri._scheme;
      var relativeUri = new System.Uri(relativeUriStr);
      this._path = baseUri._path.concat(relativeUri._path);
    }
  );

  $.Method({ Static: false, Public: true }, "get_IsAbsoluteUri",
    new JSIL.MethodSignature($.Boolean, [], []),
    function get_IsAbsoluteUri() {
      return this._scheme !== undefined;
    }
  );

  $.Method({ Static: false, Public: true }, "get_LocalPath",
    new JSIL.MethodSignature($.String, [], []),
    function get_LocalPath() {
      if (this.IsAbsoluteUri) {
        return "\\\\" + this._path.join("\\");
      } else {
        throw new System.InvalidOperationException("only an absolute uri has a LocalPath");
      }
      return this._length;
    }
  );
});

//? if ('GENERATE_STUBS' in  __out) {
JSIL.MakeClass("System.Object", "System.Uri", true, [], function ($) {
  $.Property({ Public: true, Static: false }, "LocalPath");
  $.Property({ Public: true, Static: false }, "IsAbsoluteUri");
});
//? }
