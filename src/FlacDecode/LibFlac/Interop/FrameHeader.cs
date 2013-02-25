using System;
using System.Runtime.InteropServices;

namespace FlacDecode.LibFlac.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	public struct FrameHeader
	{
		public UInt32 blockSize;
		public UInt32 sample_rate;
		public UInt32 channels;
		public FlacChannelAssignment channelAssignment;
		public UInt32 bitsPerSample;
		// more off the end not needed here
	}
}