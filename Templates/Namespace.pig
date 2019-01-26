template Namespace
{
    pass GenerateStart {
        // Generate declarations at start of the file.
        ( TranslationUnitDecl
            {{
		        if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
				{
					// Create a new file for this struct.
					var output_file_name = "g-sizet.cs";
					PiggyRuntime.Tool.GeneratedFiles.Add(output_file_name);
					PiggyRuntime.Tool.Redirect = new PiggyRuntime.Redirect(output_file_name);
					System.Console.WriteLine("namespace " + ClangSupport.namespace_name);
					System.Console.WriteLine("{");
					System.Console.WriteLine("using System;");
					System.Console.WriteLine("using System.Runtime.InteropServices;");
				}
            }} Pointer=*
			[[
	[StructLayout(LayoutKind.Sequential)]
	public struct SizeT
	{
	    private UIntPtr value;
	    public SizeT(int value)
	    {
		this.value = new UIntPtr((uint)value);
	    }
	    public SizeT(uint value)
	    {
		this.value = new UIntPtr(value);
	    }
	    public SizeT(long value)
	    {
		this.value = new UIntPtr((ulong)value);
	    }
	    public SizeT(ulong value)
	    {
		this.value = new UIntPtr(value);
	    }
	    public SizeT(UIntPtr value)
	    {
		this.value = value;
	    }
	    public SizeT(IntPtr value)
	    {
		this.value = new UIntPtr((ulong)value.ToInt64());
	    }
	    public static implicit operator int(SizeT t)
	    {
		return (int)t.value.ToUInt32();
	    }
	    public static implicit operator uint(SizeT t)
	    {
		return (t.value.ToUInt32());
	    }
	    public static implicit operator long(SizeT t)
	    {
		return (long)t.value.ToUInt64();
	    }
	    public static implicit operator ulong(SizeT t)
	    {
		return (t.value.ToUInt64());
	    }
	    public static implicit operator UIntPtr(SizeT t)
	    {
		return t.value;
	    }
	    public static implicit operator IntPtr(SizeT t)
	    {
		return new IntPtr((long)t.value.ToUInt64());
	    }
	    public static implicit operator SizeT(int value)
	    {
		return new SizeT(value);
	    }
	    public static implicit operator SizeT(uint value)
	    {
		return new SizeT(value);
	    }
	    public static implicit operator SizeT(long value)
	    {
		return new SizeT(value);
	    }
	    public static implicit operator SizeT(ulong value)
	    {
		return new SizeT(value);
	    }
	    public static implicit operator SizeT(IntPtr value)
	    {
		return new SizeT(value);
	    }
	    public static implicit operator SizeT(UIntPtr value)
	    {
		return new SizeT(value);
	    }
	    public static bool operator !=(SizeT val1, SizeT val2)
	    {
		return (val1.value != val2.value);
	    }
	    public static bool operator ==(SizeT val1, SizeT val2)
	    {
		return (val1.value == val2.value);
	    }
	    public static SizeT operator +(SizeT val1, SizeT val2)
	    {
		return new SizeT(val1.value.ToUInt64() + val2.value.ToUInt64());
	    }
	    public static SizeT operator +(SizeT val1, int val2)
	    {
		return new SizeT(val1.value.ToUInt64() + (ulong)val2);
	    }
	    public static SizeT operator +(int val1, SizeT val2)
	    {
		return new SizeT((ulong)val1 + val2.value.ToUInt64());
	    }
	    public static SizeT operator +(uint val1, SizeT val2)
	    {
		return new SizeT((ulong)val1 + val2.value.ToUInt64());
	    }
	    public static SizeT operator +(SizeT val1, uint val2)
	    {
		return new SizeT(val1.value.ToUInt64() + (ulong)val2);
	    }
	    public static SizeT operator -(SizeT val1, SizeT val2)
	    {
		return new SizeT(val1.value.ToUInt64() - val2.value.ToUInt64());
	    }
	    public static SizeT operator -(SizeT val1, int val2)
	    {
		return new SizeT(val1.value.ToUInt64() - (ulong)val2);
	    }
	    public static SizeT operator -(int val1, SizeT val2)
	    {
		return new SizeT((ulong)val1 - val2.value.ToUInt64());
	    }
	    public static SizeT operator -(SizeT val1, uint val2)
	    {
		return new SizeT(val1.value.ToUInt64() - (ulong)val2);
	    }
	    public static SizeT operator -(uint val1, SizeT val2)
	    {
		return new SizeT((ulong)val1 - val2.value.ToUInt64());
	    }
	    public static SizeT operator *(SizeT val1, SizeT val2)
	    {
		return new SizeT(val1.value.ToUInt64() * val2.value.ToUInt64());
	    }
	    public static SizeT operator *(SizeT val1, int val2)
	    {
		return new SizeT(val1.value.ToUInt64() * (ulong)val2);
	    }
	    public static SizeT operator *(int val1, SizeT val2)
	    {
		return new SizeT((ulong)val1 * val2.value.ToUInt64());
	    }
	    public static SizeT operator *(SizeT val1, uint val2)
	    {
		return new SizeT(val1.value.ToUInt64() * (ulong)val2);
	    }
	    public static SizeT operator *(uint val1, SizeT val2)
	    {
		return new SizeT((ulong)val1 * val2.value.ToUInt64());
	    }
	    public static SizeT operator /(SizeT val1, SizeT val2)
	    {
		return new SizeT(val1.value.ToUInt64() / val2.value.ToUInt64());
	    }
	    public static SizeT operator /(SizeT val1, int val2)
	    {
		return new SizeT(val1.value.ToUInt64() / (ulong)val2);
	    }
	    public static SizeT operator /(int val1, SizeT val2)
	    {
		return new SizeT((ulong)val1 / val2.value.ToUInt64());
	    }
	    public static SizeT operator /(SizeT val1, uint val2)
	    {
		return new SizeT(val1.value.ToUInt64() / (ulong)val2);
	    }
	    public static SizeT operator /(uint val1, SizeT val2)
	    {
		return new SizeT((ulong)val1 / val2.value.ToUInt64());
	    }
	    public static bool operator >(SizeT val1, SizeT val2)
	    {
		return val1.value.ToUInt64() > val2.value.ToUInt64();
	    }
	    public static bool operator >(SizeT val1, int val2)
	    {
		return val1.value.ToUInt64() > (ulong)val2;
	    }
	    public static bool operator >(int val1, SizeT val2)
	    {
		return (ulong)val1 > val2.value.ToUInt64();
	    }
	    public static bool operator >(SizeT val1, uint val2)
	    {
		return val1.value.ToUInt64() > (ulong)val2;
	    }
	    public static bool operator >(uint val1, SizeT val2)
	    {
		return (ulong)val1 > val2.value.ToUInt64();
	    }
	    public static bool operator <(SizeT val1, SizeT val2)
	    {
		return val1.value.ToUInt64() < val2.value.ToUInt64();
	    }
	    public static bool operator <(SizeT val1, int val2)
	    {
		return val1.value.ToUInt64() < (ulong)val2;
	    }
	    public static bool operator <(int val1, SizeT val2)
	    {
		return (ulong)val1 < val2.value.ToUInt64();
	    }
	    public static bool operator <(SizeT val1, uint val2)
	    {
		return val1.value.ToUInt64() < (ulong)val2;
	    }
	    public static bool operator <(uint val1, SizeT val2)
	    {
		return (ulong)val1 < val2.value.ToUInt64();
	    }
	    public override bool Equals(object obj)
	    {
		if (!(obj is SizeT)) return false;
		SizeT o = (SizeT)obj;
		return this.value.Equals(o.value);
	    }
	    public override string ToString()
	    {
		if (IntPtr.Size == 4)
		    return ((uint)this.value.ToUInt32()).ToString();
		else
		    return ((ulong)this.value.ToUInt64()).ToString();
	    }
	    public override int GetHashCode()
	    {
		return this.value.GetHashCode();
	    }
	}
			]]
			{{
				if (PiggyRuntime.Tool.OutputLocation != null && Directory.Exists(PiggyRuntime.Tool.OutputLocation))
				{
					// Create a new file for this struct.
					System.Console.WriteLine("}");
					PiggyRuntime.Tool.Redirect.Dispose();
					PiggyRuntime.Tool.Redirect = null;
				}
			}}
        )
    }
 
    pass GenerateEnd {
        ( TranslationUnitDecl
            {{
		        if (PiggyRuntime.Tool.OutputLocation == null || !Directory.Exists(PiggyRuntime.Tool.OutputLocation))
				{
					System.Console.WriteLine(@"}
	                // End of translation unit.
					");
				}
            }}
        )
    }
}