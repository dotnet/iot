#!/bin/bash

# This file is supposed to be sourced when in iot repo i.e.:
# source helpers-rpi-linux.sh
# or just
# . helpers-rpi-linux.sh
# while you are in the root folder of your repo

# is-git-repo <dir>
is-git-repo() {
    test -d "$1/.git"
}

# git-find-repo-root <dir>
git-find-repo-root() {
    dir=$1
    if [ "$dir" != "" ]; then
        if is-git-repo $dir; then
            echo $dir
        else
            parent_dir=`dirname $dir`
            git-find-repo-root $parent_dir
        fi
    fi
}

repo_root="$(git-find-repo-root "$(pwd)")"
export PATH=$repo_root/.dotnet:$PATH
export DOTNET_MULTILEVEL_LOOKUP=0

find-csproj() {
    basename "$(find . -maxdepth 1 -type f -name "*.csproj")"
}

find-project-name() {
    find-csproj | sed 's/\.[^\.]*$//'
}

get-pi() {
    if [ $# -eq 0 ]; then
        if [ -z "${rpi}" ]; then
            echo 'publish <name>' >&2
            echo 'or export rpi=<name>' >&2
            return 1
        else
            echo "$rpi"
        fi
    else
        echo "$1"
    fi
}

publish() {
    name="$(get-pi "$@")"
    if [ $? -ne 0 ]; then
        return 1
    fi

    projname=`find-project-name`
    if [ -z "${projname}" ]; then
        echo 'Project file not found.' >&2
        return 1
    fi

    dir=/home/pi/src/iot-partial/$projname
    exe=$dir/$projname

    echo 'Publishing...'
    dotnet publish -v q -r linux-arm /nologo || return 1

    ssh $name mkdir -p $dir
    rsync -r ./bin/Debug/netcoreapp2.1/linux-arm/publish/ $name:$dir || return 1

    ssh $name chmod +x $exe &>/dev/null
    if [ $? -ne 0 ]; then
        if ssh $name test -e $exe; then
            echo 'Cannot set permissions to executable.'
            echo 'File exists but you have no permissions. Solutions:'
            echo '- sudo'
            echo "- chown pi:pi $exe"
        else
            echo 'ERROR: You are publishing library instead of executable project'
        fi

        return 1
    fi
}

run() {
    name="$(get-pi "$@")"
    if [ $? -ne 0 ]; then
        return 1
    fi

    projname=`find-project-name`
    if [ -z "${projname}" ]; then
        echo 'Project file not found.' >&2
        return 1
    fi

    dir=/home/pi/src/iot-partial/$projname
    exe=$dir/$projname

    if ssh $name test -e $exe; then
        # -t - pseudo-terminal allocation
        # makes sure there are no zombies left behind
        ssh -t $name $exe
    else
        echo 'File does not exist' >&2
        return 1
    fi
}
