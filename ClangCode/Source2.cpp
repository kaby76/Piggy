//===--- ASTDumper.cpp - Dumping implementation for ASTs ------------------===//

#include "clang/AST/ASTContext.h"
#include "clang/AST/Attr.h"
#include "clang/AST/CommentVisitor.h"
#include "clang/AST/DeclCXX.h"
#include "clang/AST/DeclLookups.h"
#include "clang/AST/DeclObjC.h"
#include "clang/AST/DeclOpenMP.h"
#include "clang/AST/DeclVisitor.h"
#include "clang/AST/LocInfoType.h"
#include "clang/AST/StmtVisitor.h"
#include "clang/AST/TypeVisitor.h"
#include "clang/Basic/Builtins.h"
#include "clang/Basic/Module.h"
#include "clang/Basic/SourceManager.h"
#include "llvm/Support/raw_ostream.h"
#include <experimental/filesystem>
#include <string.h>

using namespace clang;
using namespace clang::comments;

//===----------------------------------------------------------------------===//
// ASTDumper Visitor
//===----------------------------------------------------------------------===//

namespace {

	std::string provide_escapes(std::string s)
	{
		std::string new_s = "";
		for (auto i = s.begin(); i != s.end(); ++i)
		{
			if (*i == '"' || *i == '\\')
			{
				new_s.push_back('\\');
			}
			new_s.push_back(*i);
		}
		return new_s;
	}


	class MyASTDumper
		: public ConstDeclVisitor<MyASTDumper>, public ConstStmtVisitor<MyASTDumper>,
		public ConstCommentVisitor<MyASTDumper>, public TypeVisitor<MyASTDumper> {
		raw_ostream *OS;
		const CommandTraits *Traits;
		const SourceManager *SM;

		/// The policy to use for printing; can be defaulted.
		PrintingPolicy PrintPolicy;

		/// Pending[i] is an action to dump an entity at level i.
		llvm::SmallVector<std::function<void(bool isLastChild)>, 32> Pending;

		/// Indicates whether we should trigger deserialization of nodes that had
		/// not already been loaded.
		bool Deserialize = false;

		/// Indicates whether we're at the top level.
		bool TopLevel = true;

		/// Indicates if we're handling the first child after entering a new depth.
		bool FirstChild = true;

		/// Prefix for currently-being-dumped entity.
		std::string Prefix;
		int changed = 0;

		/// Keep track of the last location we print out so that we can
		/// print out deltas from then on out.
		const char *LastLocFilename = "";
		unsigned LastLocLine = ~0U;

		/// The \c FullComment parent of the comment being dumped.
		const FullComment *FC = nullptr;

		/// Dump a child of the current node.
		template<typename Fn> void dumpChild(Fn doDumpChild) {
			// If we're at the top level, there's nothing interesting to do; just
			// run the dumper.
			if (TopLevel) {
				TopLevel = false;
				doDumpChild();
				while (!Pending.empty()) {
					Pending.back()(true);
					Pending.pop_back();
				}
				Prefix.clear();
				*OS << "\n";
				TopLevel = true;
				return;
			}

			const FullComment *OrigFC = FC;
			auto dumpWithIndent = [this, doDumpChild, OrigFC](bool isLastChild) {
				{
					*OS << '\n';
					// Add in closing parentheses.
					if (this->changed > 0)
					{
						*OS << Prefix << "  ";
						for (int i = 0; i < this->changed-1; ++i)
							*OS << ") ";
						*OS << ")";
						this->changed = 0;
						*OS << '\n';
					}
					*OS << Prefix << "  ";
					*OS << "( ";
					this->Prefix.push_back(' ');
					this->Prefix.push_back(' ');
				}

				FirstChild = true;
				unsigned Depth = Pending.size();

				FC = OrigFC;
				doDumpChild();

				// If any children are left, they're the last at their nesting level.
				// Dump those ones out now.
				while (Depth < Pending.size()) {
					Pending.back()(true);
					this->Pending.pop_back();
				}

				// Restore the old prefix.
				this->Prefix.resize(Prefix.size() - 2);
				this->changed += 1;
			};

			if (FirstChild) {
				Pending.push_back(std::move(dumpWithIndent));
			}
			else {
				Pending.back()(false);
				Pending.back() = std::move(dumpWithIndent);
			}
			FirstChild = false;
		}


	public:
		MyASTDumper(raw_ostream *OS, const CommandTraits *Traits,
			const SourceManager *SM)
			: MyASTDumper(OS, Traits, SM, LangOptions()) {}

		MyASTDumper(raw_ostream *OS, const CommandTraits *Traits,
			const SourceManager *SM,
			const PrintingPolicy &PrintPolicy)
			: OS(OS), Traits(Traits), SM(SM), PrintPolicy(PrintPolicy)
			 {}

		void setDeserialize(bool D) { Deserialize = D; }

		void start();
		void complete();
		void dumpDecl(const Decl *D);
		void dumpStmt(const Stmt *S);
		void dumpFullComment(const FullComment *C);

		// Utilities
		void dumpPointer(const void *Ptr);
		void dumpSourceRange(SourceRange R);
		void dumpLocation(SourceLocation Loc);
		void dumpBareType(QualType T, bool Desugar = true);
		void dumpType(QualType T);
		void dumpTypeAsChild(QualType T);
		void dumpTypeAsChild(const Type *T);
		void dumpBareDeclRef(const Decl *Node);
		void dumpDeclRef(const Decl *Node, const char *Label = nullptr);
		void dumpName(const NamedDecl *D);
		bool hasNodes(const DeclContext *DC);
		void dumpDeclContext(const DeclContext *DC);
		void dumpLookups(const DeclContext *DC, bool DumpDecls);
		void dumpAttr(const Attr *A);

		// C++ Utilities
		void dumpAccessSpecifier(AccessSpecifier AS);
		void dumpCXXCtorInitializer(const CXXCtorInitializer *Init);
		void dumpTemplateParameters(const TemplateParameterList *TPL);
		void dumpTemplateArgumentListInfo(const TemplateArgumentListInfo &TALI);
		void dumpTemplateArgumentLoc(const TemplateArgumentLoc &A);
		void dumpTemplateArgumentList(const TemplateArgumentList &TAL);
		void dumpTemplateArgument(const TemplateArgument &A,
			SourceRange R = SourceRange());

		// Objective-C utilities.
		void dumpObjCTypeParamList(const ObjCTypeParamList *typeParams);

		// Types
		void VisitComplexType(const ComplexType *T) {
			dumpTypeAsChild(T->getElementType());
		}
		void VisitPointerType(const PointerType *T) {
			dumpTypeAsChild(T->getPointeeType());
		}
		void VisitBlockPointerType(const BlockPointerType *T) {
			dumpTypeAsChild(T->getPointeeType());
		}
		void VisitReferenceType(const ReferenceType *T) {
			dumpTypeAsChild(T->getPointeeType());
		}
		void VisitRValueReferenceType(const ReferenceType *T) {
			if (T->isSpelledAsLValue())
				*OS << " written as lvalue reference";
			VisitReferenceType(T);
		}
		void VisitMemberPointerType(const MemberPointerType *T) {
			dumpTypeAsChild(T->getClass());
			dumpTypeAsChild(T->getPointeeType());
		}
		void VisitArrayType(const ArrayType *T) {
			switch (T->getSizeModifier()) {
			case ArrayType::Normal: break;
			case ArrayType::Static: *OS << " static"; break;
			case ArrayType::Star: *OS << " *"; break;
			}
			*OS << " " << T->getIndexTypeQualifiers().getAsString();
			dumpTypeAsChild(T->getElementType());
		}
		void VisitConstantArrayType(const ConstantArrayType *T) {
			*OS << " " << T->getSize();
			VisitArrayType(T);
		}
		void VisitVariableArrayType(const VariableArrayType *T) {
			*OS << " ";
			dumpSourceRange(T->getBracketsRange());
			VisitArrayType(T);
			dumpStmt(T->getSizeExpr());
		}
		void VisitDependentSizedArrayType(const DependentSizedArrayType *T) {
			VisitArrayType(T);
			*OS << " ";
			dumpSourceRange(T->getBracketsRange());
			dumpStmt(T->getSizeExpr());
		}
		void VisitDependentSizedExtVectorType(
			const DependentSizedExtVectorType *T) {
			*OS << " ";
			*OS << " SrcLoc=\"";
			dumpLocation(T->getAttributeLoc());
			*OS << "\"";
			dumpTypeAsChild(T->getElementType());
			dumpStmt(T->getSizeExpr());
		}
		void VisitVectorType(const VectorType *T) {
			switch (T->getVectorKind()) {
			case VectorType::GenericVector: break;
			case VectorType::AltiVecVector: *OS << " altivec"; break;
			case VectorType::AltiVecPixel: *OS << " altivec pixel"; break;
			case VectorType::AltiVecBool: *OS << " altivec bool"; break;
			case VectorType::NeonVector: *OS << " neon"; break;
			case VectorType::NeonPolyVector: *OS << " neon poly"; break;
			}
			*OS << " " << T->getNumElements();
			dumpTypeAsChild(T->getElementType());
		}
		void VisitFunctionType(const FunctionType *T) {
			auto EI = T->getExtInfo();
			if (EI.getNoReturn()) *OS << " NoReturn=\"noreturn\"";
			if (EI.getProducesResult()) *OS << " ProducesResult=\"produces_result\"";
			if (EI.getHasRegParm()) *OS << " Regparm\"" << EI.getRegParm() << "\"";
			*OS << " CallConv=\"" << FunctionType::getNameForCallConv(EI.getCC()) << "\"";
			dumpTypeAsChild(T->getReturnType());
		}
		void VisitFunctionProtoType(const FunctionProtoType *T) {
			auto EPI = T->getExtProtoInfo();
			if (EPI.HasTrailingReturn) *OS << " trailing_return";
			if (T->isConst()) *OS << " const";
			if (T->isVolatile()) *OS << " volatile";
			if (T->isRestrict()) *OS << " restrict";
			switch (EPI.RefQualifier) {
			case RQ_None: break;
			case RQ_LValue: *OS << " &"; break;
			case RQ_RValue: *OS << " &&"; break;
			}
			// FIXME: Exception specification.
			// FIXME: Consumed parameters.
			VisitFunctionType(T);
			for (QualType PT : T->getParamTypes())
				dumpTypeAsChild(PT);
			if (EPI.Variadic)
				dumpChild([=] { *OS << "..."; });
		}
		void VisitUnresolvedUsingType(const UnresolvedUsingType *T) {
			dumpDeclRef(T->getDecl());
		}
		void VisitTypedefType(const TypedefType *T) {
			dumpDeclRef(T->getDecl());
		}
		void VisitTypeOfExprType(const TypeOfExprType *T) {
			dumpStmt(T->getUnderlyingExpr());
		}
		void VisitDecltypeType(const DecltypeType *T) {
			dumpStmt(T->getUnderlyingExpr());
		}
		void VisitUnaryTransformType(const UnaryTransformType *T) {
			switch (T->getUTTKind()) {
			case UnaryTransformType::EnumUnderlyingType:
				*OS << " underlying_type";
				break;
			}
			dumpTypeAsChild(T->getBaseType());
		}
		void VisitTagType(const TagType *T) {
			dumpDeclRef(T->getDecl());
		}
		void VisitAttributedType(const AttributedType *T) {
			// FIXME: AttrKind
			dumpTypeAsChild(T->getModifiedType());
		}
		void VisitTemplateTypeParmType(const TemplateTypeParmType *T) {
			*OS << " depth " << T->getDepth() << " index " << T->getIndex();
			if (T->isParameterPack()) *OS << " pack";
			dumpDeclRef(T->getDecl());
		}
		void VisitSubstTemplateTypeParmType(const SubstTemplateTypeParmType *T) {
			dumpTypeAsChild(T->getReplacedParameter());
		}
		void VisitSubstTemplateTypeParmPackType(
			const SubstTemplateTypeParmPackType *T) {
			dumpTypeAsChild(T->getReplacedParameter());
			dumpTemplateArgument(T->getArgumentPack());
		}
		void VisitAutoType(const AutoType *T) {
			if (T->isDecltypeAuto()) *OS << " decltype(auto)";
			if (!T->isDeduced())
				*OS << " undeduced";
		}
		void VisitTemplateSpecializationType(const TemplateSpecializationType *T) {
			if (T->isTypeAlias()) *OS << " alias";
			*OS << " "; T->getTemplateName().dump(*OS);
			for (auto &Arg : *T)
				dumpTemplateArgument(Arg);
			if (T->isTypeAlias())
				dumpTypeAsChild(T->getAliasedType());
		}
		void VisitInjectedClassNameType(const InjectedClassNameType *T) {
			dumpDeclRef(T->getDecl());
		}
		void VisitObjCInterfaceType(const ObjCInterfaceType *T) {
			dumpDeclRef(T->getDecl());
		}
		void VisitObjCObjectPointerType(const ObjCObjectPointerType *T) {
			dumpTypeAsChild(T->getPointeeType());
		}
		void VisitAtomicType(const AtomicType *T) {
			dumpTypeAsChild(T->getValueType());
		}
		void VisitPipeType(const PipeType *T) {
			dumpTypeAsChild(T->getElementType());
		}
		void VisitAdjustedType(const AdjustedType *T) {
			dumpTypeAsChild(T->getOriginalType());
		}
		void VisitPackExpansionType(const PackExpansionType *T) {
			if (auto N = T->getNumExpansions()) *OS << " expansions " << *N;
			if (!T->isSugared())
				dumpTypeAsChild(T->getPattern());
		}
		// FIXME: ElaboratedType, DependentNameType,
		// DependentTemplateSpecializationType, ObjCObjectType

		// Decls
		void VisitLabelDecl(const LabelDecl *D);
		void VisitTypedefDecl(const TypedefDecl *D);
		void VisitEnumDecl(const EnumDecl *D);
		void VisitRecordDecl(const RecordDecl *D);
		void VisitEnumConstantDecl(const EnumConstantDecl *D);
		void VisitIndirectFieldDecl(const IndirectFieldDecl *D);
		void VisitFunctionDecl(const FunctionDecl *D);
		void VisitFieldDecl(const FieldDecl *D);
		void VisitVarDecl(const VarDecl *D);
		void VisitDecompositionDecl(const DecompositionDecl *D);
		void VisitBindingDecl(const BindingDecl *D);
		void VisitFileScopeAsmDecl(const FileScopeAsmDecl *D);
		void VisitImportDecl(const ImportDecl *D);
		void VisitPragmaCommentDecl(const PragmaCommentDecl *D);
		void VisitPragmaDetectMismatchDecl(const PragmaDetectMismatchDecl *D);
		void VisitCapturedDecl(const CapturedDecl *D);

		// OpenMP decls
		void VisitOMPThreadPrivateDecl(const OMPThreadPrivateDecl *D);
		void VisitOMPDeclareReductionDecl(const OMPDeclareReductionDecl *D);
		void VisitOMPCapturedExprDecl(const OMPCapturedExprDecl *D);

		// C++ Decls
		void VisitNamespaceDecl(const NamespaceDecl *D);
		void VisitUsingDirectiveDecl(const UsingDirectiveDecl *D);
		void VisitNamespaceAliasDecl(const NamespaceAliasDecl *D);
		void VisitTypeAliasDecl(const TypeAliasDecl *D);
		void VisitTypeAliasTemplateDecl(const TypeAliasTemplateDecl *D);
		void VisitCXXRecordDecl(const CXXRecordDecl *D);
		void VisitStaticAssertDecl(const StaticAssertDecl *D);
		template<typename SpecializationDecl>
		void VisitTemplateDeclSpecialization(const SpecializationDecl *D,
			bool DumpExplicitInst,
			bool DumpRefOnly);
		template<typename TemplateDecl>
		void VisitTemplateDecl(const TemplateDecl *D, bool DumpExplicitInst);
		void VisitFunctionTemplateDecl(const FunctionTemplateDecl *D);
		void VisitClassTemplateDecl(const ClassTemplateDecl *D);
		void VisitClassTemplateSpecializationDecl(
			const ClassTemplateSpecializationDecl *D);
		void VisitClassTemplatePartialSpecializationDecl(
			const ClassTemplatePartialSpecializationDecl *D);
		void VisitClassScopeFunctionSpecializationDecl(
			const ClassScopeFunctionSpecializationDecl *D);
		void VisitBuiltinTemplateDecl(const BuiltinTemplateDecl *D);
		void VisitVarTemplateDecl(const VarTemplateDecl *D);
		void VisitVarTemplateSpecializationDecl(
			const VarTemplateSpecializationDecl *D);
		void VisitVarTemplatePartialSpecializationDecl(
			const VarTemplatePartialSpecializationDecl *D);
		void VisitTemplateTypeParmDecl(const TemplateTypeParmDecl *D);
		void VisitNonTypeTemplateParmDecl(const NonTypeTemplateParmDecl *D);
		void VisitTemplateTemplateParmDecl(const TemplateTemplateParmDecl *D);
		void VisitUsingDecl(const UsingDecl *D);
		void VisitUnresolvedUsingTypenameDecl(const UnresolvedUsingTypenameDecl *D);
		void VisitUnresolvedUsingValueDecl(const UnresolvedUsingValueDecl *D);
		void VisitUsingShadowDecl(const UsingShadowDecl *D);
		void VisitConstructorUsingShadowDecl(const ConstructorUsingShadowDecl *D);
		void VisitLinkageSpecDecl(const LinkageSpecDecl *D);
		void VisitAccessSpecDecl(const AccessSpecDecl *D);
		void VisitFriendDecl(const FriendDecl *D);

		// ObjC Decls
		void VisitObjCIvarDecl(const ObjCIvarDecl *D);
		void VisitObjCMethodDecl(const ObjCMethodDecl *D);
		void VisitObjCTypeParamDecl(const ObjCTypeParamDecl *D);
		void VisitObjCCategoryDecl(const ObjCCategoryDecl *D);
		void VisitObjCCategoryImplDecl(const ObjCCategoryImplDecl *D);
		void VisitObjCProtocolDecl(const ObjCProtocolDecl *D);
		void VisitObjCInterfaceDecl(const ObjCInterfaceDecl *D);
		void VisitObjCImplementationDecl(const ObjCImplementationDecl *D);
		void VisitObjCCompatibleAliasDecl(const ObjCCompatibleAliasDecl *D);
		void VisitObjCPropertyDecl(const ObjCPropertyDecl *D);
		void VisitObjCPropertyImplDecl(const ObjCPropertyImplDecl *D);
		void VisitBlockDecl(const BlockDecl *D);

		// Stmts.
		void VisitStmt(const Stmt *Node);
		void VisitDeclStmt(const DeclStmt *Node);
		void VisitAttributedStmt(const AttributedStmt *Node);
		void VisitLabelStmt(const LabelStmt *Node);
		void VisitGotoStmt(const GotoStmt *Node);
		void VisitCXXCatchStmt(const CXXCatchStmt *Node);
		void VisitCapturedStmt(const CapturedStmt *Node);

		// OpenMP
		void VisitOMPExecutableDirective(const OMPExecutableDirective *Node);

		// Exprs
		void VisitExpr(const Expr *Node);
		void VisitCastExpr(const CastExpr *Node);
		void VisitImplicitCastExpr(const ImplicitCastExpr *Node);
		void VisitDeclRefExpr(const DeclRefExpr *Node);
		void VisitPredefinedExpr(const PredefinedExpr *Node);
		void VisitCharacterLiteral(const CharacterLiteral *Node);
		void VisitIntegerLiteral(const IntegerLiteral *Node);
		void VisitFixedPointLiteral(const FixedPointLiteral *Node);
		void VisitFloatingLiteral(const FloatingLiteral *Node);
		void VisitStringLiteral(const StringLiteral *Str);
		void VisitInitListExpr(const InitListExpr *ILE);
		void VisitArrayInitLoopExpr(const ArrayInitLoopExpr *ILE);
		void VisitArrayInitIndexExpr(const ArrayInitIndexExpr *ILE);
		void VisitUnaryOperator(const UnaryOperator *Node);
		void VisitUnaryExprOrTypeTraitExpr(const UnaryExprOrTypeTraitExpr *Node);
		void VisitMemberExpr(const MemberExpr *Node);
		void VisitExtVectorElementExpr(const ExtVectorElementExpr *Node);
		void VisitBinaryOperator(const BinaryOperator *Node);
		void VisitCompoundAssignOperator(const CompoundAssignOperator *Node);
		void VisitAddrLabelExpr(const AddrLabelExpr *Node);
		void VisitBlockExpr(const BlockExpr *Node);
		void VisitOpaqueValueExpr(const OpaqueValueExpr *Node);
		void VisitGenericSelectionExpr(const GenericSelectionExpr *E);

		// C++
		void VisitCXXNamedCastExpr(const CXXNamedCastExpr *Node);
		void VisitCXXBoolLiteralExpr(const CXXBoolLiteralExpr *Node);
		void VisitCXXThisExpr(const CXXThisExpr *Node);
		void VisitCXXFunctionalCastExpr(const CXXFunctionalCastExpr *Node);
		void VisitCXXUnresolvedConstructExpr(const CXXUnresolvedConstructExpr *Node);
		void VisitCXXConstructExpr(const CXXConstructExpr *Node);
		void VisitCXXBindTemporaryExpr(const CXXBindTemporaryExpr *Node);
		void VisitCXXNewExpr(const CXXNewExpr *Node);
		void VisitCXXDeleteExpr(const CXXDeleteExpr *Node);
		void VisitMaterializeTemporaryExpr(const MaterializeTemporaryExpr *Node);
		void VisitExprWithCleanups(const ExprWithCleanups *Node);
		void VisitUnresolvedLookupExpr(const UnresolvedLookupExpr *Node);
		void dumpCXXTemporary(const CXXTemporary *Temporary);
		void VisitLambdaExpr(const LambdaExpr *Node) {
			VisitExpr(Node);
			dumpDecl(Node->getLambdaClass());
		}
		void VisitSizeOfPackExpr(const SizeOfPackExpr *Node);
		void
			VisitCXXDependentScopeMemberExpr(const CXXDependentScopeMemberExpr *Node);

		// ObjC
		void VisitObjCAtCatchStmt(const ObjCAtCatchStmt *Node);
		void VisitObjCEncodeExpr(const ObjCEncodeExpr *Node);
		void VisitObjCMessageExpr(const ObjCMessageExpr *Node);
		void VisitObjCBoxedExpr(const ObjCBoxedExpr *Node);
		void VisitObjCSelectorExpr(const ObjCSelectorExpr *Node);
		void VisitObjCProtocolExpr(const ObjCProtocolExpr *Node);
		void VisitObjCPropertyRefExpr(const ObjCPropertyRefExpr *Node);
		void VisitObjCSubscriptRefExpr(const ObjCSubscriptRefExpr *Node);
		void VisitObjCIvarRefExpr(const ObjCIvarRefExpr *Node);
		void VisitObjCBoolLiteralExpr(const ObjCBoolLiteralExpr *Node);

		// Comments.
		const char *getCommandName(unsigned CommandID);
		void dumpComment(const Comment *C);

		// Inline comments.
		void visitTextComment(const TextComment *C);
		void visitInlineCommandComment(const InlineCommandComment *C);
		void visitHTMLStartTagComment(const HTMLStartTagComment *C);
		void visitHTMLEndTagComment(const HTMLEndTagComment *C);

		// Block comments.
		void visitBlockCommandComment(const BlockCommandComment *C);
		void visitParamCommandComment(const ParamCommandComment *C);
		void visitTParamCommandComment(const TParamCommandComment *C);
		void visitVerbatimBlockComment(const VerbatimBlockComment *C);
		void visitVerbatimBlockLineComment(const VerbatimBlockLineComment *C);
		void visitVerbatimLineComment(const VerbatimLineComment *C);
	};
}

