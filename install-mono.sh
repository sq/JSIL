#!/bin/bash

SCRIPTPATH=`readlink -f "$BASH_SOURCE"`
JSILDIR=`dirname $SCRIPTPATH`
MONODIR=$JSILDIR/mono

mkdir $MONODIR
pushd $MONODIR

echo // Downloading mono source
rm *.tar.bz2
curl -sS "http://download.mono-project.com/sources/mono/mono-4.0.4.1.tar.bz2" -o ./mono.tar.bz2

echo // Unpacking
tar -xf mono.tar.bz2

MONOSOURCEDIR=`readlink -f ./mono-*`
pushd $MONOSOURCEDIR

echo // Configuring
./configure --prefix=$MONOSOURCEDIR/build

echo // Making
make

echo // Installing
make install

popd

echo // Done. Built in $MONOSOURCEDIR/build.

popd