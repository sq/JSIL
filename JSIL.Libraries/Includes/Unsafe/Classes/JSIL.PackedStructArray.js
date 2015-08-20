JSIL.MakeClass("System.Array", "JSIL.PackedStructArray", true, ["T"], function ($) {
    var T = new JSIL.GenericParameter("T", "JSIL.PackedStructArray");
    var TRef = JSIL.Reference.Of(T);

    $.RawMethod(false, ".ctor",
      function PackedStructArray_ctor(byteArray, memoryRange) {
          this.__IsPackedArray__ = true;
          this.bytes = byteArray;
          this.buffer = byteArray.buffer;

          if (memoryRange)
              this.memoryRange = memoryRange;
          else
              this.memoryRange = JSIL.GetMemoryRangeForBuffer(this.buffer);

          this.nativeSize = this.T.__NativeSize__;
          this.elementProxyConstructor = JSIL.$GetStructElementProxyConstructor(this.T);
          this.unmarshalConstructor = JSIL.$GetStructUnmarshalConstructor(this.T);
          this.unmarshaller = JSIL.$GetStructUnmarshaller(this.T);
          this.marshaller = JSIL.$GetStructMarshaller(this.T);
          this.length = (this.memoryRange.length / this.nativeSize) | 0;
          this.byteLength = (this.length * this.nativeSize) | 0;
      }
    );

    $.Method(
      {}, "get_Item",
      new JSIL.MethodSignature(T, [$.Int32], []),
      function PackedStructArray_get_Item(index) {
          var offsetInBytes = (index * this.nativeSize) | 0;
          return new this.unmarshalConstructor(this.bytes, offsetInBytes);
      }
    );

    $.Method(
      {}, "GetItemProxy",
      new JSIL.MethodSignature(T, [$.Int32], []),
      function PackedStructArray_GetItemProxy(index) {
          var offsetInBytes = (index * this.nativeSize) | 0;
          return new this.elementProxyConstructor(this.bytes, offsetInBytes);
      }
    );

    $.Method(
      {}, "GetReference",
      new JSIL.MethodSignature(TRef, [$.Int32], []),
      function PackedStructArray_GetReference(index) {
          return new JSIL.PackedStructArrayElementReference(this, index);
      }
    );

    $.Method(
      {}, "GetItemInto",
      new JSIL.MethodSignature(null, [$.Int32, TRef], []),
      function PackedStructArray_GetItemInto(index, output) {
          var offsetInBytes = (index * this.nativeSize) | 0;
          this.unmarshaller(output.get(), this.bytes, offsetInBytes);
      }
    );

    $.Method(
      {}, "set_Item",
      new JSIL.MethodSignature(null, [$.Int32, T], []),
      function PackedStructArray_set_Item(index, value) {
          var offsetInBytes = (index * this.nativeSize) | 0;
          this.marshaller(value, this.bytes, offsetInBytes);
          return value;
      }
    );

    $.Method(
      {}, "get_Length",
      new JSIL.MethodSignature($.Int32, [], []),
      function PackedStructArray_get_Length() {
          return this.length;
      }
    );

    $.Property({}, "Length");

    $.ImplementInterfaces(
      /* 0 */ $jsilcore.TypeRef("JSIL.Runtime.IPackedArray`1", [T])
    );
});