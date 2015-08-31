#!/bin/bash

SUBTAG=$1
SOURCEDIR=$2
ZIPFILE=/tmp/$SUBTAG.zip

echo "Uploading $SUBTAG..."
rm $ZIPFILE
zip -3 -r -q $ZIPFILE "$SOURCEDIR"
curl -S -X POST "jsil.org/ci/upload.aspx?key=travisci&tag=$JSIL_STORAGE_TAG-$SUBTAG&password=$JSIL_STORAGE_PASSWORD" --data-binary "@$ZIPFILE"
echo "Done uploading $SUBTAG."