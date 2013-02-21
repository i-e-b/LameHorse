using System;
using System.Runtime.InteropServices;

namespace FlacDecode.LibFlac.Interop
{
	//http://flac.sourceforge.net/api/group__flac__stream__decoder.html
	public class LibFlacDllWindows
	{
		const string LibFlac = "libFLAC_x64.dll";


		/*
		// const char* CDECL get_lame_version(void);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr get_lame_version();*/
		//  [return: MarshalAs(UnmanagedType.Bool)]

		[DllImport(LibFlac, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FLAC__stream_decoder_new();

		[DllImport(LibFlac, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FLAC__stream_decoder_delete(IntPtr decoder);


		[DllImport(LibFlac, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FLAC__stream_decoder_init_stream(
			IntPtr decoder,
			Callbacks.Read readCallback,
			Callbacks.Seek seekCallback,
			Callbacks.NotImplemented tellCallback, // pass null,
			Callbacks.Length lengthCallback
			);
	}

	public unsafe class Callbacks
	{
		/// <summary>
		/// Called when the decoder needs more input data. The address of the buffer to be filled is supplied, along with the number of bytes the buffer can hold. The callback may choose to supply less data and modify the byte count but must be careful not to overflow the buffer. The callback then returns a status code chosen from FLAC__StreamDecoderReadStatus.
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="buffer">A pointer to a location for the callee to store data to be decoded</param>
		/// <param name="length">A pointer to the size of the buffer. On entry to the callback, it contains the maximum number of bytes that may be stored in buffer. The callee must set it to the actual number of bytes stored (0 in case of error or end-of-stream) before returning</param>
		/// <param name="clientData">The callee's client data set through FLAC__stream_decoder_init_*()</param>
		/// <returns>The callee's return status. Note that the callback should return FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM if and only if zero bytes were read and there is no more data to be read.</returns>
		public delegate FlacReadStatus Read(IntPtr decoder, byte[] buffer, int* length, IntPtr clientData);

		/// <summary>
		/// Called when the decoder needs to seek the input stream. The decoder will pass the absolute byte offset to seek to, 0 meaning the beginning of the stream.
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="newOffset">The offset from the beginning of the stream to seek to</param>
		/// <param name="clientData">The callee's client data set through FLAC__stream_decoder_init_*().</param>
		public delegate FlacSeekStatus Seek(IntPtr decoder, UInt64 newOffset, IntPtr clientData);

		/// <summary>
		/// Called when the decoder wants to know the total length of the stream in bytes.
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="length">A pointer to storage for the length of the stream in bytes</param>
		/// <param name="clientData">The callee's client data set through FLAC__stream_decoder_init_*().</param>
		public delegate FlacLengthStatus Length(IntPtr decoder, UInt64* length, IntPtr clientData);


		public delegate void NotImplemented();

		// Not yet implemented callback

		public enum FlacReadStatus
		{
			FLAC__STREAM_DECODER_READ_STATUS_CONTINUE,
			FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM,
			FLAC__STREAM_DECODER_READ_STATUS_ABORT
		}

		public enum FlacSeekStatus
		{
			FLAC__STREAM_DECODER_SEEK_STATUS_OK,
			FLAC__STREAM_DECODER_SEEK_STATUS_ERROR,
			FLAC__STREAM_DECODER_SEEK_STATUS_UNSUPPORTED
		}

		public enum FlacLengthStatus
		{
			FLAC__STREAM_DECODER_LENGTH_STATUS_OK,
			FLAC__STREAM_DECODER_LENGTH_STATUS_ERROR,
			FLAC__STREAM_DECODER_LENGTH_STATUS_UNSUPPORTED
		}

	}

}