echo on

REM First argument is the path to the user home directory (typically C:\Users\VssAdministrator)
REM Second argument is either "Debug" or "Release"
if %1!==! goto :usage

REM Defines the revision to check out in the ExtendedConfigurableFirmata repo
set FIRMATA_SIMULATOR_CHECKOUT_REVISION=a354343cebc35964450dfa01dba2cd996065fd5c
set RUN_COMPILER_TESTS=FALSE

choco install -y --no-progress arduino-cli
arduino-cli lib install "DHT sensor library"
arduino-cli lib install "Servo"

arduino-cli config init
arduino-cli config add board_manager.additional_urls https://espressif.github.io/arduino-esp32/package_esp32_dev_index.json
arduino-cli core update-index

REM directly execute PS, we can ignore any test errors.
powershell -ExecutionPolicy ByPass -command "%~dp0common\Build.ps1" -restore -build -ci -configuration %2 -preparemachine

set ArduinoRootDir=%1\Documents\Arduino
set acspath=%~dp0\..\artifacts\bin\Frontend\%2\net8.0\acs.exe

git clone https://github.com/firmata/ConfigurableFirmata %ArduinoRootDir%\libraries\ConfigurableFirmata
git clone https://github.com/pgrawehr/ExtendedConfigurableFirmata %ArduinoRootDir%\ExtendedConfigurableFirmata
arduino-cli core install esp32:esp32

git fetch --all
git branch --list
REM Check whether any compiler files have changed - if so, enable the (long running) compiler tests
git diff --name-status origin/main | find /C /I "tools/ArduinoCsCompiler"
REM Find returns 1 when the string was NOT found, we want to set the variable to true when it does find something
if %errorlevel%==0 set RUN_COMPILER_TESTS=TRUE

dir %ArduinoRootDir%

if NOT exist %acspath% goto error

%acspath% --help

rem Write runtime data to ExtendedConfigurableFirmata directory, before building
%acspath% prepare

rem bring msbuild into the path
call "c:\program files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat"

pushd %ArduinoRootDir%\ExtendedConfigurableFirmata

REM checkout to the currently fixed branch
git checkout -B main %FIRMATA_SIMULATOR_CHECKOUT_REVISION%

dir
REM First build the code for the ESP32 (this just verifies that the code builds, it does no run-time checks at all)
arduino-cli compile --fqbn esp32:esp32:esp32:PSRAM=disabled,PartitionScheme=default,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none ./ExtendedConfigurableFirmata.ino --warnings default
if errorlevel 1 goto error

REM then build the simulator code as Visual Studio C++ project
msbuild /p:Configuration=%2 ExtendedConfigurableFirmataSim\ExtendedConfigurableFirmataSim.vcxproj
if errorlevel 1 goto error

REM Start the simulator asynchronously
start ExtendedConfigurableFirmataSim\%2\ExtendedConfigurableFirmataSim.exe

popd
pushd %~dp0\..\tools\ArduinoCsCompiler\

REM This somehow gets disabled
echo on
REM and finally run the Arduino tests, now including the ones skipped previously. Set verbosity to normal to see 
REM information about all tests being executed (as this test run can take 30 mins or more)
echo Starting basic arduino tests
dotnet test -c %2 --no-build --no-restore --filter feature=firmata -l "console;verbosity=normal" -maxcpucount:1

if errorlevel 1 goto error

echo on
echo Run full compiler against simple example
REM The current directory is tools/ArduinoCsCompiler
%acspath% compile --run %~dp0\..\artifacts\bin\BlinkingLed\%2\net8.0\BlinkingLed.exe --network localhost --mapfile BlinkingLed-tokenmap.txt
if errorlevel 1 goto error

REM copy token map to output (so we have something to compare the history of sizes, if something changes unexpectedly)
type BlinkingLed-tokenmap.txt


popd
taskkill /im ExtendedConfigurableFirmataSim.exe /f

Echo All done!
exit /b 0

:error
echo Tests failed. Error code %errorlevel%
exit /b 1
:usage

echo Usage: ArduinoCsCI.cmd [path-to-home-directory] [Configuration]