//===----------------------------------------------------------------------===//
//  Utilities
//===----------------------------------------------------------------------===//

void MyASTDumper::dumpPointer(const void *Ptr) {
	*OS << ' ' << "Pointer=\"" << Ptr << "\"";
}

void MyASTDumper::dumpLocation(SourceLocation Loc) {
	if (!SM)
		return;

	SourceLocation SpellingLoc = SM->getSpellingLoc(Loc);

	// The general format we print out is filename:line:col, but we drop pieces
	// that haven't changed since the last loc printed.
	PresumedLoc PLoc = SM->getPresumedLoc(SpellingLoc);

	if (PLoc.isInvalid()) {
		*OS << "<invalid sloc>";
		return;
	}

	// Get normalized file name with path.
	std::string fn = PLoc.getFilename();
	std::experimental::filesystem::path p(fn);
	if (strcmp(p.string().c_str(), LastLocFilename) != 0) {
		*OS << p.string().c_str() << ':' << PLoc.getLine()
			<< ':' << PLoc.getColumn();
		LastLocFilename = _strdup(p.string().c_str());
		LastLocLine = PLoc.getLine();
	}
	else if (PLoc.getLine() != LastLocLine) {
		*OS << "line" << ':' << PLoc.getLine()
			<< ':' << PLoc.getColumn();
		LastLocLine = PLoc.getLine();
	}
	else {
		*OS << "col" << ':' << PLoc.getColumn();
	}
}

void MyASTDumper::dumpSourceRange(SourceRange R) {
	// Can't translate locations if a SourceManager isn't available.
	if (!SM)
		return;

	*OS << " SrcRange=\"";
	dumpLocation(R.getBegin());
	if (R.getBegin() != R.getEnd()) {
		*OS << ", ";
		dumpLocation(R.getEnd());
	}
	*OS << "\"";

	// <t2.c:123:421[blah], t2.c:412:321>

}

