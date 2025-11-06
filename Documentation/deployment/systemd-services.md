# How to start app automatically on boot using systemd

## Create your app

For the purpose of this document let's assume you have deployed your app under:

`
/home/pi/myiotappfolder/myiotapp
`

The app name is `<myiotapp>` in this documentation you would replace the entire string including the carrots in your files.

To run this you will need to make your app run as root please read the section on  [Security Considerations](#security-considerations)

Make sure to make your app ```<myiotapp>``` is executable by using the command:

```shell
# Requires root permissions
chmod +x <myiotapp>
```

## Create systemd.service unit

For this example a script titled `<myiotapp>.service` will be used.  The folder that this application is ran from is `/home/pi/myiotappfolder` Please replace accordingly with your app name or the location you decide to save your script. Remember to add the `.service`  as the file extension

Here is an example systemd.service file that you can use as a template. Make sure to use a Unix end of line when creating the systemmd.service file.

```shell
[Unit]
#The # is a comment line
#Documentation https://www.freedesktop.org/software/systemd/man/systemd.service.html

#Place short description here
Description=My IOT Device Service

#This will not start execution of this file until the network connection is made
#It can be replaced with other parameters of your choosing
After=network.target

[Service]
#Default: Startup type
Type=Simple

#Edit this with your file name. In this example the app executable is in the /home/pi/myiotappfolder
#The file we are running and made executable is <myiotapp>
#ExecStart runs this executable script
ExecStart=/home/pi/myiotappfolder/myiotapp

#Optional: Saves the output and error log of the terminal to a .log file in a directory of your choosing.
StandardOutput=file:/home/pi/myiotapp.log
StandardError=file:/home/pi/myiotapp-error.log

#Optional: To cleanly end the file on stop use this command. This sends a terminal interrupt command on the executable script
KillSignal=SIGINT

#Automatically restart on kill
Restart=always

[Install]
WantedBy=multi-user.target

```

The systemmd service file which we will call `<myiotapp>.service`  must be saved to `/etc/systemd/system`  to be ran on boot.

Note that you must have admin priviliges to save a file to the system etc folder. You can use the following to copy the service in a terminal to this folder:

```shell
# Requires root permissions
cp <myiotapp>.service /etc/systemd/system
```

Once in the folder make the file executable by going to the directory and then use the chmod command:

```shell
# Requires root permissions
chmod +x <myiotapp>.service
```

### Notes

Please use care and refer to the manual when using this code
You may also look at the other scripts under /etc/systemd/system for reference.

## Test your service

This will start the service but will not run it on boot.

```shell
# Requires root permissions
systemctl start <myiotapp>.service
```

Now you can look at the log file you created to see the output of the terminal. `/home/pi/myiotapp.log`

Or you can check the status of the service by using in a terminal:

```shell
# Requires root permissions
systemctl is-active status <myiotapp>.service
```

To stop the service run:

```shell
# Requires root permissions
systemctl stop <myiotapp>.service
```

## To make your service automatically run on boot

```shell
# Requires root permissions
 systemctl daemon-reload
 systemctl enable myscript.service
```

## To disable your service

Run this in terminal to disable the program on boot:

```shell
# Requires root permissions
 systemctl daemon-reload
 systemctl disable myscript.service
```

## Security considerations

Your app will be running with root permissions.

Please ensure only root can write to the files related with your app.
That includes any binary dependencies or any other scripts which you app may run.

Not doing so may add a risk of elevation of privileges to your device.

```shell
# The commands below should be run as root (i.e. sudo)

# Owner of every dependency and app should be root
chown -R root:root /your/app/directory/

# permissions for dependencies (files) should be rw for owner, read or none for anyone else
chmod -R 644 /your/app/directory/*

# your app additionally will need executable permissions which you may have accidentally removed with previous command
chmod 755 /your/app/directory/yourapp

# permissions for directories/subdirectories
chmod 755 /your/app/directory
chmod 755 /your/app/directory/subdirectory
```
