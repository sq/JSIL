#!/bin/bash

if curl -S "jsil.org/ci/download.aspx?key=travisci&tag=$JSIL_STORAGE_TAG&password=$JSIL_STORAGE_PASSWORD" -o "/tmp/compilecache.zip"; then
    echo "Failed to download compile cache."
else
    echo "Downloaded compile cache."
    unzip -o "/tmp/compilecache.zip" -d /tmp/JSIL Tests/
    echo "Unpacked compile cache."
fi