
/* Bootstrap note: you need to have a compatible working version of clang for the
 * generator to output a new generator. To do that, use another machine to install
 * the clang headers, then access the files over the network with a mapped drive (Z:).
 */

import_file 'c:/Users/Kenne/Documents/clang-llvm/llvm/tools/clang/include/clang-c/BuildSystem.h';
import_file 'c:/Users/Kenne/Documents/clang-llvm/llvm/tools/clang/include/clang-c/CXCompilationDatabase.h';
import_file 'c:/Users/Kenne/Documents/clang-llvm/llvm/tools/clang/include/clang-c/CXErrorCode.h';
import_file 'c:/Users/Kenne/Documents/clang-llvm/llvm/tools/clang/include/clang-c/CXString.h';
import_file 'c:/Users/Kenne/Documents/clang-llvm/llvm/tools/clang/include/clang-c/Documentation.h';
import_file 'c:/Users/Kenne/Documents/clang-llvm/llvm/tools/clang/include/clang-c/Index.h';
import_file 'c:/Users/Kenne/Documents/clang-llvm/llvm/tools/clang/include/clang-c/Platform.h';
compiler_option '-IC:\Users\Kenne\Documents\clang-llvm\llvm\tools\clang\include';
//compiler_option '--target=x86_64';

namespace ClangSharp;
dllimport 'libclang';
class_name clang;
prefix_strip clang_;
exclude clang_index_getClientEntity;
exclude index_getClientEntity;
exclude clang_parseTranslationUnit2;
exclude clang_parseTranslationUnit2FullArgv;
exclude clang_executeOnThread;
exclude clang_index_setClientEntity;


enumDecl() =>
{
	public enum <? $root.Name ?> : <? $root.BaseType.Name ?> {
		<?
			var list = $root.Children.Zip($root.Children, (x, y) => x.Name + "=" + y.Value);
			System.Console.WriteLine(String.Join(",", list));
		?>
	}
}
