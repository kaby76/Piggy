#include "clang/ASTMatchers/Dynamic/VariantValue.h"
#include "clang/Basic/LLVM.h"
#include "llvm/ADT/ArrayRef.h"
#include "llvm/ADT/StringRef.h"
#include "llvm/ADT/Twine.h"
#include "llvm/Support/raw_ostream.h"
#include "clang/Tooling/CommonOptionsParser.h"
#include "clang/ASTMatchers/Dynamic/Parser.h"
#include "clang/Frontend/TextDiagnostic.h"
#include "clang/Basic/CharInfo.h"
#include "llvm/ADT/StringRef.h"
#include "llvm/ADT/StringSwitch.h"
#include <set>
#include <string>
#include <vector>
#include "clang/ASTMatchers/ASTMatchFinder.h"

#include "clang/Tooling/Tooling.h"
#include <iostream>


struct CollectBoundNodes : clang::ast_matchers::MatchFinder::MatchCallback {
    std::vector<clang::ast_matchers::BoundNodes> &Bindings;
    CollectBoundNodes(std::vector<clang::ast_matchers::BoundNodes> &Bindings) : Bindings(Bindings) {}
    void run(const clang::ast_matchers::MatchFinder::MatchResult &Result) override {
        Bindings.push_back(Result.Nodes);
    }
};
static llvm::cl::extrahelp CommonHelp(clang::tooling::CommonOptionsParser::HelpMessage);
static llvm::cl::OptionCategory ClangQueryCategory("clang-query options");

static llvm::cl::list<std::string> Commands("c", llvm::cl::desc("Specify command to run"),
                                            llvm::cl::value_desc("command"),
                                            llvm::cl::cat(ClangQueryCategory));

static llvm::cl::list<std::string> CommandFiles("f",
                                                llvm::cl::desc("Read commands from file"),
                                                llvm::cl::value_desc("file"),
                                                llvm::cl::cat(ClangQueryCategory));



#if defined(_MSC_VER)
//  Microsoft 
#define EXPORT __declspec(dllexport)
#define IMPORT __declspec(dllimport)
#elif defined(__GNUC__)
//  GCC
#define EXPORT __attribute__((visibility("default")))
#define IMPORT
#else
//  do nothing and hope for the best?
#define EXPORT
#define IMPORT
#pragma warning Unknown dynamic link import/export semantics.
#endif

#ifndef STDCALL
# if defined(_MSC_VER)
#   define STDCALL __stdcall
# else
#   define STDCALL
# endif
#endif


static char * search_pattern;
static std::list<char*> compiler_option;
static std::list<char*> include_files;
std::vector<std::unique_ptr<clang::ASTUnit>>::iterator cur_ast;

