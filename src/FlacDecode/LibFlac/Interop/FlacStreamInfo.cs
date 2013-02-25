using System;
using System.Runtime.InteropServices;

namespace FlacDecode.LibFlac.Interop
{
	[StructLayout(LayoutKind.Sequential)]
	public struct FlacStreamInfo
	{
		public UInt32 MinBlockSize;
		public UInt32 MaxBlockSize;
		public UInt32 MinFrameSize;
		public UInt32 MaxFrameSize;

		public UInt32 SampleRate;
		public UInt32 Channels;
		public UInt32 BitsPerSample;

		public UInt64 TotalSamples;
		//[MarshalAs(UnmanagedType.ByValArray, SizeConst=16)] public byte *md5Sum;
	}
}