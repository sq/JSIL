var TypedObjects = (function () {
"use strict";  

var TYPED_OBJECT = {}
function Storage(arrayBuffer, isOpaque) {
  this.arrayBuffer = arrayBuffer;
  this.opaque = isOpaque;
  if (isOpaque)
    // we pretend reference type size is 1 byte
    this.opaqueBuffer = new Array(this.arrayBuffer.byteLength);
}

function setReadOnly(obj, name, value) {
  Object.defineProperty(obj, name,
    { value: value, writable: false, configurable: false });
}

function valueType(size, View, name) {
  var result = {}
  result._size = size;
  result._createView = function(s, byteOffset, length) {
    return new View(s.arrayBuffer, byteOffset, length);
  }
  result._createView.BYTES_PER_ELEMENT = View.BYTES_PER_ELEMENT;
  result._getItem = function(view, offset) { return view[offset]; };
  result._setItem = function(view, offset, v) { view[offset] = v; };
  result._name = name;
  result._alignment = size;
  result._clazz = TYPED_OBJECT;
  result._size = size;
  result._opaque = false;

  setReadOnly(result, 'variable', false);
  setReadOnly(result, 'opaque', false);
  setReadOnly(result, 'byteLength', size);
  setReadOnly(result, 'byteAlignment', size);

  return result;
}

function padTo(size, elemSize) {
  if (size % elemSize == 0) return size;
  return size + (elemSize - (size % elemSize));
}

// Emulate typed array for structs and arrays
function StructView(S) {
  var result = function(storage, offset, length) {
    if (!offset) { 
      offset = 0;
      length = storage.arrayBuffer.byteLength / S._alignment;
    }
    this._storage = storage;
    this._offset = offset;
  }

  result.prototype.getItem = function(index) {
    return new S(this._storage, this._offset + index * S._alignment);
  } 

  result.prototype.setItem = function(index, v) {
    new S(this._storage, this._offset + index * S._alignment, v);
  }
  return function(s, o, l) { return new result(s, o, l); }
}

var OpaqueView = function(storage, offset, length) {
  this._storage = storage;
}

OpaqueView.prototype.getItem = function(index) { 
  return this._storage.opaqueBuffer[index]; 
}
OpaqueView.prototype.setItem = function(index, v) { 
  this._storage.opaqueBuffer[index] = v; 
}

var createOpaqueView = function(s, o, l) { return new OpaqueView(s, o, l); }
createOpaqueView.BYTES_PER_ELEMENT = 1;

function referenceType(name) {
  var result = {};
  result._size = 1;
  result._createView = createOpaqueView;
  result._getItem = function(view, offset) { return view.getItem(offset); };
  result._setItem = function(view, offset, v) { view.setItem(offset, v); };
  result._name = name;
  result._alignment = 1;
  result._clazz = TYPED_OBJECT;
  result._size = 1;
  result._opaque = true;
  return result;
}

var any = referenceType("any");
var object = referenceType("object");
var string = referenceType("string"); // TODO: string conversions

function StructType(typeObj) {
  var props = Object.getOwnPropertyNames(typeObj);

  var internals = {};

  var viewTypes = [];
  var viewAccessors = [];

  var fieldOffsets = {};
  var fieldTypes = {};

  var size = 0;
  var maxElemAlign = 0;
  var opaque = false;
  for (var i = 0; i < props.length; i++) {
    var pName = props[i];
    var pType = typeObj[pName];
    if (pType._clazz !== TYPED_OBJECT)
      throw new TypeError("Property " + pName + ": unknown type");
    if (typeof pType._size == "undefined")
      throw new TypeError("Type " + pType._name + " is variable-length");
    size = padTo(size, pType._alignment);

    var viewTypeIdx = viewTypes.indexOf(pType._createView);
    if (viewTypeIdx < 0) {
      viewTypeIdx = viewTypes.length;
      viewTypes.push(pType._createView);
    }

    internals[pName] = {
      viewTypeIdx : viewTypeIdx,
      offset : size / pType._alignment,
      byteOffset : size,
      type : pType
    }

    fieldOffsets[pName] = size;
    fieldTypes[pName] = pType;

    if (pType._alignment > maxElemAlign) {
      maxElemAlign = pType._alignment;
    }

    size += pType._size;
    opaque = opaque || pType._opaque;
  }

  size = padTo(size, maxElemAlign);
  
  var result = function(o, offset, o1) {
    var views = new Array(viewTypes.length);

    var storage;
    if (o instanceof ArrayBuffer) {
      if (opaque)
        throw new TypeError("Can't create opaque type over ArrayBuffer");
      var ab = o;
      o = new Storage(ab, false);
    }
    if (o instanceof Storage) {
      storage = o;
      if (opaque && !storage.opaque)
        throw new TypeError("Can't create opaque type over non-opaque storage");
      for (var i = 0; i < viewTypes.length; i++) {
        views[i] = viewTypes[i](storage, offset, size / viewTypes[i].BYTES_PER_ELEMENT);
      }
    } else {
      var storage = new Storage(new ArrayBuffer(size), opaque);
      for (var i = 0; i < viewTypes.length; i++) {
        views[i] = viewTypes[i](storage);
      }
    }
    var opaqueInstance = opaque || storage.opaque;
    for (var i = 0; i < props.length; i++) {
      var obj = this;
      (function() {
      var pName = props[i];
      var internalDescr = internals[pName];
      var view = views[internalDescr.viewTypeIdx];
      var offset = internalDescr.offset;
      Object.defineProperty(
          obj,
          pName,
          { configurable: false,
            enumerable:   true,
            get: function() { 
              return internalDescr.type._getItem(view, offset); },
            set: function(v) { 
              internalDescr.type._setItem(view, offset, v); }
          }
          );
      }) ();
    }   
    if (!(o instanceof Storage) && o) {
      for (var i = 0; i < props.length; i++) {
        pName = props[i];
        this[pName] = o[pName]; 
      }
    } else if (o1) {
      for (var i = 0; i < props.length; i++) {
        pName = props[i];
        this[pName] = o1[pName]; 
      }
    }

    this._opaque = opaqueInstance;
    if (!opaqueInstance) {
      this._storage = { buffer : storage.arrayBuffer, 
        byteOffset : offset ? offset : 0,
        byteLength : size
      }
    }
    Object.preventExtensions(this);
  }
  result._size = size;
  result._alignment = maxElemAlign;
  result._createView = StructView(result);
  result._createView.BYTES_PER_ELEMENT = size;
  result._getItem = function(view, offset) { return view.getItem(offset); };
  result._setItem = function(view, offset, v) { view.setItem(offset, v); };
  result._clazz = TYPED_OBJECT;
  result._opaque = opaque;

  setReadOnly(result, 'variable', false);
  setReadOnly(result, 'opaque', opaque);
  if (!opaque) {
    setReadOnly(result, 'byteLength', size);
    setReadOnly(result, 'byteAlignment', maxElemAlign);
    setReadOnly(result, 'fieldOffsets', fieldOffsets);
  }
  setReadOnly(result, 'fieldTypes', fieldTypes);

  if (!opaque) {
    result.storage = function(o) { 
      if (o._opaque) throw new TypeError("Can't access storage of opaque instance");
      return o._storage; 
    };
  }
  return result;
}

function ArrayType(elementType, length) {
  if (elementType._clazz !== TYPED_OBJECT) {
    throw new TypeError("Not a type");
  }
  if (typeof elementType._size == "undefined") {
    throw new TypeError("Element type cannot be of variable length");
  }

  var fixedLength = typeof length != "undefined";
  var opaque = elementType._opaque;

  var result = function(o, byteOffset, o1) {
    var self = this;
    var view;
    var viewLength;
    byteOffset = byteOffset ? byteOffset : 0;
    var storage;
    if (fixedLength) {
      viewLength = length; 
      if (o instanceof ArrayBuffer) {
        if (opaque)
          throw new TypeError("Can't create a view of opaque type over an array buffer");
        o = new Storage(o, opaque);
      }
      if (o instanceof Storage) {
        if (byteOffset % elementType._size != 0)
          throw new RangeError("ArrayBuffer size must be a multiple of " + elementType._size);
        storage = o;
        view = elementType._createView(storage, byteOffset, viewLength);
      } else {
        var storage = new Storage(new ArrayBuffer(viewLength * elementType._size), opaque);
        view = elementType._createView(storage);
      }
    } else {
      viewLength = o;
      var storage = new Storage(new ArrayBuffer(viewLength * elementType._size), opaque);
      view = elementType._createView(storage, 0, viewLength);
    }

    for (var i = 0; i < viewLength; i++) {
      (function() {
        var off = i * elementType._size / elementType._alignment;
        Object.defineProperty(
          self,
          i,
          { configurable:false,
            enumerable:true,
            get: function() {
              return elementType._getItem(view, off);
            },
            set: function(v) {
              elementType._setItem(view, off, v);
            }
          }
      );
      })()
    }
    var opaqueInstance = opaque || storage.opaque;
    self._opaque = opaqueInstance;
    Object.defineProperty(
      self,
      "length",
      { configurable:false,
        enumerable:false,
        writable:false,
        value: viewLength
      });

    if (!opaqueInstance) {
      Object.defineProperty(
          self,
          "byteOffset",
          { configurable:false,
            enumerable:false,
            writable:false,
            value:byteOffset
          });
      Object.defineProperty(
          self,
          "byteLength",
          { configurable:false,
            enumerable:false,
            writeable:false,
            value:elementType._size * viewLength
          });
      if (!opaqueInstance) {
        self._storage = { 
          byteOffset: byteOffset,
          byteLength: self.byteLength,
          buffer: storage.arrayBuffer 
        }
      }
    }
    
    if (fixedLength && !(o instanceof Storage) && o) {
      for (i = 0; i < Math.min(viewLength, o.length); i++) {
        self[i] = o[i];
      }
    } else if (o1) {
      for (i = 0; i < Math.min(viewLength, o1.length); i++) {
        self[i] = o1[i];
      }
    }
    Object.preventExtensions(self);
  }

  
  if (fixedLength)
    result._size = length * elementType._size;
  result._alignment = elementType._alignment;
  result._createView = StructView(result);
  result._getItem = function(view, offset) { return view.getItem(offset); };
  result._setItem = function(view, offset, v) { view.setItem(offset, v); };
  result._clazz = TYPED_OBJECT;
  result._opaque = opaque;

  setReadOnly(result, 'variable', !fixedLength);
  setReadOnly(result, 'opaque', opaque);
  if (!opaque) {
    if (fixedLength)
      setReadOnly(result, 'byteLength', result._size);
    setReadOnly(result, 'byteAlignment', result._alignment);
  }
  setReadOnly(result, 'elementType', elementType);

  if (!opaque) {
    result.storage = function(o) {
      if (o._opaque) {
        throw new TypeError("Can't access storage of opaque instance");
      }
      return o._storage;
    }
  }

  return result;
}

return {
  int8 : valueType(1, Int8Array, "int8"),
  uint8 : valueType(1, Uint8Array, "uint8"),
  uint8clamped : valueType(1, Uint8ClampedArray, "uint8"),
  int16 : valueType(2, Int16Array, "int16"),
  uint16 : valueType(2, Uint16Array, "uint16"),
  int32 : valueType(4, Int32Array, "int32"),
  uint32 : valueType(4, Uint32Array, "uint32"),
  float32 : valueType(4, Float32Array, "float32"),
  float64 : valueType(8, Float64Array, "float64"),

  any : any,
  object : object,
  string : string,

  StructType: StructType,
  ArrayType: ArrayType
};
})();
