function initTouchEvents () {
  JSIL.Host.hasTouchEvents = (typeof (TouchEvent) !== "undefined");
  JSIL.Host.isTouchInUse = false;
  JSIL.Host.currentNativeTouches = [];

  if (!JSIL.Host.hasTouchEvents)
    return;

  var setTouchInUse = function () {
    if (JSIL.Host.isTouchInUse)
      return;

    JSIL.Host.isTouchInUse = true;
    if (jsilConfig && jsilConfig.onTouchInUse)
      jsilConfig.onTouchInUse();
  };

  window.addEventListener("touchstart", function (evt) {
    setTouchInUse();

    JSIL.Host.currentNativeTouches = evt.touches;
  }, true);
  
  window.addEventListener("touchmove", function (evt) {
    if (!jsilConfig || !jsilConfig.suppressTouchMoveDefault)
      evt.preventDefault();

    JSIL.Host.currentNativeTouches = evt.touches;
  }, true);

  window.addEventListener("touchend", function (evt) {
    setTouchInUse();

    JSIL.Host.currentNativeTouches = evt.touches;
  }, true);
}