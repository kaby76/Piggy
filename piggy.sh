#

# Currently, ClangSerializer seems to have two "stdout". One is used by
# the C runtime, the other by C#. Unfortunately, the C runtime seems to
# take precedence and pipes don't seem to work. For now, redirect to an
# output file and use that on the second step.

dotnet ./ClangSerializer/bin/Debug/netcoreapp2.2/ClangSerializer.dll -c "IC:/temp/include/" -f "C:/temp/include/clang-c/Index.h" > .generated_ast
cat .generated_ast | dotnet ./Piggy/bin/Debug/netcoreapp2.2/Piggy.dll -s m.pig
