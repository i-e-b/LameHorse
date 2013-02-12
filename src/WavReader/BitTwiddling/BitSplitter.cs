using System;
using System.IO;

namespace WavReader.BitTwiddling
{
	/// <summary>
	/// Splits a byte array into values, based on a series of bit lengths
	/// Translations are done from network byte order.
	/// </summary>
	public class BitSplitter
	{
		protected IByteArray src;
		protected long offset; // offset number of BITS.

		public BitSplitter(byte[] rawData)
		{
			offset = 0L;
			src = new ByteList(rawData);
		}

        public BitSplitter(FileStream fsData)
        {
            offset = 0L;
            src = new FsBytes(fsData);
        }

		/// <summary>Byte number to start</summary>
		public int ByteOffset { get { return (int)(offset / 8L); } }

		/// <summary>Bit number to start inside current byte</summary>
		public int BitOffset { get { return (int)(offset % 8L); } }

		/// <summary>
		/// Create a byte mask
		/// </summary>
		/// <param name="start">number of bits skipped</param>
		/// <param name="length">run length (bits)</param>
		protected byte[] Mask(int start, int length)
		{
			if (length < 1) return new byte[] { 0x00 };

			var Bcnt = (int)Math.Ceiling((start + length) / 8.0);
			var output = new byte[Bcnt];
			byte val = 0x80; // top bit;
			int bcnt = start + length;

			for (int i = 0; i < bcnt; i++)
			{
				int B = i / 8;
				int b = i % 8;

				if (b == 0) val = 0x80;
				else val >>= 1;

				if (i >= start) output[B] |= val;
			}
			return output;
		}

		/// <summary>
		/// Get an unshifted array of bits packed in bytes.
		/// Advances position counter.
		/// </summary>
		public byte[] GetNext(int l)
		{
			byte[] m = Mask(BitOffset, l);
			for (int i = 0; i < m.Length; i++)
			{
				m[i] = (byte)(m[i] & src[ByteOffset + i]);
			}
			offset += l;
			return m;
		}

		/// <summary>
		/// Move forward to next byte edge (moves between 1 and 8 bits)
		/// </summary>
		public void SkipToNextByte()
		{
			offset += 8 - BitOffset;
		}

		/// <summary>
		/// Move forward a given number of bits.
		/// They are consumed but not processed.
		/// </summary>
		public void SkipBits(int BitCount)
		{
			offset += BitCount;
		}

		/// <summary>
		/// Move forward a given number of bytes.
		/// They are consumed but not processed.
		/// </summary>
		public void SkipBytes(int ByteCount)
		{
			offset += ByteCount * 8;
		}

		/// <summary>
		/// Read a single bit, and return true if set to '1'.
		/// Advances position by 1 bit
		/// </summary>
		public bool GetFlag()
		{
			byte[] v = GetNext(1);
			if (v.Length != 1) throw new Exception("Unexpected bytes!");
			return v[0] != 0;
		}

		/// <summary>
		/// Move to the next byte edge (no action if already on one -- moves between 0 and 7 bits)
		/// </summary>
		public void AlignToByte()
		{
			offset += 8 - BitOffset;
		}

		/// <summary>
		/// Return an unsigned long for the integer bits (Network order)
		/// </summary>
		public ulong GetInteger(int BitLength)
		{
			ulong outval = 0L;
			int rshift;

			byte[] v = GetNext(BitLength);
			if (BitOffset == 0)
			{ // landed on a byte-boundary
				rshift = 0;
			}
			else
			{
				rshift = 8 - BitOffset;
			}
			int lshift = 8 - rshift;


			// This mess shifts the bits of a byte array by up to 8 places
			// and reverses byte order, joining the result into a ulong.
			int psh = 0;
			for (int i = v.Length - 1; i >= 0; i--)
			{
				int n0 = v[i] >> rshift;
				int n1 = (i > 0) ? (v[i - 1] << lshift) : (0);

				outval += (ulong)(((n0 + n1) & 0xFF) << psh);
				psh += 8;
			}

			return outval;
		}

		/// <summary>
		/// Return an unsigned long for the integer bits (Intel order)
		/// </summary>
		public ulong GetIntegerIntel(int BitLength)
		{
			ulong outval = 0L;
			int rshift;

			byte[] v = GetNext(BitLength);
			if (BitOffset == 0)
			{ // landed on a byte-boundary
				rshift = 0;
			}
			else
			{
				rshift = 8 - BitOffset;
			}
			int lshift = 8 - rshift;

			int psh = 0;
			for (int i = 0; i < v.Length; i++)
			{ // reverse of above
				int n0 = v[i] >> rshift;
				int n1 = (i > 0) ? (v[i - 1] << lshift) : (0);

				outval += (ulong)(((n0 + n1) & 0xFF) << psh);
				psh += 8;
			}

			return outval;
		}

        /// <summary>
        /// Return true f the following byte string is at the current position.
        /// Position is advanced if ok, not advanced if fail
        /// </summary>
		public bool CurrentIs(byte[] bytes)
		{
            var off = offset;
	        foreach (var b in bytes)
	        {
                if (GetInteger(8) != b)
                {
                    offset = off;
                    return false;
                }

	        }
            return true;
		}
	}
}
