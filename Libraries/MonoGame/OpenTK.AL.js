//
// Derived from Emscripten's OpenAL implementation (MIT-licensed)
// see https://github.com/kripken/emscripten/blob/master/src/library_openal.js
//
// Emscripten license follows:
/*
  Copyright (c) 2010-2011 Emscripten authors, see AUTHORS file.

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to deal
  in the Software without restriction, including without limitation the rights
  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
  THE SOFTWARE.
*/
//

"use strict";

if (typeof (JSIL) === "undefined") 
  throw new Error("JSIL.Core required");

if (typeof ($jsilopentk) === "undefined")
  throw new Error("OpenTK required");

JSIL.DeclareNamespace("JSIL");
JSIL.DeclareNamespace("JSIL.AL");

JSIL.DeclareNamespace("OpenTK");
JSIL.DeclareNamespace("OpenTK.Audio");
JSIL.DeclareNamespace("OpenTK.Audio.OpenAL");

JSIL.AL.$deviceToken = null;
JSIL.AL.getDeviceToken = function () {
  if (!JSIL.AL.$deviceToken)
    JSIL.AL.$deviceToken = new System.IntPtr(1);

  return JSIL.AL.$deviceToken;
};

JSIL.AL.$contextToken = null;
JSIL.AL.getContextToken = function () {
  if (!JSIL.AL.$contextToken)
    JSIL.AL.$contextToken = new OpenTK.ContextHandle(new System.IntPtr(2));

  return JSIL.AL.$contextToken;
};

