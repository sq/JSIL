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

//
// General implementation
//

JSIL.AL.$deviceToken = null;
JSIL.AL.getDeviceToken = function () {
  if (!JSIL.AL.$deviceToken)
    JSIL.AL.$deviceToken = new System.IntPtr(1);

  return JSIL.AL.$deviceToken;
};

JSIL.AL.contexts = [];
JSIL.AL.currentContext = null;

JSIL.AL.QUEUE_INTERVAL = 25;
JSIL.AL.QUEUE_LOOKAHEAD = 100;

JSIL.AL.getError = function () {
  if (!JSIL.AL.currentContext) {
    return OpenTK.Audio.OpenAL.ALError.$Cast(0xA004);
  } else {
    // Reset error on get.
    var err = JSIL.AL.currentContext.err;
    JSIL.AL.currentContext.err = OpenTK.Audio.OpenAL.ALError.NoError;
    return err;
  }
};

JSIL.AL.updateSource = function (src) {
  if (src.state !== 0x1012 /* AL_PLAYING */) {
    return;
  }

  var currentTime = JSIL.AL.currentContext.ctx.currentTime;
  var startTime = src.bufferPosition;

  for (var i = src.buffersPlayed; i < src.queue.length; i++) {
    var entry = src.queue[i];

    var startOffset = startTime - currentTime;
    var endTime = startTime + entry.buffer.duration;

    // Clean up old buffers.
    if (currentTime >= endTime) {
      // Update our location in the queue.
      src.bufferPosition = endTime;
      src.buffersPlayed = i + 1;

      // Stop / restart the source when we hit the end.
      if (src.buffersPlayed >= src.queue.length) {
        if (src.loop) {
          JSIL.AL.setSourceState(src, 0x1012 /* AL_PLAYING */);
        } else {
          JSIL.AL.setSourceState(src, 0x1014 /* AL_STOPPED */);
        }
      }
    }
    // Process all buffers that'll be played before the next tick.
    else if (startOffset < (JSIL.AL.QUEUE_LOOKAHEAD / 1000) && !entry.src) {
      // If the start offset is negative, we need to offset the actual buffer.
      var offset = Math.abs(Math.min(startOffset, 0));

      entry.src = JSIL.AL.currentContext.ctx.createBufferSource();
      entry.src.buffer = entry.buffer;
      entry.src.connect(src.gain);
      entry.src.start(startTime, offset);
    }

    startTime = endTime;
  }
};

JSIL.AL.updateSources = function (context) {
  for (var i = 0; i < context.src.length; i++) {
    JSIL.AL.updateSource(context.src[i]);
  }
};

JSIL.AL.setSourceState = function(src, state) {
  if (state === 0x1012 /* AL_PLAYING */) {
    if (src.state !== 0x1013 /* AL_PAUSED */) {
      src.state = 0x1012 /* AL_PLAYING */;
      // Reset our position.
      src.bufferPosition = JSIL.AL.currentContext.ctx.currentTime;
      src.buffersPlayed = 0;
    } else {
      src.state = 0x1012 /* AL_PLAYING */;
      // Use the current offset from src.bufferPosition to resume at the correct point.
      src.bufferPosition = JSIL.AL.currentContext.ctx.currentTime - src.bufferPosition;
    }
    JSIL.AL.stopSourceQueue(src);
    JSIL.AL.updateSource(src);
  } else if (state === 0x1013 /* AL_PAUSED */) {
    if (src.state === 0x1012 /* AL_PLAYING */) {
      src.state = 0x1013 /* AL_PAUSED */;
      // Store off the current offset to restore with on resume.
      src.bufferPosition = JSIL.AL.currentContext.ctx.currentTime - src.bufferPosition;
      JSIL.AL.stopSourceQueue(src);
    }
  } else if (state === 0x1014 /* AL_STOPPED */) {
    if (src.state !== 0x1011 /* AL_INITIAL */) {
      src.state = 0x1014 /* AL_STOPPED */;
      src.buffersPlayed = src.queue.length;
      JSIL.AL.stopSourceQueue(src);
    }
  } else if (state == 0x1011 /* AL_INITIAL */) {
    if (src.state !== 0x1011 /* AL_INITIAL */) {
      src.state = 0x1011 /* AL_INITIAL */;
      src.bufferPosition = 0;
      src.buffersPlayed = 0;
    }
  }
};

