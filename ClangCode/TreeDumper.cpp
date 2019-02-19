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


#ifdef __cplusplus
extern "C" {
#endif

extern bool _packed_ast;

#ifdef __cplusplus
}
#endif

using namespace clang;
using namespace clang::comments;

//===----------------------------------------------------------------------===//
// ASTDumper Visitor
//===----------------------------------------------------------------------===//
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
		public ConstCommentVisitor<MyASTDumper>, public TypeVisitor<MyASTDumper>
	{
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
		template<typename Fn> void dumpChild(Fn doDumpChild)
		{
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
					if (!_packed_ast) *OS << '\n';
					// Add in closing parentheses.
					if (this->changed > 0)
					{
						if (!_packed_ast) *OS << Prefix << "  ";
						for (int i = 0; i < this->changed-1; ++i)
							*OS << ") ";
						*OS << ")";
						this->changed = 0;
						if (!_packed_ast) *OS << '\n';
					}
					if (!_packed_ast) *OS << Prefix << "  ";
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

		void start()
		{
			*OS << "( ";
		}

		void complete()
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

		void dumpDecl(const Decl *D)
		{
			dumpChild([=] {
				if (!D) {
					*OS << "NullNode";
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

		void dumpStmt(const Stmt *S);
		void dumpFullComment(const FullComment *C);

		// Utilities
		void dumpPointer(const void *Ptr) {
			*OS << ' ' << "Pointer=\"" << Ptr << "\"";
		}

		void dumpSourceRange(SourceRange R);
		void dumpLocation(SourceLocation Loc);
		void dumpBareType(QualType T, bool Desugar = true);
		void dumpType(QualType T);
		void dumpTypeAsChild(QualType T);
		void dumpTypeAsChild(const Type *T);
		void dumpBareDeclRef(const Decl *Node, bool as_id = true);
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
			*OS << " Size=\"" << T->getSize() << "\"";
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
			case VectorType::AltiVecVector: *OS << " Weirdness=\"altivec\""; break;
			case VectorType::AltiVecPixel: *OS << " Weirdness=\"altivec pixel\""; break;
			case VectorType::AltiVecBool: *OS << " Weirdness=\"altivec bool\""; break;
			case VectorType::NeonVector: *OS << " Weirdness=\"neon\""; break;
			case VectorType::NeonPolyVector: *OS << " Weirdness=\"neon poly\""; break;
			}
			*OS << " Size=\"" << T->getNumElements() << "\"";
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
			*OS << " Attrs=\"";
			*OS << " depth " << T->getDepth() << " index " << T->getIndex();
			if (T->isParameterPack()) *OS << " pack";
			*OS << "\"";
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
			*OS << " GoesOnAndOn=\"";
			if (T->isTypeAlias()) *OS << " alias";
			*OS << " "; T->getTemplateName().dump(*OS);
			*OS << "\"";
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


void MyASTDumper::dumpLocation(SourceLocation Loc) {
	if (!SM)
		return;

	SourceLocation SpellingLoc = SM->getSpellingLoc(Loc);

	// The general format we print out is filename:line:col, but we drop pieces
	// that haven't changed since the last loc printed.

	// BULLSHIT! DO NOT DROP "PIECES". BULLSHIT DESIGN! NO REFERENTIAL TRANSPARENCY!
	// HAVE TO CONSTANTLY LOOK UP THE TREE TO SEE WHERE THE FUCK THIS NODE COMES FROM!
	// BULLSHIT!
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
		//LastLocFilename = _strdup(p.string().c_str());
		//LastLocLine = PLoc.getLine();
	}
	else if (PLoc.getLine() != LastLocLine) {
		*OS << "line" << ':' << PLoc.getLine()
			<< ':' << PLoc.getColumn();
		//LastLocLine = PLoc.getLine();
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
		*OS << " " << T.split().Quals.getAsString();
		*OS << "\"";
		dumpTypeAsChild(T.split().Ty);
	});
}

void MyASTDumper::dumpTypeAsChild(const Type *T) {
	dumpChild([=] {
		if (!T) {
			*OS << "NullNode ";
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

void MyASTDumper::dumpBareDeclRef(const Decl *D, bool as_id) {
	
	char* name;
	if (!D)
		name = (char*)"NullNode";
	else
		name = (char*)D->getDeclKindName();

	if (as_id)
		*OS << name;
	else
		*OS << " KindName=\"" << name << "\"";
	if (!D) return;

	dumpPointer(D);

	if (const NamedDecl *ND = dyn_cast<NamedDecl>(D)) {
		DeclarationName s = ND->getDeclName();
		std::string t = s.getAsString();
		if (strcmp("__crt_locale_pointers", t.c_str())==0)
		{
			int x = 111;
		}
		*OS << " Name=\"" << ND->getDeclName() << '\"';
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
		dumpBareDeclRef(D, Label == 0);
	});
}

void MyASTDumper::dumpName(const NamedDecl *ND) {
	if (ND->getDeclName()) {
		std::string s = ND->getNameAsString();
		if (strcmp("__crt_locale_pointers", s.c_str()) == 0)
		{
			int x = 111;
		}
		*OS << " Name=\"" << ND->getNameAsString() << "\"";
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

			std::string local_bullshit_string_for_redirection;
			llvm::raw_string_ostream fucking_bullshit(local_bullshit_string_for_redirection);
			switch (A->getKind()) {
			case attr::AMDGPUFlatWorkGroupSize: {
				const auto *SA = cast<AMDGPUFlatWorkGroupSizeAttr>(A);
				fucking_bullshit << " " << SA->getMin();
				fucking_bullshit << " " << SA->getMax();
				break;
			}
			case attr::AMDGPUNumSGPR: {
				const auto *SA = cast<AMDGPUNumSGPRAttr>(A);
				fucking_bullshit << " " << SA->getNumSGPR();
				break;
			}
			case attr::AMDGPUNumVGPR: {
				const auto *SA = cast<AMDGPUNumVGPRAttr>(A);
				fucking_bullshit << " " << SA->getNumVGPR();
				break;
			}
			case attr::AMDGPUWavesPerEU: {
				const auto *SA = cast<AMDGPUWavesPerEUAttr>(A);
				fucking_bullshit << " " << SA->getMin();
				fucking_bullshit << " " << SA->getMax();
				break;
			}
			case attr::ARMInterrupt: {
				const auto *SA = cast<ARMInterruptAttr>(A);
				switch (SA->getInterrupt()) {
				case ARMInterruptAttr::IRQ:
					fucking_bullshit << " IRQ";
					break;
				case ARMInterruptAttr::FIQ:
					fucking_bullshit << " FIQ";
					break;
				case ARMInterruptAttr::SWI:
					fucking_bullshit << " SWI";
					break;
				case ARMInterruptAttr::ABORT:
					fucking_bullshit << " ABORT";
					break;
				case ARMInterruptAttr::UNDEF:
					fucking_bullshit << " UNDEF";
					break;
				case ARMInterruptAttr::Generic:
					fucking_bullshit << " Generic";
					break;
				}
				break;
			}
			case attr::AVRInterrupt: {
				break;
			}
			case attr::AVRSignal: {
				break;
			}
			case attr::AbiTag: {
				const auto *SA = cast<AbiTagAttr>(A);
				for (const auto &Val : SA->tags())
					fucking_bullshit << " " << Val;
				break;
			}
			case attr::AcquireCapability: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<AcquireCapabilityAttr>(A);
				for (AcquireCapabilityAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::AcquiredAfter: {
				const auto *SA = cast<AcquiredAfterAttr>(A);
				for (AcquiredAfterAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::AcquiredBefore: {
				const auto *SA = cast<AcquiredBeforeAttr>(A);
				for (AcquiredBeforeAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::Alias: {
				const auto *SA = cast<AliasAttr>(A);
				fucking_bullshit << " \"" << SA->getAliasee() << "\"";
				break;
			}
			case attr::AlignMac68k: {
				break;
			}
			case attr::AlignValue: {
				const auto *SA = cast<AlignValueAttr>(A);
				dumpStmt(SA->getAlignment());
				break;
			}
			case attr::Aligned: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<AlignedAttr>(A);
				if (SA->isAlignmentExpr())
					dumpStmt(SA->getAlignmentExpr());
				else
					dumpType(SA->getAlignmentType()->getType());
				break;
			}
			case attr::AllocAlign: {
				const auto *SA = cast<AllocAlignAttr>(A);
				fucking_bullshit << " " << SA->getParamIndex().getSourceIndex();
				break;
			}
			case attr::AllocSize: {
				const auto *SA = cast<AllocSizeAttr>(A);
				fucking_bullshit << " " << SA->getElemSizeParam().getSourceIndex();
				if (SA->getNumElemsParam().isValid())
					fucking_bullshit << " " << SA->getNumElemsParam().getSourceIndex();
				break;
			}
			case attr::AlwaysInline: {
				fucking_bullshit << " " << A->getSpelling();
				break;
			}
			case attr::AnalyzerNoReturn: {
				break;
			}
			case attr::Annotate: {
				const auto *SA = cast<AnnotateAttr>(A);
				fucking_bullshit << " \"" << SA->getAnnotation() << "\"";
				break;
			}
			case attr::AnyX86Interrupt: {
				break;
			}
			case attr::AnyX86NoCallerSavedRegisters: {
				break;
			}
			case attr::AnyX86NoCfCheck: {
				break;
			}
			case attr::ArcWeakrefUnavailable: {
				break;
			}
			case attr::ArgumentWithTypeTag: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<ArgumentWithTypeTagAttr>(A);
				if (SA->getArgumentKind())
					fucking_bullshit << " " << SA->getArgumentKind()->getName();
				fucking_bullshit << " " << SA->getArgumentIdx().getSourceIndex();
				fucking_bullshit << " " << SA->getTypeTagIdx().getSourceIndex();
				if (SA->getIsPointer()) fucking_bullshit << " IsPointer";
				break;
			}
			case attr::Artificial: {
				break;
			}
			case attr::AsmLabel: {
				const auto *SA = cast<AsmLabelAttr>(A);
				fucking_bullshit << " \"" << SA->getLabel() << "\"";
				break;
			}
			case attr::AssertCapability: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<AssertCapabilityAttr>(A);
				for (AssertCapabilityAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::AssertExclusiveLock: {
				const auto *SA = cast<AssertExclusiveLockAttr>(A);
				for (AssertExclusiveLockAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::AssertSharedLock: {
				const auto *SA = cast<AssertSharedLockAttr>(A);
				for (AssertSharedLockAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::AssumeAligned: {
				const auto *SA = cast<AssumeAlignedAttr>(A);
				dumpStmt(SA->getAlignment());
				dumpStmt(SA->getOffset());
				break;
			}
			case attr::Availability: {
				const auto *SA = cast<AvailabilityAttr>(A);
				if (SA->getPlatform())
					fucking_bullshit << " " << SA->getPlatform()->getName();
				fucking_bullshit << " " << SA->getIntroduced();
				fucking_bullshit << " " << SA->getDeprecated();
				fucking_bullshit << " " << SA->getObsoleted();
				if (SA->getUnavailable()) fucking_bullshit << " Unavailable";
				fucking_bullshit << " \"" << SA->getMessage() << "\"";
				if (SA->getStrict()) fucking_bullshit << " Strict";
				fucking_bullshit << " \"" << SA->getReplacement() << "\"";
				break;
			}
			case attr::Blocks: {
				const auto *SA = cast<BlocksAttr>(A);
				switch (SA->getType()) {
				case BlocksAttr::ByRef:
					fucking_bullshit << " ByRef";
					break;
				}
				break;
			}
			case attr::C11NoReturn: {
				break;
			}
			case attr::CDecl: {
				break;
			}
			case attr::CFAuditedTransfer: {
				break;
			}
			case attr::CFConsumed: {
				break;
			}
			case attr::CFReturnsNotRetained: {
				break;
			}
			case attr::CFReturnsRetained: {
				break;
			}
			case attr::CFUnknownTransfer: {
				break;
			}
			case attr::CPUDispatch: {
				const auto *SA = cast<CPUDispatchAttr>(A);
				for (const auto &Val : SA->cpus())
					fucking_bullshit << " " << Val;
				break;
			}
			case attr::CPUSpecific: {
				const auto *SA = cast<CPUSpecificAttr>(A);
				for (const auto &Val : SA->cpus())
					fucking_bullshit << " " << Val;
				break;
			}
			case attr::CUDAConstant: {
				break;
			}
			case attr::CUDADevice: {
				break;
			}
			case attr::CUDAGlobal: {
				break;
			}
			case attr::CUDAHost: {
				break;
			}
			case attr::CUDAInvalidTarget: {
				break;
			}
			case attr::CUDALaunchBounds: {
				const auto *SA = cast<CUDALaunchBoundsAttr>(A);
				dumpStmt(SA->getMaxThreads());
				dumpStmt(SA->getMinBlocks());
				break;
			}
			case attr::CUDAShared: {
				break;
			}
			case attr::CXX11NoReturn: {
				break;
			}
			case attr::CallableWhen: {
				const auto *SA = cast<CallableWhenAttr>(A);
				for (CallableWhenAttr::callableStates_iterator I = SA->callableStates_begin(), E = SA->callableStates_end(); I != E; ++I) {
					switch (*I) {
					case CallableWhenAttr::Unknown:
						fucking_bullshit << " Unknown";
						break;
					case CallableWhenAttr::Consumed:
						fucking_bullshit << " Consumed";
						break;
					case CallableWhenAttr::Unconsumed:
						fucking_bullshit << " Unconsumed";
						break;
					}
				}
				break;
			}
			case attr::Capability: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<CapabilityAttr>(A);
				fucking_bullshit << " \"" << SA->getName() << "\"";
				break;
			}
			case attr::CapturedRecord: {
				break;
			}
			case attr::CarriesDependency: {
				break;
			}
			case attr::Cleanup: {
				const auto *SA = cast<CleanupAttr>(A);
				fucking_bullshit << " ";
				dumpBareDeclRef(SA->getFunctionDecl());
				break;
			}
			case attr::CodeSeg: {
				const auto *SA = cast<CodeSegAttr>(A);
				fucking_bullshit << " \"" << SA->getName() << "\"";
				break;
			}
			case attr::Cold: {
				break;
			}
			case attr::Common: {
				break;
			}
			case attr::Const: {
				break;
			}
			case attr::Constructor: {
				const auto *SA = cast<ConstructorAttr>(A);
				fucking_bullshit << " " << SA->getPriority();
				break;
			}
			case attr::Consumable: {
				const auto *SA = cast<ConsumableAttr>(A);
				switch (SA->getDefaultState()) {
				case ConsumableAttr::Unknown:
					fucking_bullshit << " Unknown";
					break;
				case ConsumableAttr::Consumed:
					fucking_bullshit << " Consumed";
					break;
				case ConsumableAttr::Unconsumed:
					fucking_bullshit << " Unconsumed";
					break;
				}
				break;
			}
			case attr::ConsumableAutoCast: {
				break;
			}
			case attr::ConsumableSetOnRead: {
				break;
			}
			case attr::Convergent: {
				break;
			}
			case attr::DLLExport: {
				break;
			}
			case attr::DLLImport: {
				break;
			}
			case attr::Deprecated: {
				const auto *SA = cast<DeprecatedAttr>(A);
				fucking_bullshit << " \"" << SA->getMessage() << "\"";
				fucking_bullshit << " \"" << SA->getReplacement() << "\"";
				break;
			}
			case attr::Destructor: {
				const auto *SA = cast<DestructorAttr>(A);
				fucking_bullshit << " " << SA->getPriority();
				break;
			}
			case attr::DiagnoseIf: {
				const auto *SA = cast<DiagnoseIfAttr>(A);
				fucking_bullshit << " \"" << SA->getMessage() << "\"";
				switch (SA->getDiagnosticType()) {
				case DiagnoseIfAttr::DT_Error:
					fucking_bullshit << " DT_Error";
					break;
				case DiagnoseIfAttr::DT_Warning:
					fucking_bullshit << " DT_Warning";
					break;
				}
				if (SA->getArgDependent()) fucking_bullshit << " ArgDependent";
				fucking_bullshit << " ";
				dumpBareDeclRef(SA->getParent());
				dumpStmt(SA->getCond());
				break;
			}
			case attr::DisableTailCalls: {
				break;
			}
			case attr::EmptyBases: {
				break;
			}
			case attr::EnableIf: {
				const auto *SA = cast<EnableIfAttr>(A);
				fucking_bullshit << " \"" << SA->getMessage() << "\"";
				dumpStmt(SA->getCond());
				break;
			}
			case attr::EnumExtensibility: {
				const auto *SA = cast<EnumExtensibilityAttr>(A);
				switch (SA->getExtensibility()) {
				case EnumExtensibilityAttr::Closed:
					fucking_bullshit << " Closed";
					break;
				case EnumExtensibilityAttr::Open:
					fucking_bullshit << " Open";
					break;
				}
				break;
			}
			case attr::ExclusiveTrylockFunction: {
				const auto *SA = cast<ExclusiveTrylockFunctionAttr>(A);
				dumpStmt(SA->getSuccessValue());
				for (ExclusiveTrylockFunctionAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::ExternalSourceSymbol: {
				const auto *SA = cast<ExternalSourceSymbolAttr>(A);
				fucking_bullshit << " \"" << SA->getLanguage() << "\"";
				fucking_bullshit << " \"" << SA->getDefinedIn() << "\"";
				if (SA->getGeneratedDeclaration()) fucking_bullshit << " GeneratedDeclaration";
				break;
			}
			case attr::FallThrough: {
				break;
			}
			case attr::FastCall: {
				break;
			}
			case attr::Final: {
				fucking_bullshit << " " << A->getSpelling();
				break;
			}
			case attr::FlagEnum: {
				break;
			}
			case attr::Flatten: {
				break;
			}
			case attr::Format: {
				const auto *SA = cast<FormatAttr>(A);
				if (SA->getType())
					fucking_bullshit << " " << SA->getType()->getName();
				fucking_bullshit << " " << SA->getFormatIdx();
				fucking_bullshit << " " << SA->getFirstArg();
				break;
			}
			case attr::FormatArg: {
				const auto *SA = cast<FormatArgAttr>(A);
				fucking_bullshit << " " << SA->getFormatIdx().getSourceIndex();
				break;
			}
			case attr::GNUInline: {
				break;
			}
			case attr::GuardedBy: {
				const auto *SA = cast<GuardedByAttr>(A);
				dumpStmt(SA->getArg());
				break;
			}
			case attr::GuardedVar: {
				break;
			}
			case attr::Hot: {
				break;
			}
			case attr::IBAction: {
				break;
			}
			case attr::IBOutlet: {
				break;
			}
			case attr::IBOutletCollection: {
				const auto *SA = cast<IBOutletCollectionAttr>(A);
				fucking_bullshit << " " << SA->getInterface().getAsString();
				break;
			}
			case attr::IFunc: {
				const auto *SA = cast<IFuncAttr>(A);
				fucking_bullshit << " \"" << SA->getResolver() << "\"";
				break;
			}
			case attr::InitPriority: {
				const auto *SA = cast<InitPriorityAttr>(A);
				fucking_bullshit << " " << SA->getPriority();
				break;
			}
			case attr::InitSeg: {
				const auto *SA = cast<InitSegAttr>(A);
				fucking_bullshit << " \"" << SA->getSection() << "\"";
				break;
			}
			case attr::IntelOclBicc: {
				break;
			}
			case attr::InternalLinkage: {
				break;
			}
			case attr::LTOVisibilityPublic: {
				break;
			}
			case attr::LayoutVersion: {
				const auto *SA = cast<LayoutVersionAttr>(A);
				fucking_bullshit << " " << SA->getVersion();
				break;
			}
			case attr::LifetimeBound: {
				break;
			}
			case attr::LockReturned: {
				const auto *SA = cast<LockReturnedAttr>(A);
				dumpStmt(SA->getArg());
				break;
			}
			case attr::LocksExcluded: {
				const auto *SA = cast<LocksExcludedAttr>(A);
				for (LocksExcludedAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::LoopHint: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<LoopHintAttr>(A);
				switch (SA->getOption()) {
				case LoopHintAttr::Vectorize:
					fucking_bullshit << " Vectorize";
					break;
				case LoopHintAttr::VectorizeWidth:
					fucking_bullshit << " VectorizeWidth";
					break;
				case LoopHintAttr::Interleave:
					fucking_bullshit << " Interleave";
					break;
				case LoopHintAttr::InterleaveCount:
					fucking_bullshit << " InterleaveCount";
					break;
				case LoopHintAttr::Unroll:
					fucking_bullshit << " Unroll";
					break;
				case LoopHintAttr::UnrollCount:
					fucking_bullshit << " UnrollCount";
					break;
				case LoopHintAttr::Distribute:
					fucking_bullshit << " Distribute";
					break;
				}
				switch (SA->getState()) {
				case LoopHintAttr::Enable:
					fucking_bullshit << " Enable";
					break;
				case LoopHintAttr::Disable:
					fucking_bullshit << " Disable";
					break;
				case LoopHintAttr::Numeric:
					fucking_bullshit << " Numeric";
					break;
				case LoopHintAttr::AssumeSafety:
					fucking_bullshit << " AssumeSafety";
					break;
				case LoopHintAttr::Full:
					fucking_bullshit << " Full";
					break;
				}
				dumpStmt(SA->getValue());
				break;
			}
			case attr::MSABI: {
				break;
			}
			case attr::MSInheritance: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<MSInheritanceAttr>(A);
				if (SA->getBestCase()) fucking_bullshit << " BestCase";
				break;
			}
			case attr::MSNoVTable: {
				break;
			}
			case attr::MSP430Interrupt: {
				const auto *SA = cast<MSP430InterruptAttr>(A);
				fucking_bullshit << " " << SA->getNumber();
				break;
			}
			case attr::MSStruct: {
				break;
			}
			case attr::MSVtorDisp: {
				const auto *SA = cast<MSVtorDispAttr>(A);
				fucking_bullshit << " " << SA->getVdm();
				break;
			}
			case attr::MaxFieldAlignment: {
				const auto *SA = cast<MaxFieldAlignmentAttr>(A);
				fucking_bullshit << " " << SA->getAlignment();
				break;
			}
			case attr::MayAlias: {
				break;
			}
			case attr::MicroMips: {
				break;
			}
			case attr::MinSize: {
				break;
			}
			case attr::MinVectorWidth: {
				const auto *SA = cast<MinVectorWidthAttr>(A);
				fucking_bullshit << " " << SA->getVectorWidth();
				break;
			}
			case attr::Mips16: {
				break;
			}
			case attr::MipsInterrupt: {
				const auto *SA = cast<MipsInterruptAttr>(A);
				switch (SA->getInterrupt()) {
				case MipsInterruptAttr::sw0:
					fucking_bullshit << " sw0";
					break;
				case MipsInterruptAttr::sw1:
					fucking_bullshit << " sw1";
					break;
				case MipsInterruptAttr::hw0:
					fucking_bullshit << " hw0";
					break;
				case MipsInterruptAttr::hw1:
					fucking_bullshit << " hw1";
					break;
				case MipsInterruptAttr::hw2:
					fucking_bullshit << " hw2";
					break;
				case MipsInterruptAttr::hw3:
					fucking_bullshit << " hw3";
					break;
				case MipsInterruptAttr::hw4:
					fucking_bullshit << " hw4";
					break;
				case MipsInterruptAttr::hw5:
					fucking_bullshit << " hw5";
					break;
				case MipsInterruptAttr::eic:
					fucking_bullshit << " eic";
					break;
				}
				break;
			}
			case attr::MipsLongCall: {
				fucking_bullshit << " " << A->getSpelling();
				break;
			}
			case attr::MipsShortCall: {
				fucking_bullshit << " " << A->getSpelling();
				break;
			}
			case attr::Mode: {
				const auto *SA = cast<ModeAttr>(A);
				if (SA->getMode())
					fucking_bullshit << " " << SA->getMode()->getName();
				break;
			}
			case attr::NSConsumed: {
				break;
			}
			case attr::NSConsumesSelf: {
				break;
			}
			case attr::NSReturnsAutoreleased: {
				break;
			}
			case attr::NSReturnsNotRetained: {
				break;
			}
			case attr::NSReturnsRetained: {
				break;
			}
			case attr::Naked: {
				break;
			}
			case attr::NoAlias: {
				break;
			}
			case attr::NoCommon: {
				break;
			}
			case attr::NoDebug: {
				break;
			}
			case attr::NoDuplicate: {
				break;
			}
			case attr::NoEscape: {
				break;
			}
			case attr::NoInline: {
				break;
			}
			case attr::NoInstrumentFunction: {
				break;
			}
			case attr::NoMicroMips: {
				break;
			}
			case attr::NoMips16: {
				break;
			}
			case attr::NoReturn: {
				break;
			}
			case attr::NoSanitize: {
				const auto *SA = cast<NoSanitizeAttr>(A);
				for (const auto &Val : SA->sanitizers())
					fucking_bullshit << " " << Val;
				break;
			}
			case attr::NoSplitStack: {
				break;
			}
			case attr::NoStackProtector: {
				break;
			}
			case attr::NoThreadSafetyAnalysis: {
				break;
			}
			case attr::NoThrow: {
				break;
			}
			case attr::NonNull: {
				const auto *SA = cast<NonNullAttr>(A);
				for (const auto &Val : SA->args())
					fucking_bullshit << " " << Val.getSourceIndex();
				break;
			}
			case attr::NotTailCalled: {
				break;
			}
			case attr::OMPCaptureKind: {
				const auto *SA = cast<OMPCaptureKindAttr>(A);
				fucking_bullshit << " " << SA->getCaptureKind();
				break;
			}
			case attr::OMPCaptureNoInit: {
				break;
			}
			case attr::OMPDeclareSimdDecl: {
				const auto *SA = cast<OMPDeclareSimdDeclAttr>(A);
				switch (SA->getBranchState()) {
				case OMPDeclareSimdDeclAttr::BS_Undefined:
					fucking_bullshit << " BS_Undefined";
					break;
				case OMPDeclareSimdDeclAttr::BS_Inbranch:
					fucking_bullshit << " BS_Inbranch";
					break;
				case OMPDeclareSimdDeclAttr::BS_Notinbranch:
					fucking_bullshit << " BS_Notinbranch";
					break;
				}
				for (const auto &Val : SA->modifiers())
					fucking_bullshit << " " << Val;
				dumpStmt(SA->getSimdlen());
				for (OMPDeclareSimdDeclAttr::uniforms_iterator I = SA->uniforms_begin(), E = SA->uniforms_end(); I != E; ++I)
					dumpStmt(*I);
				for (OMPDeclareSimdDeclAttr::aligneds_iterator I = SA->aligneds_begin(), E = SA->aligneds_end(); I != E; ++I)
					dumpStmt(*I);
				for (OMPDeclareSimdDeclAttr::alignments_iterator I = SA->alignments_begin(), E = SA->alignments_end(); I != E; ++I)
					dumpStmt(*I);
				for (OMPDeclareSimdDeclAttr::linears_iterator I = SA->linears_begin(), E = SA->linears_end(); I != E; ++I)
					dumpStmt(*I);
				for (OMPDeclareSimdDeclAttr::steps_iterator I = SA->steps_begin(), E = SA->steps_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::OMPDeclareTargetDecl: {
				const auto *SA = cast<OMPDeclareTargetDeclAttr>(A);
				switch (SA->getMapType()) {
				case OMPDeclareTargetDeclAttr::MT_To:
					fucking_bullshit << " MT_To";
					break;
				case OMPDeclareTargetDeclAttr::MT_Link:
					fucking_bullshit << " MT_Link";
					break;
				}
				break;
			}
			case attr::OMPReferencedVar: {
				const auto *SA = cast<OMPReferencedVarAttr>(A);
				dumpStmt(SA->getRef());
				break;
			}
			case attr::OMPThreadPrivateDecl: {
				break;
			}
			case attr::ObjCBoxable: {
				break;
			}
			case attr::ObjCBridge: {
				const auto *SA = cast<ObjCBridgeAttr>(A);
				if (SA->getBridgedType())
					fucking_bullshit << " " << SA->getBridgedType()->getName();
				break;
			}
			case attr::ObjCBridgeMutable: {
				const auto *SA = cast<ObjCBridgeMutableAttr>(A);
				if (SA->getBridgedType())
					fucking_bullshit << " " << SA->getBridgedType()->getName();
				break;
			}
			case attr::ObjCBridgeRelated: {
				const auto *SA = cast<ObjCBridgeRelatedAttr>(A);
				if (SA->getRelatedClass())
					fucking_bullshit << " " << SA->getRelatedClass()->getName();
				if (SA->getClassMethod())
					fucking_bullshit << " " << SA->getClassMethod()->getName();
				if (SA->getInstanceMethod())
					fucking_bullshit << " " << SA->getInstanceMethod()->getName();
				break;
			}
			case attr::ObjCDesignatedInitializer: {
				break;
			}
			case attr::ObjCException: {
				break;
			}
			case attr::ObjCExplicitProtocolImpl: {
				break;
			}
			case attr::ObjCIndependentClass: {
				break;
			}
			case attr::ObjCMethodFamily: {
				const auto *SA = cast<ObjCMethodFamilyAttr>(A);
				switch (SA->getFamily()) {
				case ObjCMethodFamilyAttr::OMF_None:
					fucking_bullshit << " OMF_None";
					break;
				case ObjCMethodFamilyAttr::OMF_alloc:
					fucking_bullshit << " OMF_alloc";
					break;
				case ObjCMethodFamilyAttr::OMF_copy:
					fucking_bullshit << " OMF_copy";
					break;
				case ObjCMethodFamilyAttr::OMF_init:
					fucking_bullshit << " OMF_init";
					break;
				case ObjCMethodFamilyAttr::OMF_mutableCopy:
					fucking_bullshit << " OMF_mutableCopy";
					break;
				case ObjCMethodFamilyAttr::OMF_new:
					fucking_bullshit << " OMF_new";
					break;
				}
				break;
			}
			case attr::ObjCNSObject: {
				break;
			}
			case attr::ObjCPreciseLifetime: {
				break;
			}
			case attr::ObjCRequiresPropertyDefs: {
				break;
			}
			case attr::ObjCRequiresSuper: {
				break;
			}
			case attr::ObjCReturnsInnerPointer: {
				break;
			}
			case attr::ObjCRootClass: {
				break;
			}
			case attr::ObjCRuntimeName: {
				const auto *SA = cast<ObjCRuntimeNameAttr>(A);
				fucking_bullshit << " \"" << SA->getMetadataName() << "\"";
				break;
			}
			case attr::ObjCRuntimeVisible: {
				break;
			}
			case attr::ObjCSubclassingRestricted: {
				break;
			}
			case attr::OpenCLAccess: {
				fucking_bullshit << " " << A->getSpelling();
				break;
			}
			case attr::OpenCLIntelReqdSubGroupSize: {
				const auto *SA = cast<OpenCLIntelReqdSubGroupSizeAttr>(A);
				fucking_bullshit << " " << SA->getSubGroupSize();
				break;
			}
			case attr::OpenCLKernel: {
				break;
			}
			case attr::OpenCLUnrollHint: {
				const auto *SA = cast<OpenCLUnrollHintAttr>(A);
				fucking_bullshit << " " << SA->getUnrollHint();
				break;
			}
			case attr::OptimizeNone: {
				break;
			}
			case attr::Overloadable: {
				break;
			}
			case attr::Override: {
				break;
			}
			case attr::Ownership: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<OwnershipAttr>(A);
				if (SA->getModule())
					fucking_bullshit << " " << SA->getModule()->getName();
				for (const auto &Val : SA->args())
					fucking_bullshit << " " << Val.getSourceIndex();
				break;
			}
			case attr::Packed: {
				break;
			}
			case attr::ParamTypestate: {
				const auto *SA = cast<ParamTypestateAttr>(A);
				switch (SA->getParamState()) {
				case ParamTypestateAttr::Unknown:
					fucking_bullshit << " Unknown";
					break;
				case ParamTypestateAttr::Consumed:
					fucking_bullshit << " Consumed";
					break;
				case ParamTypestateAttr::Unconsumed:
					fucking_bullshit << " Unconsumed";
					break;
				}
				break;
			}
			case attr::Pascal: {
				break;
			}
			case attr::PassObjectSize: {
				const auto *SA = cast<PassObjectSizeAttr>(A);
				fucking_bullshit << " " << SA->getType();
				break;
			}
			case attr::Pcs: {
				const auto *SA = cast<PcsAttr>(A);
				switch (SA->getPCS()) {
				case PcsAttr::AAPCS:
					fucking_bullshit << " AAPCS";
					break;
				case PcsAttr::AAPCS_VFP:
					fucking_bullshit << " AAPCS_VFP";
					break;
				}
				break;
			}
			case attr::PragmaClangBSSSection: {
				const auto *SA = cast<PragmaClangBSSSectionAttr>(A);
				fucking_bullshit << " \"" << SA->getName() << "\"";
				break;
			}
			case attr::PragmaClangDataSection: {
				const auto *SA = cast<PragmaClangDataSectionAttr>(A);
				fucking_bullshit << " \"" << SA->getName() << "\"";
				break;
			}
			case attr::PragmaClangRodataSection: {
				const auto *SA = cast<PragmaClangRodataSectionAttr>(A);
				fucking_bullshit << " \"" << SA->getName() << "\"";
				break;
			}
			case attr::PragmaClangTextSection: {
				const auto *SA = cast<PragmaClangTextSectionAttr>(A);
				fucking_bullshit << " \"" << SA->getName() << "\"";
				break;
			}
			case attr::PreserveAll: {
				break;
			}
			case attr::PreserveMost: {
				break;
			}
			case attr::PtGuardedBy: {
				const auto *SA = cast<PtGuardedByAttr>(A);
				dumpStmt(SA->getArg());
				break;
			}
			case attr::PtGuardedVar: {
				break;
			}
			case attr::Pure: {
				break;
			}
			case attr::RISCVInterrupt: {
				const auto *SA = cast<RISCVInterruptAttr>(A);
				switch (SA->getInterrupt()) {
				case RISCVInterruptAttr::user:
					fucking_bullshit << " user";
					break;
				case RISCVInterruptAttr::supervisor:
					fucking_bullshit << " supervisor";
					break;
				case RISCVInterruptAttr::machine:
					fucking_bullshit << " machine";
					break;
				}
				break;
			}
			case attr::RegCall: {
				break;
			}
			case attr::ReleaseCapability: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<ReleaseCapabilityAttr>(A);
				for (ReleaseCapabilityAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::RenderScriptKernel: {
				break;
			}
			case attr::ReqdWorkGroupSize: {
				const auto *SA = cast<ReqdWorkGroupSizeAttr>(A);
				fucking_bullshit << " " << SA->getXDim();
				fucking_bullshit << " " << SA->getYDim();
				fucking_bullshit << " " << SA->getZDim();
				break;
			}
			case attr::RequireConstantInit: {
				break;
			}
			case attr::RequiresCapability: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<RequiresCapabilityAttr>(A);
				for (RequiresCapabilityAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::Restrict: {
				fucking_bullshit << " " << A->getSpelling();
				break;
			}
			case attr::ReturnTypestate: {
				const auto *SA = cast<ReturnTypestateAttr>(A);
				switch (SA->getState()) {
				case ReturnTypestateAttr::Unknown:
					fucking_bullshit << " Unknown";
					break;
				case ReturnTypestateAttr::Consumed:
					fucking_bullshit << " Consumed";
					break;
				case ReturnTypestateAttr::Unconsumed:
					fucking_bullshit << " Unconsumed";
					break;
				}
				break;
			}
			case attr::ReturnsNonNull: {
				break;
			}
			case attr::ReturnsTwice: {
				break;
			}
			case attr::ScopedLockable: {
				break;
			}
			case attr::Section: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<SectionAttr>(A);
				fucking_bullshit << " \"" << SA->getName() << "\"";
				break;
			}
			case attr::SelectAny: {
				break;
			}
			case attr::Sentinel: {
				const auto *SA = cast<SentinelAttr>(A);
				fucking_bullshit << " " << SA->getSentinel();
				fucking_bullshit << " " << SA->getNullPos();
				break;
			}
			case attr::SetTypestate: {
				const auto *SA = cast<SetTypestateAttr>(A);
				switch (SA->getNewState()) {
				case SetTypestateAttr::Unknown:
					fucking_bullshit << " Unknown";
					break;
				case SetTypestateAttr::Consumed:
					fucking_bullshit << " Consumed";
					break;
				case SetTypestateAttr::Unconsumed:
					fucking_bullshit << " Unconsumed";
					break;
				}
				break;
			}
			case attr::SharedTrylockFunction: {
				const auto *SA = cast<SharedTrylockFunctionAttr>(A);
				dumpStmt(SA->getSuccessValue());
				for (SharedTrylockFunctionAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::StdCall: {
				break;
			}
			case attr::Suppress: {
				const auto *SA = cast<SuppressAttr>(A);
				for (const auto &Val : SA->diagnosticIdentifiers())
					fucking_bullshit << " " << Val;
				break;
			}
			case attr::SwiftCall: {
				break;
			}
			case attr::SwiftContext: {
				break;
			}
			case attr::SwiftErrorResult: {
				break;
			}
			case attr::SwiftIndirectResult: {
				break;
			}
			case attr::SysVABI: {
				break;
			}
			case attr::TLSModel: {
				const auto *SA = cast<TLSModelAttr>(A);
				fucking_bullshit << " \"" << SA->getModel() << "\"";
				break;
			}
			case attr::Target: {
				const auto *SA = cast<TargetAttr>(A);
				fucking_bullshit << " \"" << SA->getFeaturesStr() << "\"";
				break;
			}
			case attr::TestTypestate: {
				const auto *SA = cast<TestTypestateAttr>(A);
				switch (SA->getTestState()) {
				case TestTypestateAttr::Consumed:
					fucking_bullshit << " Consumed";
					break;
				case TestTypestateAttr::Unconsumed:
					fucking_bullshit << " Unconsumed";
					break;
				}
				break;
			}
			case attr::ThisCall: {
				break;
			}
			case attr::Thread: {
				break;
			}
			case attr::TransparentUnion: {
				break;
			}
			case attr::TrivialABI: {
				break;
			}
			case attr::TryAcquireCapability: {
				fucking_bullshit << " " << A->getSpelling();
				const auto *SA = cast<TryAcquireCapabilityAttr>(A);
				dumpStmt(SA->getSuccessValue());
				for (TryAcquireCapabilityAttr::args_iterator I = SA->args_begin(), E = SA->args_end(); I != E; ++I)
					dumpStmt(*I);
				break;
			}
			case attr::TypeTagForDatatype: {
				const auto *SA = cast<TypeTagForDatatypeAttr>(A);
				if (SA->getArgumentKind())
					fucking_bullshit << " " << SA->getArgumentKind()->getName();
				fucking_bullshit << " " << SA->getMatchingCType().getAsString();
				if (SA->getLayoutCompatible()) fucking_bullshit << " LayoutCompatible";
				if (SA->getMustBeNull()) fucking_bullshit << " MustBeNull";
				break;
			}
			case attr::TypeVisibility: {
				const auto *SA = cast<TypeVisibilityAttr>(A);
				switch (SA->getVisibility()) {
				case TypeVisibilityAttr::Default:
					fucking_bullshit << " Default";
					break;
				case TypeVisibilityAttr::Hidden:
					fucking_bullshit << " Hidden";
					break;
				case TypeVisibilityAttr::Protected:
					fucking_bullshit << " Protected";
					break;
				}
				break;
			}
			case attr::Unavailable: {
				const auto *SA = cast<UnavailableAttr>(A);
				fucking_bullshit << " \"" << SA->getMessage() << "\"";
				switch (SA->getImplicitReason()) {
				case UnavailableAttr::IR_None:
					fucking_bullshit << " IR_None";
					break;
				case UnavailableAttr::IR_ARCForbiddenType:
					fucking_bullshit << " IR_ARCForbiddenType";
					break;
				case UnavailableAttr::IR_ForbiddenWeak:
					fucking_bullshit << " IR_ForbiddenWeak";
					break;
				case UnavailableAttr::IR_ARCForbiddenConversion:
					fucking_bullshit << " IR_ARCForbiddenConversion";
					break;
				case UnavailableAttr::IR_ARCInitReturnsUnrelated:
					fucking_bullshit << " IR_ARCInitReturnsUnrelated";
					break;
				case UnavailableAttr::IR_ARCFieldWithOwnership:
					fucking_bullshit << " IR_ARCFieldWithOwnership";
					break;
				}
				break;
			}
			case attr::Unused: {
				fucking_bullshit << " " << A->getSpelling();
				break;
			}
			case attr::Used: {
				break;
			}
			case attr::Uuid: {
				const auto *SA = cast<UuidAttr>(A);
				fucking_bullshit << " \"" << SA->getGuid() << "\"";
				break;
			}
			case attr::VecReturn: {
				break;
			}
			case attr::VecTypeHint: {
				const auto *SA = cast<VecTypeHintAttr>(A);
				fucking_bullshit << " " << SA->getTypeHint().getAsString();
				break;
			}
			case attr::VectorCall: {
				break;
			}
			case attr::Visibility: {
				const auto *SA = cast<VisibilityAttr>(A);
				switch (SA->getVisibility()) {
				case VisibilityAttr::Default:
					fucking_bullshit << " Default";
					break;
				case VisibilityAttr::Hidden:
					fucking_bullshit << " Hidden";
					break;
				case VisibilityAttr::Protected:
					fucking_bullshit << " Protected";
					break;
				}
				break;
			}
			case attr::WarnUnused: {
				break;
			}
			case attr::WarnUnusedResult: {
				fucking_bullshit << " " << A->getSpelling();
				break;
			}
			case attr::Weak: {
				break;
			}
			case attr::WeakImport: {
				break;
			}
			case attr::WeakRef: {
				const auto *SA = cast<WeakRefAttr>(A);
				fucking_bullshit << " \"" << SA->getAliasee() << "\"";
				break;
			}
			case attr::WorkGroupSizeHint: {
				const auto *SA = cast<WorkGroupSizeHintAttr>(A);
				fucking_bullshit << " " << SA->getXDim();
				fucking_bullshit << " " << SA->getYDim();
				fucking_bullshit << " " << SA->getZDim();
				break;
			}
			case attr::X86ForceAlignArgPointer: {
				break;
			}
			case attr::XRayInstrument: {
				fucking_bullshit << " " << A->getSpelling();
				break;
			}
			case attr::XRayLogArgs: {
				const auto *SA = cast<XRayLogArgsAttr>(A);
				fucking_bullshit << " " << SA->getArgumentCount();
				break;
			}
			}
			fucking_bullshit.flush();
			*OS << provide_escapes(local_bullshit_string_for_redirection);
		}
		*OS << "\"";

	});
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
			dumpBareDeclRef(Init->getAnyMember(), false);
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

		*OS << " Kind=\"";
		switch (A.getKind()) {
		case TemplateArgument::Null:
			*OS << "null";
			*OS << "\"";
			break;
		case TemplateArgument::Type:
			*OS << "type";
			*OS << "\"";
			dumpType(A.getAsType());
			break;
		case TemplateArgument::Declaration:
			*OS << "decl ";
			*OS << "\"";
			dumpDeclRef(A.getAsDecl());
			break;
		case TemplateArgument::NullPtr:
			*OS << "nullptr";
			*OS << "\"";
			break;
		case TemplateArgument::Integral:
			*OS << "integral " << A.getAsIntegral();
			*OS << "\"";
			break;
		case TemplateArgument::Template:
			*OS << "template ";
			A.getAsTemplate().dump(*OS);
			*OS << "\"";
			break;
		case TemplateArgument::TemplateExpansion:
			*OS << "template expansion ";
			A.getAsTemplateOrTemplatePattern().dump(*OS);
			*OS << "\"";
			break;
		case TemplateArgument::Expression:
			*OS << "expr";
			*OS << "\"";
			dumpStmt(A.getAsExpr());
			break;
		case TemplateArgument::Pack:
			*OS << "pack";
			*OS << "\"";
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
	*OS << D->getKindName();
	*OS << "\"";
	dumpName(D);
	*OS << " Attrs=\"";
	int space = 0;
	if (D->isModulePrivate() && D->isCompleteDefinition())
		space = 1;
	if (D->isModulePrivate())
		*OS << "__module_private__";
	if (space) *OS << ' ';
	if (D->isCompleteDefinition())
		*OS << "definition";
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
				*OS << "Overrides Overrides=\"[ ";
				dumpOverride(*Overrides.begin());
				for (const auto *Override :
					llvm::make_range(Overrides.begin() + 1, Overrides.end())) {
					*OS << ", ";
					dumpOverride(Override);
				}
				*OS << " ]\"";
			});
		}
	}

	if (D->doesThisDeclarationHaveABody())
		dumpStmt(D->getBody());
}

void MyASTDumper::VisitFieldDecl(const FieldDecl *D) {
	dumpName(D);
	dumpType(D->getType());
	*OS << " Modifiers=\"";
	if (D->isMutable())
		*OS << " mutable";
	if (D->isModulePrivate())
		*OS << " __module_private__";
	*OS << "\"";
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
	*OS << " Kind=\"";
	switch (D->getCommentKind()) {
	case PCK_Unknown:  llvm_unreachable("unexpected pragma comment kind");
	case PCK_Compiler: *OS << "compiler"; break;
	case PCK_ExeStr:   *OS << "exestr"; break;
	case PCK_Lib:      *OS << "lib"; break;
	case PCK_Linker:   *OS << "linker"; break;
	case PCK_User:     *OS << "user"; break;
	}
	*OS << "\"";
	StringRef Arg = D->getArg();
	if (!Arg.empty())
		*OS << " Arg=\"" << Arg << "\"";
}

void MyASTDumper::VisitPragmaDetectMismatchDecl(
	const PragmaDetectMismatchDecl *D) {
	*OS << " Name=\"" << D->getName() << "\" Value=\"" << D->getValue() << "\"";
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
	*OS << " BareDeclRef=\"";

	std::string str;
	llvm::raw_string_ostream f(str);
	auto save = OS;
	OS = &f;

	dumpBareDeclRef(D->getNominatedNamespace());

	f.flush();
	str = provide_escapes(str);
	OS = save;

	*OS << "\"";

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
		*OS << " Data=\"";

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
		*OS << "\"";

		dumpChild([=] {
			{
				*OS << "DefaultConstructor";
			}
			*OS << " Data=\"";
			FLAG(hasDefaultConstructor, exists);
			FLAG(hasTrivialDefaultConstructor, trivial);
			FLAG(hasNonTrivialDefaultConstructor, non_trivial);
			FLAG(hasUserProvidedDefaultConstructor, user_provided);
			FLAG(hasConstexprDefaultConstructor, constexpr);
			FLAG(needsImplicitDefaultConstructor, needs_implicit);
			FLAG(defaultedDefaultConstructorIsConstexpr, defaulted_is_constexpr);
			*OS << "\"";
		});

		dumpChild([=] {
			{
				*OS << "CopyConstructor";
			}
			*OS << " Data=\"";
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
			*OS << "\"";
		});

		dumpChild([=] {
			{
				*OS << "MoveConstructor";
			}
			*OS << " Data=\"";
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
			*OS << "\"";
		});

		dumpChild([=] {
			{
				*OS << "CopyAssignment";
			}
			*OS << " Data=\"";
			FLAG(hasTrivialCopyAssignment, trivial);
			FLAG(hasNonTrivialCopyAssignment, non_trivial);
			FLAG(hasCopyAssignmentWithConstParam, has_const_param);
			FLAG(hasUserDeclaredCopyAssignment, user_declared);
			FLAG(needsImplicitCopyAssignment, needs_implicit);
			FLAG(needsOverloadResolutionForCopyAssignment, needs_overload_resolution);
			FLAG(implicitCopyAssignmentHasConstParam, implicit_has_const_param);
			*OS << "\"";
		});

		dumpChild([=] {
			{
				*OS << "MoveAssignment";
			}
			*OS << " Data=\"";
			FLAG(hasMoveAssignment, exists);
			FLAG(hasSimpleMoveAssignment, simple);
			FLAG(hasTrivialMoveAssignment, trivial);
			FLAG(hasNonTrivialMoveAssignment, non_trivial);
			FLAG(hasUserDeclaredMoveAssignment, user_declared);
			FLAG(needsImplicitMoveAssignment, needs_implicit);
			FLAG(needsOverloadResolutionForMoveAssignment, needs_overload_resolution);
			*OS << "\"";
		});

		dumpChild([=] {
			{
				*OS << "Destructor";
			}
			*OS << " Data=\"";
			FLAG(hasSimpleDestructor, simple);
			FLAG(hasIrrelevantDestructor, irrelevant);
			FLAG(hasTrivialDestructor, trivial);
			FLAG(hasNonTrivialDestructor, non_trivial);
			FLAG(hasUserDeclaredDestructor, user_declared);
			FLAG(needsImplicitDestructor, needs_implicit);
			FLAG(needsOverloadResolutionForDestructor, needs_overload_resolution);
			if (!D->needsOverloadResolutionForDestructor())
				FLAG(defaultedDestructorIsDeleted, defaulted_is_deleted);
			*OS << "\"";
		});
	});

	for (const auto &I : D->bases()) {
		dumpChild([=] {
			*OS << " InheritsFrom ";
			*OS << " Type=\"";
		
			std::string str;
			llvm::raw_string_ostream f(str);
			auto save = OS;
			OS = &f;

			if (I.isVirtual())
				*OS << "virtual ";
			dumpAccessSpecifier(I.getAccessSpecifier());
			dumpType(I.getType());
			if (I.isPackExpansion())
				*OS << "...";

			f.flush();
			str = provide_escapes(str);
			OS = save;

			*OS << "\"";
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
	*OS << " Parm=\"";
	if (D->wasDeclaredWithTypename())
		*OS << " typename";
	else
		*OS << " class";
	*OS << " depth " << D->getDepth() << " index " << D->getIndex();
	if (D->isParameterPack())
		*OS << " ...";
	*OS << "\"";
	dumpName(D);
	if (D->hasDefaultArgument())
		dumpTemplateArgument(D->getDefaultArgument());
}

void MyASTDumper::VisitNonTypeTemplateParmDecl(const NonTypeTemplateParmDecl *D) {
	dumpType(D->getType());
	*OS << " Attrs=\"";
	*OS << "depth " << D->getDepth() << " index " << D->getIndex();
	if (D->isParameterPack())
		*OS << " ...";
	*OS << "\"";
	dumpName(D);
	if (D->hasDefaultArgument())
		dumpTemplateArgument(D->getDefaultArgument());
}

void MyASTDumper::VisitTemplateTemplateParmDecl(
	const TemplateTemplateParmDecl *D) {
	*OS << " Depth=\"" << D->getDepth() << "\"";
	*OS << " Index=\"" << D->getIndex() << "\"";
	if (D->isParameterPack())
		*OS << " More=\"...\"";
	dumpName(D);
	dumpTemplateParameters(D->getTemplateParameters());
	if (D->hasDefaultArgument())
		dumpTemplateArgumentLoc(D->getDefaultArgument());
}

void MyASTDumper::VisitUsingDecl(const UsingDecl *D) {
	if (D->getQualifier())
	{
		*OS << " Qualifier=\"";
		D->getQualifier()->print(*OS, D->getASTContext().getPrintingPolicy());
		*OS << "\"";
	}
	*OS << " Name=\"" << D->getNameAsString() << "\"";
}

void MyASTDumper::VisitUnresolvedUsingTypenameDecl(
	const UnresolvedUsingTypenameDecl *D) {
	if (D->getQualifier())
	{
		*OS << " Qualifier=\"";
		D->getQualifier()->print(*OS, D->getASTContext().getPrintingPolicy());
		*OS << "\"";
	}
	*OS << " Name=\"" << D->getNameAsString() << "\"";
}

void MyASTDumper::VisitUnresolvedUsingValueDecl(const UnresolvedUsingValueDecl *D) {
	if (D->getQualifier())
	{
		*OS << " Qualifier=\"";
		D->getQualifier()->print(*OS, D->getASTContext().getPrintingPolicy());
		*OS << "\"";
	}
	*OS << " Name=\"" << D->getNameAsString() << "\"";
	dumpType(D->getType());
}

void MyASTDumper::VisitUsingShadowDecl(const UsingShadowDecl *D) {
	*OS << ' ';
	dumpBareDeclRef(D->getTargetDecl(), false);
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
	case LinkageSpecDecl::lang_c: *OS << " Linkage=\"C\""; break;
	case LinkageSpecDecl::lang_cxx: *OS << " Linkage=\"C++\""; break;
	}
}

void MyASTDumper::VisitAccessSpecDecl(const AccessSpecDecl *D) {
	*OS << " Type=\"";
	dumpAccessSpecifier(D->getAccess());
	*OS << "\"";
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
			*OS << "NullNode";
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
		//*OS << " StmtClassName=\"" << Node->getStmtClassName() << "\"";
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
	*OS << " Label=\"" << Node->getName() << "\"";
}

void MyASTDumper::VisitGotoStmt(const GotoStmt *Node) {
	VisitStmt(Node);
	*OS << " Label=\"" << Node->getLabel()->getName() << "\"";
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
				*OS << "NullNode";
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
			*OS << " Type=\"lvalue\"";
			break;
		case VK_XValue:
			*OS << " Type=\"xvalue\"";
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
	*OS << " Expr=\"" << str << "\"";
}

void MyASTDumper::VisitUnresolvedLookupExpr(const UnresolvedLookupExpr *Node) {
	VisitExpr(Node);

	*OS << " WhateverThisShitIs=\"(";

	if (!Node->requiresADL())
		*OS << "no ";
	*OS << "ADL) = '" << Node->getName() << '\'';

	UnresolvedLookupExpr::decls_iterator
		I = Node->decls_begin(), E = Node->decls_end();
	if (I == E)
		*OS << " empty";

	*OS << "\"";

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
	*OS << " Value=\"" << PredefinedExpr::getIdentTypeName(Node->getIdentType()) << "\"";
}

void MyASTDumper::VisitCharacterLiteral(const CharacterLiteral *Node) {
	VisitExpr(Node);
	*OS << " Value=\"" << Node->getValue() << "\"";
}

void MyASTDumper::VisitIntegerLiteral(const IntegerLiteral *Node) {
	VisitExpr(Node);

	bool isSigned = Node->getType()->isSignedIntegerType();
	*OS << " Value=\"" << Node->getValue().toString(10, isSigned) << "\"";
}

void MyASTDumper::VisitFixedPointLiteral(const FixedPointLiteral *Node) {
	VisitExpr(Node);

	*OS << " Value=\"" << Node->getValueAsString(/*Radix=*/10) << "\"";
}

void MyASTDumper::VisitFloatingLiteral(const FloatingLiteral *Node) {
	VisitExpr(Node);
	*OS << " Value=\"" << Node->getValueAsApproximateDouble() << "\"";
}

void MyASTDumper::VisitStringLiteral(const StringLiteral *Str) {
	VisitExpr(Str);
	*OS << " ";
	*OS << "Value=\"";

	std::string str;
	llvm::raw_string_ostream f(str);
	auto save = OS;
	OS = &f;
	Str->outputString(*OS);
	f.flush();

	str = provide_escapes(str);
	OS = save;

	*OS << "\"";
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
		*OS << " Attr=\"cannot overflow\"";
}

void MyASTDumper::VisitUnaryExprOrTypeTraitExpr(
	const UnaryExprOrTypeTraitExpr *Node) {
	VisitExpr(Node);
	*OS << " Kind=\"";
	switch (Node->getKind()) {
	case UETT_SizeOf:
		*OS << "sizeof";
		break;
	case UETT_AlignOf:
		*OS << "alignof";
		break;
	case UETT_VecStep:
		*OS << "vec_step";
		break;
	case UETT_OpenMPRequiredSimdAlign:
		*OS << "__builtin_omp_required_simd_align";
		break;
	}
	*OS << "\"";
	if (Node->isArgumentType())
		dumpType(Node->getArgumentType());
}

void MyASTDumper::VisitMemberExpr(const MemberExpr *Node) {
	VisitExpr(Node);
	*OS << " Qual=\""
	    << (Node->isArrow() ? "->" : ".")
	    << *Node->getMemberDecl()
	    << "\" ";
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
	*OS << " Operator=\"" << BinaryOperator::getOpcodeStr(Node->getOpcode())
		<< "\" ComputeLHSTy=";
	*OS << "\"";
	dumpBareType(Node->getComputationLHSType());
	*OS << "\"";
	*OS << " ComputeResultTy=";
	*OS << "\"";
	dumpBareType(Node->getComputationResultType());
	*OS << "\"";
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
	*OS << " BigFatCast=\"" << Node->getCastName()
		<< "<" << Node->getTypeAsWritten().getAsString() << ">"
		<< " <" << Node->getCastKindName();
	dumpBasePath(OS, Node);
	*OS << ">";
	*OS << "\"";
}

void MyASTDumper::VisitCXXBoolLiteralExpr(const CXXBoolLiteralExpr *Node) {
	VisitExpr(Node);
	*OS << " Value=\"";
	*OS << " " << (Node->getValue() ? "true" : "false");
	*OS << "\"";
}

void MyASTDumper::VisitCXXThisExpr(const CXXThisExpr *Node) {
	VisitExpr(Node);
	*OS << " This=\"this\"";
}

void MyASTDumper::VisitCXXFunctionalCastExpr(const CXXFunctionalCastExpr *Node) {
	VisitExpr(Node);
	*OS << " CastTo=\"functional cast to " << Node->getTypeAsWritten().getAsString()
		<< " <" << Node->getCastKindName() << ">\"";
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
	*OS << " MoreShit=\"";
	if (Node->isElidable())
		*OS << " elidable";
	if (Node->isListInitialization())
		*OS << " list";
	if (Node->isStdInitListInitialization())
		*OS << " std::initializer_list";
	if (Node->requiresZeroInitialization())
		*OS << " zeroing";
	*OS << "\"";
}

void MyASTDumper::VisitCXXBindTemporaryExpr(const CXXBindTemporaryExpr *Node) {
	VisitExpr(Node);
	*OS << " ";
	dumpCXXTemporary(Node->getTemporary());
}

void MyASTDumper::VisitCXXNewExpr(const CXXNewExpr *Node) {
	VisitExpr(Node);
	if (Node->isGlobalNew())
		*OS << " IsGlobal=\"true\"";
	if (Node->isArray())
		*OS << " IsArray=\"true\"";
	if (Node->getOperatorNew()) {
		std::string str;
		llvm::raw_string_ostream f(str);
		auto save = OS;
		OS = &f;
		dumpBareDeclRef(Node->getOperatorNew());
		f.flush();
		str = provide_escapes(str);
		OS = save;
		*OS << " DeclRef=\"" << str << "\"";
	}
	// We could dump the deallocation function used in case of error, but it's
	// usually not that interesting.
}

void MyASTDumper::VisitCXXDeleteExpr(const CXXDeleteExpr *Node) {
	VisitExpr(Node);
	if (Node->isGlobalDelete())
		*OS << " IsGlobal=\"true\"";
	if (Node->isArrayForm())
		*OS << " IsArray=\"true\"";
	if (Node->getOperatorDelete()) {
		std::string str;
		llvm::raw_string_ostream f(str);
		auto save = OS;
		OS = &f;
		dumpBareDeclRef(Node->getOperatorDelete());
		f.flush();
		str = provide_escapes(str);
		OS = save;
		*OS << " DeclRef=\"" << str << "\"";
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
	*OS << "( CXXTemporary";
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
	*OS << " Accessing=\"" << (Node->isArrow() ? "->" : ".") << Node->getMember() << "\"";
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
			*OS << "NullNode";
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

std::string RunTheDamnThing(clang::ASTContext &Ctx)
{
	// Set to false for debugging the AST serializer code.
	const SourceManager &SM = Ctx.getSourceManager();
	std::string crap;
	llvm::raw_string_ostream more_crap(crap);
	MyASTDumper P(&more_crap, &Ctx.getCommentCommandTraits(), &SM);
	TranslationUnitDecl* tu = Ctx.getTranslationUnitDecl();
	P.start();
	P.dumpDecl(tu);
	P.complete();
	more_crap.flush();
	return crap;
}

#ifdef __cplusplus
}
#endif
