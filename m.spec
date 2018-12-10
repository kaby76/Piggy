import_file 'c:/temp/include/clang-c/Index.h';
compiler_option '-IC:/temp/include';

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

pass Enums;

template
   ( EnumDecl
      {
         vars["first"] = true;
         result.Append("enum " + tree.Peek(0).Attr("Name") + " " + "\u007B" + Environment.NewLine);
      }
      (%
         ( EnumConstantDecl
            ( IntegerLiteral
               {
                  if ((bool)vars["first"])
                     vars["first"] = false;
                  else
                     result.Append(", ");
                  var tt = tree.Peek(1);
                  var na = tt.Attr("Name");
                  var t2 = tree.Peek(0);
                  var va = t2.Attr("Value");
                  result.Append(tree.Peek(1).Attr("Name") + " xx= " + tree.Peek(0).Attr("Value") + Environment.NewLine);
               }
            )
         )
         |
         ( EnumConstantDecl
            {
               if ((bool)vars["first"])
                  vars["first"] = false;
               else
                  result.Append(", ");
               result.Append(tree.Peek(0).Attr("Name") + Environment.NewLine);
            }
         )
      %)*
      {
         result.Append("\u007D");  // Closing curly.
         result.Append(Environment.NewLine);
         result.Append(Environment.NewLine);
      }
   )
   ;

pass Functions;

template ( ParmVarDecl Type="const wchar_t *"
   {
        result.Append("int " + tree.Peek(0).Attr("Name") + Environment.NewLine);
   }
   )
   ;

template ( ParmVarDecl
   {
        result.Append("int " + tree.Peek(0).Attr("Name") + Environment.NewLine);
   }
   )
   ;

template ( FunctionDecl
   {
      result.Append("[DllImport(\"foobar\", CallingConvention = global::System.Runtime.InteropServices.CallingConvention.ThisCall," + Environment.NewLine);
      result.Append("\t EntryPoint=\"" + tree.Peek(0).Attr("Name") + "\")]" + Environment.NewLine);
      result.Append("internal static extern " + tree.Peek(0).Attr("Type") + " "
         + tree.Peek(0).Attr("Name") + "(" + tree.Peek(0).ChildrenOutput() + ");" + Environment.NewLine);
   }
   )
   ;