void MyASTDumper::dumpBareType(QualType T, bool Desugar) {

	SplitQualType T_split = T.split();
	*OS << QualType::getAsString(T_split, PrintPolicy);

	if (Desugar && !T.isNull()) {
		// If the type is sugared, also dump a (shallow) desugared type.
		SplitQualType D_split = T.getSplitDesugaredType();
		if (T_split != D_split)
			*OS << ":" << QualType::getAsString(D_split, PrintPolicy);
	}
}

void MyASTDumper::dumpType(QualType T) {
	*OS << ' ';
	*OS << "Type=\"";
	dumpBareType(T);
	*OS << "\"";
}

void MyASTDumper::dumpTypeAsChild(QualType T) {
	SplitQualType SQT = T.split();
	if (!SQT.Quals.hasQualifiers())
		return dumpTypeAsChild(SQT.Ty);

	dumpChild([=] {
		*OS << "QualType";
		dumpPointer(T.getAsOpaquePtr());
		*OS << " ";
		*OS << "BareType=\"";
		dumpBareType(T, false);
		*OS << "\"";
		*OS << " " << T.split().Quals.getAsString();
		dumpTypeAsChild(T.split().Ty);
	});
}

void MyASTDumper::dumpTypeAsChild(const Type *T) {
	dumpChild([=] {
		if (!T) {
			*OS << "<<<NULL>>>";
			return;
		}
		if (const LocInfoType *LIT = llvm::dyn_cast<LocInfoType>(T)) {
			{
				*OS << "LocInfo Type";
			}
			dumpPointer(T);
			dumpTypeAsChild(LIT->getTypeSourceInfo()->getType());
			return;
		}

		{
			*OS << T->getTypeClassName() << "Type";
		}
		dumpPointer(T);
		*OS << " ";
		*OS << "BareType=\"";
		dumpBareType(QualType(T, 0), false);
		*OS << "\"";

		QualType SingleStepDesugar =
			T->getLocallyUnqualifiedSingleStepDesugaredType();
		*OS << " Sugar=\"";
		if (SingleStepDesugar != QualType(T, 0))
			*OS << " sugar";
		if (T->isDependentType())
			*OS << " dependent";
		else if (T->isInstantiationDependentType())
			*OS << " instantiation_dependent";
		if (T->isVariablyModifiedType())
			*OS << " variably_modified";
		if (T->containsUnexpandedParameterPack())
			*OS << " contains_unexpanded_pack";
		if (T->isFromAST())
			*OS << " imported";
		*OS << "\"";

		TypeVisitor<MyASTDumper>::Visit(T);

		if (SingleStepDesugar != QualType(T, 0))
			dumpTypeAsChild(SingleStepDesugar);
	});
}

void MyASTDumper::dumpBareDeclRef(const Decl *D) {
	if (!D) {
		*OS << "<<<NULL>>>";
		return;
	}

	{
		*OS << D->getDeclKindName();
	}
	dumpPointer(D);

	if (const NamedDecl *ND = dyn_cast<NamedDecl>(D)) {
		DeclarationName s = ND->getDeclName();
		std::string t = s.getAsString();
		if (strcmp("__crt_locale_pointers", t.c_str())==0)
		{
			int x = 111;
		}
		*OS << "Name=\"" << ND->getDeclName() << '\"';
	}

	if (const ValueDecl *VD = dyn_cast<ValueDecl>(D))
		dumpType(VD->getType());
}

void MyASTDumper::dumpDeclRef(const Decl *D, const char *Label) {
	if (!D)
		return;

	dumpChild([=] {
		if (Label)
			*OS << Label << ' ';
		dumpBareDeclRef(D);
	});
}

void MyASTDumper::dumpName(const NamedDecl *ND) {
	if (ND->getDeclName()) {
		std::string s = ND->getNameAsString();
		if (strcmp("__crt_locale_pointers", s.c_str()) == 0)
		{
			int x = 111;
		}
		*OS << ' ' << "Name=\"" << ND->getNameAsString() << "\"";
	}
}

bool MyASTDumper::hasNodes(const DeclContext *DC) {
	if (!DC)
		return false;

	return DC->hasExternalLexicalStorage() ||
		(Deserialize ? DC->decls_begin() != DC->decls_end()
			: DC->noload_decls_begin() != DC->noload_decls_end());
}

void MyASTDumper::dumpDeclContext(const DeclContext *DC) {
	if (!DC)
		return;

	for (auto *D : (Deserialize ? DC->decls() : DC->noload_decls()))
		dumpDecl(D);

	if (DC->hasExternalLexicalStorage()) {
		dumpChild([=] {
			*OS << "<undeserialized declarations>";
		});
	}
}

void MyASTDumper::dumpLookups(const DeclContext *DC, bool DumpDecls) {
	dumpChild([=] {
		*OS << "StoredDeclsMap ";
		dumpBareDeclRef(cast<Decl>(DC));

		const DeclContext *Primary = DC->getPrimaryContext();
		if (Primary != DC) {
			*OS << " primary";
			dumpPointer(cast<Decl>(Primary));
		}

		bool HasUndeserializedLookups = Primary->hasExternalVisibleStorage();

		auto Range = Deserialize
			? Primary->lookups()
			: Primary->noload_lookups(/*PreserveInternalState=*/true);
		for (auto I = Range.begin(), E = Range.end(); I != E; ++I) {
			DeclarationName Name = I.getLookupName();
			DeclContextLookupResult R = *I;

			dumpChild([=] {
				*OS << "DeclarationName ";
				{
					*OS << '\'' << Name << '\'';
				}

				for (DeclContextLookupResult::iterator RI = R.begin(), RE = R.end();
					RI != RE; ++RI) {
					dumpChild([=] {
						dumpBareDeclRef(*RI);

						if ((*RI)->isHidden())
							*OS << " hidden";

						// If requested, dump the redecl chain for this lookup.
						if (DumpDecls) {
							// Dump earliest decl first.
							std::function<void(Decl *)> DumpWithPrev = [&](Decl *D) {
								if (Decl *Prev = D->getPreviousDecl())
									DumpWithPrev(Prev);
								dumpDecl(D);
							};
							DumpWithPrev(*RI);
						}
					});
				}
			});
		}

		if (HasUndeserializedLookups) {
			dumpChild([=] {
				*OS << "<undeserialized lookups>";
			});
		}
	});
}


void MyASTDumper::dumpAttr(const Attr *A) {
	dumpChild([=] {
		{

			switch (A->getKind()) {
#define ATTR(X) case attr::X: *OS << #X; break;
#include "clang/Basic/AttrList.inc"
			}
			*OS << "Attr";
		}
		dumpPointer(A);
		dumpSourceRange(A->getRange());
		int attrs = 0;
		if (A->isInherited())
			*OS << (attrs++ == 0 ? " Attrs=\"" : ",") << "Inherited";
		if (A->isImplicit())
			*OS << (attrs++ == 0 ? " Attrs=\"" : ",") << "Implicit";
		if (attrs)
			*OS << "\"";
		*OS << " Value=\"";
		{
#include "fix_AttrDump.inc"
			fucking_bullshit.flush();
			*OS << provide_escapes(local_bullshit_string_for_redirection);
		}
		*OS << "\"";

	});
}

static void dumpPreviousDeclImpl(raw_ostream &OS, ...) {}

template<typename T>
static void dumpPreviousDeclImpl(raw_ostream *OS, const Mergeable<T> *D) {
	const T *First = D->getFirstDecl();
	if (First != D)
	{
		*OS << "First=\"" << First << "\"";
	}
}

template<typename T>
static void dumpPreviousDeclImpl(raw_ostream *OS, const Redeclarable<T> *D) {
	const T *Prev = D->getPreviousDecl();
	if (Prev)
	{
		*OS << " Prev=\"" << Prev << "\"";
	}
}

