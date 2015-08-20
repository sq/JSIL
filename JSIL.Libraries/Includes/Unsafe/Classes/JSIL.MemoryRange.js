JSIL.MakeClass("System.Object", "JSIL.MemoryRange", true, [], function ($) {
    $.RawMethod(false, ".ctor",
      function MemoryRange_ctor(buffer, offset, length) {
          this.buffer = buffer;
          this.offset = offset | 0;

          if (typeof (length) === "number") {
              this.length = length | 0;
          } else {
              this.length = buffer.byteLength;
          }

          if (this.offset < 0)
              JSIL.RuntimeError("MemoryRange offset must be >= 0");
          else if (this.length < 0)
              JSIL.RuntimeError("MemoryRange length must be >= 0");

          if (typeof (Map) !== "undefined") {
              this.viewCache = new Map();
              this.viewCacheIsMap = true;
          } else {
              this.viewCache = JSIL.CreateDictionaryObject(null);
              this.viewCacheIsMap = false;
          }

          this.viewCacheByElementType = JSIL.CreateDictionaryObject(null);
      }
    );

    $.RawMethod(false, "getCachedView",
      function MemoryRange_getCachedView(key) {
          if (this.viewCacheIsMap) {
              return this.viewCache.get(key);
          } else {
              var ctorKey = key.name || String(key.constructor);
              return this.viewCache[ctorKey];
          }
      }
    );

    $.RawMethod(false, "setCachedView",
      function MemoryRange_setCachedView(key, value) {
          if (this.viewCacheIsMap) {
              this.viewCache.set(key, value);
          } else {
              var ctorKey = key.name || String(key.constructor);
              this.viewCache[ctorKey] = value;
          }
      }
    );

    $.RawMethod(false, "storeExistingView",
      function MemoryRange_storeExistingView(view) {
          var arrayCtor = Object.getPrototypeOf(view);

          var cachedView = this.getCachedView(arrayCtor);
          if (cachedView) {
              // FIXME: I don't think this is actually a problem?
              //  I'm not really sure why it happens, though...
              /*
              if (cachedView !== view)
                JSIL.RuntimeError("A different view is already stored for this element type");
              */
          } else {
              this.setCachedView(arrayCtor, view);
          }
      }
    );

    $.RawMethod(false, "getView",
      function MemoryRange_getView(elementTypeObject, byteFallback) {
          var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(elementTypeObject, byteFallback);
          if (!arrayCtor)
              return null;

          var result = this.getCachedView(arrayCtor);
          if (!result) {
              var elementCount = (this.length / arrayCtor.BYTES_PER_ELEMENT) | 0;
              result = new arrayCtor(this.buffer, this.offset, elementCount);
              this.setCachedView(arrayCtor, result);
          }

          return result;
      }
    );
});