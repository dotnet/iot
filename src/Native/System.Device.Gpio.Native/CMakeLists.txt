project (System.Device.Gpio.Native C)

set(CMAKE_INCLUDE_CURRENT_DIR ON)

add_library(System.Device.Gpio.Native
	SHARED
	libgpiod.c
)

set_target_properties(System.Device.Gpio.Native PROPERTIES PREFIX "")

target_link_libraries(System.Device.Gpio.Native LINK_PUBLIC gpiod)
