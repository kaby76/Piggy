
cmake  -MORELIBS="AArch64AsmParser AArch64AsmPrinter AArch64CodeGen AArch64Desc AArch64Disassembler AArch64Info AArch64Utils" -DCMAKE_BUILD_TYPE=Release -DLLVM_DIR="/mnt/c/Users/Kenne/Documents/clang-llvm/build-linux/lib/cmake/llvm"  .
make VERBOSE=1
