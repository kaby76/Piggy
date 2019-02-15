cmake_minimum_required(VERSION 3.4.3)
project (ClangCode)

find_package(LLVM REQUIRED CONFIG)

message(STATUS "Found LLVM ${LLVM_PACKAGE_VERSION}")
message(STATUS "Using LLVMConfig.cmake in: ${LLVM_DIR}")


# Set your project compile flags.
# E.g. if using the C++ header files
# you will need to enable C++11 support
# for your compiler.
set( CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11")


include_directories(/mnt/c/Users/Kenne/Documents/clang-llvm/llvm/tools/clang/include)
include_directories(/mnt/c/Users/Kenne/Documents/clang-llvm/build-linux/tools/clang/include/)
include_directories(/mnt/c/Users/Kenne/Documents/clang-llvm/llvm/include)
include_directories(/mnt/c/Users/Kenne/Documents/clang-llvm/build-linux/include)

add_definitions(${LLVM_DEFINITIONS})

# Set specific targets.
# set(LLVM_BUILD_TARGETS X86)

# Now build our tools
# add_library(llvm-wrap STATIC LLVM_wrap.cpp)
add_library(ClangCode MODULE Source.cpp Source2.cpp)

# Find the libraries that correspond to the LLVM components
# that we wish to use
llvm_map_components_to_libnames(llvm_libs support Core irreader mcparser bitreader mc option mcjit native linker ipo executionengine runtimedyld orcjit interpreter ObjCARCOpts 
  AArch64AsmParser AArch64AsmPrinter AArch64CodeGen AArch64Desc AArch64Disassembler AArch64Info AArch64Utils 
  AMDGPUAsmParser AMDGPUAsmPrinter AMDGPUCodeGen AMDGPUDesc AMDGPUDisassembler AMDGPUInfo AMDGPUUtils 
  ARMAsmParser ARMAsmPrinter ARMCodeGen ARMDesc ARMDisassembler ARMInfo
  HexagonAsmParser HexagonCodeGen HexagonDesc HexagonDisassembler HexagonInfo
  InstCombine
  MipsAsmParser MipsAsmPrinter MipsCodeGen MipsDesc MipsDisassembler MipsInfo
 MIRParser
  MSP430AsmPrinter MSP430CodeGen MSP430Desc MSP430Info
  NVPTXAsmPrinter NVPTXCodeGen NVPTXDesc NVPTXInfo
  PowerPCAsmParser PowerPCAsmPrinter PowerPCCodeGen PowerPCDesc PowerPCDisassembler PowerPCInfo
  SparcAsmParser SparcAsmPrinter SparcCodeGen SparcDesc SparcDisassembler SparcInfo
  SystemZAsmParser SystemZAsmPrinter SystemZCodeGen SystemZDesc SystemZDisassembler SystemZInfo
  X86AsmParser X86AsmPrinter X86CodeGen X86Desc X86Disassembler X86Info X86Utils 
  XCoreAsmPrinter XCoreCodeGen XCoreDesc XCoreDisassembler XCoreInfo
)


# Link against LLVM libraries
target_link_libraries(ClangCode ${llvm_libs})
set_target_properties(ClangCode PROPERTIES PREFIX "")



