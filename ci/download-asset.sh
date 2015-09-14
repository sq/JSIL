#!/bin/bash

SUBTAG=$1
DESTDIR=$2
ZIPFILE=/tmp/$SUBTAG.zip

echo "Downloading $SUBTAG..."
curl -S "jsil.org/ci/download.aspx?key=travisci&tag=$JSIL_STORAGE_TAG-$SUBTAG&password=$JSIL_STORAGE_PASSWORD" -o "$ZIPFILE"
ls -la "$ZIPFILE"
echo "Unarchiving $SUBTAG..."
unzip -q -o "$ZIPFILE" -d "$DESTDIR"
rm -rf "$DESTDIR/$SUBTAG/$SUBTAG"