/// Dump the previous declaration in the redeclaration chain for a declaration,
/// if any.
static void dumpPreviousDecl(raw_ostream &OS, const Decl *D) {
	
	switch (D->getKind()) {
#define DECL(DERIVED, BASE) \
  case Decl::DERIVED: \
    return dumpPreviousDeclImpl(OS, cast<DERIVED##Decl>(D));
#define ABSTRACT_DECL(DECL)
#include "clang/AST/DeclNodes.inc"
	}
	llvm_unreachable("Decl that isn't part of DeclNodes.inc!");
}

//===----------------------------------------------------------------------===//
//  C++ Utilities
//===----------------------------------------------------------------------===//

void MyASTDumper::dumpAccessSpecifier(AccessSpecifier AS) {
	switch (AS) {
	case AS_none:
		break;
	case AS_public:
		*OS << "public";
		break;
	case AS_protected:
		*OS << "protected";
		break;
	case AS_private:
		*OS << "private";
		break;
	}
}

void MyASTDumper::dumpCXXCtorInitializer(const CXXCtorInitializer *Init) {
	dumpChild([=] {
		*OS << "CXXCtorInitializer";
		if (Init->isAnyMemberInitializer()) {
			*OS << ' ';
			dumpBareDeclRef(Init->getAnyMember());
		}
		else if (Init->isBaseInitializer()) {
			dumpType(QualType(Init->getBaseClass(), 0));
		}
		else if (Init->isDelegatingInitializer()) {
			dumpType(Init->getTypeSourceInfo()->getType());
		}
		else {
			llvm_unreachable("Unknown initializer type");
		}
		dumpStmt(Init->getInit());
	});
}

void MyASTDumper::dumpTemplateParameters(const TemplateParameterList *TPL) {
	if (!TPL)
		return;

	for (TemplateParameterList::const_iterator I = TPL->begin(), E = TPL->end();
		I != E; ++I)
		dumpDecl(*I);
}

void MyASTDumper::dumpTemplateArgumentListInfo(
	const TemplateArgumentListInfo &TALI) {
	for (unsigned i = 0, e = TALI.size(); i < e; ++i)
		dumpTemplateArgumentLoc(TALI[i]);
}

void MyASTDumper::dumpTemplateArgumentLoc(const TemplateArgumentLoc &A) {
	dumpTemplateArgument(A.getArgument(), A.getSourceRange());
}

void MyASTDumper::dumpTemplateArgumentList(const TemplateArgumentList &TAL) {
	for (unsigned i = 0, e = TAL.size(); i < e; ++i)
		dumpTemplateArgument(TAL[i]);
}

void MyASTDumper::dumpTemplateArgument(const TemplateArgument &A, SourceRange R) {
	dumpChild([=] {
		*OS << "TemplateArgument";
		if (R.isValid())
			dumpSourceRange(R);

		switch (A.getKind()) {
		case TemplateArgument::Null:
			*OS << " null";
			break;
		case TemplateArgument::Type:
			*OS << " type";
			dumpType(A.getAsType());
			break;
		case TemplateArgument::Declaration:
			*OS << " decl";
			dumpDeclRef(A.getAsDecl());
			break;
		case TemplateArgument::NullPtr:
			*OS << " nullptr";
			break;
		case TemplateArgument::Integral:
			*OS << " integral " << A.getAsIntegral();
			break;
		case TemplateArgument::Template:
			*OS << " template ";
			A.getAsTemplate().dump(*OS);
			break;
		case TemplateArgument::TemplateExpansion:
			*OS << " template expansion";
			A.getAsTemplateOrTemplatePattern().dump(*OS);
			break;
		case TemplateArgument::Expression:
			*OS << " expr";
			dumpStmt(A.getAsExpr());
			break;
		case TemplateArgument::Pack:
			*OS << " pack";
			for (TemplateArgument::pack_iterator I = A.pack_begin(), E = A.pack_end();
				I != E; ++I)
				dumpTemplateArgument(*I);
			break;
		}
	});
}

//===----------------------------------------------------------------------===//
//  Objective-C Utilities
//===----------------------------------------------------------------------===//
void MyASTDumper::dumpObjCTypeParamList(const ObjCTypeParamList *typeParams) {
	if (!typeParams)
		return;

	for (auto typeParam : *typeParams) {
		dumpDecl(typeParam);
	}
}

//===----------------------------------------------------------------------===//
//  Decl dumping methods.
//===----------------------------------------------------------------------===//
void MyASTDumper::start()
{
	*OS << "( ";
}

void MyASTDumper::complete()
{
	*OS << '\n';
	// Add in closing parentheses.
	if (this->changed > 0)
	{
		for (int i = 0; i < this->changed; ++i)
			*OS << ") ";
		*OS << ")";
		this->changed = 0;
		*OS << '\n';
	}
}

void MyASTDumper::dumpDecl(const Decl *D) {
	dumpChild([=] {
		if (!D) {
			*OS << "<<<NULL>>>";
			return;
		}

		{
			*OS << D->getDeclKindName() << "Decl";
		}
		dumpPointer(D);
		if (D->getLexicalDeclContext() != D->getDeclContext())
			*OS << "Parent=\"" << cast<Decl>(D->getDeclContext()) << "\"";
		dumpPreviousDecl(*OS, D);
		dumpSourceRange(D->getSourceRange());
		*OS << ' ';
		*OS << " SrcLoc=\"";
		dumpLocation(D->getLocation());
		*OS << "\"";
		if (D->isFromASTFile())
			*OS << " imported";
		if (Module *M = D->getOwningModule())
			*OS << " in " << M->getFullModuleName();
		if (auto *ND = dyn_cast<NamedDecl>(D))
			for (Module *M : D->getASTContext().getModulesWithMergedDefinition(
				const_cast<NamedDecl *>(ND)))
				dumpChild([=] { *OS << "also in " << M->getFullModuleName(); });

		int attrs = 0;
		if (const NamedDecl *ND = dyn_cast<NamedDecl>(D))
			if (ND->isHidden())
				*OS << (attrs++ ? "," : " Attrs=\"") << "hidden";
		if (D->isImplicit())
			*OS << (attrs++ ? "," : " Attrs=\"") << "implicit";
		if (D->isUsed())
			*OS << (attrs++ ? "," : " Attrs=\"") << "used";
		else if (D->isThisDeclarationReferenced())
			*OS << (attrs++ ? "," : " Attrs=\"") << "referenced";
		if (D->isInvalidDecl())
			*OS << (attrs++ ? "," : " Attrs=\"") << "invalid";
		if (const FunctionDecl *FD = dyn_cast<FunctionDecl>(D))
			if (FD->isConstexpr())
				*OS << (attrs++ ? "," : " Attrs=\"") << "constexpr";
		if (attrs)
			*OS << "\"";

		ConstDeclVisitor<MyASTDumper>::Visit(D);

		for (Decl::attr_iterator I = D->attr_begin(), E = D->attr_end(); I != E;
			++I)
			dumpAttr(*I);

		if (const FullComment *Comment =
			D->getASTContext().getLocalCommentForDeclUncached(D))
			dumpFullComment(Comment);

		// Decls within functions are visited by the body.
		if (!isa<FunctionDecl>(*D) && !isa<ObjCMethodDecl>(*D) &&
			hasNodes(dyn_cast<DeclContext>(D)))
			dumpDeclContext(cast<DeclContext>(D));
	});
}

void MyASTDumper::VisitLabelDecl(const LabelDecl *D) {
	dumpName(D);
}

void MyASTDumper::VisitTypedefDecl(const TypedefDecl *D) {
	dumpName(D);
	dumpType(D->getUnderlyingType());
	if (D->isModulePrivate())
		*OS << " __module_private__";
	dumpTypeAsChild(D->getUnderlyingType());
}

void MyASTDumper::VisitEnumDecl(const EnumDecl *D) {
	if (D->isScoped()) {
		if (D->isScopedUsingClassTag())
			*OS << "Scope=\"class\"";
		else
			*OS << "Scope=\"struct\"";
	}
	dumpName(D);
	if (D->isModulePrivate())
		*OS << " __module_private__";
	if (D->isFixed())
		dumpType(D->getIntegerType());
}

void MyASTDumper::VisitRecordDecl(const RecordDecl *D) {
	*OS << " KindName=\"";
	*OS << ' ' << D->getKindName();
	*OS << "\"";
	dumpName(D);
	*OS << " Attrs=\"";
	if (D->isModulePrivate())
		*OS << " __module_private__";
	if (D->isCompleteDefinition())
		*OS << " definition";
	*OS << "\"";
}

void MyASTDumper::VisitEnumConstantDecl(const EnumConstantDecl *D) {
	dumpName(D);
	dumpType(D->getType());
	if (const Expr *Init = D->getInitExpr())
		dumpStmt(Init);
}

void MyASTDumper::VisitIndirectFieldDecl(const IndirectFieldDecl *D) {
	dumpName(D);
	dumpType(D->getType());

	for (auto *Child : D->chain())
		dumpDeclRef(Child);
}

void MyASTDumper::VisitFunctionDecl(const FunctionDecl *D) {
	dumpName(D);
	dumpType(D->getType());

	StorageClass SC = D->getStorageClass();
	*OS << " Attrs=\"";
	if (SC != SC_None)
		*OS << VarDecl::getStorageClassSpecifierString(SC);
	if (D->isInlineSpecified())
		*OS << " inline";
	if (D->isVirtualAsWritten())
		*OS << " virtual";
	if (D->isModulePrivate())
		*OS << " __module_private__";

	if (D->isPure())
		*OS << " pure";
	if (D->isDefaulted()) {
		*OS << " default";
		if (D->isDeleted())
			*OS << "_delete";
	}
	if (D->isDeletedAsWritten())
		*OS << " delete";
	if (D->isTrivial())
		*OS << " trivial";

	if (const FunctionProtoType *FPT = D->getType()->getAs<FunctionProtoType>()) {
		FunctionProtoType::ExtProtoInfo EPI = FPT->getExtProtoInfo();
		switch (EPI.ExceptionSpec.Type) {
		default: break;
		case EST_Unevaluated:
			*OS << " noexcept-unevaluated " << EPI.ExceptionSpec.SourceDecl;
			break;
		case EST_Uninstantiated:
			*OS << " noexcept-uninstantiated " << EPI.ExceptionSpec.SourceTemplate;
			break;
		}
	}
	*OS << "\"";

	if (const FunctionTemplateSpecializationInfo *FTSI =
		D->getTemplateSpecializationInfo())
		dumpTemplateArgumentList(*FTSI->TemplateArguments);

	if (!D->param_begin() && D->getNumParams())
		dumpChild([=] { *OS << "<<NULL params x " << D->getNumParams() << ">>"; });
	else
		for (const ParmVarDecl *Parameter : D->parameters())
			dumpDecl(Parameter);

	if (const CXXConstructorDecl *C = dyn_cast<CXXConstructorDecl>(D))
		for (CXXConstructorDecl::init_const_iterator I = C->init_begin(),
			E = C->init_end();
			I != E; ++I)
			dumpCXXCtorInitializer(*I);

	if (const CXXMethodDecl *MD = dyn_cast<CXXMethodDecl>(D)) {
		if (MD->size_overridden_methods() != 0) {
			auto dumpOverride = [=](const CXXMethodDecl *D) {
				SplitQualType T_split = D->getType().split();
				*OS << D << " " << D->getParent()->getName()
					<< "::" << D->getNameAsString() << " '"
					<< QualType::getAsString(T_split, PrintPolicy) << "'";
			};

			dumpChild([=] {
				auto Overrides = MD->overridden_methods();
				*OS << "Overrides: [ ";
				dumpOverride(*Overrides.begin());
				for (const auto *Override :
					llvm::make_range(Overrides.begin() + 1, Overrides.end())) {
					*OS << ", ";
					dumpOverride(Override);
				}
				*OS << " ]";
			});
		}
	}

	if (D->doesThisDeclarationHaveABody())
		dumpStmt(D->getBody());
}

void MyASTDumper::VisitFieldDecl(const FieldDecl *D) {
	dumpName(D);
	dumpType(D->getType());
	if (D->isMutable())
		*OS << " mutable";
	if (D->isModulePrivate())
		*OS << " __module_private__";

	if (D->isBitField())
		dumpStmt(D->getBitWidth());
	if (Expr *Init = D->getInClassInitializer())
		dumpStmt(Init);
}

void MyASTDumper::VisitVarDecl(const VarDecl *D) {
	dumpName(D);
	dumpType(D->getType());
	StorageClass SC = D->getStorageClass();
	*OS << " Attrs=\"";
	if (SC != SC_None)
		*OS << ' ' << VarDecl::getStorageClassSpecifierString(SC);
	switch (D->getTLSKind()) {
	case VarDecl::TLS_None: break;
	case VarDecl::TLS_Static: *OS << " tls"; break;
	case VarDecl::TLS_Dynamic: *OS << " tls_dynamic"; break;
	}
	if (D->isModulePrivate())
		*OS << " __module_private__";
	if (D->isNRVOVariable())
		*OS << " nrvo";
	if (D->isInline())
		*OS << " inline";
	if (D->isConstexpr())
		*OS << " constexpr";
	if (D->hasInit()) {
		switch (D->getInitStyle()) {
		case VarDecl::CInit: *OS << " cinit"; break;
		case VarDecl::CallInit: *OS << " callinit"; break;
		case VarDecl::ListInit: *OS << " listinit"; break;
		}
		dumpStmt(D->getInit());
	}
	*OS << "\"";

}

void MyASTDumper::VisitDecompositionDecl(const DecompositionDecl *D) {
	VisitVarDecl(D);
	for (auto *B : D->bindings())
		dumpDecl(B);
}

void MyASTDumper::VisitBindingDecl(const BindingDecl *D) {
	dumpName(D);
	dumpType(D->getType());
	if (auto *E = D->getBinding())
		dumpStmt(E);
}

void MyASTDumper::VisitFileScopeAsmDecl(const FileScopeAsmDecl *D) {
	dumpStmt(D->getAsmString());
}

void MyASTDumper::VisitImportDecl(const ImportDecl *D) {
	*OS << ' ' << D->getImportedModule()->getFullModuleName();
}

void MyASTDumper::VisitPragmaCommentDecl(const PragmaCommentDecl *D) {
	*OS << ' ';
	switch (D->getCommentKind()) {
	case PCK_Unknown:  llvm_unreachable("unexpected pragma comment kind");
	case PCK_Compiler: *OS << "compiler"; break;
	case PCK_ExeStr:   *OS << "exestr"; break;
	case PCK_Lib:      *OS << "lib"; break;
	case PCK_Linker:   *OS << "linker"; break;
	case PCK_User:     *OS << "user"; break;
	}
	StringRef Arg = D->getArg();
	if (!Arg.empty())
		*OS << " \"" << Arg << "\"";
}

void MyASTDumper::VisitPragmaDetectMismatchDecl(
	const PragmaDetectMismatchDecl *D) {
	*OS << " \"" << D->getName() << "\" \"" << D->getValue() << "\"";
}

void MyASTDumper::VisitCapturedDecl(const CapturedDecl *D) {
	dumpStmt(D->getBody());
}

//===----------------------------------------------------------------------===//
// OpenMP Declarations
//===----------------------------------------------------------------------===//

void MyASTDumper::VisitOMPThreadPrivateDecl(const OMPThreadPrivateDecl *D) {
	for (auto *E : D->varlists())
		dumpStmt(E);
}

void MyASTDumper::VisitOMPDeclareReductionDecl(const OMPDeclareReductionDecl *D) {
	dumpName(D);
	dumpType(D->getType());
	*OS << " combiner";
	dumpStmt(D->getCombiner());
	if (auto *Initializer = D->getInitializer()) {
		*OS << " initializer";
		switch (D->getInitializerKind()) {
		case OMPDeclareReductionDecl::DirectInit:
			*OS << " omp_priv = ";
			break;
		case OMPDeclareReductionDecl::CopyInit:
			*OS << " omp_priv ()";
			break;
		case OMPDeclareReductionDecl::CallInit:
			break;
		}
		dumpStmt(Initializer);
	}
}

void MyASTDumper::VisitOMPCapturedExprDecl(const OMPCapturedExprDecl *D) {
	dumpName(D);
	dumpType(D->getType());
	dumpStmt(D->getInit());
}

//===----------------------------------------------------------------------===//
// C++ Declarations
//===----------------------------------------------------------------------===//

void MyASTDumper::VisitNamespaceDecl(const NamespaceDecl *D) {
	dumpName(D);
	if (D->isInline())
		*OS << " inline";
	if (!D->isOriginalNamespace())
		dumpDeclRef(D->getOriginalNamespace(), "original");
}

void MyASTDumper::VisitUsingDirectiveDecl(const UsingDirectiveDecl *D) {
	*OS << ' ';
	dumpBareDeclRef(D->getNominatedNamespace());
}

void MyASTDumper::VisitNamespaceAliasDecl(const NamespaceAliasDecl *D) {
	dumpName(D);
	dumpDeclRef(D->getAliasedNamespace());
}

void MyASTDumper::VisitTypeAliasDecl(const TypeAliasDecl *D) {
	dumpName(D);
	dumpType(D->getUnderlyingType());
	dumpTypeAsChild(D->getUnderlyingType());
}

void MyASTDumper::VisitTypeAliasTemplateDecl(const TypeAliasTemplateDecl *D) {
	dumpName(D);
	dumpTemplateParameters(D->getTemplateParameters());
	dumpDecl(D->getTemplatedDecl());
}

void MyASTDumper::VisitCXXRecordDecl(const CXXRecordDecl *D) {
	VisitRecordDecl(D);
	if (!D->isCompleteDefinition())
		return;

	dumpChild([=] {
		{
			*OS << "DefinitionData";
		}
#define FLAG(fn, name) if (D->fn()) *OS << " " #name;
		FLAG(isParsingBaseSpecifiers, parsing_base_specifiers);

		FLAG(isGenericLambda, generic);
		FLAG(isLambda, lambda);

		FLAG(canPassInRegisters, pass_in_registers);
		FLAG(isEmpty, empty);
		FLAG(isAggregate, aggregate);
		FLAG(isStandardLayout, standard_layout);
		FLAG(isTriviallyCopyable, trivially_copyable);
		FLAG(isPOD, pod);
		FLAG(isTrivial, trivial);
		FLAG(isPolymorphic, polymorphic);
		FLAG(isAbstract, abstract);
		FLAG(isLiteral, literal);

		FLAG(hasUserDeclaredConstructor, has_user_declared_ctor);
		FLAG(hasConstexprNonCopyMoveConstructor, has_constexpr_non_copy_move_ctor);
		FLAG(hasMutableFields, has_mutable_fields);
		FLAG(hasVariantMembers, has_variant_members);
		FLAG(allowConstDefaultInit, can_const_default_init);

		dumpChild([=] {
			{
				*OS << "DefaultConstructor";
			}
			FLAG(hasDefaultConstructor, exists);
			FLAG(hasTrivialDefaultConstructor, trivial);
			FLAG(hasNonTrivialDefaultConstructor, non_trivial);
			FLAG(hasUserProvidedDefaultConstructor, user_provided);
			FLAG(hasConstexprDefaultConstructor, constexpr);
			FLAG(needsImplicitDefaultConstructor, needs_implicit);
			FLAG(defaultedDefaultConstructorIsConstexpr, defaulted_is_constexpr);
		});

		dumpChild([=] {
			{
				*OS << "CopyConstructor";
			}
			FLAG(hasSimpleCopyConstructor, simple);
			FLAG(hasTrivialCopyConstructor, trivial);
			FLAG(hasNonTrivialCopyConstructor, non_trivial);
			FLAG(hasUserDeclaredCopyConstructor, user_declared);
			FLAG(hasCopyConstructorWithConstParam, has_const_param);
			FLAG(needsImplicitCopyConstructor, needs_implicit);
			FLAG(needsOverloadResolutionForCopyConstructor,
				needs_overload_resolution);
			if (!D->needsOverloadResolutionForCopyConstructor())
				FLAG(defaultedCopyConstructorIsDeleted, defaulted_is_deleted);
			FLAG(implicitCopyConstructorHasConstParam, implicit_has_const_param);
		});

		dumpChild([=] {
			{
				*OS << "MoveConstructor";
			}
			FLAG(hasMoveConstructor, exists);
			FLAG(hasSimpleMoveConstructor, simple);
			FLAG(hasTrivialMoveConstructor, trivial);
			FLAG(hasNonTrivialMoveConstructor, non_trivial);
			FLAG(hasUserDeclaredMoveConstructor, user_declared);
			FLAG(needsImplicitMoveConstructor, needs_implicit);
			FLAG(needsOverloadResolutionForMoveConstructor,
				needs_overload_resolution);
			if (!D->needsOverloadResolutionForMoveConstructor())
				FLAG(defaultedMoveConstructorIsDeleted, defaulted_is_deleted);
		});

		dumpChild([=] {
			{
				*OS << "CopyAssignment";
			}
			FLAG(hasTrivialCopyAssignment, trivial);
			FLAG(hasNonTrivialCopyAssignment, non_trivial);
			FLAG(hasCopyAssignmentWithConstParam, has_const_param);
			FLAG(hasUserDeclaredCopyAssignment, user_declared);
			FLAG(needsImplicitCopyAssignment, needs_implicit);
			FLAG(needsOverloadResolutionForCopyAssignment, needs_overload_resolution);
			FLAG(implicitCopyAssignmentHasConstParam, implicit_has_const_param);
		});

		dumpChild([=] {
			{
				*OS << "MoveAssignment";
			}
			FLAG(hasMoveAssignment, exists);
			FLAG(hasSimpleMoveAssignment, simple);
			FLAG(hasTrivialMoveAssignment, trivial);
			FLAG(hasNonTrivialMoveAssignment, non_trivial);
			FLAG(hasUserDeclaredMoveAssignment, user_declared);
			FLAG(needsImplicitMoveAssignment, needs_implicit);
			FLAG(needsOverloadResolutionForMoveAssignment, needs_overload_resolution);
		});

		dumpChild([=] {
			{
				*OS << "Destructor";
			}
			FLAG(hasSimpleDestructor, simple);
			FLAG(hasIrrelevantDestructor, irrelevant);
			FLAG(hasTrivialDestructor, trivial);
			FLAG(hasNonTrivialDestructor, non_trivial);
			FLAG(hasUserDeclaredDestructor, user_declared);
			FLAG(needsImplicitDestructor, needs_implicit);
			FLAG(needsOverloadResolutionForDestructor, needs_overload_resolution);
			if (!D->needsOverloadResolutionForDestructor())
				FLAG(defaultedDestructorIsDeleted, defaulted_is_deleted);
		});
	});

	for (const auto &I : D->bases()) {
		dumpChild([=] {
			if (I.isVirtual())
				*OS << "virtual ";
			dumpAccessSpecifier(I.getAccessSpecifier());
			dumpType(I.getType());
			if (I.isPackExpansion())
				*OS << "...";
		});
	}
}

void MyASTDumper::VisitStaticAssertDecl(const StaticAssertDecl *D) {
	dumpStmt(D->getAssertExpr());
	dumpStmt(D->getMessage());
}

template<typename SpecializationDecl>
void MyASTDumper::VisitTemplateDeclSpecialization(const SpecializationDecl *D,
	bool DumpExplicitInst,
	bool DumpRefOnly) {
	bool DumpedAny = false;
	for (auto *RedeclWithBadType : D->redecls()) {
		// FIXME: The redecls() range sometimes has elements of a less-specific
		// type. (In particular, ClassTemplateSpecializationDecl::redecls() gives
		// us TagDecls, and should give CXXRecordDecls).
		auto *Redecl = dyn_cast<SpecializationDecl>(RedeclWithBadType);
		if (!Redecl) {
			// Found the injected-class-name for a class template. This will be dumped
			// as part of its surrounding class so we don't need to dump it here.
			assert(isa<CXXRecordDecl>(RedeclWithBadType) &&
				"expected an injected-class-name");
			continue;
		}

		switch (Redecl->getTemplateSpecializationKind()) {
		case TSK_ExplicitInstantiationDeclaration:
		case TSK_ExplicitInstantiationDefinition:
			if (!DumpExplicitInst)
				break;
			LLVM_FALLTHROUGH;
		case TSK_Undeclared:
		case TSK_ImplicitInstantiation:
			if (DumpRefOnly)
				dumpDeclRef(Redecl);
			else
				dumpDecl(Redecl);
			DumpedAny = true;
			break;
		case TSK_ExplicitSpecialization:
			break;
		}
	}

	// Ensure we dump at least one decl for each specialization.
	if (!DumpedAny)
		dumpDeclRef(D);
}

template<typename TemplateDecl>
void MyASTDumper::VisitTemplateDecl(const TemplateDecl *D,
	bool DumpExplicitInst) {
	dumpName(D);
	dumpTemplateParameters(D->getTemplateParameters());

	dumpDecl(D->getTemplatedDecl());

	for (auto *Child : D->specializations())
		VisitTemplateDeclSpecialization(Child, DumpExplicitInst,
			!D->isCanonicalDecl());
}

void MyASTDumper::VisitFunctionTemplateDecl(const FunctionTemplateDecl *D) {
	// FIXME: We don't add a declaration of a function template specialization
	// to its context when it's explicitly instantiated, so dump explicit
	// instantiations when we dump the template itself.
	VisitTemplateDecl(D, true);
}

void MyASTDumper::VisitClassTemplateDecl(const ClassTemplateDecl *D) {
	VisitTemplateDecl(D, false);
}

void MyASTDumper::VisitClassTemplateSpecializationDecl(
	const ClassTemplateSpecializationDecl *D) {
	VisitCXXRecordDecl(D);
	dumpTemplateArgumentList(D->getTemplateArgs());
}

void MyASTDumper::VisitClassTemplatePartialSpecializationDecl(
	const ClassTemplatePartialSpecializationDecl *D) {
	VisitClassTemplateSpecializationDecl(D);
	dumpTemplateParameters(D->getTemplateParameters());
}

void MyASTDumper::VisitClassScopeFunctionSpecializationDecl(
	const ClassScopeFunctionSpecializationDecl *D) {
	dumpDecl(D->getSpecialization());
	if (D->hasExplicitTemplateArgs())
		dumpTemplateArgumentListInfo(D->templateArgs());
}

void MyASTDumper::VisitVarTemplateDecl(const VarTemplateDecl *D) {
	VisitTemplateDecl(D, false);
}

void MyASTDumper::VisitBuiltinTemplateDecl(const BuiltinTemplateDecl *D) {
	dumpName(D);
	dumpTemplateParameters(D->getTemplateParameters());
}

void MyASTDumper::VisitVarTemplateSpecializationDecl(
	const VarTemplateSpecializationDecl *D) {
	dumpTemplateArgumentList(D->getTemplateArgs());
	VisitVarDecl(D);
}

void MyASTDumper::VisitVarTemplatePartialSpecializationDecl(
	const VarTemplatePartialSpecializationDecl *D) {
	dumpTemplateParameters(D->getTemplateParameters());
	VisitVarTemplateSpecializationDecl(D);
}

void MyASTDumper::VisitTemplateTypeParmDecl(const TemplateTypeParmDecl *D) {
	if (D->wasDeclaredWithTypename())
		*OS << " typename";
	else
		*OS << " class";
	*OS << " depth " << D->getDepth() << " index " << D->getIndex();
	if (D->isParameterPack())
		*OS << " ...";
	dumpName(D);
	if (D->hasDefaultArgument())
		dumpTemplateArgument(D->getDefaultArgument());
}

void MyASTDumper::VisitNonTypeTemplateParmDecl(const NonTypeTemplateParmDecl *D) {
	dumpType(D->getType());
	*OS << " depth " << D->getDepth() << " index " << D->getIndex();
	if (D->isParameterPack())
		*OS << " ...";
	dumpName(D);
	if (D->hasDefaultArgument())
		dumpTemplateArgument(D->getDefaultArgument());
}

void MyASTDumper::VisitTemplateTemplateParmDecl(
	const TemplateTemplateParmDecl *D) {
	*OS << " depth " << D->getDepth() << " index " << D->getIndex();
	if (D->isParameterPack())
		*OS << " ...";
	dumpName(D);
	dumpTemplateParameters(D->getTemplateParameters());
	if (D->hasDefaultArgument())
		dumpTemplateArgumentLoc(D->getDefaultArgument());
}

void MyASTDumper::VisitUsingDecl(const UsingDecl *D) {
	*OS << ' ';
	if (D->getQualifier())
		D->getQualifier()->print(*OS, D->getASTContext().getPrintingPolicy());
	*OS << D->getNameAsString();
}

void MyASTDumper::VisitUnresolvedUsingTypenameDecl(
	const UnresolvedUsingTypenameDecl *D) {
	*OS << ' ';
	if (D->getQualifier())
		D->getQualifier()->print(*OS, D->getASTContext().getPrintingPolicy());
	*OS << D->getNameAsString();
}

void MyASTDumper::VisitUnresolvedUsingValueDecl(const UnresolvedUsingValueDecl *D) {
	*OS << ' ';
	if (D->getQualifier())
		D->getQualifier()->print(*OS, D->getASTContext().getPrintingPolicy());
	*OS << D->getNameAsString();
	dumpType(D->getType());
}

void MyASTDumper::VisitUsingShadowDecl(const UsingShadowDecl *D) {
	*OS << ' ';
	dumpBareDeclRef(D->getTargetDecl());
	if (auto *TD = dyn_cast<TypeDecl>(D->getUnderlyingDecl()))
		dumpTypeAsChild(TD->getTypeForDecl());
}

void MyASTDumper::VisitConstructorUsingShadowDecl(
	const ConstructorUsingShadowDecl *D) {
	if (D->constructsVirtualBase())
		*OS << " virtual";

	dumpChild([=] {
		*OS << "target ";
		dumpBareDeclRef(D->getTargetDecl());
	});

	dumpChild([=] {
		*OS << "nominated ";
		dumpBareDeclRef(D->getNominatedBaseClass());
		*OS << ' ';
		dumpBareDeclRef(D->getNominatedBaseClassShadowDecl());
	});

	dumpChild([=] {
		*OS << "constructed ";
		dumpBareDeclRef(D->getConstructedBaseClass());
		*OS << ' ';
		dumpBareDeclRef(D->getConstructedBaseClassShadowDecl());
	});
}

void MyASTDumper::VisitLinkageSpecDecl(const LinkageSpecDecl *D) {
	switch (D->getLanguage()) {
	case LinkageSpecDecl::lang_c: *OS << " C"; break;
	case LinkageSpecDecl::lang_cxx: *OS << " C++"; break;
	}
}

void MyASTDumper::VisitAccessSpecDecl(const AccessSpecDecl *D) {
	*OS << ' ';
	dumpAccessSpecifier(D->getAccess());
}

void MyASTDumper::VisitFriendDecl(const FriendDecl *D) {
	if (TypeSourceInfo *T = D->getFriendType())
		dumpType(T->getType());
	else
		dumpDecl(D->getFriendDecl());
}

//===----------------------------------------------------------------------===//
// Obj-C Declarations
//===----------------------------------------------------------------------===//

void MyASTDumper::VisitObjCIvarDecl(const ObjCIvarDecl *D) {
	dumpName(D);
	dumpType(D->getType());
	if (D->getSynthesize())
		*OS << " synthesize";

	switch (D->getAccessControl()) {
	case ObjCIvarDecl::None:
		*OS << " none";
		break;
	case ObjCIvarDecl::Private:
		*OS << " private";
		break;
	case ObjCIvarDecl::Protected:
		*OS << " protected";
		break;
	case ObjCIvarDecl::Public:
		*OS << " public";
		break;
	case ObjCIvarDecl::Package:
		*OS << " package";
		break;
	}
}

void MyASTDumper::VisitObjCMethodDecl(const ObjCMethodDecl *D) {
	if (D->isInstanceMethod())
		*OS << " -";
	else
		*OS << " +";
	dumpName(D);
	dumpType(D->getReturnType());

	if (D->isThisDeclarationADefinition()) {
		dumpDeclContext(D);
	}
	else {
		for (const ParmVarDecl *Parameter : D->parameters())
			dumpDecl(Parameter);
	}

	if (D->isVariadic())
		dumpChild([=] { *OS << "..."; });

	if (D->hasBody())
		dumpStmt(D->getBody());
}

void MyASTDumper::VisitObjCTypeParamDecl(const ObjCTypeParamDecl *D) {
	dumpName(D);
	switch (D->getVariance()) {
	case ObjCTypeParamVariance::Invariant:
		break;

	case ObjCTypeParamVariance::Covariant:
		*OS << " covariant";
		break;

	case ObjCTypeParamVariance::Contravariant:
		*OS << " contravariant";
		break;
	}

	if (D->hasExplicitBound())
		*OS << " bounded";
	dumpType(D->getUnderlyingType());
}

void MyASTDumper::VisitObjCCategoryDecl(const ObjCCategoryDecl *D) {
	dumpName(D);
	dumpDeclRef(D->getClassInterface());
	dumpObjCTypeParamList(D->getTypeParamList());
	dumpDeclRef(D->getImplementation());
	for (ObjCCategoryDecl::protocol_iterator I = D->protocol_begin(),
		E = D->protocol_end();
		I != E; ++I)
		dumpDeclRef(*I);
}

void MyASTDumper::VisitObjCCategoryImplDecl(const ObjCCategoryImplDecl *D) {
	dumpName(D);
	dumpDeclRef(D->getClassInterface());
	dumpDeclRef(D->getCategoryDecl());
}

void MyASTDumper::VisitObjCProtocolDecl(const ObjCProtocolDecl *D) {
	dumpName(D);

	for (auto *Child : D->protocols())
		dumpDeclRef(Child);
}

void MyASTDumper::VisitObjCInterfaceDecl(const ObjCInterfaceDecl *D) {
	dumpName(D);
	dumpObjCTypeParamList(D->getTypeParamListAsWritten());
	dumpDeclRef(D->getSuperClass(), "super");

	dumpDeclRef(D->getImplementation());
	for (auto *Child : D->protocols())
		dumpDeclRef(Child);
}

void MyASTDumper::VisitObjCImplementationDecl(const ObjCImplementationDecl *D) {
	dumpName(D);
	dumpDeclRef(D->getSuperClass(), "super");
	dumpDeclRef(D->getClassInterface());
	for (ObjCImplementationDecl::init_const_iterator I = D->init_begin(),
		E = D->init_end();
		I != E; ++I)
		dumpCXXCtorInitializer(*I);
}

void MyASTDumper::VisitObjCCompatibleAliasDecl(const ObjCCompatibleAliasDecl *D) {
	dumpName(D);
	dumpDeclRef(D->getClassInterface());
}

void MyASTDumper::VisitObjCPropertyDecl(const ObjCPropertyDecl *D) {
	dumpName(D);
	dumpType(D->getType());

	if (D->getPropertyImplementation() == ObjCPropertyDecl::Required)
		*OS << " required";
	else if (D->getPropertyImplementation() == ObjCPropertyDecl::Optional)
		*OS << " optional";

	ObjCPropertyDecl::PropertyAttributeKind Attrs = D->getPropertyAttributes();
	if (Attrs != ObjCPropertyDecl::OBJC_PR_noattr) {
		if (Attrs & ObjCPropertyDecl::OBJC_PR_readonly)
			*OS << " readonly";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_assign)
			*OS << " assign";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_readwrite)
			*OS << " readwrite";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_retain)
			*OS << " retain";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_copy)
			*OS << " copy";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_nonatomic)
			*OS << " nonatomic";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_atomic)
			*OS << " atomic";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_weak)
			*OS << " weak";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_strong)
			*OS << " strong";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_unsafe_unretained)
			*OS << " unsafe_unretained";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_class)
			*OS << " class";
		if (Attrs & ObjCPropertyDecl::OBJC_PR_getter)
			dumpDeclRef(D->getGetterMethodDecl(), "getter");
		if (Attrs & ObjCPropertyDecl::OBJC_PR_setter)
			dumpDeclRef(D->getSetterMethodDecl(), "setter");
	}
}