JSIL.ImplementExternals("OpenTK.Audio.OpenAL.Alc", function ($) {
  $.Method({Static:true , Public:true }, "CloseDevice", 
    new JSIL.MethodSignature($.Boolean, [$mgasms[2].TypeRef("System.IntPtr")], []), 
    function CloseDevice (device) {
      // FIXME
    }
  );

  $.Method({Static:true , Public:true }, "CreateContext", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.ContextHandle"), [$mgasms[2].TypeRef("System.IntPtr"), $jsilcore.TypeRef("JSIL.Pointer", [$.Int32])], []), 
    function CreateContext (device, attrlist) {
      if (device !== JSIL.AL.getDeviceToken())
        throw new Error("Invalid device");

      // FIXME: attrList

      return JSIL.AL.getContextToken();
    }
  );

  $.Method({Static:true , Public:true }, "CreateContext", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.ContextHandle"), [$mgasms[2].TypeRef("System.IntPtr"), $jsilcore.TypeRef("System.Array", [$.Int32])], []), 
    function CreateContext (device, attriblist) {
      if (device !== JSIL.AL.getDeviceToken())
        throw new Error("Invalid device");

      // FIXME: attribList

      return JSIL.AL.getContextToken();
    }
  );

  $.Method({Static:true , Public:true }, "DestroyContext", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function DestroyContext (context) {
      // FIXME
    }
  );

  $.Method({Static:true , Public:true }, "GetContextsDevice", 
    new JSIL.MethodSignature($mgasms[2].TypeRef("System.IntPtr"), [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function GetContextsDevice (context) {
      if (context !== JSIL.AL.getContextToken())
        throw new Error("Invalid context");
    }
  );

  $.Method({Static:true , Public:true }, "GetCurrentContext", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.ContextHandle"), [], []), 
    function GetCurrentContext () {
      // FIXME: Always returns same context
      return JSIL.AL.getContextToken();
    }
  );

  $.Method({Static:true , Public:true }, "GetEnumValue", 
    new JSIL.MethodSignature($.Int32, [$mgasms[2].TypeRef("System.IntPtr"), $.String], []), 
    function GetEnumValue (device, enumname) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "GetError", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Audio.OpenAL.AlcError"), [$mgasms[2].TypeRef("System.IntPtr")], []), 
    function GetError (device) {
      // FIXME
      return OpenTK.Audio.OpenAL.AlcError.NoError;
    }
  );

  $.Method({Static:true , Public:true }, "GetInteger", 
    new JSIL.MethodSignature(null, [
        $mgasms[2].TypeRef("System.IntPtr"), $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.AlcGetInteger"), 
        $.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.Int32])
      ], []), 
    function GetInteger (device, param, size, /* ref */ data) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "GetInteger", 
    new JSIL.MethodSignature(null, [
        $mgasms[2].TypeRef("System.IntPtr"), $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.AlcGetInteger"), 
        $.Int32, $jsilcore.TypeRef("System.Array", [$.Int32])
      ], []), 
    function GetInteger (device, param, size, data) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "GetString", 
    new JSIL.MethodSignature($.String, [$mgasms[2].TypeRef("System.IntPtr"), $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.AlcGetString")], []), 
    function GetString (device, param) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "GetString", 
    new JSIL.MethodSignature($mgasms[2].TypeRef("System.Collections.Generic.IList`1", [$.String]), [$mgasms[2].TypeRef("System.IntPtr"), $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.AlcGetStringList")], []), 
    function GetString (device, param) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "IsExtensionPresent", 
    new JSIL.MethodSignature($.Boolean, [$mgasms[2].TypeRef("System.IntPtr"), $.String], []), 
    function IsExtensionPresent (device, extname) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "MakeContextCurrent", 
    new JSIL.MethodSignature($.Boolean, [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function MakeContextCurrent (context) {
      if (context !== JSIL.AL.getContextToken())
        return false;
      else
        return true;
    }
  );

  $.Method({Static:true , Public:true }, "OpenDevice", 
    new JSIL.MethodSignature($mgasms[2].TypeRef("System.IntPtr"), [$.String], []), 
    function OpenDevice (devicename) {
      // FIXME: devicename      
      return JSIL.AL.getDeviceToken();
    }
  );

  $.Method({Static:true , Public:true }, "ProcessContext", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function ProcessContext (context) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SuspendContext", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function SuspendContext (context) {
      throw new Error('Not implemented');
    }
  );
});

//
// ContextHandle
//

JSIL.ImplementExternals("OpenTK.ContextHandle", function ($) {
  $.RawMethod(true, ".cctor2", function () {
    OpenTK.ContextHandle.Zero = new OpenTK.ContextHandle(new System.IntPtr(0));
  });

  $.Method({Static:false, Public:true }, ".ctor", 
    new JSIL.MethodSignature(null, [$mgasms[2].TypeRef("System.IntPtr")], []), 
    function _ctor (h) {
      this.handle = h;
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "CompareTo", 
    new JSIL.MethodSignature($.Int32, [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function CompareTo (other) {
      return this.handle.CompareTo(other.handle);
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "Object.Equals", 
    new JSIL.MethodSignature($.Boolean, [$.Object], []), 
    function Object_Equals (obj) {
      return (this === obj) || (this && obj && (this.handle.Equals(obj.handle)));
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "Equals", 
    new JSIL.MethodSignature($.Boolean, [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function Equals (other) {
      return (this === other) || (this && other && (this.handle.Equals(other.handle)));
    }
  );

  $.Method({Static:false, Public:true }, "get_Handle", 
    new JSIL.MethodSignature($mgasms[2].TypeRef("System.IntPtr"), [], []), 
    function get_Handle () {
      return this.handle;
    }
  );

  $.Method({Static:true , Public:true }, "op_Equality", 
    new JSIL.MethodSignature($.Boolean, [$mgasms[3].TypeRef("OpenTK.ContextHandle"), $mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function op_Equality (left, right) {
      return System.IntPtr.op_Equality(left.handle, right.handle);
    }
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    new JSIL.MethodSignature($mgasms[2].TypeRef("System.IntPtr"), [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function op_Explicit (c) {
      return c.handle;
    }
  );

  $.Method({Static:true , Public:true }, "op_Explicit", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.ContextHandle"), [$mgasms[2].TypeRef("System.IntPtr")], []), 
    function op_Explicit (p) {
      return new OpenTK.ContextHandle(p);
    }
  );

  $.Method({Static:true , Public:true }, "op_Inequality", 
    new JSIL.MethodSignature($.Boolean, [$mgasms[3].TypeRef("OpenTK.ContextHandle"), $mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function op_Inequality (left, right) {
      return System.IntPtr.op_Inequality(left.handle, right.handle);
    }
  );

  $.Method({Static:false, Public:true , Virtual:true }, "toString", 
    new JSIL.MethodSignature($.String, [], []), 
    function toString () {
      return this.handle.toString();
    }
  );
});