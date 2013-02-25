using System;
using System.Runtime.InteropServices;

namespace FlacDecode.LibFlac.Interop
{
	public struct FlacMetadataHeader
	{
		/// <summary>
		/// Only type of Stream Info is supported!
		/// </summary>
		public FlacMetadataType Type;
		[MarshalAs(UnmanagedType.Bool)] public bool isLast;
		public UInt32 length;
		public FlacStreamInfo streamInfo;
	}
}