#

dotnet ./Serializers/C/bin/Debug/netcoreapp2.1/C.dll \
  -c "Ic:\Users\Kenne\Documents\clang-llvm\llvm\include" \
  "Ic:\Users\Kenne\Documents\clang-llvm\build\include" \
  -f "llvm-includes.cpp" \
  > .generated_ast

cat .generated_ast | dotnet ./Piggy/bin/Debug/netcoreapp2.1/Piggy.dll -s m.pig
