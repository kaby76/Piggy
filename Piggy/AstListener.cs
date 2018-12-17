using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;
using org.antlr.symtab;

namespace Piggy
{
    public class AstListener : AstParserBaseListener
    {
        private Dictionary<IParseTree, IParseTree> _parent;
        private IParseTree _ast;
        Stack<Scope> stack = new Stack<Scope>();

        public AstListener(IParseTree ast)
        {
            _ast = ast;
            _parent = Parents.Compute(ast);
        }

        public override void EnterDecl(AstParserParser.DeclContext context)
        {
            IParseTree id = context.GetChild(1);
            string name = id.GetText();
            switch (name)
            {
                case "TranslationUnitDecl":
                {
                    var t = new Tree(_parent, _ast, context);
                    Scope scope = new GlobalScope(null);
                    stack.Push(scope);
                    break;
                }
            }
        }
    }
}
