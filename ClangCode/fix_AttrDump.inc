
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
    switch(SA->getInterrupt()) {
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
    switch(SA->getType()) {
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
      switch(*I) {
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
    switch(SA->getDefaultState()) {
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
    switch(SA->getDiagnosticType()) {
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
    switch(SA->getExtensibility()) {
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
    switch(SA->getOption()) {
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
    switch(SA->getState()) {
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
    switch(SA->getInterrupt()) {
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
    switch(SA->getBranchState()) {
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
    switch(SA->getMapType()) {
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
    switch(SA->getFamily()) {
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
    switch(SA->getParamState()) {
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
    switch(SA->getPCS()) {
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
    switch(SA->getInterrupt()) {
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
    switch(SA->getState()) {
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
    switch(SA->getNewState()) {
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
    switch(SA->getTestState()) {
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
    switch(SA->getVisibility()) {
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
    switch(SA->getImplicitReason()) {
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
    switch(SA->getVisibility()) {
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
