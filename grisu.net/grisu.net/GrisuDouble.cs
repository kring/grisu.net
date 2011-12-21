using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace grisu.net
{
    internal struct GrisuDouble
    {
        const ulong kSignMask = 0x8000000000000000;
        const ulong kExponentMask = 0x7FF0000000000000;
        const ulong kSignificandMask = 0x000FFFFFFFFFFFFF;
        const ulong kHiddenBit = 0x0010000000000000;
        const int kPhysicalSignificandSize = 52;  // Excludes the hidden bit.
        const int kSignificandSize = 53;

        const int kExponentBias = 0x3FF + kPhysicalSignificandSize;
        const int kDenormalExponent = -kExponentBias + 1;
        const int kMaxExponent = 0x7FF - kExponentBias;
        const ulong kInfinity = 0x7FF0000000000000;
        const ulong kNaN = 0x7FF8000000000000;

        public GrisuDouble(double d)
        {
            value_ = d;
            d64_ = (ulong)BitConverter.DoubleToInt64Bits(d);
        }

        public GrisuDouble(ulong d64)
        {
            d64_ = d64;
            value_ = BitConverter.Int64BitsToDouble((long)d64);
        }

        public GrisuDouble(DiyFp diy_fp)
        {
            d64_ = DiyFpToUInt64(diy_fp);
            value_ = BitConverter.Int64BitsToDouble((long)d64_);
        }

        // The value encoded by this Double must be greater or equal to +0.0.
        // It must not be special (infinity, or NaN).
        public DiyFp AsDiyFp()
        {
            Debug.Assert(Sign > 0);
            Debug.Assert(!IsSpecial);
            return new DiyFp(Significand, Exponent);
        }

        // The value encoded by this Double must be strictly greater than 0.
        public DiyFp AsNormalizedDiyFp()
        {
            Debug.Assert(Value > 0.0);
            ulong f = Significand;
            int e = Exponent;

            // The current double could be a denormal.
            while ((f & kHiddenBit) == 0)
            {
                f <<= 1;
                e--;
            }
            // Do the final shifts in one go.
            f <<= DiyFp.kSignificandSize - kSignificandSize;
            e -= DiyFp.kSignificandSize - kSignificandSize;
            return new DiyFp(f, e);
        }

        // Returns the double's bit as UInt64.
        public ulong AsUInt64()
        {
            return d64_;
        }

        // Returns the next greater double. Returns +infinity on input +infinity.
        double NextDouble()
        {
            if (d64_ == kInfinity) return double.PositiveInfinity;
            if (Sign < 0 && Significand == 0)
            {
                // -0.0
                return 0.0;
            }
            if (Sign < 0)
            {
                return new GrisuDouble(d64_ - 1).Value;
            }
            else
            {
                return new GrisuDouble(d64_ + 1).Value;
            }
        }

        public int Exponent
        {
            get
            {
                if (IsDenormal) return kDenormalExponent;

                ulong d64 = AsUInt64();
                int biased_e =
                    (int)((d64 & kExponentMask) >> kPhysicalSignificandSize);
                return biased_e - kExponentBias;
            }
        }

        public ulong Significand
        {
            get
            {
                ulong d64 = AsUInt64();
                ulong significand = d64 & kSignificandMask;
                if (!IsDenormal)
                {
                    return significand + kHiddenBit;
                }
                else
                {
                    return significand;
                }
            }
        }

        // Returns true if the double is a denormal.
        public bool IsDenormal
        {
            get
            {
                ulong d64 = AsUInt64();
                return (d64 & kExponentMask) == 0;
            }
        }

        // We consider denormals not to be special.
        // Hence only Infinity and NaN are special.
        public bool IsSpecial
        {
            get
            {
                ulong d64 = AsUInt64();
                return (d64 & kExponentMask) == kExponentMask;
            }
        }

        public bool IsNan
        {
            get
            {
                ulong d64 = AsUInt64();
                return ((d64 & kExponentMask) == kExponentMask) &&
                ((d64 & kSignificandMask) != 0);
            }
        }

        public bool IsInfinite
        {
            get
            {
                ulong d64 = AsUInt64();
                return ((d64 & kExponentMask) == kExponentMask) &&
                    ((d64 & kSignificandMask) == 0);
            }
        }

        public int Sign
        {
            get
            {
                ulong d64 = AsUInt64();
                return (d64 & kSignMask) == 0 ? 1 : -1;
            }
        }

        // Precondition: the value encoded by this Double must be greater or equal
        // than +0.0.
        public DiyFp UpperBoundary()
        {
            Debug.Assert(Sign > 0);
            return new DiyFp(Significand * 2 + 1, Exponent - 1);
        }

        // Computes the two boundaries of this.
        // The bigger boundary (m_plus) is normalized. The lower boundary has the same
        // exponent as m_plus.
        // Precondition: the value encoded by this Double must be greater than 0.
        public void NormalizedBoundaries(out DiyFp out_m_minus, out DiyFp out_m_plus)
        {
            Debug.Assert(Value > 0.0);
            DiyFp v = AsDiyFp();
            bool significand_is_zero = (v.F == kHiddenBit);
            DiyFp temp = new DiyFp((v.F << 1) + 1, v.E - 1);
            DiyFp m_plus = DiyFp.Normalize(ref temp);
            DiyFp m_minus;
            if (significand_is_zero && v.E != kDenormalExponent)
            {
                // The boundary is closer. Think of v = 1000e10 and v- = 9999e9.
                // Then the boundary (== (v - v-)/2) is not just at a distance of 1e9 but
                // at a distance of 1e8.
                // The only exception is for the smallest normal: the largest denormal is
                // at the same distance as its successor.
                // Note: denormals have the same exponent as the smallest normals.
                m_minus = new DiyFp((v.F << 2) - 1, v.E - 2);
            }
            else
            {
                m_minus = new DiyFp((v.F << 1) - 1, v.E - 1);
            }
            m_minus.F = m_minus.F << (m_minus.E - m_plus.E);
            m_minus.E = m_plus.E;
            out_m_plus = m_plus;
            out_m_minus = m_minus;
        }

        public double Value
        {
            get { return value_; }
        }

        // Returns the significand size for a given order of magnitude.
        // If v = f*2^e with 2^p-1 <= f <= 2^p then p+e is v's order of magnitude.
        // This function returns the number of significant binary digits v will have
        // once it's encoded into a double. In almost all cases this is equal to
        // kSignificandSize. The only exceptions are denormals. They start with
        // leading zeroes and their effective significand-size is hence smaller.
        public static int SignificandSizeForOrderOfMagnitude(int order)
        {
            if (order >= (kDenormalExponent + kSignificandSize))
            {
                return kSignificandSize;
            }
            if (order <= kDenormalExponent) return 0;
            return order - kDenormalExponent;
        }

        public static double Infinity
        {
            get
            {
                return double.PositiveInfinity;
            }
        }

        public static double NaN
        {
            get
            {
                return double.NaN;
            }
        }

        private static ulong DiyFpToUInt64(DiyFp diy_fp)
        {
            ulong significand = diy_fp.F;
            int exponent = diy_fp.E;
            while (significand > kHiddenBit + kSignificandMask)
            {
                significand >>= 1;
                exponent++;
            }
            if (exponent >= kMaxExponent)
            {
                return kInfinity;
            }
            if (exponent < kDenormalExponent)
            {
                return 0;
            }
            while (exponent > kDenormalExponent && (significand & kHiddenBit) == 0)
            {
                significand <<= 1;
                exponent--;
            }
            ulong biased_exponent;
            if (exponent == kDenormalExponent && (significand & kHiddenBit) == 0)
            {
                biased_exponent = 0;
            }
            else
            {
                biased_exponent = (ulong)(exponent + kExponentBias);
            }
            return (significand & kSignificandMask) |
                (biased_exponent << kPhysicalSignificandSize);
        }

        private ulong d64_;
        private double value_;
    }
}
