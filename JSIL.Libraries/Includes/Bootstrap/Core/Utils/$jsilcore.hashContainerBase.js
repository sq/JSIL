/*? if (!('$jsilcore_hashContainerBase' in __out)) { __out.$jsilcore_hashContainerBase = true; */
$jsilcore.hashContainerBase = function ($) {
  var mscorlib = JSIL.GetCorlib();

  var BucketEntry = function (key, value) {
    this.key = key;
    this.value = value;
  };

  $.RawMethod(false, "$areEqual", function HashContainer_AreEqual(lhs, rhs) {
    if (lhs === rhs)
      return true;

    return JSIL.ObjectEquals(lhs, rhs);
  });

  $.RawMethod(false, "$searchBucket", function HashContainer_SearchBucket(key) {
    var hashCode = JSIL.ObjectHashCode(key, true);
    var bucket = this._dict[hashCode];
    if (!bucket)
      return null;

    for (var i = 0, l = bucket.length; i < l; i++) {
      var bucketEntry = bucket[i];

      if (this.$areEqual(bucketEntry.key, key))
        return bucketEntry;
    }

    return null;
  });

  $.RawMethod(false, "$removeByKey", function HashContainer_Remove(key) {
    var hashCode = JSIL.ObjectHashCode(key, true);
    var bucket = this._dict[hashCode];
    if (!bucket)
      return false;

    for (var i = 0, l = bucket.length; i < l; i++) {
      var bucketEntry = bucket[i];

      if (this.$areEqual(bucketEntry.key, key)) {
        bucket.splice(i, 1);
        this._count -= 1;
        return true;
      }
    }

    return false;
  });

  $.RawMethod(false, "$addToBucket", function HashContainer_Add(key, value) {
    var hashCode = JSIL.ObjectHashCode(key, true);
    var bucket = this._dict[hashCode];
    if (!bucket)
      this._dict[hashCode] = bucket = [];

    bucket.push(new BucketEntry(key, value));
    this._count += 1;
    return value;
  });
};
/*? }*/