void MyASTDumper::VisitObjCPropertyImplDecl(const ObjCPropertyImplDecl *D) {
	dumpName(D->getPropertyDecl());
	if (D->getPropertyImplementation() == ObjCPropertyImplDecl::Synthesize)
		*OS << " synthesize";
	else
		*OS << " dynamic";
	dumpDeclRef(D->getPropertyDecl());
	dumpDeclRef(D->getPropertyIvarDecl());
}

void MyASTDumper::VisitBlockDecl(const BlockDecl *D) {
	for (auto I : D->parameters())
		dumpDecl(I);

	if (D->isVariadic())
		dumpChild([=] { *OS << "..."; });

	if (D->capturesCXXThis())
		dumpChild([=] { *OS << "capture this"; });

	for (const auto &I : D->captures()) {
		dumpChild([=] {
			*OS << "capture";
			if (I.isByRef())
				*OS << " byref";
			if (I.isNested())
				*OS << " nested";
			if (I.getVariable()) {
				*OS << ' ';
				dumpBareDeclRef(I.getVariable());
			}
			if (I.hasCopyExpr())
				dumpStmt(I.getCopyExpr());
		});
	}
	dumpStmt(D->getBody());
}

//===----------------------------------------------------------------------===//
//  Stmt dumping methods.
//===----------------------------------------------------------------------===//

