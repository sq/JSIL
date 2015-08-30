JSIL.MakeClass("JSIL.Reference", "JSIL.MemberReference", true, [], function ($) {
    $.RawMethod(false, ".ctor",
      function MemberReference_ctor(object, memberName) {
          this.object = object;
          this.memberName = memberName;
      }
    );

    $.RawMethod(false, "get",
      function MemberReference_Get() {
          return this.object[this.memberName];
      }
    );

    $.RawMethod(false, "set",
      function MemberReference_Set(value) {
          return this.object[this.memberName] = value;
      }
    );
});