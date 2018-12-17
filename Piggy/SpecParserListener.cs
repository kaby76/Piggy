//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from SpecParser.g4 by ANTLR 4.7.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using IParseTreeListener = Antlr4.Runtime.Tree.IParseTreeListener;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete listener for a parse tree produced by
/// <see cref="SpecParserParser"/>.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.1")]
[System.CLSCompliant(false)]
public interface ISpecParserListener : IParseTreeListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.spec"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSpec([NotNull] SpecParserParser.SpecContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.spec"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSpec([NotNull] SpecParserParser.SpecContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.items"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterItems([NotNull] SpecParserParser.ItemsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.items"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitItems([NotNull] SpecParserParser.ItemsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.extends"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExtends([NotNull] SpecParserParser.ExtendsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.extends"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExtends([NotNull] SpecParserParser.ExtendsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.namespace"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNamespace([NotNull] SpecParserParser.NamespaceContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.namespace"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNamespace([NotNull] SpecParserParser.NamespaceContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.clang_file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClang_file([NotNull] SpecParserParser.Clang_fileContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.clang_file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClang_file([NotNull] SpecParserParser.Clang_fileContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.clang_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClang_option([NotNull] SpecParserParser.Clang_optionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.clang_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClang_option([NotNull] SpecParserParser.Clang_optionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.template"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterTemplate([NotNull] SpecParserParser.TemplateContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.template"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitTemplate([NotNull] SpecParserParser.TemplateContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRexp([NotNull] SpecParserParser.RexpContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRexp([NotNull] SpecParserParser.RexpContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.simple_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSimple_rexp([NotNull] SpecParserParser.Simple_rexpContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.simple_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSimple_rexp([NotNull] SpecParserParser.Simple_rexpContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.basic_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasic_rexp([NotNull] SpecParserParser.Basic_rexpContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.basic_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasic_rexp([NotNull] SpecParserParser.Basic_rexpContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.star_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStar_rexp([NotNull] SpecParserParser.Star_rexpContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.star_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStar_rexp([NotNull] SpecParserParser.Star_rexpContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.plus_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPlus_rexp([NotNull] SpecParserParser.Plus_rexpContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.plus_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPlus_rexp([NotNull] SpecParserParser.Plus_rexpContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.elementary_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterElementary_rexp([NotNull] SpecParserParser.Elementary_rexpContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.elementary_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitElementary_rexp([NotNull] SpecParserParser.Elementary_rexpContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.group_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGroup_rexp([NotNull] SpecParserParser.Group_rexpContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.group_rexp"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGroup_rexp([NotNull] SpecParserParser.Group_rexpContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.basic"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasic([NotNull] SpecParserParser.BasicContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.basic"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasic([NotNull] SpecParserParser.BasicContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.simple_basic"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSimple_basic([NotNull] SpecParserParser.Simple_basicContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.simple_basic"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSimple_basic([NotNull] SpecParserParser.Simple_basicContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.kleene_star_basic"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterKleene_star_basic([NotNull] SpecParserParser.Kleene_star_basicContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.kleene_star_basic"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitKleene_star_basic([NotNull] SpecParserParser.Kleene_star_basicContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.id_or_star_or_empty"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterId_or_star_or_empty([NotNull] SpecParserParser.Id_or_star_or_emptyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.id_or_star_or_empty"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitId_or_star_or_empty([NotNull] SpecParserParser.Id_or_star_or_emptyContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.more"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterMore([NotNull] SpecParserParser.MoreContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.more"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitMore([NotNull] SpecParserParser.MoreContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.code"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCode([NotNull] SpecParserParser.CodeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.code"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCode([NotNull] SpecParserParser.CodeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterText([NotNull] SpecParserParser.TextContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.text"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitText([NotNull] SpecParserParser.TextContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.attr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAttr([NotNull] SpecParserParser.AttrContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.attr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAttr([NotNull] SpecParserParser.AttrContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.pass"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPass([NotNull] SpecParserParser.PassContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.pass"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPass([NotNull] SpecParserParser.PassContext context);
}
