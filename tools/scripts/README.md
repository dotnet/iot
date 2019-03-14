# Scripts

## Easy CLI workflow on Linux with Raspberry PI (Raspbian)

[script](helpers-rpi-linux.sh)

To use (run from repo root for quick copy & paste - will work anywhere in the repo though)

```shell
source tools/scripts/helpers-rpi-linux.sh

# modify this with your device name (you may be interested in ~/.ssh/config)
export rpi=pi@192.168.1.123 # or root@ if you require sudo access

# now go to your device sample project (or any exe project in the repo)
publish && run

# first publish will take a bit longer
# consecutive ones will only publish incrementally
```
