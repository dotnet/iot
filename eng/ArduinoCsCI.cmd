echo on

if %1!=! goto :usage

choco install -y arduino-cli
arduino-cli lib install "DHT sensor library"
arduino-cli lib install "Servo"
       
arduino-cli config init
arduino-cli config add board_manager.additional_urls https://dl.espressif.com/dl/package_esp32_index.json
arduino-cli core update-index

dir %1
mkdir %1\Arduino
mkdir %1\Arduino\libraries
git checkout https://github.com/pgrawehr/ConfigurableFirmata $(UserProfile)\Arduino\libraries
git checkout https://github.com/pgrawehr/ExtendedConfigurableFirmata $(UserProfile)\Arduino
arduino-cli core install esp32:esp32

pushd
cd %1\Arduino\ExtendedConfigurableFirmata
arduino-cli compile --fqbn esp32:esp32:esp32:PSRAM=disabled,PartitionScheme=default,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none ./ExtendedConfigurableFirmata.ino --warnings more

popd

Echo All done!

:usage
:error
echo Usage: ArduinoCsCIcmd [path-to-home-directory]