#ifdef __cplusplus
extern "C" {
#endif

	EXPORT void SearchSetPattern(char * ss)
	{
		search_pattern = _strdup(ss);
	}

	EXPORT void SearchAddCompilerOption(char * i)
	{
		char* a = _strdup((char const *)i);
		compiler_option.insert(compiler_option.end(), a);
	}

	EXPORT void SearchAddFile(char * i)
	{
		char* a = _strdup((char const *)i);
		include_files.insert(include_files.end(), a);
	}

	EXPORT clang::ast_type_traits::DynTypedNode** Search()
	{
		int count = 3 + compiler_option.size() + include_files.size();
		int argc = count - 1;
		char **argv = (char **)malloc(count * sizeof(char*));
		char ** p = argv;
		*p++ = (char*)"program";
		for (auto i = compiler_option.begin(); i != compiler_option.end(); ++i)
		{
			std::string s = std::string("-extra-arg-before=") + *i;
			*p++ = (char*)_strdup(s.c_str());
		}
		for (auto i = include_files.begin(); i != include_files.end(); ++i)
		{
			*p++ = *i;
		}
		*p++ = (char*)"--";
		*p++ = 0;

		clang::tooling::CommonOptionsParser OptionsParser(argc, (const char **)argv, ClangQueryCategory);

		// Get AST.
		clang::tooling::ClangTool Tool(OptionsParser.getCompilations(),
			OptionsParser.getSourcePathList());
		std::vector<std::unique_ptr<clang::ASTUnit>> ASTs;
		if (Tool.buildASTs(ASTs) != 0)
			return nullptr;

		// Get Matcher.
		clang::ast_matchers::dynamic::Diagnostics Diag;
		llvm::StringMap<clang::ast_matchers::dynamic::VariantValue> NamedValues;
		llvm::Optional<clang::ast_matchers::internal::DynTypedMatcher> Matcher
			= clang::ast_matchers::dynamic::Parser::parseMatcherExpression(
				StringRef(search_pattern), nullptr, &NamedValues, &Diag);
		if (!Matcher)
			return nullptr;

		clang::ast_matchers::MatchFinder Finder;
		std::vector<clang::ast_matchers::BoundNodes> Matches;
		CollectBoundNodes Collect(Matches);
		llvm::Optional<clang::ast_matchers::internal::DynTypedMatcher> M = Matcher->tryBind("root");
		clang::ast_matchers::internal::DynTypedMatcher MaybeBoundMatcher = *M;
		Finder.addDynamicMatcher(MaybeBoundMatcher, &Collect);
		cur_ast = ASTs.begin();
		std::unique_ptr<clang::ASTUnit>::pointer aa = cur_ast->get();
		Finder.matchAST(aa->getASTContext());
		int c = Matches.size();
		clang::ast_type_traits::DynTypedNode** result = (clang::ast_type_traits::DynTypedNode**)malloc((c + 1) * sizeof(clang::ast_type_traits::DynTypedNode*));
		clang::ast_type_traits::DynTypedNode** r = result;
		for (std::vector<clang::ast_matchers::BoundNodes>::iterator MI = Matches.begin(), ME = Matches.end(); MI != ME; ++MI) {
			for (std::map<std::basic_string<char>, clang::ast_type_traits::DynTypedNode>::const_iterator
				BI = MI->getMap().begin(), BE = MI->getMap().end(); BI != BE;
				++BI) {
					{
						clang::ast_type_traits::DynTypedNode x = BI->second;
						clang::ast_type_traits::DynTypedNode* y = (clang::ast_type_traits::DynTypedNode*)malloc(sizeof(clang::ast_type_traits::DynTypedNode));
						*y = x;
						*r++ = y;
					}
			}
		}
		*r++ = nullptr;
		return result;
	}

	EXPORT char * Name(clang::ast_type_traits::DynTypedNode* p)
	{
		//if (const clang::TemplateArgument *TA = p->get<clang::TemplateArgument>())
		//	TA->print(PP, OS);
		//else if (const clang::TemplateName *TN = p->get<clang::TemplateName>())
		//	TN->print(OS, PP);
		//else if (const clang::NestedNameSpecifier *NNS = p->get<clang::NestedNameSpecifier>())
		//	NNS->print(OS, PP);
		//else if (const clang::NestedNameSpecifierLoc *NNSL = p->get<clang::NestedNameSpecifierLoc>())
		//	NNSL->getNestedNameSpecifier()->print(OS, PP);
		//else if (const clang::QualType *QT = p->get<clang::QualType>())
		//	QT->print(OS, PP);
		//else if (const clang::TypeLoc *TL = p->get<clang::TypeLoc>())
		//	TL->getType().print(OS, PP);
		//else if (const clang::Decl *D = p->get<clang::Decl>())
		//	D->print(OS, PP);
		//else if (const clang::Stmt *S = p->get<clang::Stmt>())
		//	S->printPretty(OS, nullptr, PP);
		//else if (const clang::Type *T = p->get<clang::Type>())
		//	QualType(T, 0).print(OS, PP);
		//else
		//	OS << "Unable to print values of type " << NodeKind.asStringRef() << "\n";
		return nullptr;
	}


#ifdef __cplusplus
}
#endif
