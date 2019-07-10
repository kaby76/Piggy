using System;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Runtime;

namespace C
{
    public class Deserialize
    {
        public static IParseTree ReconstructTreeAux(Parser grammar, Lexer lexer, IParseTree ast_tree, ParserRuleContext parent)
        {
            if (ast_tree == null)
                return null;
            // Pre order visit.
            if (ast_tree as AstParserParser.NodeContext != null)
            {
                // Convert.
                var v = ast_tree as AstParserParser.NodeContext;
                var id = v.GetChild(1).GetText();
                if (id == "TOKEN")
                {
                    var type_attr = v.GetChild(2);
                    var type_str = type_attr.GetChild(2).GetText();
                    type_str = type_str.Substring(1, type_str.Length - 2);
                    var type = Int32.Parse(type_str);
                    var text_attr = v.GetChild(3);
                    var text_str = text_attr.GetChild(2).GetText();
                    text_str = text_str.Substring(1, text_str.Length - 2);
                    var text = text_str;
                    var sym = new CommonToken(type, text);
                    var x = new TerminalNodeImpl(sym);
                    if (parent != null) parent.AddChild(x);
                    return x;
                }
                else
                {
                    // Look up "<id>Context" in grammar.
                    id = id + "Context";
                    var u = grammar.GetType().GetNestedTypes().Where(t =>
                    {
                        if (t.IsClass && t.Name.ToLower() == id.ToLower())
                            return true;
                        return false;
                    });
                    var w = u.FirstOrDefault();
                    object[] parms = new object[2];
                    parms[0] = parent;
                    parms[1] = 0;
                    var x = (ParserRuleContext)Activator.CreateInstance(w, parms);
                    if (parent != null) parent.AddChild(x);
                    for (int i = 0; i < ast_tree.ChildCount; ++i)
                    {
                        var c = ast_tree.GetChild(i);
                        var eq = ReconstructTreeAux(grammar, lexer, c, x);
                    }
                    return x;
                }
            }
            else if (ast_tree as AstParserParser.AttrContext != null)
            {
                return null;
            }
            else
            {
                var tni = ast_tree as TerminalNodeImpl;
                var sym = tni.Symbol;
                var pp = sym.GetType().FullName;
                return null;
            }
        }

        public static IParseTree ReconstructTree(Parser grammar, Lexer lexer, string ast_string)
        {
            ///////////////////////////////////////////////////////////////////
            // Parse as a parenthesized expression tree.
            ///////////////////////////////////////////////////////////////////
            var ast_stream = CharStreams.fromstring(ast_string);
            ITokenSource ast_lexer = new AstLexer(ast_stream);
            var ast_tokens = new CommonTokenStream(ast_lexer);
            var ast_parser = new AstParserParser(ast_tokens);
            ast_parser.BuildParseTree = true;
            var listener = new ErrorListener<IToken>();
            ast_parser.AddErrorListener(listener);
            IParseTree ast_tree = ast_parser.ast();
            ast_tree = ast_tree.GetChild(0);
            if (listener.had_error) throw new Exception();
            ///////////////////////////////////////////////////////////////////
            // Convert parenthesized expression tree back into parse tree
            // of original grammar.
            ///////////////////////////////////////////////////////////////////
            var reconstructed_tree = ReconstructTreeAux(grammar, lexer, ast_tree, null);
            return reconstructed_tree;
        }
    }
}
