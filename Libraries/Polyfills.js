if (typeof (Object.create) !== "function") {
  throw new Error("JSIL requires support for ES5 Object.create");
}
if (typeof (Object.defineProperty) !== "function") {
  throw new Error("JSIL requires support for Object.defineProperty");
}

// Safari does not provide Function.prototype.bind, and we need it.
if (typeof (Function.prototype.bind) !== "function") {
  // Implementation from https://developer.mozilla.org/en/JavaScript/Reference/Global_Objects/Function/bind
  Function.prototype.bind = function( obj ) {
    var slice = [].slice,
        args = slice.call(arguments, 1), 
        self = this, 
        nop = function () {}, 
        bound = function () {
          return self.apply( this instanceof nop ? this : ( obj || {} ), 
                              args.concat( slice.call(arguments) ) );    
        };

    nop.prototype = self.prototype;

    bound.prototype = new nop();

    return bound;
  };
}


if (typeof (Math.fround) === "function") {
  // Math.fround builtin truncates f64 to f32
  // (Note that f32s are often not stored on the heap, but truncation works)

} else if (typeof (Float32Array) !== "undefined") {
  // Simple Math.fround polyfill

  (function () {
    var f32 = new Float32Array(1);

    Math.fround = function (d) {
      return f32[0] = d, f32[0];
    };
  })();
} else {
  // FIXME: This is *inaccurate* and will cause DoubleFloatCasts.cs to fail!
  // Maybe generate a warning on first use? Most code won't care...

  Math.fround = function (d) {
    return +d;
  };
}