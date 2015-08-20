JSIL.ImplementExternals("System.Runtime.InteropServices.Marshal", function ($) {
    $.Method({ Static: true, Public: true }, "StructureToPtr",
      (new JSIL.MethodSignature(null, [
            $.Object, $.IntPtr,
            $.Boolean
      ], [])),
      function StructureToPtr(structure, ptr, fDeleteOld) {
          throw new Error('Not implemented');
      }
    );

    $.Method({ Static: true, Public: true }, "SizeOf",
      (new JSIL.MethodSignature($.Int32, [$.Object], [])),
      function SizeOf(structure) {
          var type = JSIL.GetType(structure);
          return JSIL.GetNativeSizeOf(type, true);
      }
    )

    $.Method({ Static: true, Public: true }, "SizeOf",
      (new JSIL.MethodSignature($.Int32, [$jsilcore.TypeRef("System.Type")], [])),
      function SizeOf(type) {
          return JSIL.GetNativeSizeOf(type, true);
      }
    );

    $.Method({ Static: true, Public: true }, "OffsetOf",
      (new JSIL.MethodSignature($.IntPtr, [$jsilcore.TypeRef("System.Type"), $.String], [])),
      function OffsetOf(type, fieldName) {
          var fields = JSIL.GetFieldList(type);

          for (var i = 0, l = fields.length; i < l; i++) {
              var field = fields[i];
              if (field.name === fieldName)
                  return new System.IntPtr(field.offsetBytes);
          }

          throw new System.Exception("No field named '" + fieldName + "' declared in type");
      }
    );

    $.Method({ Static: true, Public: true }, "Copy",
      new JSIL.MethodSignature(null, [
          $.IntPtr, $jsilcore.TypeRef("System.Array", [$.Byte]),
          $.Int32, $.Int32
      ]),
      function Copy(source, destination, startIndex, length) {
          if (!source.pointer)
              JSIL.RuntimeError("Source argument must be a pointer into a pinned buffer, not a raw value");

          var pSource = source.pointer.cast($jsilcore.System.Byte);
          for (var i = 0, l = length | 0, s = startIndex | 0; i < l; i++) {
              var d = (s + i) | 0;

              destination[d] = pSource.getElement(i);
          }
      }
    );

    $.Method({ Static: true, Public: true }, "Copy",
      new JSIL.MethodSignature(null, [
          $jsilcore.TypeRef("System.Array", [$.Byte]), $.Int32, $.IntPtr, $.Int32
      ]),
      function Copy(source, startIndex, destination, length) {
          if (!destination.pointer)
              JSIL.RuntimeError("Destination argument must be a pointer into a pinned buffer, not a raw value");

          var pDest = destination.pointer.cast($jsilcore.System.Byte);
          for (var i = 0, l = length | 0, s = startIndex | 0; i < l; i++) {
              pDest.setElement(
                i,
                source[(s + i) | 0]
              );
          }
      }
    );

    var mallocImpl = function (sizeBytes) {
        var module = JSIL.PInvoke.GetDefaultModule();
        var address = module._malloc(sizeBytes | 0);
        var result = JSIL.PInvoke.CreateIntPtrForModule(module, address);
        return result;
    };

    $.Method({ Static: true, Public: true }, "AllocHGlobal",
      new JSIL.MethodSignature($.IntPtr, [$.IntPtr]),
      function AllocHGlobal(cb) {
          return mallocImpl(cb.ToInt32());
      }
    );

    $.Method({ Static: true, Public: true }, "AllocHGlobal",
      new JSIL.MethodSignature($.IntPtr, [$.Int32]),
      function AllocHGlobal(cb) {
          return mallocImpl(cb);
      }
    );

    $.Method({ Static: true, Public: true }, "FreeHGlobal",
      JSIL.MethodSignature.Action($.IntPtr),
      function FreeHGlobal(hglobal) {
          var module = JSIL.PInvoke.PickModuleForPointer(hglobal, true);
          module._free(hglobal.ToInt32());
      }
    );

});
JSIL.MakeStaticClass("System.Runtime.InteropServices.Marshal", true, [], function ($) {
});