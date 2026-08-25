cmake_minimum_required(VERSION 3.16)
project(DemoMfc LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_MFC_FLAG 2)

set(GENMOTION_INCLUDE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../GenMotionEasyCpp/include")
set(GENMOTION_LIB_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../GenMotionEasyCpp/build/Debug")
set(GTS_INCLUDE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../Gen_Gantry1.6/Gen_Gantry")

file(GLOB SOURCES *.cpp *.h)

add_executable(${PROJECT_NAME} WIN32 ${SOURCES}
    resource.h
    DemoMfc.rc
)

target_include_directories(${PROJECT_NAME} PRIVATE
    ${GENMOTION_INCLUDE_DIR}
    ${GTS_INCLUDE_DIR}
)

target_link_directories(${PROJECT_NAME} PRIVATE ${GENMOTION_LIB_DIR})
target_link_libraries(${PROJECT_NAME} PRIVATE GenMotionEasyCpp.lib)
