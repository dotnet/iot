echo on

if %1!==! goto :usage

choco install -y --no-progress arduino-cli
arduino-cli lib install "DHT sensor library"
arduino-cli lib install "Servo"

arduino-cli config init
arduino-cli config add board_manager.additional_urls https://dl.espressif.com/dl/package_esp32_index.json
arduino-cli core update-index

set ArduinoRootDir=%1\Documents\Arduino
set acspath=%~dp0\..\tools\ArduinoCsCompiler\Frontend\bin\%2\net6.0\acs.exe
dir %ArduinoRootDir%

git clone https://github.com/firmata/ConfigurableFirmata %ArduinoRootDir%\libraries\ConfigurableFirmata
git clone https://github.com/pgrawehr/ExtendedConfigurableFirmata %ArduinoRootDir%\ExtendedConfigurableFirmata
arduino-cli core install esp32:esp32

dir %ArduinoRootDir%

%acspath% --help

rem Write runtime data to ExtendedConfigurableFirmata directory, before building
%acspath% prepare

pushd %ArduinoRootDir%\ExtendedConfigurableFirmata

dir
REM First build the code for the ESP32 (this just verifies that the code builds, it does no run-time checks at all)
arduino-cli compile --fqbn esp32:esp32:esp32:PSRAM=disabled,PartitionScheme=default,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none ./ExtendedConfigurableFirmata.ino --warnings default
if errorlevel 1 goto error

REM then build the simulator code as Visual Studio C++ project
msbuild /p:Configuration=%1 ExtendedConfigurableFirmataSim\ExtendedConfigurableFirmataSim.vcxproj
if errorlevel 1 goto error

REM Start the simulator asynchronously
start ExtendedConfigurableFirmataSim\%1\ExtendedConfigurableFirmataSim.exe

popd
pushd %~dp0\..\tools\ArduinoCsCompiler\

REM and finally run the Arduino tests, now including the ones skipped previously
dotnet test -c %1 --no-build --no-restore --filter feature=firmata -v:n -maxcpucount:1
if errorlevel 1 goto error

popd
taskkill /im ExtendedConfigurableFirmataSim.exe /f

Echo All done!

:usage
:error
echo Usage: ArduinoCsCI.cmd [path-to-home-directory] [Configuration]

