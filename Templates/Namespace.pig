template Namespace
{
    pass GenerateStart {
        // Generate declarations at start of the file.
        ( TranslationUnitDecl
            [[
            // ----------------------------------------------------------------------------
            // This is autogenerated code by Piggy.
            // Do not edit this file or all your changes will be lost after re-generation.
            // ----------------------------------------------------------------------------
            using System;
            using System.Collections.Generic;
            using System.Reflection;
            using System.Text;
            using System.Threading.Tasks;
            using System.IO;
            using System.Runtime.InteropServices;
            using System.Text.RegularExpressions;
            using System.Linq;

            namespace ]] {{ result.Append(namespace_name); }} [[ {

                [StructLayout(LayoutKind.Sequential)]
                public struct SizeT
                {
                    private UIntPtr value;
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    public SizeT(int value)
                    {
                        this.value = new UIntPtr((uint)value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    public SizeT(uint value)
                    {
                        this.value = new UIntPtr(value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    public SizeT(long value)
                    {
                        this.value = new UIntPtr((ulong)value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    public SizeT(ulong value)
                    {
                        this.value = new UIntPtr(value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    public SizeT(UIntPtr value)
                    {
                        this.value = value;
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    public SizeT(IntPtr value)
                    {
                        this.value = new UIntPtr((ulong)value.ToInt64());
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="t"></param>
                    /// <returns></returns>
                    public static implicit operator int(SizeT t)
                    {
                        return (int)t.value.ToUInt32();
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="t"></param>
                    /// <returns></returns>
                    public static implicit operator uint(SizeT t)
                    {
                        return (t.value.ToUInt32());
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="t"></param>
                    /// <returns></returns>
                    public static implicit operator long(SizeT t)
                    {
                        return (long)t.value.ToUInt64();
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="t"></param>
                    /// <returns></returns>
                    public static implicit operator ulong(SizeT t)
                    {
                        return (t.value.ToUInt64());
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="t"></param>
                    /// <returns></returns>
                    public static implicit operator UIntPtr(SizeT t)
                    {
                        return t.value;
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="t"></param>
                    /// <returns></returns>
                    public static implicit operator IntPtr(SizeT t)
                    {
                        return new IntPtr((long)t.value.ToUInt64());
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    /// <returns></returns>
                    public static implicit operator SizeT(int value)
                    {
                        return new SizeT(value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    /// <returns></returns>
                    public static implicit operator SizeT(uint value)
                    {
                        return new SizeT(value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    /// <returns></returns>
                    public static implicit operator SizeT(long value)
                    {
                        return new SizeT(value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    /// <returns></returns>
                    public static implicit operator SizeT(ulong value)
                    {
                        return new SizeT(value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    /// <returns></returns>
                    public static implicit operator SizeT(IntPtr value)
                    {
                        return new SizeT(value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="value"></param>
                    /// <returns></returns>
                    public static implicit operator SizeT(UIntPtr value)
                    {
                        return new SizeT(value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator !=(SizeT val1, SizeT val2)
                    {
                        return (val1.value != val2.value);
                    }
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator ==(SizeT val1, SizeT val2)
                    {
                        return (val1.value == val2.value);
                    }
                    #region +
                    /// <summary>
                    /// Define operator + on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator +(SizeT val1, SizeT val2)
                    {
                        return new SizeT(val1.value.ToUInt64() + val2.value.ToUInt64());
                    }
                    /// <summary>
                    /// Define operator + on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator +(SizeT val1, int val2)
                    {
                        return new SizeT(val1.value.ToUInt64() + (ulong)val2);
                    }
                    /// <summary>
                    /// Define operator + on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator +(int val1, SizeT val2)
                    {
                        return new SizeT((ulong)val1 + val2.value.ToUInt64());
                    }
                    /// <summary>
                    /// Define operator + on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator +(uint val1, SizeT val2)
                    {
                        return new SizeT((ulong)val1 + val2.value.ToUInt64());
                    }
                    /// <summary>
                    /// Define operator + on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator +(SizeT val1, uint val2)
                    {
                        return new SizeT(val1.value.ToUInt64() + (ulong)val2);
                    }
                    #endregion
                    #region -
                    /// <summary>
                    /// Define operator - on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator -(SizeT val1, SizeT val2)
                    {
                        return new SizeT(val1.value.ToUInt64() - val2.value.ToUInt64());
                    }
                    /// <summary>
                    /// Define operator - on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator -(SizeT val1, int val2)
                    {
                        return new SizeT(val1.value.ToUInt64() - (ulong)val2);
                    }
                    /// <summary>
                    /// Define operator - on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator -(int val1, SizeT val2)
                    {
                        return new SizeT((ulong)val1 - val2.value.ToUInt64());
                    }
                    /// <summary>
                    /// Define operator - on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator -(SizeT val1, uint val2)
                    {
                        return new SizeT(val1.value.ToUInt64() - (ulong)val2);
                    }
                    /// <summary>
                    /// Define operator - on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator -(uint val1, SizeT val2)
                    {
                        return new SizeT((ulong)val1 - val2.value.ToUInt64());
                    }
                    #endregion
                    #region *
                    /// <summary>
                    /// Define operator * on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator *(SizeT val1, SizeT val2)
                    {
                        return new SizeT(val1.value.ToUInt64() * val2.value.ToUInt64());
                    }
                    /// <summary>
                    /// Define operator * on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator *(SizeT val1, int val2)
                    {
                        return new SizeT(val1.value.ToUInt64() * (ulong)val2);
                    }
                    /// <summary>
                    /// Define operator * on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator *(int val1, SizeT val2)
                    {
                        return new SizeT((ulong)val1 * val2.value.ToUInt64());
                    }
                    /// <summary>
                    /// Define operator * on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator *(SizeT val1, uint val2)
                    {
                        return new SizeT(val1.value.ToUInt64() * (ulong)val2);
                    }
                    /// <summary>
                    /// Define operator * on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator *(uint val1, SizeT val2)
                    {
                        return new SizeT((ulong)val1 * val2.value.ToUInt64());
                    }
                    #endregion
                    #region /
                    /// <summary>
                    /// Define operator / on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator /(SizeT val1, SizeT val2)
                    {
                        return new SizeT(val1.value.ToUInt64() / val2.value.ToUInt64());
                    }
                    /// <summary>
                    /// Define operator / on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator /(SizeT val1, int val2)
                    {
                        return new SizeT(val1.value.ToUInt64() / (ulong)val2);
                    }
                    /// <summary>
                    /// Define operator / on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator /(int val1, SizeT val2)
                    {
                        return new SizeT((ulong)val1 / val2.value.ToUInt64());
                    }
                    /// <summary>
                    /// Define operator / on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator /(SizeT val1, uint val2)
                    {
                        return new SizeT(val1.value.ToUInt64() / (ulong)val2);
                    }
                    /// <summary>
                    /// Define operator / on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static SizeT operator /(uint val1, SizeT val2)
                    {
                        return new SizeT((ulong)val1 / val2.value.ToUInt64());
                    }
                    #endregion
                    #region >
                    /// <summary>
                    /// Define operator &gt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator >(SizeT val1, SizeT val2)
                    {
                        return val1.value.ToUInt64() > val2.value.ToUInt64();
                    }
                    /// <summary>
                    /// Define operator &gt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator >(SizeT val1, int val2)
                    {
                        return val1.value.ToUInt64() > (ulong)val2;
                    }
                    /// <summary>
                    /// Define operator &gt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator >(int val1, SizeT val2)
                    {
                        return (ulong)val1 > val2.value.ToUInt64();
                    }
                    /// <summary>
                    /// Define operator &gt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator >(SizeT val1, uint val2)
                    {
                        return val1.value.ToUInt64() > (ulong)val2;
                    }
                    /// <summary>
                    /// Define operator &gt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator >(uint val1, SizeT val2)
                    {
                        return (ulong)val1 > val2.value.ToUInt64();
                    }
                    #endregion
                    #region <
                    /// <summary>
                    /// Define operator &lt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator <(SizeT val1, SizeT val2)
                    {
                        return val1.value.ToUInt64() < val2.value.ToUInt64();
                    }
                    /// <summary>
                    /// Define operator &lt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator <(SizeT val1, int val2)
                    {
                        return val1.value.ToUInt64() < (ulong)val2;
                    }
                    /// <summary>
                    /// Define operator &lt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator <(int val1, SizeT val2)
                    {
                        return (ulong)val1 < val2.value.ToUInt64();
                    }
                    /// <summary>
                    /// Define operator &lt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator <(SizeT val1, uint val2)
                    {
                        return val1.value.ToUInt64() < (ulong)val2;
                    }
                    /// <summary>
                    /// Define operator &lt; on converted to ulong values to avoid fall back to int
                    /// </summary>
                    /// <param name="val1"></param>
                    /// <param name="val2"></param>
                    /// <returns></returns>
                    public static bool operator <(uint val1, SizeT val2)
                    {
                        return (ulong)val1 < val2.value.ToUInt64();
                    }
                    #endregion
                    /// <summary>
                    /// 
                    /// </summary>
                    /// <param name="obj"></param>
                    /// <returns></returns>
                    public override bool Equals(object obj)
                    {
                        if (!(obj is SizeT)) return false;
                        SizeT o = (SizeT)obj;
                        return this.value.Equals(o.value);
                    }
                    /// <summary>
                    /// returns this.value.ToString()
                    /// </summary>
                    /// <returns></returns>
                    public override string ToString()
                    {
                        if (IntPtr.Size == 4)
                            return ((uint)this.value.ToUInt32()).ToString();
                        else
                            return ((ulong)this.value.ToUInt64()).ToString();
                    }
                    /// <summary>
                    /// Returns this.value.GetHashCode()
                    /// </summary>
                    /// <returns></returns>
                    public override int GetHashCode()
                    {
                        return this.value.GetHashCode();
                    }
                }


            ]] Pointer=*
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