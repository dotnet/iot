#!/usr/bin/env bash

__scriptpath=$(cd "$(dirname "$0")"; pwd -P)
__rootRepo="$__scriptpath/../.."
__artifactsDir="$__rootRepo/artifacts"
__libgpiodPath="$__artifactsDir/Libgpiod"

sudo cp -r "$__libgpiodPath/lib/" /crossrootfs/arm/usr/local/
if [ $? != 0 ]; then
    echo "Error copying files to crossrootfs"
    exit 1
fi
sudo cp -r "$__libgpiodPath/include/" /crossrootfs/arm/usr/local/
if [ $? != 0 ]; then
    echo "Error copying files to crossrootfs"
    exit 1
fi