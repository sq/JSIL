#!/bin/bash

SUBTAG=$1
SOURCEDIR=$2
ZIPFILE=/tmp/$SUBTAG.zip

rm "$ZIPFILE"
echo "Archiving $SUBTAG..."
zip -3 -r -q "$ZIPFILE" "$SOURCEDIR"
echo "Uploading $SUBTAG..."
curl -S -X POST "jsil.org/ci/upload.aspx?key=travisci&tag=$JSIL_STORAGE_TAG-$SUBTAG&password=$JSIL_STORAGE_PASSWORD" --data-binary "@$ZIPFILE"
echo "Done uploading $SUBTAG."