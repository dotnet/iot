# How to start IOT app automatically on boot

## Create your app

For the purpose of this document let's assume you have deployed your app under:

```
/home/pi/myiotapp/myiotapp
```

This can also be a shell-script or anything with executable permissions (make sure to `chmod +x` your app/script).

## Create init.d service

Example script. `myiotapp` will be used everywhere. Please replace accordingly with your app name.

File path should be `/etc/init.d/myiotapp` (note no extension - it may be used but it will become part of the name)

**Please make sure to read comments in the code below**

```shell
#! /bin/sh
# /etc/init.d/myiotapp

### BEGIN INIT INFO
# Provides:          myiotapp
# Required-Start:    $remote_fs $syslog $local_fs $network $time $named
# Required-Stop:     $remote_fs $syslog
# Default-Start:     2 3 4 5
# Default-Stop:      0 1 6
# Short-Description: Short description for your app
# Description:       Longer description for your app
### END INIT INFO

case "$1" in
    start)
        # It is important that there is no output here
        # nohup and the ambersand in the end mean that your app will be detached from the terminal
        # If your app uses any of the console capabilities (i.e. Console.KeyAvailable)
        # it may crash on the startup
        nohup /home/pi/myiotapp/myiotapp 2> /home/pi/myiotapp-error.log > /home/pi/myiotapp.log &

        # If you really need console capabilities you may also start your app under tmux
        # and connect to the app later using tmux attach
	    # tmux new-session -d -s myiotapp
	    # tmux send-keys '/home/pi/myiotapp/myiotapp' 'C-m'

        # You may run multiple apps here
	;;
    stop)
        # This is not strictly required - can be commented out or removed
        # You may also provide more sophisticated way of disabling your app
	    killall myiotapp
	;;
    *)
	echo "Usage: /etc/init.d/myiotapp {start|stop}"
	exit 1
	;;
esac

exit 0

```

### Notes

The comment block in the script is used by update-rc.d.
Please use care and refer to the manual before modifying.
You may also look at the other scripts under /etc/init.d/ for reference.

## Now ensure your service starts automatically when system boots

```shell
# Requires root permissions
update-rc.d myiotapp defaults
```

## To start your service immediately

This will only run your app and not setup the service.
You can use it to check if everything starts correctly.

```shell
/etc/init.d/myiotapp start
```

Note that your app will run without terminal attached - some of the Console APIs may cause your app to crash.

## Security considerations

Your app will be running with root permissions.

Please ensure only root can write to the files related with your app.
That includes any binary dependencies or any other scripts which you app may run.

Not doing so may add a risk of elevation of privileges to your device.

```shell
# The commands below should be run as root (i.e. sudo)

# Owher of every dependency and app should be root
chown -R root:root /your/app/directory/

# permissions for dependencies (files) should be rw for owner, read or none for anyone else
chmod -R 644 /your/app/directory/*

# your app additionally will need executable permissions which you may have accidentally removed with previous command
chmod 755 /your/app/directory/yourapp

# permissions for directories/subdirectories
chmod 755 /your/app/directory
chmod 755 /your/app/directory/subdirectory
```
