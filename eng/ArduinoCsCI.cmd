echo on

if %1!==! goto :usage

choco install -y arduino-cli
arduino-cli lib install "DHT sensor library"
arduino-cli lib install "Servo"

arduino-cli config init
arduino-cli config add board_manager.additional_urls https://dl.espressif.com/dl/package_esp32_index.json
arduino-cli core update-index

set ArduinoRootDir=%1\Documents\Arduino
dir %ArduinoRootDir%

git clone https://github.com/firmata/ConfigurableFirmata %ArduinoRootDir%\libraries
git checkout https://github.com/pgrawehr/ExtendedConfigurableFirmata %ArduinoRootDir%
arduino-cli core install esp32:esp32

dir %ArduinoRootDir%

pushd
cd %ArduinoRootDir%\ExtendedConfigurableFirmata
arduino-cli compile --fqbn esp32:esp32:esp32:PSRAM=disabled,PartitionScheme=default,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none ./ExtendedConfigurableFirmata.ino --warnings more
popd

if errorlevel 1 goto error

Echo All done!

:usage
:error
echo Usage: ArduinoCsCI.cmd [path-to-home-directory]

