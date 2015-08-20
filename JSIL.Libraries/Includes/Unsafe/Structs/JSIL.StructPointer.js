JSIL.MakeStruct("JSIL.Pointer", "JSIL.StructPointer", true, [], function ($) {
    function ElementTypeRecord(elementType) {
        this.type = elementType;
        this.typeId = elementType.__TypeId__;
        this.nativeSize = elementType.__NativeSize__;
        this.unmarshalConstructor = JSIL.$GetStructUnmarshalConstructor(elementType);
        // this.unmarshaller = JSIL.$GetStructUnmarshaller(structType);
        this.marshaller = JSIL.$GetStructMarshaller(elementType);
    };

    var elementTypeRecords = JSIL.CreateDictionaryObject(null);

    function getElementTypeRecord(elementType) {
        var result = elementTypeRecords[elementType.__TypeId__];
        if (!result)
            result = elementTypeRecords[elementType.__TypeId__] = new ElementTypeRecord(elementType);

        return result;
    };

    $.RawMethod(false, ".ctor",
      function StructPointer_ctor(elementType, memoryRange, view, offsetInBytes) {
          if (arguments.length !== 4)
              JSIL.RuntimeError("Pointer ctor expects (elementType, memoryRange, view, offsetInBytes)");

          this.typeRecord = getElementTypeRecord(elementType);
          this.memoryRange = memoryRange;
          this.view = view;
          this.offsetInBytes = offsetInBytes | 0;
          this.proxy = null;
      }
    );


    $.RawMethod(false, "get_elementType",
      function StructPointer_get_elementType() {
          return this.typeRecord.type;
      }
    );

    $.Property({}, "elementType");


    $.RawMethod(false, "__CopyMembers__",
      function StructPointer_CopyMembers(source, target) {
          target.typeRecord = source.typeRecord;
          target.memoryRange = source.memoryRange;
          target.view = source.view;
          target.offsetInBytes = source.offsetInBytes;
          target.proxy = null;
      }
    );

    $.RawMethod(false, "add",
      function StructPointer_Add(offsetInBytes, modifyInPlace) {
          if (modifyInPlace === true) {
              this.offsetInBytes = (this.offsetInBytes + offsetInBytes) | 0;
          } else {
              return new JSIL.StructPointer(
                this.elementType,
                this.memoryRange, this.view,
                (this.offsetInBytes + offsetInBytes) | 0
              );
          }
      }
    );

    $.RawMethod(false, "addElements",
      function StructPointer_AddElements(offsetInElements, modifyInPlace) {
          if (modifyInPlace === true) {
              this.offsetInBytes = (this.offsetInBytes + ((offsetInElements * this.typeRecord.nativeSize) | 0)) | 0;
          } else {
              return new JSIL.StructPointer(
                this.elementType,
                this.memoryRange, this.view,
                (this.offsetInBytes + ((offsetInElements * this.typeRecord.nativeSize) | 0)) | 0
              );
          }
      }
    );

    $.RawMethod(false, "get",
      function StructPointer_Get() {
          var result = new this.typeRecord.unmarshalConstructor(this.view, this.offsetInBytes);
          return result;
      }
    );

    $.RawMethod(false, "getProxy",
      function StructPointer_GetProxy() {
          if (this.proxy === null)
              this.proxy = JSIL.MakeElementProxy(this.elementType);

          this.proxy.retargetBytes(this.view, this.offsetInBytes);
          return this.proxy;
      }
    );

    $.RawMethod(false, "set",
      function StructPointer_Set(value) {
          this.typeRecord.marshaller(value, this.view, this.offsetInBytes);
          return value;
      }
    );

    $.RawMethod(false, "getElement",
      function StructPointer_GetElement(offsetInElements) {
          var record = this.typeRecord;
          var offsetInBytes = (this.offsetInBytes + (offsetInElements * record.nativeSize) | 0) | 0;

          var result = new record.unmarshalConstructor(this.view, offsetInBytes);
          return result;
      }
    );

    $.RawMethod(false, "setElement",
      function StructPointer_SetElement(offsetInElements, value) {
          var record = this.typeRecord;
          var offsetInBytes = (this.offsetInBytes + (offsetInElements * record.nativeSize) | 0) | 0;

          record.marshaller(value, this.view, offsetInBytes);
          return value;
      }
    );

    $.RawMethod(false, "getOffset",
      function StructPointer_GetOffset(offsetInBytes) {
          var result = new this.typeRecord.unmarshalConstructor(this.view, (this.offsetInBytes + offsetInBytes) | 0);
          return result;
      }
    );

    $.RawMethod(false, "setOffset",
      function StructPointer_SetOffset(offsetInBytes, value) {
          this.typeRecord.marshaller(value, this.view, (this.offsetInBytes + offsetInBytes) | 0);
          return value;
      }
    );
});