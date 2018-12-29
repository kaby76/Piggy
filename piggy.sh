#

# Currently, ClangSerializer seems to have two "stdout". One is used by
# the C runtime, the other by C#. Unfortunately, the C runtime seems to
# take precedence and pipes don't seem to work. For now, redirect to an
# output file and use that on the second step.

echo dotnet ./ClangSerializer/bin/Debug/netcoreapp2.2/ClangSerializer.dll \
  -c "Ic:\Users\Kenne\Documents\clang-llvm\llvm\include" \
  "Ic:\Users\Kenne\Documents\clang-llvm\build\include" \
  -f "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Analysis.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\BitReader.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\BitWriter.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Core.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\DebugInfo.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Disassembler.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\ErrorHandling.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\ExecutionEngine.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Initialization.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\IRReader.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Linker.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\OrcBindings.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Support.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Target.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\TargetMachine.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Transforms\IPO.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Transforms\PassManagerBuilder.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Transforms\Scalar.h" \
  "c:\Users\Kenne\Documents\clang-llvm\llvm\include\llvm-c\Transforms\Vectorize.h" \
  > .generated_ast
cat .generated_ast | dotnet ./Piggy/bin/Debug/netcoreapp2.2/Piggy.dll -s m.pig
