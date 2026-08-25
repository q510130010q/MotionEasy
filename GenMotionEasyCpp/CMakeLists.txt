cmake_minimum_required(VERSION 3.16)
project(GenMotionEasyCpp VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON)

# 头文件
set(GENMOTION_HEADERS
    include/GenMotionEasyCpp/GenMotionEasyCpp.h
    include/GenMotionEasyCpp/GtnTypes.h
    include/GenMotionEasyCpp/StatusInfo.h
    include/GenMotionEasyCpp/GtnError.h
    include/GenMotionEasyCpp/MotionControlManager.h
    include/GenMotionEasyCpp/AxisController.h
    include/GenMotionEasyCpp/EcatIO.h
    include/GenMotionEasyCpp/ExpansionIO.h
    include/GenMotionEasyCpp/IAxisController.h
    include/GenMotionEasyCpp/IAxisHome.h
    include/GenMotionEasyCpp/IMoveMotion.h
    include/GenMotionEasyCpp/IJogMotion.h
    include/GenMotionEasyCpp/IPTMotion.h
    include/GenMotionEasyCpp/IPVTMotion.h
    include/GenMotionEasyCpp/IGearMotion.h
    include/GenMotionEasyCpp/IFollowMotion.h
    include/GenMotionEasyCpp/IFollowExMotion.h
    include/GenMotionEasyCpp/IInterpMotion.h
    include/GenMotionEasyCpp/IPointMotion.h
)

# 源文件
set(GENMOTION_SOURCES
    src/GtnError.cpp
    src/EcatIO.cpp
    src/ExpansionIO.cpp
    src/MotionControlManager.cpp
    src/AxisController.cpp
    src/GenMotionEasyCpp_export.cpp
)

# 创建库目标
add_library(${PROJECT_NAME} SHARED
    ${GENMOTION_HEADERS}
    ${GENMOTION_SOURCES}
)

# 包含路径
target_include_directories(${PROJECT_NAME}
    PUBLIC
        $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
        $<INSTALL_INTERFACE:include>
    PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/../Gen_Gantry1.6/Gen_Gantry
)

# 链接 gts.lib（需用户自行提供）
# 将 gts.lib 放在 external/ 目录下，或指定完整路径
set(GTS_LIB_PATH "${CMAKE_CURRENT_SOURCE_DIR}/external/gts.lib")
if(EXISTS ${GTS_LIB_PATH})
    target_link_libraries(${PROJECT_NAME} PRIVATE ${GTS_LIB_PATH})
else()
    message(WARNING "gts.lib not found at ${GTS_LIB_PATH}. "
        "Place gts.lib in the external/ directory and rebuild.")
endif()

# 定义导出宏
target_compile_definitions(${PROJECT_NAME} PRIVATE GENMOTIONCPP_EXPORTS)

# 安装规则
install(TARGETS ${PROJECT_NAME}
    EXPORT GenMotionEasyCppTargets
    RUNTIME DESTINATION bin
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
)

install(FILES ${GENMOTION_HEADERS}
    DESTINATION include/GenMotionEasyCpp
)

install(EXPORT GenMotionEasyCppTargets
    DESTINATION cmake
)
