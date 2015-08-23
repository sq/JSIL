JSIL.ImplementExternals(
  "System.Threading.SemaphoreSlim", function ($) {
      $.Method({ Static: false, Public: true }, ".ctor",
        (new JSIL.MethodSignature(null, [$.Int32], [])),
        function _ctor(initialCount) {
            this._count = initialCount;

            this._tcs_queue = new (System.Collections.Generic.Queue$b1.Of(System.Threading.Tasks.TaskCompletionSource$b1.Of(System.Boolean)))();
        }
      );

      $.Method({ Static: false, Public: true }, ".ctor",
          (new JSIL.MethodSignature(null, [$.Int32, $.Int32], [])),
          function _ctor(initialCount, maxCount) {
              // FIXME: Implement MaxCount ctor for SemaphoreSlim
              this._count = initialCount;
              this._max_count = maxCount;

              this._tcs_queue = new (System.Collections.Generic.Queue$b1.Of(System.Threading.Tasks.TaskCompletionSource$b1.Of(System.Boolean)))();
          }
      );

      $.Method({ Static: false, Public: true }, "WaitAsync",
        (new JSIL.MethodSignature($jsilcore.TypeRef("System.Threading.Tasks.Task"), [], [])),
        function WaitAsync() {
            var tcs = new (System.Threading.Tasks.TaskCompletionSource$b1.Of(System.Boolean))();
            if (this._count > 0) {
                tcs.TrySetResult(true);
            } else {
                this._tcs_queue.Enqueue(tcs);
            }
            this._count--;
            return tcs.Task;
        }
      );

      $.Method({ Static: false, Public: true }, "Release",
        (new JSIL.MethodSignature($.Int32, [], [])),
        function Release() {
            this._count++;
            if (this._tcs_queue.Count > 0) {
                var tcs = this._tcs_queue.Dequeue();
                tcs.TrySetResult(true);
            }
        }
      );

      $.Method({ Static: false, Public: true }, "get_CurrentCount",
        (new JSIL.MethodSignature($.Int32, [], [])),
        function get_CurrentCount() {
            return this._count;
        }
      );
  }
);