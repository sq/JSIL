"use strict";

if (typeof (JSIL) === "undefined") 
  throw new Error("JSIL.Core required");

if (typeof ($jsilopentk) === "undefined")
  throw new Error("OpenTK required");


OpenTK.Service = function () {
};

OpenTK.Service.prototype.StartRunLoop = function (platform) {
  var gameWindow = platform._window.window;
  var eventArgs = new OpenTK.FrameEventArgs();

  var dispatcher = function () {
    try {
      gameWindow.UpdateFrame(platform, eventArgs);
      gameWindow.RenderFrame(platform, eventArgs);
      
      JSIL.Host.scheduleTick(dispatcher);
    } finally {
      platform.RaiseAsyncRunLoopEnded();
    }
  };

  JSIL.Host.scheduleTick(dispatcher);
};


(function () {
  JSIL.Host.registerServices({
    opentk: new OpenTK.Service()
  });
})();