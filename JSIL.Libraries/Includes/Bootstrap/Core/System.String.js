// HACK: Unfortunately necessary :-(
String.prototype.Object_Equals = function (rhs) {
  return this === rhs;
};

String.prototype.GetHashCode = function () {
  var h = 0;

  for (var i = 0; i < this.length; i++) {
    h = ((h << 5) - h + this.charCodeAt(i)) & ~0;
  }

  return h;
};