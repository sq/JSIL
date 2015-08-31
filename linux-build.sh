#!/bin/bash

SCRIPTPATH=`realpath "$BASH_SOURCE"`
JSILDIR=`dirname $SCRIPTPATH`

pushd $JSILDIR

# are the reference assemblies already loaded?
if [ -n "$XBUILD_FRAMEWORK_FOLDERS_PATH" ]; then
  true
else
  # well, install them then
  echo // Installing PCL reference assemblies
  source ./Meta/install-pcl-reference-assemblies.sh
fi

# restore nuget packages
echo // Installing NuGet packages
nuget.exe restore

# build with xna projects disabled
echo // Building
xbuild "/property:Configuration=Debug" "/p:Platform=NoXNA" /v:m JSIL.sln

popd