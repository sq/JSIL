#!/bin/bash

if curl "jsil.org/ci/download.aspx?key=travisci-compilecache&password=$JSIL_STORAGE_PASSWORD" -o "/tmp/compilecache.zip"; then
    echo "Failed to download compile cache."
else
    echo "Downloaded and unpacked compile cache."
    unzip -o "/tmp/compilecache.zip" -d /tmp/JSIL Tests/
fi