void MyASTDumper::dumpStmt(const Stmt *S) {
	dumpChild([=] {
		if (!S) {
			*OS << "<<<NULL>>>";
			return;
		}

		// Some statements have custom mechanisms for dumping their children.
		if (const DeclStmt *DS = dyn_cast<DeclStmt>(S)) {
			VisitDeclStmt(DS);
			return;
		}
		if (const GenericSelectionExpr *GSE = dyn_cast<GenericSelectionExpr>(S)) {
			VisitGenericSelectionExpr(GSE);
			return;
		}

		ConstStmtVisitor<MyASTDumper>::Visit(S);

		for (const Stmt *SubStmt : S->children())
			dumpStmt(SubStmt);
	});
}

void MyASTDumper::VisitStmt(const Stmt *Node) {
	{
		*OS << Node->getStmtClassName();
	}
	dumpPointer(Node);
	dumpSourceRange(Node->getSourceRange());
}

void MyASTDumper::VisitDeclStmt(const DeclStmt *Node) {
	VisitStmt(Node);
	for (DeclStmt::const_decl_iterator I = Node->decl_begin(),
		E = Node->decl_end();
		I != E; ++I)
		dumpDecl(*I);
}

void MyASTDumper::VisitAttributedStmt(const AttributedStmt *Node) {
	VisitStmt(Node);
	for (ArrayRef<const Attr *>::iterator I = Node->getAttrs().begin(),
		E = Node->getAttrs().end();
		I != E; ++I)
		dumpAttr(*I);
}

void MyASTDumper::VisitLabelStmt(const LabelStmt *Node) {
	VisitStmt(Node);
	*OS << " '" << Node->getName() << "'";
}

void MyASTDumper::VisitGotoStmt(const GotoStmt *Node) {
	VisitStmt(Node);
	*OS << " '" << Node->getLabel()->getName() << "'";
	dumpPointer(Node->getLabel());
}

void MyASTDumper::VisitCXXCatchStmt(const CXXCatchStmt *Node) {
	VisitStmt(Node);
	dumpDecl(Node->getExceptionDecl());
}

void MyASTDumper::VisitCapturedStmt(const CapturedStmt *Node) {
	VisitStmt(Node);
	dumpDecl(Node->getCapturedDecl());
}

//===----------------------------------------------------------------------===//
//  OpenMP dumping methods.
//===----------------------------------------------------------------------===//

void MyASTDumper::VisitOMPExecutableDirective(
	const OMPExecutableDirective *Node) {
	VisitStmt(Node);
	for (auto *C : Node->clauses()) {
		dumpChild([=] {
			if (!C) {
				*OS << "<<<NULL>>> OMPClause";
				return;
			}
			{
				StringRef ClauseName(getOpenMPClauseName(C->getClauseKind()));
				*OS << "OMP" << ClauseName.substr(/*Start=*/0, /*N=*/1).upper()
					<< ClauseName.drop_front() << "Clause";
			}
			dumpPointer(C);
			dumpSourceRange(SourceRange(C->getLocStart(), C->getLocEnd()));
			if (C->isImplicit())
				*OS << " <implicit>";
			for (auto *S : C->children())
				dumpStmt(S);
		});
	}
}

//===----------------------------------------------------------------------===//
//  Expr dumping methods.
//===----------------------------------------------------------------------===//

void MyASTDumper::VisitExpr(const Expr *Node) {
	VisitStmt(Node);
	dumpType(Node->getType());

	{
		switch (Node->getValueKind()) {
		case VK_RValue:
			break;
		case VK_LValue:
			*OS << "Type=\"lvalue\"";
			break;
		case VK_XValue:
			*OS << "Type=\"xvalue\"";
			break;
		}
	}

	{
		switch (Node->getObjectKind()) {
		case OK_Ordinary:
			break;
		case OK_BitField:
			*OS << " Type=\"bitfield\"";
			break;
		case OK_ObjCProperty:
			*OS << " Type=\"objcproperty\"";
			break;
		case OK_ObjCSubscript:
			*OS << " Type=\"objcsubscript\"";
			break;
		case OK_VectorComponent:
			*OS << " Type=\"vectorcomponent\"";
			break;
		}
	}
}

static void dumpBasePath(raw_ostream *OS, const CastExpr *Node) {
	if (Node->path_empty())
		return;

	*OS << " (";
	bool First = true;
	for (CastExpr::path_const_iterator I = Node->path_begin(),
		E = Node->path_end();
		I != E; ++I) {
		const CXXBaseSpecifier *Base = *I;
		if (!First)
			*OS << " -> ";

		const CXXRecordDecl *RD =
			cast<CXXRecordDecl>(Base->getType()->getAs<RecordType>()->getDecl());

		if (Base->isVirtual())
			*OS << "virtual ";
		*OS << RD->getName();
		First = false;
	}

	*OS << ')';
}

void MyASTDumper::VisitCastExpr(const CastExpr *Node) {
	VisitExpr(Node);
	*OS << " Cast=\"<";
	{
		*OS << Node->getCastKindName();
	}
	dumpBasePath(OS, Node);
	*OS << ">\"";
}

void MyASTDumper::VisitImplicitCastExpr(const ImplicitCastExpr *Node) {
	VisitCastExpr(Node);
	if (Node->isPartOfExplicitCast())
		*OS << " AdditionalCastInfo=\"part_of_explicit_cast\"";
}

void MyASTDumper::VisitDeclRefExpr(const DeclRefExpr *Node) {
	VisitExpr(Node);

	std::string str;
	llvm::raw_string_ostream f(str);
	auto save = OS;
	OS = &f;
	*OS << " ";
	dumpBareDeclRef(Node->getDecl());
	if (Node->getDecl() != Node->getFoundDecl()) {
		*OS << " (";
		dumpBareDeclRef(Node->getFoundDecl());
		*OS << ")";
	}

	f.flush();
	str = provide_escapes(str);
	OS = save;
	*OS << "Expr=\"" << str << "\"";
}

