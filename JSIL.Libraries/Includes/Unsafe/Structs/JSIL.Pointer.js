JSIL.MakeStruct("System.ValueType", "JSIL.Pointer", true, ["T"], function ($) {
    var shiftTable = [];
    for (var i = 0; i < 256; i++) {
        shiftTable[i] = (Math.log(i) / Math.LN2) | 0;
    }

    $.SetValue("__IsPointer__", true);

    function Pointer_ctor(elementType, memoryRange, view, offsetInBytes) {
        if (arguments.length !== 4)
            JSIL.RuntimeError("Pointer ctor expects (elementType, memoryRange, view, offsetInBytes)");

        this.memoryRange = memoryRange;
        this.view = view;
        this.offsetInBytes = offsetInBytes | 0;

        if (this.view) {
            this.shift = shiftTable[this.view.BYTES_PER_ELEMENT] | 0;
        } else {
            this.shift = 0;
        }

        this.offsetInElements = offsetInBytes >> this.shift;
        this.elementType = elementType;
    };

    $.RawMethod(false, ".ctor", Pointer_ctor);

    $.RawMethod(false, "__CopyMembers__",
      function Pointer_CopyMembers(source, target) {
          target.memoryRange = source.memoryRange;
          target.view = source.view;
          target.offsetInBytes = source.offsetInBytes;
          target.offsetInElements = source.offsetInElements;
          target.shift = source.shift;
          target.elementType = source.elementType;
      }
    );

    $.RawMethod(false, "get",
      function Pointer_Get() {
          return this.view[this.offsetInElements];
      }
    );

    $.RawMethod(false, "set",
      function Pointer_Set(value) {
          this.view[this.offsetInElements] = value;
      }
    );

    $.RawMethod(false, "getElement",
      function Pointer_GetElement(offsetInElements) {
          return this.view[(this.offsetInElements + offsetInElements) | 0];
      }
    );

    $.RawMethod(false, "setElement",
      function Pointer_SetElement(offsetInElements, value) {
          this.view[(this.offsetInElements + offsetInElements) | 0] = value;
      }
    );

    $.RawMethod(false, "getOffset",
      function Pointer_GetOffset(offsetInBytes) {
          var index = ((this.offsetInBytes + offsetInBytes) | 0) >> this.shift;
          return this.view[index];
      }
    );

    $.RawMethod(false, "setOffset",
      function Pointer_SetOffset(offsetInBytes, value) {
          var index = ((this.offsetInBytes + offsetInBytes) | 0) >> this.shift;
          this.view[index] = value;
      }
    );

    $.RawMethod(false, "cast",
      function Pointer_Cast(elementType) {
          var typeObject =
            elementType.__Type__ || elementType;

          var view = this.memoryRange.getView(typeObject, true);
          return JSIL.NewPointer(typeObject, this.memoryRange, view, this.offsetInBytes);
      }
    );

    $.RawMethod(false, "asView",
      function Pointer_asView(elementType, sizeInBytes) {
          var underlyingBufferSize = (this.memoryRange.buffer.byteLength - this.offsetInBytes) | 0;

          // FIXME: Maybe make this null instead? Int-only is probably better.
          sizeInBytes = sizeInBytes | 0;
          if (sizeInBytes === -1)
              sizeInBytes = underlyingBufferSize;

          var arrayCtor = JSIL.GetTypedArrayConstructorForElementType(elementType.__Type__, true);
          var offsetInElements = (this.offsetBytes / arrayCtor.BYTES_PER_ELEMENT) | 0;
          var sizeInElements = ((sizeInBytes | 0) / arrayCtor.BYTES_PER_ELEMENT) | 0;

          if ((this.offsetInBytes % arrayCtor.BYTES_PER_ELEMENT) !== 0)
              JSIL.RuntimeError("Pointer must be element-aligned");
          if ((sizeInBytes % arrayCtor.BYTES_PER_ELEMENT) !== 0)
              JSIL.RuntimeError("Size must be an integral multiple of element size");

          // Where possible, return a cached view to avoid creating garbage.
          // FIXME: Maybe don't even apply the size constraint here?
          if ((offsetInElements === 0) && (sizeInBytes === underlyingBufferSize))
              return this.memoryRange.getView(elementType.__Type__, true);

          var view = new arrayCtor(this.memoryRange.buffer, offsetInElements, sizeInElements);
          return view;
      }
    );

    $.RawMethod(false, "add",
      function Pointer_Add(offsetInBytes, modifyInPlace) {
          if (modifyInPlace === true) {
              this.offsetInBytes = (this.offsetInBytes + offsetInBytes) | 0;
              this.offsetInElements = this.offsetInBytes >> this.shift;
          } else {
              var ctor = this.__ThisType__.__PublicInterface__;
              return new (ctor)(
                this.elementType, this.memoryRange, this.view, (this.offsetInBytes + offsetInBytes) | 0
              );
          }
      }
    );

    $.RawMethod(false, "addElements",
      function Pointer_AddElements(offsetInElements, modifyInPlace) {
          if (modifyInPlace === true) {
              this.offsetInElements = (this.offsetInElements + offsetInElements) | 0;
              this.offsetInBytes = (this.offsetInBytes + (offsetInElements << this.shift)) | 0;
          } else {
              var ctor = this.__ThisType__.__PublicInterface__;
              return new (ctor)(
                this.elementType, this.memoryRange, this.view, (this.offsetInBytes + (offsetInElements << this.shift)) | 0
              );
          }
      }
    );

    $.RawMethod(false, "deltaBytes",
      function Pointer_DeltaBytes(otherPointer) {
          if (otherPointer.memoryRange.buffer !== this.memoryRange.buffer)
              JSIL.RuntimeError("Cannot subtract two pointers from different pinned buffers");

          return (this.offsetInBytes - otherPointer.offsetInBytes) | 0;
      }
    );

    function getBuffer(ptr) {
        if (ptr.memoryRange !== null)
            return ptr.memoryRange.buffer;
        else
            return null;
    };

    $.RawMethod(false, "equals",
      function Pointer_Equals(rhs) {
          if (rhs === null)
              return false;
          else if (rhs === this)
              return true;
          else
              return (getBuffer(this) === getBuffer(rhs)) &&
                (this.offsetInBytes === rhs.offsetInBytes);
      }
    );

    $.RawMethod(false, "lessThan",
      function Pointer_LessThan(rhs) {
          if (rhs === null)
              return false;
          else
              return (getBuffer(this) === getBuffer(rhs)) &&
                (this.offsetInBytes < rhs.offsetInBytes);
      }
    );

    $.RawMethod(false, "greaterThan",
      function Pointer_GreaterThan(rhs) {
          if (rhs === null)
              return false;
          else
              return (getBuffer(this) === getBuffer(rhs)) &&
                (this.offsetInBytes > rhs.offsetInBytes);
      }
    );

    $.Method({ Static: false, Public: true }, "Object.Equals",
      new JSIL.MethodSignature($.Boolean, [$.Object]),
      function Pointer_Equals(rhs) {
          return this.equals(rhs);
      }
    );

    $.RawMethod(false, "toString",
      function Pointer_ToString() {
          return "<ptr " + this.view + " + " + this.offsetInBytes + ">";
      }
    );
});