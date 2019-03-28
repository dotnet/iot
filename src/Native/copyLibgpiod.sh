#!/usr/bin/env bash

__scriptpath=$(cd "$(dirname "$0")"; pwd -P)
__rootRepo="$__scriptpath/../.."
__artifactsDir="$__rootRepo/artifacts"

__libgpiodPath="$__artifactsDir/libgpiod"

mkdir -p "$__libgpiodPath"

docker create --name iot microsoft/dotnet-buildtools-prereqs:debian-iot-arm32v7-be5b37d-20190211200049
docker cp iot:/usr/local/lib "$__libgpiodPath"
if [ $? != 0 ]; then
    echo "Failed to extract the libgpiod lib assets."
    exit 1
fi
docker cp iot:/usr/local/include "$__libgpiodPath"
if [ $? != 0 ]; then
    echo "Failed to extract the libgpiod include assets."
    exit 1
fi
docker container rm -f iot