#----------------------------------------------------------------
# Generated CMake target import file for configuration "MinSizeRel".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "GenMotionEasyCpp" for configuration "MinSizeRel"
set_property(TARGET GenMotionEasyCpp APPEND PROPERTY IMPORTED_CONFIGURATIONS MINSIZEREL)
set_target_properties(GenMotionEasyCpp PROPERTIES
  IMPORTED_IMPLIB_MINSIZEREL "${_IMPORT_PREFIX}/lib/GenMotionEasyCpp.lib"
  IMPORTED_LOCATION_MINSIZEREL "${_IMPORT_PREFIX}/bin/GenMotionEasyCpp.dll"
  )

list(APPEND _cmake_import_check_targets GenMotionEasyCpp )
list(APPEND _cmake_import_check_files_for_GenMotionEasyCpp "${_IMPORT_PREFIX}/lib/GenMotionEasyCpp.lib" "${_IMPORT_PREFIX}/bin/GenMotionEasyCpp.dll" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
