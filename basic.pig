
template Enums
{
	header {{
	    bool first = true;
	    List<string> signatures = new List<string>();
		public string limit = "";
	}}

	pass GenerateHeader {

	   // Generate declarations at start of the file.
	   ( TranslationUnitDecl
		   [[
		   // ----------------------------------------------------------------------------
		   // This is autogenerated code by Piggy.
		   // Do not edit this file or all your changes will be lost after re-generation.
		   // ----------------------------------------------------------------------------
		   using System;
		   using System.Runtime.InteropServices;
		   using System.Security;

		   namespace clangc {

		   ]] Pointer=*
	   )
	}

	pass GenerateEnums {

	   ( SrcRange=$"{Enums.limit}"
		  (* EnumDecl
			 {{
				 first = true;
				 result.Append("public enum @" + tree.Peek(0).Attr("Name") + " {" + Environment.NewLine);
			 }}
			 (%
				( EnumConstantDecl
				   ( IntegerLiteral
					  {{
						 if (first)
							first = false;
						 else
							result.Append(", ");
						 result.Append("@" + tree.Peek(1).Attr("Name") + " = " + tree.Peek(0).Attr("Value") + Environment.NewLine);
					  }}
				   )
				)
				|
				( EnumConstantDecl
				   {{
					  if (first)
						 first = false;
					  else
						 result.Append(", ");
					  result.Append("@" + tree.Peek(0).Attr("Name") + Environment.NewLine);
				   }}
				)
			 %)*
			 [[}

			 ]]
		  *)
	   )
	}

	pass CollectReturns {

	   ( SrcRange=$"{Enums.limit}"
		  (* FunctionDecl Name=*
			 {{
				signatures.Add((string)tree.Peek(0).Attr("Type"));
			 }}
		  *)
	   )
	}

	pass GenerateReturns {

	   ( TranslationUnitDecl
		  {{
			 foreach (var l in signatures)
			 {
				var m = PiggyRuntime.TemplateHelpers.GetFunctionReturn(l);
				var b = PiggyRuntime.TemplateHelpers.BaseType(m);
				if (!b) continue;
				result.AppendLine(
				   @"
				   public partial struct " + l + @"
				   {
					  public " + l + @"(IntPtr pointer)
					  {
						 this.Pointer = pointer;
					  }
					  public IntPtr Pointer;
				   }

				   ");
			 }
		  }}
	   )
	}

	pass Functions {

	   ( SrcRange=$"{Enums.limit}"
		  (* FunctionDecl Name=*
			 {{
				result.Append("[DllImport(\"foobar\", CallingConvention = global::System.Runtime.InteropServices.CallingConvention.ThisCall,"
				   + " EntryPoint=\"" + tree.Peek(0).Attr("Name") + "\")]" + Environment.NewLine);
				result.Append("public static extern "
				   + PiggyRuntime.TemplateHelpers.GetFunctionReturn((string)tree.Peek(0).Attr("Type")) + " "
				   + tree.Peek(0).Attr("Name") + "(");
				 first = true;
			 }}
			 ( ParmVarDecl Name=* Type=*
				{{
				   if (first)
					  first = false;
				   else
					  result.Append(", ");
				   var premod_type = (string)tree.Peek(0).Attr("Type");
				   var postmod_type = PiggyRuntime.TemplateHelpers.ModParamType(premod_type);
				   result.Append(postmod_type + " " + tree.Peek(0).Attr("Name"));
				}}
			 )*
			 [[);

			 ]]
		  *)
	   )
	}

	pass GenerateEnd {

	   ( TranslationUnitDecl
		  [[
		  }
		  // End of translation unit.
		  ]]
	   )
	}
}