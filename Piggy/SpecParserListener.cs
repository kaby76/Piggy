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
	/// Enter a parse tree produced by <see cref="SpecParserParser.exclude"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterExclude([NotNull] SpecParserParser.ExcludeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.exclude"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitExclude([NotNull] SpecParserParser.ExcludeContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.import_file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterImport_file([NotNull] SpecParserParser.Import_fileContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.import_file"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitImport_file([NotNull] SpecParserParser.Import_fileContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.dllimport"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterDllimport([NotNull] SpecParserParser.DllimportContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.dllimport"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitDllimport([NotNull] SpecParserParser.DllimportContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.add_after_usings"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAdd_after_usings([NotNull] SpecParserParser.Add_after_usingsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.add_after_usings"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAdd_after_usings([NotNull] SpecParserParser.Add_after_usingsContext context);
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
	/// Enter a parse tree produced by <see cref="SpecParserParser.prefix_strip"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPrefix_strip([NotNull] SpecParserParser.Prefix_stripContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.prefix_strip"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPrefix_strip([NotNull] SpecParserParser.Prefix_stripContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.class_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterClass_name([NotNull] SpecParserParser.Class_nameContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.class_name"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitClass_name([NotNull] SpecParserParser.Class_nameContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.calling_convention"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCalling_convention([NotNull] SpecParserParser.Calling_conventionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.calling_convention"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCalling_convention([NotNull] SpecParserParser.Calling_conventionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.compiler_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterCompiler_option([NotNull] SpecParserParser.Compiler_optionContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.compiler_option"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitCompiler_option([NotNull] SpecParserParser.Compiler_optionContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.pattern"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPattern([NotNull] SpecParserParser.PatternContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.pattern"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPattern([NotNull] SpecParserParser.PatternContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.re"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRe([NotNull] SpecParserParser.ReContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.re"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRe([NotNull] SpecParserParser.ReContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.simple_re"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSimple_re([NotNull] SpecParserParser.Simple_reContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.simple_re"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSimple_re([NotNull] SpecParserParser.Simple_reContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.basic_re"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterBasic_re([NotNull] SpecParserParser.Basic_reContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.basic_re"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitBasic_re([NotNull] SpecParserParser.Basic_reContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.star"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterStar([NotNull] SpecParserParser.StarContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.star"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitStar([NotNull] SpecParserParser.StarContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.plus"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPlus([NotNull] SpecParserParser.PlusContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.plus"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPlus([NotNull] SpecParserParser.PlusContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.elementary_re"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterElementary_re([NotNull] SpecParserParser.Elementary_reContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.elementary_re"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitElementary_re([NotNull] SpecParserParser.Elementary_reContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.group"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterGroup([NotNull] SpecParserParser.GroupContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.group"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitGroup([NotNull] SpecParserParser.GroupContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.any"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterAny([NotNull] SpecParserParser.AnyContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.any"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitAny([NotNull] SpecParserParser.AnyContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.eos"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterEos([NotNull] SpecParserParser.EosContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.eos"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitEos([NotNull] SpecParserParser.EosContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.char"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterChar([NotNull] SpecParserParser.CharContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.char"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitChar([NotNull] SpecParserParser.CharContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.set"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSet([NotNull] SpecParserParser.SetContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.set"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSet([NotNull] SpecParserParser.SetContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.positive_set"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterPositive_set([NotNull] SpecParserParser.Positive_setContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.positive_set"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitPositive_set([NotNull] SpecParserParser.Positive_setContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.negative_set"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterNegative_set([NotNull] SpecParserParser.Negative_setContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.negative_set"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitNegative_set([NotNull] SpecParserParser.Negative_setContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.set_items"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSet_items([NotNull] SpecParserParser.Set_itemsContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.set_items"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSet_items([NotNull] SpecParserParser.Set_itemsContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.set_item"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterSet_item([NotNull] SpecParserParser.Set_itemContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.set_item"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitSet_item([NotNull] SpecParserParser.Set_itemContext context);
	/// <summary>
	/// Enter a parse tree produced by <see cref="SpecParserParser.range"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void EnterRange([NotNull] SpecParserParser.RangeContext context);
	/// <summary>
	/// Exit a parse tree produced by <see cref="SpecParserParser.range"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	void ExitRange([NotNull] SpecParserParser.RangeContext context);
}