void MyASTDumper::VisitUnresolvedLookupExpr(const UnresolvedLookupExpr *Node) {
	VisitExpr(Node);
	*OS << " (";
	if (!Node->requiresADL())
		*OS << "no ";
	*OS << "ADL) = '" << Node->getName() << '\'';

	UnresolvedLookupExpr::decls_iterator
		I = Node->decls_begin(), E = Node->decls_end();
	if (I == E)
		*OS << " empty";
	for (; I != E; ++I)
		dumpPointer(*I);
}

void MyASTDumper::VisitObjCIvarRefExpr(const ObjCIvarRefExpr *Node) {
	VisitExpr(Node);

	{
		*OS << " " << Node->getDecl()->getDeclKindName() << "Decl";
	}
	*OS << "='" << *Node->getDecl() << "'";
	dumpPointer(Node->getDecl());
	if (Node->isFreeIvar())
		*OS << " isFreeIvar";
}

void MyASTDumper::VisitPredefinedExpr(const PredefinedExpr *Node) {
	VisitExpr(Node);
	*OS << " " << PredefinedExpr::getIdentTypeName(Node->getIdentType());
}

void MyASTDumper::VisitCharacterLiteral(const CharacterLiteral *Node) {
	VisitExpr(Node);
	*OS << " " << Node->getValue();
}

void MyASTDumper::VisitIntegerLiteral(const IntegerLiteral *Node) {
	VisitExpr(Node);

	bool isSigned = Node->getType()->isSignedIntegerType();
	*OS << " Value=\"" << Node->getValue().toString(10, isSigned) << "\"";
}

void MyASTDumper::VisitFixedPointLiteral(const FixedPointLiteral *Node) {
	VisitExpr(Node);

	*OS << " " << Node->getValueAsString(/*Radix=*/10);
}

void MyASTDumper::VisitFloatingLiteral(const FloatingLiteral *Node) {
	VisitExpr(Node);
	*OS << " " << Node->getValueAsApproximateDouble();
}

void MyASTDumper::VisitStringLiteral(const StringLiteral *Str) {
	VisitExpr(Str);
	*OS << " ";
	Str->outputString(*OS);
}

void MyASTDumper::VisitInitListExpr(const InitListExpr *ILE) {
	VisitExpr(ILE);
	if (auto *Filler = ILE->getArrayFiller()) {
		dumpChild([=] {
			*OS << "array filler";
			dumpStmt(Filler);
		});
	}
	if (auto *Field = ILE->getInitializedFieldInUnion()) {
		*OS << " field ";
		dumpBareDeclRef(Field);
	}
}

void MyASTDumper::VisitArrayInitLoopExpr(const ArrayInitLoopExpr *E) {
	VisitExpr(E);
}

void MyASTDumper::VisitArrayInitIndexExpr(const ArrayInitIndexExpr *E) {
	VisitExpr(E);
}

void MyASTDumper::VisitUnaryOperator(const UnaryOperator *Node) {
	VisitExpr(Node);
	*OS << " PrePost=\"" << (Node->isPostfix() ? "postfix" : "prefix") << "\"";
	*OS << " Op=\"" << UnaryOperator::getOpcodeStr(Node->getOpcode()) << "\"";
	if (!Node->canOverflow())
		*OS << " cannot overflow";
}

void MyASTDumper::VisitUnaryExprOrTypeTraitExpr(
	const UnaryExprOrTypeTraitExpr *Node) {
	VisitExpr(Node);
	switch (Node->getKind()) {
	case UETT_SizeOf:
		*OS << " sizeof";
		break;
	case UETT_AlignOf:
		*OS << " alignof";
		break;
	case UETT_VecStep:
		*OS << " vec_step";
		break;
	case UETT_OpenMPRequiredSimdAlign:
		*OS << " __builtin_omp_required_simd_align";
		break;
	}
	if (Node->isArgumentType())
		dumpType(Node->getArgumentType());
}

void MyASTDumper::VisitMemberExpr(const MemberExpr *Node) {
	VisitExpr(Node);
	*OS << " " << (Node->isArrow() ? "->" : ".") << *Node->getMemberDecl();
	dumpPointer(Node->getMemberDecl());
}

void MyASTDumper::VisitExtVectorElementExpr(const ExtVectorElementExpr *Node) {
	VisitExpr(Node);
	*OS << " " << Node->getAccessor().getNameStart();
}

void MyASTDumper::VisitBinaryOperator(const BinaryOperator *Node) {
	VisitExpr(Node);
	*OS << " Op=\"" << BinaryOperator::getOpcodeStr(Node->getOpcode()) << "\"";
}

void MyASTDumper::VisitCompoundAssignOperator(
	const CompoundAssignOperator *Node) {
	VisitExpr(Node);
	*OS << " '" << BinaryOperator::getOpcodeStr(Node->getOpcode())
		<< "' ComputeLHSTy=";
	dumpBareType(Node->getComputationLHSType());
	*OS << " ComputeResultTy=";
	dumpBareType(Node->getComputationResultType());
}

void MyASTDumper::VisitBlockExpr(const BlockExpr *Node) {
	VisitExpr(Node);
	dumpDecl(Node->getBlockDecl());
}

void MyASTDumper::VisitOpaqueValueExpr(const OpaqueValueExpr *Node) {
	VisitExpr(Node);

	if (Expr *Source = Node->getSourceExpr())
		dumpStmt(Source);
}

void MyASTDumper::VisitGenericSelectionExpr(const GenericSelectionExpr *E) {
	VisitExpr(E);
	if (E->isResultDependent())
		*OS << " result_dependent";
	dumpStmt(E->getControllingExpr());
	dumpTypeAsChild(E->getControllingExpr()->getType()); // FIXME: remove

	for (unsigned I = 0, N = E->getNumAssocs(); I != N; ++I) {
		dumpChild([=] {
			if (const TypeSourceInfo *TSI = E->getAssocTypeSourceInfo(I)) {
				*OS << "case ";
				dumpType(TSI->getType());
			}
			else {
				*OS << "default";
			}

			if (!E->isResultDependent() && E->getResultIndex() == I)
				*OS << " selected";

			if (const TypeSourceInfo *TSI = E->getAssocTypeSourceInfo(I))
				dumpTypeAsChild(TSI->getType());
			dumpStmt(E->getAssocExpr(I));
		});
	}
}

// GNU extensions.

void MyASTDumper::VisitAddrLabelExpr(const AddrLabelExpr *Node) {
	VisitExpr(Node);
	*OS << " " << Node->getLabel()->getName();
	dumpPointer(Node->getLabel());
}

//===----------------------------------------------------------------------===//
// C++ Expressions
//===----------------------------------------------------------------------===//

void MyASTDumper::VisitCXXNamedCastExpr(const CXXNamedCastExpr *Node) {
	VisitExpr(Node);
	*OS << " " << Node->getCastName()
		<< "<" << Node->getTypeAsWritten().getAsString() << ">"
		<< " <" << Node->getCastKindName();
	dumpBasePath(OS, Node);
	*OS << ">";
}

void MyASTDumper::VisitCXXBoolLiteralExpr(const CXXBoolLiteralExpr *Node) {
	VisitExpr(Node);
	*OS << " " << (Node->getValue() ? "true" : "false");
}

void MyASTDumper::VisitCXXThisExpr(const CXXThisExpr *Node) {
	VisitExpr(Node);
	*OS << " this";
}

void MyASTDumper::VisitCXXFunctionalCastExpr(const CXXFunctionalCastExpr *Node) {
	VisitExpr(Node);
	*OS << " functional cast to " << Node->getTypeAsWritten().getAsString()
		<< " <" << Node->getCastKindName() << ">";
}

void MyASTDumper::VisitCXXUnresolvedConstructExpr(
	const CXXUnresolvedConstructExpr *Node) {
	VisitExpr(Node);
	dumpType(Node->getTypeAsWritten());
	if (Node->isListInitialization())
		*OS << " list";
}

void MyASTDumper::VisitCXXConstructExpr(const CXXConstructExpr *Node) {
	VisitExpr(Node);
	CXXConstructorDecl *Ctor = Node->getConstructor();
	dumpType(Ctor->getType());
	if (Node->isElidable())
		*OS << " elidable";
	if (Node->isListInitialization())
		*OS << " list";
	if (Node->isStdInitListInitialization())
		*OS << " std::initializer_list";
	if (Node->requiresZeroInitialization())
		*OS << " zeroing";
}

void MyASTDumper::VisitCXXBindTemporaryExpr(const CXXBindTemporaryExpr *Node) {
	VisitExpr(Node);
	*OS << " ";
	dumpCXXTemporary(Node->getTemporary());
}

void MyASTDumper::VisitCXXNewExpr(const CXXNewExpr *Node) {
	VisitExpr(Node);
	if (Node->isGlobalNew())
		*OS << " global";
	if (Node->isArray())
		*OS << " array";
	if (Node->getOperatorNew()) {
		*OS << ' ';
		dumpBareDeclRef(Node->getOperatorNew());
	}
	// We could dump the deallocation function used in case of error, but it's
	// usually not that interesting.
}

void MyASTDumper::VisitCXXDeleteExpr(const CXXDeleteExpr *Node) {
	VisitExpr(Node);
	if (Node->isGlobalDelete())
		*OS << " global";
	if (Node->isArrayForm())
		*OS << " array";
	if (Node->getOperatorDelete()) {
		*OS << ' ';
		dumpBareDeclRef(Node->getOperatorDelete());
	}
}

void
MyASTDumper::VisitMaterializeTemporaryExpr(const MaterializeTemporaryExpr *Node) {
	VisitExpr(Node);
	if (const ValueDecl *VD = Node->getExtendingDecl()) {
		*OS << " extended by ";
		dumpBareDeclRef(VD);
	}
}

void MyASTDumper::VisitExprWithCleanups(const ExprWithCleanups *Node) {
	VisitExpr(Node);
	for (unsigned i = 0, e = Node->getNumObjects(); i != e; ++i)
		dumpDeclRef(Node->getObject(i), "cleanup");
}

void MyASTDumper::dumpCXXTemporary(const CXXTemporary *Temporary) {
	*OS << "(CXXTemporary";
	dumpPointer(Temporary);
	*OS << ")";
}

void MyASTDumper::VisitSizeOfPackExpr(const SizeOfPackExpr *Node) {
	VisitExpr(Node);
	dumpPointer(Node->getPack());
	dumpName(Node->getPack());
	if (Node->isPartiallySubstituted())
		for (const auto &A : Node->getPartialArguments())
			dumpTemplateArgument(A);
}

void MyASTDumper::VisitCXXDependentScopeMemberExpr(
	const CXXDependentScopeMemberExpr *Node) {
	VisitExpr(Node);
	*OS << " " << (Node->isArrow() ? "->" : ".") << Node->getMember();
}

//===----------------------------------------------------------------------===//
// Obj-C Expressions
//===----------------------------------------------------------------------===//

void MyASTDumper::VisitObjCMessageExpr(const ObjCMessageExpr *Node) {
	VisitExpr(Node);
	*OS << " selector=";
	Node->getSelector().print(*OS);
	switch (Node->getReceiverKind()) {
	case ObjCMessageExpr::Instance:
		break;

	case ObjCMessageExpr::Class:
		*OS << " class=";
		dumpBareType(Node->getClassReceiver());
		break;

	case ObjCMessageExpr::SuperInstance:
		*OS << " super (instance)";
		break;

	case ObjCMessageExpr::SuperClass:
		*OS << " super (class)";
		break;
	}
}

void MyASTDumper::VisitObjCBoxedExpr(const ObjCBoxedExpr *Node) {
	VisitExpr(Node);
	if (auto *BoxingMethod = Node->getBoxingMethod()) {
		*OS << " selector=";
		BoxingMethod->getSelector().print(*OS);
	}
}

void MyASTDumper::VisitObjCAtCatchStmt(const ObjCAtCatchStmt *Node) {
	VisitStmt(Node);
	if (const VarDecl *CatchParam = Node->getCatchParamDecl())
		dumpDecl(CatchParam);
	else
		*OS << " catch all";
}

void MyASTDumper::VisitObjCEncodeExpr(const ObjCEncodeExpr *Node) {
	VisitExpr(Node);
	dumpType(Node->getEncodedType());
}

void MyASTDumper::VisitObjCSelectorExpr(const ObjCSelectorExpr *Node) {
	VisitExpr(Node);

	*OS << " ";
	Node->getSelector().print(*OS);
}

void MyASTDumper::VisitObjCProtocolExpr(const ObjCProtocolExpr *Node) {
	VisitExpr(Node);

	*OS << ' ' << *Node->getProtocol();
}

