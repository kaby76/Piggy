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
using IErrorNode = Antlr4.Runtime.Tree.IErrorNode;
using ITerminalNode = Antlr4.Runtime.Tree.ITerminalNode;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

/// <summary>
/// This class provides an empty implementation of <see cref="ISpecParserListener"/>,
/// which can be extended to create a listener which only needs to handle a subset
/// of the available methods.
/// </summary>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.1")]
[System.CLSCompliant(false)]
public partial class SpecParserBaseListener : ISpecParserListener {
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.spec"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSpec([NotNull] SpecParserParser.SpecContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.spec"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSpec([NotNull] SpecParserParser.SpecContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.items"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterItems([NotNull] SpecParserParser.ItemsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.items"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitItems([NotNull] SpecParserParser.ItemsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.namespace"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterNamespace([NotNull] SpecParserParser.NamespaceContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.namespace"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitNamespace([NotNull] SpecParserParser.NamespaceContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.exclude"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterExclude([NotNull] SpecParserParser.ExcludeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.exclude"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitExclude([NotNull] SpecParserParser.ExcludeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.import_file"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterImport_file([NotNull] SpecParserParser.Import_fileContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.import_file"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitImport_file([NotNull] SpecParserParser.Import_fileContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.dllimport"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterDllimport([NotNull] SpecParserParser.DllimportContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.dllimport"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitDllimport([NotNull] SpecParserParser.DllimportContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.add_after_usings"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAdd_after_usings([NotNull] SpecParserParser.Add_after_usingsContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.add_after_usings"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAdd_after_usings([NotNull] SpecParserParser.Add_after_usingsContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.code"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCode([NotNull] SpecParserParser.CodeContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.code"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCode([NotNull] SpecParserParser.CodeContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.prefix_strip"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPrefix_strip([NotNull] SpecParserParser.Prefix_stripContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.prefix_strip"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPrefix_strip([NotNull] SpecParserParser.Prefix_stripContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.class_name"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterClass_name([NotNull] SpecParserParser.Class_nameContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.class_name"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitClass_name([NotNull] SpecParserParser.Class_nameContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.calling_convention"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCalling_convention([NotNull] SpecParserParser.Calling_conventionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.calling_convention"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCalling_convention([NotNull] SpecParserParser.Calling_conventionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.compiler_option"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterCompiler_option([NotNull] SpecParserParser.Compiler_optionContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.compiler_option"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitCompiler_option([NotNull] SpecParserParser.Compiler_optionContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.template"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterTemplate([NotNull] SpecParserParser.TemplateContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.template"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitTemplate([NotNull] SpecParserParser.TemplateContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterRexp([NotNull] SpecParserParser.RexpContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitRexp([NotNull] SpecParserParser.RexpContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.simple_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterSimple_rexp([NotNull] SpecParserParser.Simple_rexpContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.simple_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitSimple_rexp([NotNull] SpecParserParser.Simple_rexpContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.basic_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBasic_rexp([NotNull] SpecParserParser.Basic_rexpContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.basic_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBasic_rexp([NotNull] SpecParserParser.Basic_rexpContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.star_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterStar_rexp([NotNull] SpecParserParser.Star_rexpContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.star_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitStar_rexp([NotNull] SpecParserParser.Star_rexpContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.plus_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPlus_rexp([NotNull] SpecParserParser.Plus_rexpContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.plus_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPlus_rexp([NotNull] SpecParserParser.Plus_rexpContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.elementary_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterElementary_rexp([NotNull] SpecParserParser.Elementary_rexpContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.elementary_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitElementary_rexp([NotNull] SpecParserParser.Elementary_rexpContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.group_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterGroup_rexp([NotNull] SpecParserParser.Group_rexpContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.group_rexp"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitGroup_rexp([NotNull] SpecParserParser.Group_rexpContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.basic"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterBasic([NotNull] SpecParserParser.BasicContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.basic"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitBasic([NotNull] SpecParserParser.BasicContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.more"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterMore([NotNull] SpecParserParser.MoreContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.more"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitMore([NotNull] SpecParserParser.MoreContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.text"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterText([NotNull] SpecParserParser.TextContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.text"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitText([NotNull] SpecParserParser.TextContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.attr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterAttr([NotNull] SpecParserParser.AttrContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.attr"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitAttr([NotNull] SpecParserParser.AttrContext context) { }
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.pass"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void EnterPass([NotNull] SpecParserParser.PassContext context) { }
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.pass"/>.
	/// <para>The default implementation does nothing.</para>
	/// </summary>
	/// <param name="context">The parse tree.</param>
	public virtual void ExitPass([NotNull] SpecParserParser.PassContext context) { }

	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void EnterEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void ExitEveryRule([NotNull] ParserRuleContext context) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitTerminal([NotNull] ITerminalNode node) { }
	/// <inheritdoc/>
	/// <remarks>The default implementation does nothing.</remarks>
	public virtual void VisitErrorNode([NotNull] IErrorNode node) { }
}
