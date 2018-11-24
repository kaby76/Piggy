import_file './help.h';
compiler_option '-I.';

namespace ClangSharp;
dllimport 'libclang';
class_name clang;
prefix_strip clang_;

// () ast match
// <> text
// {} code

// (... (...)) <> vs (... <> (...)). An AST expression matches a set of sub-tree. The
// first matches the entire sub tree. The later matches the node, then matches additional
// sub-tree information (presumably for more template processing). In the implementation,
// the matcher processes in a tree traversal, so templated text is outputed while walking
// the tree.


template (% ParmVarDecl Name=* Type="const wchar_t *"
   {
		System.Console.Write("int " + $$.Name);
   }
   %)
   ;

template (% ParmVarDecl Name=* Type=*
   {
		System.Console.Write(MapDefaultType($1.Type) + " " + $$.Name);
   }
   %)
   ;

template (% FunctionDecl Name=* Type=*
   {
      System.Console.WriteLine("[DllImport(\"foobar\", CallingConvention = global::System.Runtime.InteropServices.CallingConvention.ThisCall,");
      System.Console.WriteLine("\t EntryPoint=\"" + tree.Peek(0).Attr("Name") + "\")]");
      System.Console.WriteLine("internal static extern " + tree.Peek(0).Attr("Type") + " "
		+ tree.Peek(0).Attr("Name") + "(" + tree.Peek(0).ChildrenOutput() + ");");
   }
   %)
   ;

template
   (% EnumDecl Name=*
      { vars["first"] = true; }
      < enum $$.Name { >
		(
			(% EnumConstantDecl Name=*
				(% IntegerLiteral Value=*
					{
                  if ((bool)vars["first"])
                     vars["first"] = false;
                  else
                     result.Append(",")
               }
               < $$.Name = $$.Value >
				%)
			%)
         |
			(% EnumConstantDecl Name=*
				{
					if ((bool)vars["first"])
						vars["first"] = false;
					else
						result.Append(",")
				}
				< $$.Name >
			%)
      )*
      < } >
   %)
    ;

