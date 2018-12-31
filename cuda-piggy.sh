#

# Currently, ClangSerializer seems to have two "stdout". One is used by
# the C runtime, the other by C#. Unfortunately, the C runtime seems to
# take precedence and pipes don't seem to work. For now, redirect to an
# output file and use that on the second step.

dotnet ./ClangSerializer/bin/Debug/netcoreapp2.2/ClangSerializer.dll \
  -c "Ic:\Program Files\NVIDIA GPU Computing Toolkit\cuda\v10.0\include" \
  -f "cuda-includes.cpp" \
  > .generated_cuda_ast

cat .generated_cuda_ast | dotnet ./Piggy/bin/Debug/netcoreapp2.2/Piggy.dll -s cuda.pig
