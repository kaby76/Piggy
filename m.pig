clang_file 'c:/temp/include/clang-c/Index.h';
clang_option '-IC:/temp/include';

header {{
   // Override limits in matching.
   static bool DoInit()
   {
      limit = ".*\\clang-c\\.*";
   }
   static bool fun = DoInit();
}}

// Include defaults. Order of rules are first come, first served.
include 'basic.pig';