JSIL.AL.stopSourceQueue = function(src) {
  for (var i = 0; i < src.queue.length; i++) {
    var entry = src.queue[i];
    if (entry.src) {
      entry.src.stop(0);
      entry.src = null;
    }
  }
};

//
// OpenAL.Alc
//

JSIL.ImplementExternals("OpenTK.Audio.OpenAL.Alc", function ($) {
  $.Method({Static:true , Public:true }, "CloseDevice", 
    new JSIL.MethodSignature($.Boolean, [$mgasms[2].TypeRef("System.IntPtr")], []), 
    function CloseDevice (device) {
      // FIXME
    }
  );

  var createContext = function (device) {
    if (device !== JSIL.AL.getDeviceToken())
      throw new Error("Invalid device");

    var ctx;
    try {
      ctx = new AudioContext();
    } catch (e) {
      try {
        ctx = new webkitAudioContext();
      } catch (e) {}
    }

    if (ctx) {
      var context = {
        ctx: ctx,
        err: 0,
        src: [],
        buf: [],
        originalBitCounts: [],
        interval: setInterval(
          function() { JSIL.AL.updateSources(context); }, JSIL.AL.QUEUE_INTERVAL
        )
      };

      JSIL.AL.contexts.push(context);

      return new OpenTK.ContextHandle(new System.IntPtr(JSIL.AL.contexts.length));
    } else {
      return OpenTK.ContextHandle.Zero;
    }
  };

  $.Method({Static:true , Public:true }, "CreateContext", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.ContextHandle"), [$mgasms[2].TypeRef("System.IntPtr"), $jsilcore.TypeRef("JSIL.Pointer", [$.Int32])], []), 
    function CreateContext (device, attrlist) {
      // FIXME: attrList
      return createContext(device);
    }
  );

  $.Method({Static:true , Public:true }, "CreateContext", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.ContextHandle"), [$mgasms[2].TypeRef("System.IntPtr"), $jsilcore.TypeRef("System.Array", [$.Int32])], []), 
    function CreateContext (device, attriblist) {
      // FIXME: attribList
      return createContext(device);
    }
  );

  $.Method({Static:true , Public:true }, "DestroyContext", 
    new JSIL.MethodSignature(null, [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function DestroyContext (context) {
      // FIXME: Use a service instead of calling clearInterval directly
      var index = context.handle.ToInt32();
      clearInterval(JSIL.AL.contexts[index - 1].interval);
    }
  );

  $.Method({Static:true , Public:true }, "GetContextsDevice", 
    new JSIL.MethodSignature($mgasms[2].TypeRef("System.IntPtr"), [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function GetContextsDevice (context) {
      // FIXME: context
      return JSIL.AL.getDeviceToken();
    }
  );

  $.Method({Static:true , Public:true }, "GetCurrentContext", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.ContextHandle"), [], []), 
    function GetCurrentContext () {
      for (var i = 0; i < JSIL.AL.contexts.length; ++i) {
        if (JSIL.AL.contexts[i] === JSIL.AL.currentContext)
          return new OpenTK.ContextHandle(new System.IntPtr(i + 1));
      }

      return OpenTK.ContextHandle.Zero;
    }
  );

  $.Method({Static:true , Public:true }, "GetError", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Audio.OpenAL.AlcError"), [$mgasms[2].TypeRef("System.IntPtr")], []), 
    function GetError (device) {
      // FIXME
      return OpenTK.Audio.OpenAL.AlcError.NoError;
    }
  );

  $.Method({Static:true , Public:true }, "MakeContextCurrent", 
    new JSIL.MethodSignature($.Boolean, [$mgasms[3].TypeRef("OpenTK.ContextHandle")], []), 
    function MakeContextCurrent (context) {
      var index = context.handle.ToInt32();

      if ((index > 0) && (index <= JSIL.AL.contexts.length)) {
        JSIL.AL.currentContext = JSIL.AL.contexts[index - 1];
        return true;
      } else if (index === 0) {
        JSIL.AL.currentContext = null;
        return true;
      }

      return false;
    }
  );

  $.Method({Static:true , Public:true }, "OpenDevice", 
    new JSIL.MethodSignature($mgasms[2].TypeRef("System.IntPtr"), [$.String], []), 
    function OpenDevice (devicename) {
      // FIXME: devicename      
      return JSIL.AL.getDeviceToken();
    }
  );
});

//
// OpenAL.AL
//

JSIL.ImplementExternals("OpenTK.Audio.OpenAL.AL", function ($interfaceBuilder) {
  var $ = $interfaceBuilder;

  var bufferData = function (buffer, format, data, size, freq) {
    var channels, bytes;

    switch (format) {
      case 0x1100 /* AL_FORMAT_MONO8 */:
        bytes = 1;
        channels = 1;
        break;
      case 0x1101 /* AL_FORMAT_MONO16 */:
        bytes = 2;
        channels = 1;
        break;
      case 0x1102 /* AL_FORMAT_STEREO8 */:
        bytes = 1;
        channels = 2;
        break;
      case 0x1103 /* AL_FORMAT_STEREO16 */:
        bytes = 2;
        channels = 2;
        break;
      default:
        JSIL.AL.currentContext.err = 0xA003 /* AL_INVALID_VALUE */;
        return;
    }

    try {
      JSIL.AL.currentContext.buf[buffer - 1] = JSIL.AL.currentContext.ctx.createBuffer(channels, size / (bytes * channels), freq);
    } catch (e) {
      JSIL.AL.currentContext.err = 0xA003 /* AL_INVALID_VALUE */;
      return;
    }

    var buf = new Array(channels);
    for (var i = 0; i < channels; ++i) {
      buf[i] = JSIL.AL.currentContext.buf[buffer - 1].getChannelData(i);
    }

    JSIL.AL.currentContext.originalBitCounts[buffer - 1] = bytes * 8;

    var memoryRange = JSIL.GetMemoryRangeForBuffer(data.buffer);
    var nativeView = null;

    switch (bytes) {
      case 1:
        nativeView = memoryRange.getView($jsilcore.System.Byte.__Type__);
        break;
      case 2:
        nativeView = memoryRange.getView($jsilcore.System.Int16.__Type__);
        break;
    }

    for (var i = 0; i < size / (bytes * channels); ++i) {
      for (var j = 0; j < channels; ++j) {
        switch (bytes) {
        case 1:
          var val = nativeView[i*channels+j]; // unsigned int8
          buf[j][i] = -1.0 + val * (2/256);
          break;
        case 2:
          var val = nativeView[i*channels+j]; // signed int16?
          buf[j][i] = val / 32768;
          break;
        }
      }
    }
  };

  $.Method({Static:true , Public:true }, "BufferData", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALFormat"), 
        $jsilcore.TypeRef("System.Array", ["!!0"]), $.Int32, 
        $.Int32
      ], ["TBuffer"]), 
    function BufferData$b1 (TBuffer, bid, format, buffer, size, freq) {
      bufferData(bid, format.value, buffer, size, freq);
    }
  );

  $.Method({Static:true , Public:true }, "DeleteBuffer", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function DeleteBuffer (/* ref */ buffer) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteBuffer", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function DeleteBuffer (buffer) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteBuffers", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Pointer", [$.UInt32])], []), 
    function DeleteBuffers (n, buffers) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteBuffers", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Pointer", [$.Int32])], []), 
    function DeleteBuffers (n, buffers) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteBuffers", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function DeleteBuffers (n, /* ref */ buffers) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteBuffers", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], []), 
    function DeleteBuffers (n, /* ref */ buffers) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteBuffers", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.UInt32])], []), 
    function DeleteBuffers (buffers) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteBuffers", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Int32])], []), 
    function DeleteBuffers (buffers) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteSource", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function DeleteSource (/* ref */ source) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteSource", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function DeleteSource (source) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteSources", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Pointer", [$.UInt32])], []), 
    function DeleteSources (n, sources) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteSources", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function DeleteSources (n, /* ref */ sources) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteSources", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], []), 
    function DeleteSources (n, /* ref */ sources) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteSources", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.UInt32])], []), 
    function DeleteSources (sources) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "DeleteSources", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Int32])], []), 
    function DeleteSources (sources) {
      throw new Error('Not implemented');
    }
  );

  var genBuffers = function (count, result) {
    for (var i = 0; i < count; ++i) {
      JSIL.AL.currentContext.buf.push(null);
      JSIL.AL.currentContext.originalBitCounts.push(0);

      result[i] = JSIL.AL.currentContext.buf.length;
    }
  };


  $.Method({Static:true , Public:true }, "GenBuffer", 
    new JSIL.MethodSignature($.Int32, [], []), 
    function GenBuffer () {
      var result = [];
      genBuffers(1, result);
      return result[0];    
    }
  );

  $.Method({Static:true , Public:true }, "GenBuffer", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function GenBuffer (/* ref */ buffer) {
      var result = [];
      genBuffers(1, result);
      buffer.set(result[0]);
    }
  );

  $.Method({Static:true , Public:true }, "GenBuffers", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Int32]), [$.Int32], []), 
    function GenBuffers (n) {
      var result = JSIL.Array.New(System.Int32, n);
      genBuffers(n, result);
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "GenBuffers", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function GenBuffers (n, /* ref */ buffers) {
      if (n !== 1)
        throw new Error("Not implemented");

      var result = [];
      genBuffers(1, result);

      buffers.set(result[0]);      
    }
  );

  $.Method({Static:true , Public:true }, "GenBuffers", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], []), 
    function GenBuffers (n, /* ref */ buffers) {
      if (n !== 1)
        throw new Error("Not implemented");

      var result = [];
      genBuffers(1, result);

      buffers.set(result[0]);      
    }
  );

  var genSources = function (count, result) {
    for (var i = 0; i < count; ++i) {
      var gain = JSIL.AL.currentContext.ctx.createGain();
      gain.connect(JSIL.AL.currentContext.ctx.destination);

      JSIL.AL.currentContext.src.push({
        state: 0x1011 /* AL_INITIAL */,
        queue: [],
        loop: false,
        get refDistance() {
          return this._refDistance || 1;
        },
        set refDistance(val) {
          this._refDistance = val;
          if (this.panner) this.panner.refDistance = val;
        },
        get maxDistance() {
          return this._maxDistance || 10000;
        },
        set maxDistance(val) {
          this._maxDistance = val;
          if (this.panner) this.panner.maxDistance = val;
        },
        get rolloffFactor() {
          return this._rolloffFactor || 1;
        },
        set rolloffFactor(val) {
          this._rolloffFactor = val;
          if (this.panner) this.panner.rolloffFactor = val;
        },
        get position() {
          return this._position || [0, 0, 0];
        },
        set position(val) {
          this._position = val;
          if (this.panner) this.panner.setPosition(val[0], val[1], val[2]);
        },
        get velocity() {
          return this._velocity || [0, 0, 0];
        },
        set velocity(val) {
          this._velocity = val;
          if (this.panner) this.panner.setVelocity(val[0], val[1], val[2]);
        },
        gain: gain,
        panner: null,
        buffersPlayed: 0,
        bufferPosition: 0
      });

      result[i] = JSIL.AL.currentContext.src.length;
    }
  };

  $.Method({Static:true , Public:true }, "GenSource", 
    new JSIL.MethodSignature($.Int32, [], []), 
    function GenSource () {
      var result = [];
      genSources(1, result);
      return result[0];
    }
  );

  $.Method({Static:true , Public:true }, "GenSource", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function GenSource (/* ref */ source) {
      var result = [];
      genSources(1, result);
      source.set(result[0]);
    }
  );

  $.Method({Static:true , Public:true }, "GenSources", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function GenSources (n, /* ref */ sources) {
      if (n !== 1)
        throw new Error("Not implemented");

      var result = [];
      genSources(1, result);
      sources.set(result[0]);
    }
  );

  $.Method({Static:true , Public:true }, "GenSources", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.Int32])], []), 
    function GenSources (n, /* ref */ sources) {
      if (n !== 1)
        throw new Error("Not implemented");

      var result = [];
      genSources(1, result);
      sources.set(result[0]);
    }
  );

  $.Method({Static:true , Public:true }, "GenSources", 
    new JSIL.MethodSignature(null, [$jsilcore.TypeRef("System.Array", [$.Int32])], []), 
    function GenSources (sources) {
      genSources(sources.length, sources);
    }
  );

  $.Method({Static:true , Public:true }, "GenSources", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Int32]), [$.Int32], []), 
    function GenSources (n) {
      var result = JSIL.Array.New(System.Int32, n);
      genSources(n, result);
      return result;
    }
  );

  $.Method({Static:true , Public:true }, "Get", 
    new JSIL.MethodSignature($.String, [$mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALGetString")], []), 
    function Get (param) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "Get", 
    new JSIL.MethodSignature($.Int32, [$mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALGetInteger")], []), 
    function Get (param) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "Get", 
    new JSIL.MethodSignature($.Single, [$mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALGetFloat")], []), 
    function Get (param) {
      throw new Error('Not implemented');
    }
  );

  var getBuffer = function (buffer, param) {
    var buffer = JSIL.AL.currentContext.buf[buffer - 1];

    // FIXME: Handle AL_ enumerants
    switch (param.value) {
      // bits
      case 8194:
        // HACK
        return JSIL.AL.currentContext.originalBitCounts[buffer - 1] || 16;

      // channels
      case 8195:
        return buffer.numberOfChannels;

      default:
        JSIL.Host.warning("alGetBuffer not implemented for param " + param.toString());
        return 0;
    }
  };

  $.Method({Static:true , Public:true }, "GetBuffer", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALGetBufferi"), 
        $jsilcore.TypeRef("JSIL.Reference", [$.Int32])
      ], []), 
    function GetBuffer (bid, param, /* ref */ value) {
      value.set(getBuffer(bid, param));
    }
  );

  $.Method({Static:true , Public:true }, "GetBuffer", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALGetBufferi"), 
        $jsilcore.TypeRef("JSIL.Reference", [$.Int32])
      ], []), 
    function GetBuffer (bid, param, /* ref */ value) {
      value.set(getBuffer(bid, param));
    }
  );

  $.Method({Static:true , Public:true }, "GetError", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALError"), [], []), 
    function GetError () {
      return JSIL.AL.getError();
    }
  );

  $.Method({Static:true , Public:true }, "GetErrorString", 
    new JSIL.MethodSignature($.String, [$mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALError")], []), 
    function GetErrorString (param) {
      throw new Error('Not implemented');
    }
  );

  var getSourceState = function (sid) {
    var src = JSIL.AL.currentContext.src[sid - 1];
    // FIXME
    return OpenTK.Audio.OpenAL.ALSourceState.$Cast(src.state);
  };

  $.Method({Static:true , Public:true }, "GetSourceState", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSourceState"), [$.UInt32], []), 
    function GetSourceState (sid) {
      return getSourceState(sid);
    }
  );

  $.Method({Static:true , Public:true }, "GetSourceState", 
    new JSIL.MethodSignature($mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSourceState"), [$.Int32], []), 
    function GetSourceState (sid) {
      return getSourceState(sid);
    }
  );

  var sourcei = function(source, param, value) {
    var src = JSIL.AL.currentContext.src[source - 1];
    if (!src) {
      JSIL.AL.currentContext.err = 0xA001 /* AL_INVALID_NAME */;
      return;
    }

    switch (param.value) {
    case 0x1007 /* AL_LOOPING */:
      src.loop = (value === 1 /* AL_TRUE */);
      break;

    case 0x1009 /* AL_BUFFER */:
      var buffer = JSIL.AL.currentContext.buf[value - 1];
      if (value == 0) {
        src.queue = [];
      } else {
        src.queue = [{ buffer: buffer }];
      }
      JSIL.AL.updateSource(src);
      break;

    case 0x202 /* AL_SOURCE_RELATIVE */:
      if (value === 1 /* AL_TRUE */) {
        if (src.panner) {
          src.panner = null;
            
          // Disconnect from the panner.
          src.gain.disconnect();

          src.gain.connect(JSIL.AL.currentContext.ctx.destination);
        }
      } else if (value === 0 /* AL_FALSE */) {
        if (!src.panner) {
          var panner = src.panner = JSIL.AL.currentContext.ctx.createPanner();
          panner.panningModel = "equalpower";
          panner.distanceModel = "linear";
          panner.refDistance = src.refDistance;
          panner.maxDistance = src.maxDistance;
          panner.rolloffFactor = src.rolloffFactor;
          panner.setPosition(src.position[0], src.position[1], src.position[2]);
          panner.setVelocity(src.velocity[0], src.velocity[1], src.velocity[2]);
          panner.connect(JSIL.AL.currentContext.ctx.destination);

          // Disconnect from the default source.
          src.gain.disconnect();

          src.gain.connect(panner);
        }
      } else {
        JSIL.AL.currentContext.err = 0xA003 /* AL_INVALID_VALUE */;
      }
      break;

    default:
      JSIL.Host.warning("alSourcei with param " + param.toString() + " not implemented yet");
      JSIL.AL.currentContext.err = 0xA002 /* AL_INVALID_ENUM */;
      break;
    }
  };

  var sourcef = function(source, param, value) {
    var src = JSIL.AL.currentContext.src[source - 1];
    if (!src) {
      JSIL.AL.currentContext.err = 0xA001 /* AL_INVALID_NAME */;
      return;
    }

    switch (param.value) {
    case 0x1003 /* AL_PITCH */:
      JSIL.Host.warning("alSourcef was called with 0x1003 /* AL_PITCH */, but Web Audio does not support static pitch changes");
      break;
    case 0x100A /* AL_GAIN */:
      src.gain.gain.value = value;
      break;
    case 0x1023 /* AL_MAX_DISTANCE */:
      src.maxDistance = value;
      break;
    case 0x1021 /* AL_ROLLOFF_FACTOR */:
      src.rolloffFactor = value;
      break;
    case 0x1020 /* AL_REFERENCE_DISTANCE */:
      src.refDistance = value;
      break;
    default:
      JSIL.AL.currentContext.err = 0xA002 /* AL_INVALID_ENUM */;
      break;
    }
  };

  var source3f = function(source, param, v1, v2, v3) {
    var src = JSIL.AL.currentContext.src[source - 1];
    if (!src) {
      JSIL.AL.currentContext.err = 0xA001 /* AL_INVALID_NAME */;
      return;
    }

    switch (param.value) {
    case 0x1004 /* AL_POSITION */:
      src.position = [v1, v2, v3];
      break;
    case 0x1006 /* AL_VELOCITY */:
      src.velocity = [v1, v2, v3];
      break;
    default:
      JSIL.AL.currentContext.err = 0xA002 /* AL_INVALID_ENUM */;
      break;
    }
  };

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSourcef"), 
        $.Single
      ], []), 
    function Source (sid, param, value) {
      sourcef(sid, param, value);
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSourcef"), 
        $.Single
      ], []), 
    function Source (sid, param, value) {
      sourcef(sid, param, value);
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSource3f"), 
        $.Single, $.Single, 
        $.Single
      ], []), 
    function Source (sid, param, value1, value2, value3) {
      source3f(sid, param, value1, value2, value3);
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSource3f"), 
        $.Single, $.Single, 
        $.Single
      ], []), 
    function Source (sid, param, value1, value2, value3) {
      source3f(sid, param, value1, value2, value3);
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSource3f"), 
        $jsilcore.TypeRef("JSIL.Reference", [$mgasms[3].TypeRef("OpenTK.Vector3")])
      ], []), 
    function Source (sid, param, /* ref */ values) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSource3f"), 
        $jsilcore.TypeRef("JSIL.Reference", [$mgasms[3].TypeRef("OpenTK.Vector3")])
      ], []), 
    function Source (sid, param, /* ref */ values) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSourcei"), 
        $.Int32
      ], []), 
    function Source (sid, param, value) {
      sourcei(sid, param, value);
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSourcei"), 
        $.Int32
      ], []), 
    function Source (sid, param, value) {
      sourcei(sid, param, value);
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSourceb"), 
        $.Boolean
      ], []), 
    function Source (sid, param, value) {
      sourcei(sid, param, value ? 1 : 0);
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSourceb"), 
        $.Boolean
      ], []), 
    function Source (sid, param, value) {
      sourcei(sid, param, value ? 1 : 0);
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSource3i"), 
        $.Int32, $.Int32, 
        $.Int32
      ], []), 
    function Source (sid, param, value1, value2, value3) {
      source3i(sid, param, value1, value2, value3);
    }
  );

  $.Method({Static:true , Public:true }, "Source", 
    new JSIL.MethodSignature(null, [
        $.Int32, $mgasms[3].TypeRef("OpenTK.Audio.OpenAL.ALSource3i"), 
        $.Int32, $.Int32, 
        $.Int32
      ], []), 
    function Source (sid, param, value1, value2, value3) {
      source3i(sid, param, value1, value2, value3);
    }
  );

  $.Method({Static:true , Public:true }, "SourcePause", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Pointer", [$.UInt32])], []), 
    function SourcePause (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourcePause", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("System.Array", [$.UInt32])], []), 
    function SourcePause (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourcePause", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("System.Array", [$.Int32])], []), 
    function SourcePause (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourcePause", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function SourcePause (ns, /* ref */ sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourcePause", 
    new JSIL.MethodSignature(null, [$.UInt32], []), 
    function SourcePause (sid) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourcePause", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function SourcePause (sid) {
      throw new Error('Not implemented');
    }
  );

  var sourcePlay = function (source) {
    var src = JSIL.AL.currentContext.src[source - 1];

    if (!src) {
      JSIL.AL.currentContext.err = 0xA001 /* AL_INVALID_NAME */;
      return;
    }

    JSIL.AL.setSourceState(src, 0x1012 /* AL_PLAYING */);
  };

  $.Method({Static:true , Public:true }, "SourcePlay", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Pointer", [$.UInt32])], []), 
    function SourcePlay (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourcePlay", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("System.Array", [$.UInt32])], []), 
    function SourcePlay (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourcePlay", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("System.Array", [$.Int32])], []), 
    function SourcePlay (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourcePlay", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function SourcePlay (ns, /* ref */ sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourcePlay", 
    new JSIL.MethodSignature(null, [$.UInt32], []), 
    function SourcePlay (sid) {
      sourcePlay(sid);
    }
  );

  $.Method({Static:true , Public:true }, "SourcePlay", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function SourcePlay (sid) {
      sourcePlay(sid);
    }
  );

  $.Method({Static:true , Public:true }, "SourceQueueBuffer", 
    new JSIL.MethodSignature(null, [$.Int32, $.Int32], []), 
    function SourceQueueBuffer (source, buffer) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceQueueBuffers", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $.Int32, 
        $jsilcore.TypeRef("JSIL.Pointer", [$.UInt32])
      ], []), 
    function SourceQueueBuffers (sid, numEntries, bids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceQueueBuffers", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $.Int32, 
        $jsilcore.TypeRef("System.Array", [$.UInt32])
      ], []), 
    function SourceQueueBuffers (sid, numEntries, bids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceQueueBuffers", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $jsilcore.TypeRef("System.Array", [$.Int32])
      ], []), 
    function SourceQueueBuffers (sid, numEntries, bids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceQueueBuffers", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $.Int32, 
        $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])
      ], []), 
    function SourceQueueBuffers (sid, numEntries, /* ref */ bids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceRewind", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Pointer", [$.UInt32])], []), 
    function SourceRewind (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceRewind", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("System.Array", [$.UInt32])], []), 
    function SourceRewind (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceRewind", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("System.Array", [$.Int32])], []), 
    function SourceRewind (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceRewind", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function SourceRewind (ns, /* ref */ sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceRewind", 
    new JSIL.MethodSignature(null, [$.UInt32], []), 
    function SourceRewind (sid) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceRewind", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function SourceRewind (sid) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceStop", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Pointer", [$.UInt32])], []), 
    function SourceStop (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceStop", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("System.Array", [$.UInt32])], []), 
    function SourceStop (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceStop", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("System.Array", [$.Int32])], []), 
    function SourceStop (ns, sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceStop", 
    new JSIL.MethodSignature(null, [$.Int32, $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])], []), 
    function SourceStop (ns, /* ref */ sids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceStop", 
    new JSIL.MethodSignature(null, [$.UInt32], []), 
    function SourceStop (sid) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceStop", 
    new JSIL.MethodSignature(null, [$.Int32], []), 
    function SourceStop (sid) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceUnqueueBuffer", 
    new JSIL.MethodSignature($.Int32, [$.Int32], []), 
    function SourceUnqueueBuffer (sid) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceUnqueueBuffers", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $.Int32, 
        $jsilcore.TypeRef("JSIL.Pointer", [$.UInt32])
      ], []), 
    function SourceUnqueueBuffers (sid, numEntries, bids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceUnqueueBuffers", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $.Int32, 
        $jsilcore.TypeRef("System.Array", [$.UInt32])
      ], []), 
    function SourceUnqueueBuffers (sid, numEntries, bids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceUnqueueBuffers", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $jsilcore.TypeRef("System.Array", [$.Int32])
      ], []), 
    function SourceUnqueueBuffers (sid, numEntries, bids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceUnqueueBuffers", 
    new JSIL.MethodSignature(null, [
        $.UInt32, $.Int32, 
        $jsilcore.TypeRef("JSIL.Reference", [$.UInt32])
      ], []), 
    function SourceUnqueueBuffers (sid, numEntries, /* ref */ bids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceUnqueueBuffers", 
    new JSIL.MethodSignature(null, [
        $.Int32, $.Int32, 
        $jsilcore.TypeRef("JSIL.Reference", [$.Int32])
      ], []), 
    function SourceUnqueueBuffers (sid, numEntries, /* ref */ bids) {
      throw new Error('Not implemented');
    }
  );

  $.Method({Static:true , Public:true }, "SourceUnqueueBuffers", 
    new JSIL.MethodSignature($jsilcore.TypeRef("System.Array", [$.Int32]), [$.Int32, $.Int32], []), 
    function SourceUnqueueBuffers (sid, numEntries) {
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