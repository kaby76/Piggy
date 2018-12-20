# Piggy

_NB Status: Piggy is currently being developed and not available yet._

Welcome to Piggy (*P*/*I*nvoke *G*enerator for C#). This free and open source software
is a pinvoke generator from C++ headers. It is a powerful source-to-source
transformational system that goes well beyond any other pinvoke generator!!
It uses the same basic algorithm of DFS traversal of the
AST commonly used in ClangSharp and CppSharp,
but abstracts the visitor code into _templates_. A _template_ is a combination of a tree
regular expression, C# code and plain text blocks. The pattern matcher 
operates on Clang AST. After matching, output is generated by 
a DFS in-order tree walk, executing the code blocks and outputing the text blocks in
the template. Pattern matching of trees follows the syntax of
[TreeRegEx](https://treeregexlib.github.io/), with extensions for node attributes (_attr=value_),
and dynamic string interpolation of values during the match of attributes.

As with ClangSharp and CppSharp, Piggy inputs C files, parsed
by Clang to get an [abstract syntax tree (AST)](http://clang.llvm.org/docs/IntroductionToTheClangAST.html).
Piggy reads a specification file that contains passes and templates. Output is generated by a DFS traversal
of the AST, matching the tree with patterns. All C#
code and text blocks in the templates is collected into a C# class, JIT compiled, then executed during a second
traversal.

This tool does not read DLLs for P/Invoke generation,
only the headers.

Piggy extends the ideas of other pinvoke generators:
* [SWIG](http://swig.org/), the original pinvoke generator, which uses a specification file containing type maps.
* [ClangSharp](https://github.com/Microsoft/ClangSharp) [(Mukul Sabharwal; mjsabby)](https://github.com/mjsabby),
 and [CppSharp](https://github.com/mono/CppSharp), which use Clang AST visitors of the Clang-C API.
* [Lumír Kojecký's](https://www.codeproject.com/script/Membership/View.aspx?mid=9709944)
 [CodeProject article for dynamically compiling and executing C# code in the Net Framework](https://www.codeproject.com/Tips/715891/Compiling-Csharp-Code-at-Runtime).
* [Clang-query](https://github.com/llvm-mirror/clang-tools-extra/tree/master/clang-query),
which was used as a starting point for serializing an AST (the XML serializer no longer exists).
* [Jared Parsons' PInvoke Interop Assistant](https://github.com/jaredpar/pinvoke),
which is another open-source pinvoke generator.
* [xInterop C++.Net Bridge, by Shawn Liu](https://www.xinterop.com/). Commercial product, no longer available.

## Piggy Specification File

Instead of using and extending the unwieldy command-line arguments for input,
Piggy uses a specification file. This file specifies what options to use
for Clang and C# code generation. The grammar and parser for the specification file is for Antlr, a
high quality parser generator.

For an example of the Piggy specification file, see the Enums example in the root directory. There are two files,
m.pig and basic.pig. The m.pig file sets up the Clang input: C files and compiler options. Then, it includes
the main specification file basic.pig. In that file, multiple passes are specified in order to generate enums and 
function pinvoke declarations. The example demonstrates some very important featuers: Kleene star tree expressions,
dynamic string interpolation for selecting only "clang-c" directories.

### Spec file grammar

For the latest Antlr grammar files describing the input into Piggy, see
[SpecParser.g4](https://github.com/kaby76/Piggy/blob/master/Piggy/SpecParser.g4)
and [SpecLexer.g4](https://github.com/kaby76/Piggy/blob/master/Piggy/SpecLexer.g4).

(Note: I highly recommend using my [AntlrVSIX](https://marketplace.visualstudio.com/items?itemName=KenDomino.AntlrVSIX) plugin for reading and editing Antlr grammars in Visual Studio 2017!)

## Building Piggy ##

Download [llvm](http://releases.llvm.org/7.0.0/llvm-7.0.0.src.tar.xz),
 [clang](http://releases.llvm.org/7.0.0/cfe-7.0.0.src.tar.xz),
 and [clang extra](http://releases.llvm.org/7.0.0/clang-tools-extra-7.0.0.src.tar.xz).

Untar the downloads. Then,
~~~~
 mv llvm-7.0.0.src clang-llvm               # rename directory to "llvm"
 mv cfe-7.0.0.src clang-llvm/tools/clang;   # move and rename directory to "clang" 
 mv clang-tools-extra-7.0.0.src clang-llvm/tools/clang/tools/extra  # #move and rename
~~~~
Build using cmake (see [instructions](https://clang.llvm.org/get_started.html)).
~~~~
mkdir build; cd build
cmake -G "Visual Studio 15 2017" -A x64 -Thost=x64 ..\llvm
msbuild LLVM.sln /p:Configuration=Debug /p:Platform=x64
~~~~
Once you have built LLVM and Clang, you can build Piggy. Make sure to map e:/ to the
location of clang-llvm/.

## Relation to template engines

Piggy is similar to other engines, like CppSharp and ClangSharp,
where it uses hardwired tree walking code to output pinvoke declarations.
Like JSP (1), which turned the concept of a servlet inside-out into a template
which we now call HTML (2), Piggy turns the tree walking matcher "inside-out" into
a tree matching template.

Like JSP, Piggy does not separate "model/view" as discussed
by Parr (2). So, the logic of the translation to pinvoke declarations is
interspersed with the model, which is the AST.
In the future, a tree matching template could be formalized
in order to further the concept of a tree matching template.

## References

(1) JavaServer Pages Technology. https://www.oracle.com/technetwork/java/jsp-138432.html. Accessed Dec 20, 2018.

(2) Parr, Terence John. "Enforcing strict model-view separation in template engines." Proceedings of the 13th international conference on World Wide Web. ACM, 2004.



