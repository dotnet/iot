cmake_minimum_required(VERSION 2.8.12)

project (IOT C)

set(CMAKE_INCLUDE_CURRENT_DIR ON)

set(CMAKE_SHARED_LIBRARY_PREFIX "")

add_compile_options(-fPIC)

if(CMAKE_SYSTEM_NAME STREQUAL Linux)
    add_subdirectory(System.Device.Gpio.Native)
endif()
