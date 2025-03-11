cmake_minimum_required(VERSION 3.31)
project (ProcessMemoryNativeLibrary)

option(ENABLE_CLANG_TIDY "Enable Clang-tidy" OFF)
option(BUILD_SHARED_LIBS "Build using shared libraries" OFF)
option(BUILD_OUR_SHARED_LIBS "Build our libraries as shared libraries" ON)

list(APPEND CMAKE_MODULE_PATH "${CMAKE_CURRENT_SOURCE_DIR}/cmake")
include(SetFromEnv)
include(SetProjectCompileOptions)
include(SetProjectCompileDefinitions)
include(SetProjectLinkOptions)
include(ProjectSettings)
include(ClangTidy)

add_subdirectory(src/ProcessMemoryNative)
