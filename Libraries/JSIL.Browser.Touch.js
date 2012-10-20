function initTouchEvents () {
  JSIL.Host.hasTouchEvents = (typeof (TouchEvent) !== "undefined") && ("createTouch" in document);
  JSIL.Host.isTouchInUse = false;

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
  }, true);
  window.addEventListener("touchmove", function (evt) {
  }, true);
  window.addEventListener("touchend", function (evt) {
    setTouchInUse();
  }, true);
}