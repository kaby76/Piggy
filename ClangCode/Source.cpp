#include "clang/ASTMatchers/Dynamic/VariantValue.h"
#include "clang/Frontend/TextDiagnosticPrinter.h"
#include "clang/Basic/DiagnosticOptions.h"
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


#ifdef __cplusplus
extern "C" {
#endif

	class SearchingAst
	{
	public:
		std::list<char*> compiler_option;
		std::list<char*> include_files;
		clang::tooling::CommonOptionsParser * options_parser;
		clang::tooling::ClangTool * tool;
		std::vector<std::unique_ptr<clang::ASTUnit>> ASTs;
	};
	static SearchingAst * search = new SearchingAst();
#define _strdup strdup

	void Internal_ClangAddOption(char * i)
	{
		char* a = _strdup((char const *)i);
		search->compiler_option.insert(search->compiler_option.end(), a);
	}

	void Internal_ClangAddFile(char * i)
	{
		char* a = _strdup((char const *)i);
		search->include_files.insert(search->include_files.end(), a);
	}

	extern std::string RunTheDamnThing(clang::ASTContext &Context);

	bool _packed_ast;

	char * Internal_ClangSerializeAst()
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

		clang::IntrusiveRefCntPtr<clang::DiagnosticOptions> DiagOpts = new clang::DiagnosticOptions();
		clang::TextDiagnosticPrinter * diagnostic_consumer = new clang::TextDiagnosticPrinter(llvm::errs(), &*DiagOpts);

		search->tool->setDiagnosticConsumer(diagnostic_consumer);
		
		int r = search->tool->buildASTs(search->ASTs);
		int s = diagnostic_consumer->getNumErrors();
		delete diagnostic_consumer;

		std::cout.flush();
		std::cerr.flush();

		if (r != 0)
			return nullptr;

		if (s > 0)
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

	void Internal_ClangSetPackedAst(int packed_ast)
	{
		_packed_ast = (bool)packed_ast;
	}

	char * Internal_Name(void * q)
	{
		clang::ast_type_traits::DynTypedNode* p = (clang::ast_type_traits::DynTypedNode*)q;
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

#ifdef __cplusplus
}
#endif
