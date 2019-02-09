#include "clang/ASTMatchers/Dynamic/VariantValue.h"
#include "llvm/ADT/StringRef.h"
#include "clang/Tooling/CommonOptionsParser.h"
#include "clang/ASTMatchers/Dynamic/Parser.h"
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



#ifdef __cplusplus
extern "C" {
#endif

	class SearchingAst
	{
	public:
		char * search_pattern;
		std::list<char*> compiler_option;
		std::list<char*> include_files;
		std::vector<std::unique_ptr<clang::ASTUnit>>::iterator cur_ast;
		clang::tooling::CommonOptionsParser * options_parser;
		clang::tooling::ClangTool * tool;
		std::vector<std::unique_ptr<clang::ASTUnit>> ASTs;
		clang::ast_matchers::dynamic::Diagnostics Diag;
		llvm::StringMap<clang::ast_matchers::dynamic::VariantValue> NamedValues;
		llvm::Optional<clang::ast_matchers::internal::DynTypedMatcher> Matcher;
		clang::ast_matchers::MatchFinder Finder;
		std::vector<clang::ast_matchers::BoundNodes> Matches;
		CollectBoundNodes * Collect;
		std::vector<clang::ast_matchers::BoundNodes>::iterator MI;
		std::vector<clang::ast_matchers::BoundNodes>::iterator ME;
	};
	static SearchingAst * search = new SearchingAst();

	EXPORT void ClangAddOption(char * i)
	{
		char* a = _strdup((char const *)i);
		search->compiler_option.insert(search->compiler_option.end(), a);
	}

	EXPORT void ClangAddFile(char * i)
	{
		char* a = _strdup((char const *)i);
		search->include_files.insert(search->include_files.end(), a);
	}

	extern std::string RunTheDamnThing(clang::ASTContext &Context);

	bool _packed_ast;

	EXPORT char * ClangSerializeAst()
	{
		int count = 3 + search->compiler_option.size() + search->include_files.size();
		int argc = count - 1;
		char **argv = (char **)malloc(count * sizeof(char*));
		char ** p = argv;
		*p++ = (char*)"program";
		for (auto i = search->compiler_option.begin(); i != search->compiler_option.end(); ++i)
		{
			std::string s = std::string("-extra-arg-before=") + *i;
			*p++ = (char*)_strdup(s.c_str());
		}
		for (auto i = search->include_files.begin(); i != search->include_files.end(); ++i)
		{
			*p++ = *i;
		}
		*p++ = (char*)"--";
		*p++ = 0;

		search->options_parser = new clang::tooling::CommonOptionsParser(argc, (const char **)argv, ClangQueryCategory);
		search->tool = new clang::tooling::ClangTool(search->options_parser->getCompilations(),
			search->options_parser->getSourcePathList());
		if (search->tool->buildASTs(search->ASTs) != 0)
			return nullptr;

		// Let's try tree walking.
		std::vector<std::unique_ptr<clang::ASTUnit>>::iterator a = search->ASTs.begin();
		std::string scratch;
		for ( ; a != search->ASTs.end(); ++a)
		{
			std::unique_ptr<clang::ASTUnit>::pointer aa = a->get();
			std::string r = RunTheDamnThing(aa->getASTContext());
			scratch.append(r);
		}
		return strdup(scratch.c_str());
	}

	EXPORT void ClangSetPackedAst(bool packed_ast)
	{
		_packed_ast = packed_ast;
	}

	EXPORT char * Name(clang::ast_type_traits::DynTypedNode* p)
	{
		if (const clang::TemplateArgument *TA = p->get<clang::TemplateArgument>())
			std::cout << "ta" << std::endl;
		//	TA->print(PP, OS);
		else if (const clang::TemplateName *TN = p->get<clang::TemplateName>())
			std::cout << "TN" << std::endl;
		//	TN->print(OS, PP);
		else if (const clang::NestedNameSpecifier *NNS = p->get<clang::NestedNameSpecifier>())
			std::cout << "NNS" << std::endl;
		//	NNS->print(OS, PP);
		else if (const clang::NestedNameSpecifierLoc *NNSL = p->get<clang::NestedNameSpecifierLoc>())
			std::cout << "NNSL" << std::endl;
		//	NNSL->getNestedNameSpecifier()->print(OS, PP);
		else if (const clang::QualType *QT = p->get<clang::QualType>())
			std::cout << "QT" << std::endl;
		//	QT->print(OS, PP);
		else if (const clang::TypeLoc *TL = p->get<clang::TypeLoc>())
			std::cout << "TL" << std::endl;
		//	TL->getType().print(OS, PP);
		else if (const clang::EnumDecl *ED = p->get<clang::EnumDecl>())
		{
			const clang::NamedDecl *D = p->get<clang::NamedDecl>();
			StringRef a1 = D->getName();
			clang::DeclarationName a2 = D->getDeclName();
			const char* a3 = a1.data();
			return (char*)a3;
		}
		//	TL->getType().print(OS, PP);
		else if (const clang::Decl *D = p->get<clang::Decl>())
		{
			auto vv = D->getKind();
			char * s = (char *)D->getDeclKindName();
			return s;
		}
		//	D->print(OS, PP);
		else if (const clang::Stmt *S = p->get<clang::Stmt>())
			std::cout << "S" << std::endl;
		//	S->printPretty(OS, nullptr, PP);
		else if (const clang::Type *T = p->get<clang::Type>())
			std::cout << "T" << std::endl;
		//	QualType(T, 0).print(OS, PP);
		//else
		//	OS << "Unable to print values of type " << NodeKind.asStringRef() << "\n";
		return nullptr;
	}

	EXPORT void DumpyAST()
	{
		int count = 3 + search->compiler_option.size() + search->include_files.size();
		int argc = count - 1;
		char **argv = (char **)malloc(count * sizeof(char*));
		char ** p = argv;
		*p++ = (char*)"program";
		for (auto i = search->compiler_option.begin(); i != search->compiler_option.end(); ++i)
		{
			std::string s = std::string("-extra-arg-before=") + *i;
			*p++ = (char*)_strdup(s.c_str());
		}
		for (auto i = search->include_files.begin(); i != search->include_files.end(); ++i)
		{
			*p++ = *i;
		}
		*p++ = (char*)"--";
		*p++ = 0;

		search->options_parser = new clang::tooling::CommonOptionsParser(argc, (const char **)argv, ClangQueryCategory);
		search->tool = new clang::tooling::ClangTool(search->options_parser->getCompilations(),
			search->options_parser->getSourcePathList());
		if (search->tool->buildASTs(search->ASTs) != 0)
			return;


	}

#ifdef __cplusplus
}
#endif