void MyASTDumper::VisitObjCPropertyRefExpr(const ObjCPropertyRefExpr *Node) {
	VisitExpr(Node);
	if (Node->isImplicitProperty()) {
		*OS << " Kind=MethodRef Getter=\"";
		if (Node->getImplicitPropertyGetter())
			Node->getImplicitPropertyGetter()->getSelector().print(*OS);
		else
			*OS << "(null)";

		*OS << "\" Setter=\"";
		if (ObjCMethodDecl *Setter = Node->getImplicitPropertySetter())
			Setter->getSelector().print(*OS);
		else
			*OS << "(null)";
		*OS << "\"";
	}
	else {
		*OS << " Kind=PropertyRef Property=\"" << *Node->getExplicitProperty() << '"';
	}

	if (Node->isSuperReceiver())
		*OS << " super";

	*OS << " Messaging=";
	if (Node->isMessagingGetter() && Node->isMessagingSetter())
		*OS << "Getter&Setter";
	else if (Node->isMessagingGetter())
		*OS << "Getter";
	else if (Node->isMessagingSetter())
		*OS << "Setter";
}

void MyASTDumper::VisitObjCSubscriptRefExpr(const ObjCSubscriptRefExpr *Node) {
	VisitExpr(Node);
	if (Node->isArraySubscriptRefExpr())
		*OS << " Kind=ArraySubscript GetterForArray=\"";
	else
		*OS << " Kind=DictionarySubscript GetterForDictionary=\"";
	if (Node->getAtIndexMethodDecl())
		Node->getAtIndexMethodDecl()->getSelector().print(*OS);
	else
		*OS << "(null)";

	if (Node->isArraySubscriptRefExpr())
		*OS << "\" SetterForArray=\"";
	else
		*OS << "\" SetterForDictionary=\"";
	if (Node->setAtIndexMethodDecl())
		Node->setAtIndexMethodDecl()->getSelector().print(*OS);
	else
		*OS << "(null)";
}

void MyASTDumper::VisitObjCBoolLiteralExpr(const ObjCBoolLiteralExpr *Node) {
	VisitExpr(Node);
	*OS << " " << (Node->getValue() ? "__objc_yes" : "__objc_no");
}

//===----------------------------------------------------------------------===//
// Comments
//===----------------------------------------------------------------------===//

const char *MyASTDumper::getCommandName(unsigned CommandID) {
	if (Traits)
		return Traits->getCommandInfo(CommandID)->Name;
	const CommandInfo *Info = CommandTraits::getBuiltinCommandInfo(CommandID);
	if (Info)
		return Info->Name;
	return "<not a builtin command>";
}

void MyASTDumper::dumpFullComment(const FullComment *C) {
	if (!C)
		return;

	FC = C;
	dumpComment(C);
	FC = nullptr;
}

void MyASTDumper::dumpComment(const Comment *C) {
	dumpChild([=] {
		if (!C) {
			*OS << "<<<NULL>>>";
			return;
		}

		{
			*OS << C->getCommentKindName();
		}
		dumpPointer(C);
		dumpSourceRange(C->getSourceRange());
		ConstCommentVisitor<MyASTDumper>::visit(C);
		for (Comment::child_iterator I = C->child_begin(), E = C->child_end();
			I != E; ++I)
			dumpComment(*I);
	});
}

void MyASTDumper::visitTextComment(const TextComment *C) {
	*OS << " Text=\"" << provide_escapes(C->getText()) << "\"";
}

void MyASTDumper::visitInlineCommandComment(const InlineCommandComment *C) {
	*OS << " Name=\"" << getCommandName(C->getCommandID()) << "\"";
	switch (C->getRenderKind()) {
	case InlineCommandComment::RenderNormal:
		*OS << " AdditionalInline=\"RenderNormal\"";
		break;
	case InlineCommandComment::RenderBold:
		*OS << " AdditionalInline=\"RenderBold\"";
		break;
	case InlineCommandComment::RenderMonospaced:
		*OS << " AdditionalInline=\"RenderMonospaced\"";
		break;
	case InlineCommandComment::RenderEmphasized:
		*OS << " AdditionalInline=\"RenderEmphasized\"";
		break;
	}

	for (unsigned i = 0, e = C->getNumArgs(); i != e; ++i)
		*OS << " Arg=\"[" << i << "] " << C->getArgText(i) << "\"";
}

void MyASTDumper::visitHTMLStartTagComment(const HTMLStartTagComment *C) {
	*OS << " Name=\"" << C->getTagName() << "\"";
	if (C->getNumAttrs() != 0) {
		*OS << " Attrs: ";
		for (unsigned i = 0, e = C->getNumAttrs(); i != e; ++i) {
			const HTMLStartTagComment::Attribute &Attr = C->getAttr(i);
			*OS << " \"" << Attr.Name << "=\"" << Attr.Value << "\"";
		}
	}
	if (C->isSelfClosing())
		*OS << " SelfClosing";
}

void MyASTDumper::visitHTMLEndTagComment(const HTMLEndTagComment *C) {
	*OS << " Name=\"" << C->getTagName() << "\"";
}

void MyASTDumper::visitBlockCommandComment(const BlockCommandComment *C) {
	*OS << " Name=\"" << getCommandName(C->getCommandID()) << "\"";
	for (unsigned i = 0, e = C->getNumArgs(); i != e; ++i)
		*OS << " Arg=\"[" << i << "] " << C->getArgText(i) << "\"";
}

void MyASTDumper::visitParamCommandComment(const ParamCommandComment *C) {
	*OS << " ParamCommandComment=\"" << ParamCommandComment::getDirectionAsString(C->getDirection());

	if (C->isDirectionExplicit())
		*OS << " explicitly";
	else
		*OS << " implicitly";

	*OS << "\"";

	if (C->hasParamName()) {
		if (C->isParamIndexValid())
			*OS << " Param=\"" << C->getParamName(FC) << "\"";
		else
			*OS << " Param=\"" << C->getParamNameAsWritten() << "\"";
	}

	if (C->isParamIndexValid() && !C->isVarArgParam())
		*OS << " ParamIndex=\"" << C->getParamIndex() << "\"";
}

void MyASTDumper::visitTParamCommandComment(const TParamCommandComment *C) {
	if (C->hasParamName()) {
		if (C->isPositionValid())
			*OS << " Param=\"" << C->getParamName(FC) << "\"";
		else
			*OS << " Param=\"" << C->getParamNameAsWritten() << "\"";
	}

	if (C->isPositionValid()) {
		*OS << " Position=<";
		for (unsigned i = 0, e = C->getDepth(); i != e; ++i) {
			*OS << C->getIndex(i);
			if (i != e - 1)
				*OS << ", ";
		}
		*OS << ">";
	}
}

void MyASTDumper::visitVerbatimBlockComment(const VerbatimBlockComment *C) {
	*OS << " Name=\"" << getCommandName(C->getCommandID()) << "\""
		" CloseName=\"" << C->getCloseName() << "\"";
}

void MyASTDumper::visitVerbatimBlockLineComment(
	const VerbatimBlockLineComment *C) {
	*OS << " Text=\"" << provide_escapes(C->getText()) << "\"";
}

void MyASTDumper::visitVerbatimLineComment(const VerbatimLineComment *C) {
	*OS << " Text=\"" << C->getText() << "\"";
}

//===----------------------------------------------------------------------===//
// Type method implementations
//===----------------------------------------------------------------------===//

void QualType::dump(const char *msg) const {
	if (msg)
		llvm::errs() << msg << ": ";
	dump();
}

LLVM_DUMP_METHOD void QualType::dump() const { dump(llvm::errs()); }

LLVM_DUMP_METHOD void QualType::dump(llvm::raw_ostream &OS) const {
	MyASTDumper Dumper(&OS, nullptr, nullptr);
	Dumper.dumpTypeAsChild(*this);
}

LLVM_DUMP_METHOD void Type::dump() const { dump(llvm::errs()); }

LLVM_DUMP_METHOD void Type::dump(llvm::raw_ostream &OS) const {
	QualType(this, 0).dump(OS);
}

//===----------------------------------------------------------------------===//
// Decl method implementations
//===----------------------------------------------------------------------===//

LLVM_DUMP_METHOD void Decl::dump() const { dump(llvm::errs()); }

LLVM_DUMP_METHOD void Decl::dump(raw_ostream &OS, bool Deserialize) const {
	const ASTContext &Ctx = getASTContext();
	const SourceManager &SM = Ctx.getSourceManager();
	MyASTDumper P(&OS, &Ctx.getCommentCommandTraits(), &SM,
		Ctx.getPrintingPolicy());
	P.setDeserialize(Deserialize);
	P.dumpDecl(this);
}

LLVM_DUMP_METHOD void Decl::dumpColor() const {
	const ASTContext &Ctx = getASTContext();
	MyASTDumper P(&llvm::errs(), &Ctx.getCommentCommandTraits(),
		&Ctx.getSourceManager(),
		Ctx.getPrintingPolicy());
	P.dumpDecl(this);
}

LLVM_DUMP_METHOD void DeclContext::dumpLookups() const {
	dumpLookups(llvm::errs());
}

LLVM_DUMP_METHOD void DeclContext::dumpLookups(raw_ostream &OS,
	bool DumpDecls,
	bool Deserialize) const {
	const DeclContext *DC = this;
	while (!DC->isTranslationUnit())
		DC = DC->getParent();
	ASTContext &Ctx = cast<TranslationUnitDecl>(DC)->getASTContext();
	const SourceManager &SM = Ctx.getSourceManager();
	MyASTDumper P(&OS, &Ctx.getCommentCommandTraits(), &Ctx.getSourceManager(),
		Ctx.getPrintingPolicy());
	P.setDeserialize(Deserialize);
	P.dumpLookups(this, DumpDecls);
}

//===----------------------------------------------------------------------===//
// Stmt method implementations
//===----------------------------------------------------------------------===//

LLVM_DUMP_METHOD void Stmt::dump(SourceManager &SM) const {
	dump(llvm::errs(), SM);
}

LLVM_DUMP_METHOD void Stmt::dump(raw_ostream &OS, SourceManager &SM) const {
	MyASTDumper P(&OS, nullptr, &SM);
	P.dumpStmt(this);
}

LLVM_DUMP_METHOD void Stmt::dump(raw_ostream &OS) const {
	MyASTDumper P(&OS, nullptr, nullptr);
	P.dumpStmt(this);
}

LLVM_DUMP_METHOD void Stmt::dump() const {
	MyASTDumper P(&llvm::errs(), nullptr, nullptr);
	P.dumpStmt(this);
}

LLVM_DUMP_METHOD void Stmt::dumpColor() const {
	MyASTDumper P(&llvm::errs(), nullptr, nullptr);
	P.dumpStmt(this);
}

//===----------------------------------------------------------------------===//
// Comment method implementations
//===----------------------------------------------------------------------===//

LLVM_DUMP_METHOD void Comment::dump() const {
	dump(llvm::errs(), nullptr, nullptr);
}

LLVM_DUMP_METHOD void Comment::dump(const ASTContext &Context) const {
	dump(llvm::errs(), &Context.getCommentCommandTraits(),
		&Context.getSourceManager());
}

void Comment::dump(raw_ostream &OS, const CommandTraits *Traits,
	const SourceManager *SM) const {
	const FullComment *FC = dyn_cast<FullComment>(this);
	MyASTDumper D(&OS, Traits, SM);
	D.dumpFullComment(FC);
}

LLVM_DUMP_METHOD void Comment::dumpColor() const {
	const FullComment *FC = dyn_cast<FullComment>(this);
	MyASTDumper D(&llvm::errs(), nullptr, nullptr);
	D.dumpFullComment(FC);
}

#ifdef __cplusplus
extern "C" {
#endif

char* RunTheDamnThing(clang::ASTContext &Ctx)
{
	// Set to false for debugging the AST serializer code.
	if (true)
	{
		const SourceManager &SM = Ctx.getSourceManager();
		std::string crap;
		llvm::raw_string_ostream more_crap(crap);
		MyASTDumper P(&more_crap, &Ctx.getCommentCommandTraits(), &SM);
		TranslationUnitDecl* tu = Ctx.getTranslationUnitDecl();
		P.start();
		P.dumpDecl(tu);
		P.complete();
		more_crap.flush();
		return _strdup(crap.c_str());
	}
	else
	{
		const SourceManager &SM = Ctx.getSourceManager();
		MyASTDumper P(&llvm::outs(), &Ctx.getCommentCommandTraits(), &SM);
		TranslationUnitDecl* tu = Ctx.getTranslationUnitDecl();
		P.start();
		P.dumpDecl(tu);
		P.complete();
		llvm::outs().flush();
		return (char*)"";
	}
}

#ifdef __cplusplus
}
#endif
