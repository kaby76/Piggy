//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.7.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from SpecLexer.g4 by ANTLR 4.7.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.7.1")]
[System.CLSCompliant(false)]
public partial class SpecLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		SINGLE_LINE_DOC_COMMENT=1, DELIMITED_DOC_COMMENT=2, SINGLE_LINE_COMMENT=3, 
		DELIMITED_COMMENT=4, CODE=5, CLANG_FILE=6, CLANG_OPTION=7, EXTENDS=8, 
		NAMESPACE=9, PASS=10, TEMPLATE=11, REWRITE=12, EQ=13, SEMI=14, OR=15, 
		STAR=16, PLUS=17, DOT=18, DOLLAR=19, OPEN_RE=20, CLOSE_RE=21, OPEN_PAREN=22, 
		CLOSE_PAREN=23, OPEN_KLEENE_STAR_PAREN=24, CLOSE_KLEENE_STAR_PAREN=25, 
		OPEN_BRACKET_NOT=26, OPEN_BRACKET=27, CLOSE_BRACKET=28, MINUS=29, LCURLY=30, 
		LANG=31, StringLiteral=32, ID=33, WS=34, RCURLY=35, OTHER=36, RANG=37, 
		OTHER_ANG=38;
	public const int
		COMMENTS_CHANNEL=2;
	public const int
		CODE_0=1, TEXT_0=2;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN", "COMMENTS_CHANNEL"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE", "CODE_0", "TEXT_0"
	};

	public static readonly string[] ruleNames = {
		"SINGLE_LINE_DOC_COMMENT", "DELIMITED_DOC_COMMENT", "SINGLE_LINE_COMMENT", 
		"DELIMITED_COMMENT", "CODE", "CLANG_FILE", "CLANG_OPTION", "EXTENDS", 
		"NAMESPACE", "PASS", "TEMPLATE", "REWRITE", "EQ", "SEMI", "OR", "STAR", 
		"PLUS", "DOT", "DOLLAR", "OPEN_RE", "CLOSE_RE", "OPEN_PAREN", "CLOSE_PAREN", 
		"OPEN_KLEENE_STAR_PAREN", "CLOSE_KLEENE_STAR_PAREN", "OPEN_BRACKET_NOT", 
		"OPEN_BRACKET", "CLOSE_BRACKET", "MINUS", "LCURLY", "LANG", "StringLiteral", 
		"ID", "InputCharacter", "Escape", "WS", "CODE_0_LCURLY", "RCURLY", "OTHER", 
		"TEXT_0_LANG", "RANG", "OTHER_ANG"
	};


	public SpecLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public SpecLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, null, null, null, null, "'code'", "'clang_file'", "'clang_option'", 
		"'extends'", "'namespace'", "'pass'", "'template'", "'=>'", "'='", "';'", 
		"'|'", "'*'", "'+'", "'.'", "'$'", "'(%'", "'%)'", "'('", "')'", "'(*'", 
		"'*)'", "'[^'", "'['", "']'", "'-'", null, null, null, null, null, "'}}'", 
		null, "']]'"
	};
	private static readonly string[] _SymbolicNames = {
		null, "SINGLE_LINE_DOC_COMMENT", "DELIMITED_DOC_COMMENT", "SINGLE_LINE_COMMENT", 
		"DELIMITED_COMMENT", "CODE", "CLANG_FILE", "CLANG_OPTION", "EXTENDS", 
		"NAMESPACE", "PASS", "TEMPLATE", "REWRITE", "EQ", "SEMI", "OR", "STAR", 
		"PLUS", "DOT", "DOLLAR", "OPEN_RE", "CLOSE_RE", "OPEN_PAREN", "CLOSE_PAREN", 
		"OPEN_KLEENE_STAR_PAREN", "CLOSE_KLEENE_STAR_PAREN", "OPEN_BRACKET_NOT", 
		"OPEN_BRACKET", "CLOSE_BRACKET", "MINUS", "LCURLY", "LANG", "StringLiteral", 
		"ID", "WS", "RCURLY", "OTHER", "RANG", "OTHER_ANG"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "SpecLexer.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static SpecLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x2', '(', '\x13E', '\b', '\x1', '\b', '\x1', '\b', '\x1', 
		'\x4', '\x2', '\t', '\x2', '\x4', '\x3', '\t', '\x3', '\x4', '\x4', '\t', 
		'\x4', '\x4', '\x5', '\t', '\x5', '\x4', '\x6', '\t', '\x6', '\x4', '\a', 
		'\t', '\a', '\x4', '\b', '\t', '\b', '\x4', '\t', '\t', '\t', '\x4', '\n', 
		'\t', '\n', '\x4', '\v', '\t', '\v', '\x4', '\f', '\t', '\f', '\x4', '\r', 
		'\t', '\r', '\x4', '\xE', '\t', '\xE', '\x4', '\xF', '\t', '\xF', '\x4', 
		'\x10', '\t', '\x10', '\x4', '\x11', '\t', '\x11', '\x4', '\x12', '\t', 
		'\x12', '\x4', '\x13', '\t', '\x13', '\x4', '\x14', '\t', '\x14', '\x4', 
		'\x15', '\t', '\x15', '\x4', '\x16', '\t', '\x16', '\x4', '\x17', '\t', 
		'\x17', '\x4', '\x18', '\t', '\x18', '\x4', '\x19', '\t', '\x19', '\x4', 
		'\x1A', '\t', '\x1A', '\x4', '\x1B', '\t', '\x1B', '\x4', '\x1C', '\t', 
		'\x1C', '\x4', '\x1D', '\t', '\x1D', '\x4', '\x1E', '\t', '\x1E', '\x4', 
		'\x1F', '\t', '\x1F', '\x4', ' ', '\t', ' ', '\x4', '!', '\t', '!', '\x4', 
		'\"', '\t', '\"', '\x4', '#', '\t', '#', '\x4', '$', '\t', '$', '\x4', 
		'%', '\t', '%', '\x4', '&', '\t', '&', '\x4', '\'', '\t', '\'', '\x4', 
		'(', '\t', '(', '\x4', ')', '\t', ')', '\x4', '*', '\t', '*', '\x4', '+', 
		'\t', '+', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', 
		'\x2', '\a', '\x2', '_', '\n', '\x2', '\f', '\x2', '\xE', '\x2', '\x62', 
		'\v', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\a', '\x3', 'k', '\n', '\x3', '\f', 
		'\x3', '\xE', '\x3', 'n', '\v', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x4', '\x3', '\x4', '\x3', 
		'\x4', '\x3', '\x4', '\a', '\x4', 'y', '\n', '\x4', '\f', '\x4', '\xE', 
		'\x4', '|', '\v', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x5', '\x3', 
		'\x5', '\x3', '\x5', '\x3', '\x5', '\a', '\x5', '\x84', '\n', '\x5', '\f', 
		'\x5', '\xE', '\x5', '\x87', '\v', '\x5', '\x3', '\x5', '\x3', '\x5', 
		'\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x6', '\x3', '\x6', 
		'\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\a', '\x3', '\a', '\x3', 
		'\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\a', 
		'\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\b', '\x3', '\b', '\x3', 
		'\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', 
		'\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', 
		'\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', 
		'\x3', '\t', '\x3', '\t', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', 
		'\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\n', 
		'\x3', '\n', '\x3', '\v', '\x3', '\v', '\x3', '\v', '\x3', '\v', '\x3', 
		'\v', '\x3', '\f', '\x3', '\f', '\x3', '\f', '\x3', '\f', '\x3', '\f', 
		'\x3', '\f', '\x3', '\f', '\x3', '\f', '\x3', '\f', '\x3', '\r', '\x3', 
		'\r', '\x3', '\r', '\x3', '\xE', '\x3', '\xE', '\x3', '\xF', '\x3', '\xF', 
		'\x3', '\x10', '\x3', '\x10', '\x3', '\x11', '\x3', '\x11', '\x3', '\x12', 
		'\x3', '\x12', '\x3', '\x13', '\x3', '\x13', '\x3', '\x14', '\x3', '\x14', 
		'\x3', '\x15', '\x3', '\x15', '\x3', '\x15', '\x3', '\x16', '\x3', '\x16', 
		'\x3', '\x16', '\x3', '\x17', '\x3', '\x17', '\x3', '\x18', '\x3', '\x18', 
		'\x3', '\x19', '\x3', '\x19', '\x3', '\x19', '\x3', '\x1A', '\x3', '\x1A', 
		'\x3', '\x1A', '\x3', '\x1B', '\x3', '\x1B', '\x3', '\x1B', '\x3', '\x1C', 
		'\x3', '\x1C', '\x3', '\x1D', '\x3', '\x1D', '\x3', '\x1E', '\x3', '\x1E', 
		'\x3', '\x1F', '\x3', '\x1F', '\x3', '\x1F', '\x3', '\x1F', '\x3', '\x1F', 
		'\x3', ' ', '\x3', ' ', '\x3', ' ', '\x3', ' ', '\x3', ' ', '\x3', '!', 
		'\x3', '!', '\x3', '!', '\a', '!', '\x102', '\n', '!', '\f', '!', '\xE', 
		'!', '\x105', '\v', '!', '\x3', '!', '\x3', '!', '\x3', '!', '\x3', '!', 
		'\a', '!', '\x10B', '\n', '!', '\f', '!', '\xE', '!', '\x10E', '\v', '!', 
		'\x3', '!', '\x5', '!', '\x111', '\n', '!', '\x3', '\"', '\x6', '\"', 
		'\x114', '\n', '\"', '\r', '\"', '\xE', '\"', '\x115', '\x3', '#', '\x3', 
		'#', '\x3', '$', '\x3', '$', '\x3', '$', '\x3', '%', '\x3', '%', '\x3', 
		'%', '\x3', '%', '\x3', '&', '\x3', '&', '\x3', '&', '\x3', '&', '\x3', 
		'&', '\x3', '\'', '\x3', '\'', '\x3', '\'', '\x3', '\'', '\x3', '\'', 
		'\x3', '(', '\x3', '(', '\x3', '(', '\x5', '(', '\x12E', '\n', '(', '\x3', 
		')', '\x3', ')', '\x3', ')', '\x3', ')', '\x3', ')', '\x3', '*', '\x3', 
		'*', '\x3', '*', '\x3', '*', '\x3', '*', '\x3', '+', '\x3', '+', '\x3', 
		'+', '\x5', '+', '\x13D', '\n', '+', '\x4', 'l', '\x85', '\x2', ',', '\x5', 
		'\x3', '\a', '\x4', '\t', '\x5', '\v', '\x6', '\r', '\a', '\xF', '\b', 
		'\x11', '\t', '\x13', '\n', '\x15', '\v', '\x17', '\f', '\x19', '\r', 
		'\x1B', '\xE', '\x1D', '\xF', '\x1F', '\x10', '!', '\x11', '#', '\x12', 
		'%', '\x13', '\'', '\x14', ')', '\x15', '+', '\x16', '-', '\x17', '/', 
		'\x18', '\x31', '\x19', '\x33', '\x1A', '\x35', '\x1B', '\x37', '\x1C', 
		'\x39', '\x1D', ';', '\x1E', '=', '\x1F', '?', ' ', '\x41', '!', '\x43', 
		'\"', '\x45', '#', 'G', '\x2', 'I', '\x2', 'K', '$', 'M', '\x2', 'O', 
		'%', 'Q', '&', 'S', '\x2', 'U', '\'', 'W', '(', '\x5', '\x2', '\x3', '\x4', 
		'\t', '\x5', '\x2', '\f', '\f', '\xF', '\xF', ')', ')', '\x5', '\x2', 
		'\f', '\f', '\xF', '\xF', '$', '$', '\a', '\x2', '\x30', '\x30', '\x32', 
		';', '\x43', '\\', '\x61', '\x61', '\x63', '|', '\x6', '\x2', '\f', '\f', 
		'\xF', '\xF', '\x87', '\x87', '\x202A', '\x202B', '\x5', '\x2', '\v', 
		'\f', '\xF', '\xF', '\"', '\"', '\x3', '\x2', '\x7F', '\x7F', '\x3', '\x2', 
		'_', '_', '\x2', '\x145', '\x2', '\x5', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\a', '\x3', '\x2', '\x2', '\x2', '\x2', '\t', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\v', '\x3', '\x2', '\x2', '\x2', '\x2', '\r', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\xF', '\x3', '\x2', '\x2', '\x2', '\x2', '\x11', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x13', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x15', '\x3', '\x2', '\x2', '\x2', '\x2', '\x17', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x19', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1B', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x1D', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x1F', '\x3', '\x2', '\x2', '\x2', '\x2', '!', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '#', '\x3', '\x2', '\x2', '\x2', '\x2', '%', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\'', '\x3', '\x2', '\x2', '\x2', '\x2', ')', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '+', '\x3', '\x2', '\x2', '\x2', '\x2', '-', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '/', '\x3', '\x2', '\x2', '\x2', '\x2', '\x31', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\x33', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\x35', '\x3', '\x2', '\x2', '\x2', '\x2', '\x37', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\x39', '\x3', '\x2', '\x2', '\x2', '\x2', ';', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '=', '\x3', '\x2', '\x2', '\x2', '\x2', '?', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\x41', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\x43', '\x3', '\x2', '\x2', '\x2', '\x2', '\x45', '\x3', '\x2', 
		'\x2', '\x2', '\x2', 'K', '\x3', '\x2', '\x2', '\x2', '\x3', 'M', '\x3', 
		'\x2', '\x2', '\x2', '\x3', 'O', '\x3', '\x2', '\x2', '\x2', '\x3', 'Q', 
		'\x3', '\x2', '\x2', '\x2', '\x4', 'S', '\x3', '\x2', '\x2', '\x2', '\x4', 
		'U', '\x3', '\x2', '\x2', '\x2', '\x4', 'W', '\x3', '\x2', '\x2', '\x2', 
		'\x5', 'Y', '\x3', '\x2', '\x2', '\x2', '\a', '\x65', '\x3', '\x2', '\x2', 
		'\x2', '\t', 't', '\x3', '\x2', '\x2', '\x2', '\v', '\x7F', '\x3', '\x2', 
		'\x2', '\x2', '\r', '\x8D', '\x3', '\x2', '\x2', '\x2', '\xF', '\x92', 
		'\x3', '\x2', '\x2', '\x2', '\x11', '\x9D', '\x3', '\x2', '\x2', '\x2', 
		'\x13', '\xAA', '\x3', '\x2', '\x2', '\x2', '\x15', '\xB2', '\x3', '\x2', 
		'\x2', '\x2', '\x17', '\xBC', '\x3', '\x2', '\x2', '\x2', '\x19', '\xC1', 
		'\x3', '\x2', '\x2', '\x2', '\x1B', '\xCA', '\x3', '\x2', '\x2', '\x2', 
		'\x1D', '\xCD', '\x3', '\x2', '\x2', '\x2', '\x1F', '\xCF', '\x3', '\x2', 
		'\x2', '\x2', '!', '\xD1', '\x3', '\x2', '\x2', '\x2', '#', '\xD3', '\x3', 
		'\x2', '\x2', '\x2', '%', '\xD5', '\x3', '\x2', '\x2', '\x2', '\'', '\xD7', 
		'\x3', '\x2', '\x2', '\x2', ')', '\xD9', '\x3', '\x2', '\x2', '\x2', '+', 
		'\xDB', '\x3', '\x2', '\x2', '\x2', '-', '\xDE', '\x3', '\x2', '\x2', 
		'\x2', '/', '\xE1', '\x3', '\x2', '\x2', '\x2', '\x31', '\xE3', '\x3', 
		'\x2', '\x2', '\x2', '\x33', '\xE5', '\x3', '\x2', '\x2', '\x2', '\x35', 
		'\xE8', '\x3', '\x2', '\x2', '\x2', '\x37', '\xEB', '\x3', '\x2', '\x2', 
		'\x2', '\x39', '\xEE', '\x3', '\x2', '\x2', '\x2', ';', '\xF0', '\x3', 
		'\x2', '\x2', '\x2', '=', '\xF2', '\x3', '\x2', '\x2', '\x2', '?', '\xF4', 
		'\x3', '\x2', '\x2', '\x2', '\x41', '\xF9', '\x3', '\x2', '\x2', '\x2', 
		'\x43', '\x110', '\x3', '\x2', '\x2', '\x2', '\x45', '\x113', '\x3', '\x2', 
		'\x2', '\x2', 'G', '\x117', '\x3', '\x2', '\x2', '\x2', 'I', '\x119', 
		'\x3', '\x2', '\x2', '\x2', 'K', '\x11C', '\x3', '\x2', '\x2', '\x2', 
		'M', '\x120', '\x3', '\x2', '\x2', '\x2', 'O', '\x125', '\x3', '\x2', 
		'\x2', '\x2', 'Q', '\x12D', '\x3', '\x2', '\x2', '\x2', 'S', '\x12F', 
		'\x3', '\x2', '\x2', '\x2', 'U', '\x134', '\x3', '\x2', '\x2', '\x2', 
		'W', '\x13C', '\x3', '\x2', '\x2', '\x2', 'Y', 'Z', '\a', '\x31', '\x2', 
		'\x2', 'Z', '[', '\a', '\x31', '\x2', '\x2', '[', '\\', '\a', '\x31', 
		'\x2', '\x2', '\\', '`', '\x3', '\x2', '\x2', '\x2', ']', '_', '\x5', 
		'G', '#', '\x2', '^', ']', '\x3', '\x2', '\x2', '\x2', '_', '\x62', '\x3', 
		'\x2', '\x2', '\x2', '`', '^', '\x3', '\x2', '\x2', '\x2', '`', '\x61', 
		'\x3', '\x2', '\x2', '\x2', '\x61', '\x63', '\x3', '\x2', '\x2', '\x2', 
		'\x62', '`', '\x3', '\x2', '\x2', '\x2', '\x63', '\x64', '\b', '\x2', 
		'\x2', '\x2', '\x64', '\x6', '\x3', '\x2', '\x2', '\x2', '\x65', '\x66', 
		'\a', '\x31', '\x2', '\x2', '\x66', 'g', '\a', ',', '\x2', '\x2', 'g', 
		'h', '\a', ',', '\x2', '\x2', 'h', 'l', '\x3', '\x2', '\x2', '\x2', 'i', 
		'k', '\v', '\x2', '\x2', '\x2', 'j', 'i', '\x3', '\x2', '\x2', '\x2', 
		'k', 'n', '\x3', '\x2', '\x2', '\x2', 'l', 'm', '\x3', '\x2', '\x2', '\x2', 
		'l', 'j', '\x3', '\x2', '\x2', '\x2', 'm', 'o', '\x3', '\x2', '\x2', '\x2', 
		'n', 'l', '\x3', '\x2', '\x2', '\x2', 'o', 'p', '\a', ',', '\x2', '\x2', 
		'p', 'q', '\a', '\x31', '\x2', '\x2', 'q', 'r', '\x3', '\x2', '\x2', '\x2', 
		'r', 's', '\b', '\x3', '\x2', '\x2', 's', '\b', '\x3', '\x2', '\x2', '\x2', 
		't', 'u', '\a', '\x31', '\x2', '\x2', 'u', 'v', '\a', '\x31', '\x2', '\x2', 
		'v', 'z', '\x3', '\x2', '\x2', '\x2', 'w', 'y', '\x5', 'G', '#', '\x2', 
		'x', 'w', '\x3', '\x2', '\x2', '\x2', 'y', '|', '\x3', '\x2', '\x2', '\x2', 
		'z', 'x', '\x3', '\x2', '\x2', '\x2', 'z', '{', '\x3', '\x2', '\x2', '\x2', 
		'{', '}', '\x3', '\x2', '\x2', '\x2', '|', 'z', '\x3', '\x2', '\x2', '\x2', 
		'}', '~', '\b', '\x4', '\x2', '\x2', '~', '\n', '\x3', '\x2', '\x2', '\x2', 
		'\x7F', '\x80', '\a', '\x31', '\x2', '\x2', '\x80', '\x81', '\a', ',', 
		'\x2', '\x2', '\x81', '\x85', '\x3', '\x2', '\x2', '\x2', '\x82', '\x84', 
		'\v', '\x2', '\x2', '\x2', '\x83', '\x82', '\x3', '\x2', '\x2', '\x2', 
		'\x84', '\x87', '\x3', '\x2', '\x2', '\x2', '\x85', '\x86', '\x3', '\x2', 
		'\x2', '\x2', '\x85', '\x83', '\x3', '\x2', '\x2', '\x2', '\x86', '\x88', 
		'\x3', '\x2', '\x2', '\x2', '\x87', '\x85', '\x3', '\x2', '\x2', '\x2', 
		'\x88', '\x89', '\a', ',', '\x2', '\x2', '\x89', '\x8A', '\a', '\x31', 
		'\x2', '\x2', '\x8A', '\x8B', '\x3', '\x2', '\x2', '\x2', '\x8B', '\x8C', 
		'\b', '\x5', '\x2', '\x2', '\x8C', '\f', '\x3', '\x2', '\x2', '\x2', '\x8D', 
		'\x8E', '\a', '\x65', '\x2', '\x2', '\x8E', '\x8F', '\a', 'q', '\x2', 
		'\x2', '\x8F', '\x90', '\a', '\x66', '\x2', '\x2', '\x90', '\x91', '\a', 
		'g', '\x2', '\x2', '\x91', '\xE', '\x3', '\x2', '\x2', '\x2', '\x92', 
		'\x93', '\a', '\x65', '\x2', '\x2', '\x93', '\x94', '\a', 'n', '\x2', 
		'\x2', '\x94', '\x95', '\a', '\x63', '\x2', '\x2', '\x95', '\x96', '\a', 
		'p', '\x2', '\x2', '\x96', '\x97', '\a', 'i', '\x2', '\x2', '\x97', '\x98', 
		'\a', '\x61', '\x2', '\x2', '\x98', '\x99', '\a', 'h', '\x2', '\x2', '\x99', 
		'\x9A', '\a', 'k', '\x2', '\x2', '\x9A', '\x9B', '\a', 'n', '\x2', '\x2', 
		'\x9B', '\x9C', '\a', 'g', '\x2', '\x2', '\x9C', '\x10', '\x3', '\x2', 
		'\x2', '\x2', '\x9D', '\x9E', '\a', '\x65', '\x2', '\x2', '\x9E', '\x9F', 
		'\a', 'n', '\x2', '\x2', '\x9F', '\xA0', '\a', '\x63', '\x2', '\x2', '\xA0', 
		'\xA1', '\a', 'p', '\x2', '\x2', '\xA1', '\xA2', '\a', 'i', '\x2', '\x2', 
		'\xA2', '\xA3', '\a', '\x61', '\x2', '\x2', '\xA3', '\xA4', '\a', 'q', 
		'\x2', '\x2', '\xA4', '\xA5', '\a', 'r', '\x2', '\x2', '\xA5', '\xA6', 
		'\a', 'v', '\x2', '\x2', '\xA6', '\xA7', '\a', 'k', '\x2', '\x2', '\xA7', 
		'\xA8', '\a', 'q', '\x2', '\x2', '\xA8', '\xA9', '\a', 'p', '\x2', '\x2', 
		'\xA9', '\x12', '\x3', '\x2', '\x2', '\x2', '\xAA', '\xAB', '\a', 'g', 
		'\x2', '\x2', '\xAB', '\xAC', '\a', 'z', '\x2', '\x2', '\xAC', '\xAD', 
		'\a', 'v', '\x2', '\x2', '\xAD', '\xAE', '\a', 'g', '\x2', '\x2', '\xAE', 
		'\xAF', '\a', 'p', '\x2', '\x2', '\xAF', '\xB0', '\a', '\x66', '\x2', 
		'\x2', '\xB0', '\xB1', '\a', 'u', '\x2', '\x2', '\xB1', '\x14', '\x3', 
		'\x2', '\x2', '\x2', '\xB2', '\xB3', '\a', 'p', '\x2', '\x2', '\xB3', 
		'\xB4', '\a', '\x63', '\x2', '\x2', '\xB4', '\xB5', '\a', 'o', '\x2', 
		'\x2', '\xB5', '\xB6', '\a', 'g', '\x2', '\x2', '\xB6', '\xB7', '\a', 
		'u', '\x2', '\x2', '\xB7', '\xB8', '\a', 'r', '\x2', '\x2', '\xB8', '\xB9', 
		'\a', '\x63', '\x2', '\x2', '\xB9', '\xBA', '\a', '\x65', '\x2', '\x2', 
		'\xBA', '\xBB', '\a', 'g', '\x2', '\x2', '\xBB', '\x16', '\x3', '\x2', 
		'\x2', '\x2', '\xBC', '\xBD', '\a', 'r', '\x2', '\x2', '\xBD', '\xBE', 
		'\a', '\x63', '\x2', '\x2', '\xBE', '\xBF', '\a', 'u', '\x2', '\x2', '\xBF', 
		'\xC0', '\a', 'u', '\x2', '\x2', '\xC0', '\x18', '\x3', '\x2', '\x2', 
		'\x2', '\xC1', '\xC2', '\a', 'v', '\x2', '\x2', '\xC2', '\xC3', '\a', 
		'g', '\x2', '\x2', '\xC3', '\xC4', '\a', 'o', '\x2', '\x2', '\xC4', '\xC5', 
		'\a', 'r', '\x2', '\x2', '\xC5', '\xC6', '\a', 'n', '\x2', '\x2', '\xC6', 
		'\xC7', '\a', '\x63', '\x2', '\x2', '\xC7', '\xC8', '\a', 'v', '\x2', 
		'\x2', '\xC8', '\xC9', '\a', 'g', '\x2', '\x2', '\xC9', '\x1A', '\x3', 
		'\x2', '\x2', '\x2', '\xCA', '\xCB', '\a', '?', '\x2', '\x2', '\xCB', 
		'\xCC', '\a', '@', '\x2', '\x2', '\xCC', '\x1C', '\x3', '\x2', '\x2', 
		'\x2', '\xCD', '\xCE', '\a', '?', '\x2', '\x2', '\xCE', '\x1E', '\x3', 
		'\x2', '\x2', '\x2', '\xCF', '\xD0', '\a', '=', '\x2', '\x2', '\xD0', 
		' ', '\x3', '\x2', '\x2', '\x2', '\xD1', '\xD2', '\a', '~', '\x2', '\x2', 
		'\xD2', '\"', '\x3', '\x2', '\x2', '\x2', '\xD3', '\xD4', '\a', ',', '\x2', 
		'\x2', '\xD4', '$', '\x3', '\x2', '\x2', '\x2', '\xD5', '\xD6', '\a', 
		'-', '\x2', '\x2', '\xD6', '&', '\x3', '\x2', '\x2', '\x2', '\xD7', '\xD8', 
		'\a', '\x30', '\x2', '\x2', '\xD8', '(', '\x3', '\x2', '\x2', '\x2', '\xD9', 
		'\xDA', '\a', '&', '\x2', '\x2', '\xDA', '*', '\x3', '\x2', '\x2', '\x2', 
		'\xDB', '\xDC', '\a', '*', '\x2', '\x2', '\xDC', '\xDD', '\a', '\'', '\x2', 
		'\x2', '\xDD', ',', '\x3', '\x2', '\x2', '\x2', '\xDE', '\xDF', '\a', 
		'\'', '\x2', '\x2', '\xDF', '\xE0', '\a', '+', '\x2', '\x2', '\xE0', '.', 
		'\x3', '\x2', '\x2', '\x2', '\xE1', '\xE2', '\a', '*', '\x2', '\x2', '\xE2', 
		'\x30', '\x3', '\x2', '\x2', '\x2', '\xE3', '\xE4', '\a', '+', '\x2', 
		'\x2', '\xE4', '\x32', '\x3', '\x2', '\x2', '\x2', '\xE5', '\xE6', '\a', 
		'*', '\x2', '\x2', '\xE6', '\xE7', '\a', ',', '\x2', '\x2', '\xE7', '\x34', 
		'\x3', '\x2', '\x2', '\x2', '\xE8', '\xE9', '\a', ',', '\x2', '\x2', '\xE9', 
		'\xEA', '\a', '+', '\x2', '\x2', '\xEA', '\x36', '\x3', '\x2', '\x2', 
		'\x2', '\xEB', '\xEC', '\a', ']', '\x2', '\x2', '\xEC', '\xED', '\a', 
		'`', '\x2', '\x2', '\xED', '\x38', '\x3', '\x2', '\x2', '\x2', '\xEE', 
		'\xEF', '\a', ']', '\x2', '\x2', '\xEF', ':', '\x3', '\x2', '\x2', '\x2', 
		'\xF0', '\xF1', '\a', '_', '\x2', '\x2', '\xF1', '<', '\x3', '\x2', '\x2', 
		'\x2', '\xF2', '\xF3', '\a', '/', '\x2', '\x2', '\xF3', '>', '\x3', '\x2', 
		'\x2', '\x2', '\xF4', '\xF5', '\a', '}', '\x2', '\x2', '\xF5', '\xF6', 
		'\a', '}', '\x2', '\x2', '\xF6', '\xF7', '\x3', '\x2', '\x2', '\x2', '\xF7', 
		'\xF8', '\b', '\x1F', '\x3', '\x2', '\xF8', '@', '\x3', '\x2', '\x2', 
		'\x2', '\xF9', '\xFA', '\a', ']', '\x2', '\x2', '\xFA', '\xFB', '\a', 
		']', '\x2', '\x2', '\xFB', '\xFC', '\x3', '\x2', '\x2', '\x2', '\xFC', 
		'\xFD', '\b', ' ', '\x4', '\x2', '\xFD', '\x42', '\x3', '\x2', '\x2', 
		'\x2', '\xFE', '\x103', '\a', ')', '\x2', '\x2', '\xFF', '\x102', '\x5', 
		'I', '$', '\x2', '\x100', '\x102', '\n', '\x2', '\x2', '\x2', '\x101', 
		'\xFF', '\x3', '\x2', '\x2', '\x2', '\x101', '\x100', '\x3', '\x2', '\x2', 
		'\x2', '\x102', '\x105', '\x3', '\x2', '\x2', '\x2', '\x103', '\x101', 
		'\x3', '\x2', '\x2', '\x2', '\x103', '\x104', '\x3', '\x2', '\x2', '\x2', 
		'\x104', '\x106', '\x3', '\x2', '\x2', '\x2', '\x105', '\x103', '\x3', 
		'\x2', '\x2', '\x2', '\x106', '\x111', '\a', ')', '\x2', '\x2', '\x107', 
		'\x10C', '\a', '$', '\x2', '\x2', '\x108', '\x10B', '\x5', 'I', '$', '\x2', 
		'\x109', '\x10B', '\n', '\x3', '\x2', '\x2', '\x10A', '\x108', '\x3', 
		'\x2', '\x2', '\x2', '\x10A', '\x109', '\x3', '\x2', '\x2', '\x2', '\x10B', 
		'\x10E', '\x3', '\x2', '\x2', '\x2', '\x10C', '\x10A', '\x3', '\x2', '\x2', 
		'\x2', '\x10C', '\x10D', '\x3', '\x2', '\x2', '\x2', '\x10D', '\x10F', 
		'\x3', '\x2', '\x2', '\x2', '\x10E', '\x10C', '\x3', '\x2', '\x2', '\x2', 
		'\x10F', '\x111', '\a', '$', '\x2', '\x2', '\x110', '\xFE', '\x3', '\x2', 
		'\x2', '\x2', '\x110', '\x107', '\x3', '\x2', '\x2', '\x2', '\x111', '\x44', 
		'\x3', '\x2', '\x2', '\x2', '\x112', '\x114', '\t', '\x4', '\x2', '\x2', 
		'\x113', '\x112', '\x3', '\x2', '\x2', '\x2', '\x114', '\x115', '\x3', 
		'\x2', '\x2', '\x2', '\x115', '\x113', '\x3', '\x2', '\x2', '\x2', '\x115', 
		'\x116', '\x3', '\x2', '\x2', '\x2', '\x116', '\x46', '\x3', '\x2', '\x2', 
		'\x2', '\x117', '\x118', '\n', '\x5', '\x2', '\x2', '\x118', 'H', '\x3', 
		'\x2', '\x2', '\x2', '\x119', '\x11A', '\a', ')', '\x2', '\x2', '\x11A', 
		'\x11B', '\a', ')', '\x2', '\x2', '\x11B', 'J', '\x3', '\x2', '\x2', '\x2', 
		'\x11C', '\x11D', '\t', '\x6', '\x2', '\x2', '\x11D', '\x11E', '\x3', 
		'\x2', '\x2', '\x2', '\x11E', '\x11F', '\b', '%', '\x5', '\x2', '\x11F', 
		'L', '\x3', '\x2', '\x2', '\x2', '\x120', '\x121', '\a', '}', '\x2', '\x2', 
		'\x121', '\x122', '\a', '}', '\x2', '\x2', '\x122', '\x123', '\x3', '\x2', 
		'\x2', '\x2', '\x123', '\x124', '\b', '&', '\x6', '\x2', '\x124', 'N', 
		'\x3', '\x2', '\x2', '\x2', '\x125', '\x126', '\a', '\x7F', '\x2', '\x2', 
		'\x126', '\x127', '\a', '\x7F', '\x2', '\x2', '\x127', '\x128', '\x3', 
		'\x2', '\x2', '\x2', '\x128', '\x129', '\b', '\'', '\a', '\x2', '\x129', 
		'P', '\x3', '\x2', '\x2', '\x2', '\x12A', '\x12B', '\a', '\x7F', '\x2', 
		'\x2', '\x12B', '\x12E', '\n', '\a', '\x2', '\x2', '\x12C', '\x12E', '\n', 
		'\a', '\x2', '\x2', '\x12D', '\x12A', '\x3', '\x2', '\x2', '\x2', '\x12D', 
		'\x12C', '\x3', '\x2', '\x2', '\x2', '\x12E', 'R', '\x3', '\x2', '\x2', 
		'\x2', '\x12F', '\x130', '\a', ']', '\x2', '\x2', '\x130', '\x131', '\a', 
		']', '\x2', '\x2', '\x131', '\x132', '\x3', '\x2', '\x2', '\x2', '\x132', 
		'\x133', '\b', ')', '\b', '\x2', '\x133', 'T', '\x3', '\x2', '\x2', '\x2', 
		'\x134', '\x135', '\a', '_', '\x2', '\x2', '\x135', '\x136', '\a', '_', 
		'\x2', '\x2', '\x136', '\x137', '\x3', '\x2', '\x2', '\x2', '\x137', '\x138', 
		'\b', '*', '\a', '\x2', '\x138', 'V', '\x3', '\x2', '\x2', '\x2', '\x139', 
		'\x13A', '\a', '_', '\x2', '\x2', '\x13A', '\x13D', '\n', '\b', '\x2', 
		'\x2', '\x13B', '\x13D', '\n', '\b', '\x2', '\x2', '\x13C', '\x139', '\x3', 
		'\x2', '\x2', '\x2', '\x13C', '\x13B', '\x3', '\x2', '\x2', '\x2', '\x13D', 
		'X', '\x3', '\x2', '\x2', '\x2', '\x11', '\x2', '\x3', '\x4', '`', 'l', 
		'z', '\x85', '\x101', '\x103', '\x10A', '\x10C', '\x110', '\x115', '\x12D', 
		'\x13C', '\t', '\x2', '\x4', '\x2', '\a', '\x3', '\x2', '\a', '\x4', '\x2', 
		'\b', '\x2', '\x2', '\t', '&', '\x2', '\x6', '\x2', '\x2', '\t', '(', 
		'\x2',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
