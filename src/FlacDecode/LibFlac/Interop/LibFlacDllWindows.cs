using System;
using System.Runtime.InteropServices;

namespace FlacDecode.LibFlac.Interop
{
	//http://flac.sourceforge.net/api/group__flac__stream__decoder.html
	public class LibFlacDllWindows
	{
		const string LibFlac = "libFLAC_x64.dll";

		[DllImport(LibFlac, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FLAC__stream_decoder_new();

		[DllImport(LibFlac, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FLAC__stream_decoder_delete(IntPtr decoder);

		[DllImport(LibFlac, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FLAC__stream_decoder_finish(IntPtr decoder);


		[DllImport(LibFlac, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FLAC__stream_decoder_init_stream(
			IntPtr decoder,
			Callbacks.Read readCallback, // not null
			Callbacks.Seek seekCallback, // can be null
			Callbacks.Tell tellCallback, // can be null
			Callbacks.Length lengthCallback, // can be null
			Callbacks.Eof eofCallback, // can be null
			Callbacks.Write writeCallback, // not null
			Callbacks.Metadata metadataCallback, // can be  null
			Callbacks.Error errorCallback, //not null
			IntPtr clientData
			);

		/// <summary>
		/// Initialize the decoder instance to decode native FLAC files
		/// </summary>
		[DllImport(LibFlac, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FLAC__stream_decoder_init_file(
			IntPtr decoder,
			string filename,
			Callbacks.Write writeCallback, // not null
			Callbacks.Metadata metadataCallback, // can be  null
			Callbacks.Error errorCallback, //not null
			IntPtr clientData);

		/// <summary>
		/// Decode until the end of the stream. This version instructs the decoder to decode from the current position and continue until the end of stream (the read callback returns FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM), or until the callbacks return a fatal error.
		/// &#13;&#13;
		/// As the decoder needs more input it will call the read callback. As each metadata block and frame is decoded, the metadata or write callback will be called with the decoded metadata or frame.
		/// </summary>
		/// <param name="decoder">An initialized decoder instance. Not null.</param>
		[DllImport(LibFlac, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FLAC__stream_decoder_process_until_end_of_stream(IntPtr decoder);
	}
}