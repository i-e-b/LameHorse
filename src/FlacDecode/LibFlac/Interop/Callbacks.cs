using System;
using System.Runtime.InteropServices;

namespace FlacDecode.LibFlac.Interop
{
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

		/// <summary>
		/// called when the decoder needs to know if the end of the stream has been reached.
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback.</param>
		/// <param name="clientData">The callee's client data set through FLAC__stream_decoder_init_*()</param>
		/// <returns>true if the currently at the end of the stream, else false.</returns>
		[return: MarshalAs(UnmanagedType.Bool)]
		public delegate bool Eof(IntPtr decoder, IntPtr clientData);

		/// <summary>
		/// called when the decoder wants to know the current position of the stream. The callback should return the byte offset from the beginning of the stream.
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback</param>
		/// <param name="absoluteByteOffset">A pointer to storage for the current offset from the beginning of the stream</param>
		/// <param name="clientData">The callee's client data set through FLAC__stream_decoder_init_*()</param>
		public delegate FlacTellStatus Tell(IntPtr decoder, UInt64* absoluteByteOffset, IntPtr clientData);

		/// <summary>
		/// Called when the decoder has decoded a single audio frame. The decoder will pass the frame metadata as well as an array of pointers (one for each channel) pointing to the decoded audio.
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback.</param>
		/// <param name="frame">The description of the decoded frame</param>
		/// <param name="buffer">An array of pointers to decoded channels of data. Each pointer will point to an array of signed samples of length frame->header.blocksize. Channels will be ordered according to the FLAC specification</param>
		/// <param name="clientData">The callee's client data set through FLAC__stream_decoder_init_*().</param>
		public delegate FlacWriteStatus Write(IntPtr decoder, FrameHeader *frame, IntPtr buffer, IntPtr clientData);

		/// <summary>
		/// called when the decoder has decoded a metadata block. In a valid FLAC file there will always be one STREAMINFO block, followed by zero or more other metadata blocks. These will be supplied by the decoder in the same order as they appear in the stream and always before the first audio frame (i.e. write callback). The metadata block that is passed in must not be modified, and it doesn't live beyond the callback, so you should make a copy of it with FLAC__metadata_object_clone() if you will need it elsewhere. Since metadata blocks can potentially be large, by default the decoder only calls the metadata callback for the STREAMINFO block; you can instruct the decoder to pass or filter other blocks with FLAC__stream_decoder_set_metadata_*() calls.
		/// </summary>
		/// <param name="decoder">The decoder instance calling the callback. In general, FLAC__StreamDecoder functions which change the state should not be called on the decoder while in the callback.</param>
		/// <param name="metadata">The decoded metadata block</param>
		/// <param name="clientData">The callee's client data set through FLAC__stream_decoder_init_*().</param>
		public delegate void Metadata(IntPtr decoder, ref FlacMetadataHeader metadata, IntPtr clientData);

		public delegate void Error(IntPtr decoder, FlacErrorStatus status, IntPtr clientData);

		public enum FlacErrorStatus
		{
			FLAC__STREAM_DECODER_ERROR_STATUS_LOST_SYNC,
			FLAC__STREAM_DECODER_ERROR_STATUS_BAD_HEADER,
			FLAC__STREAM_DECODER_ERROR_STATUS_FRAME_CRC_MISMATCH,
			FLAC__STREAM_DECODER_ERROR_STATUS_UNPARSEABLE_STREAM
		}

		public enum FlacTellStatus
		{
			FLAC__STREAM_DECODER_TELL_STATUS_OK,
			FLAC__STREAM_DECODER_TELL_STATUS_ERROR,
			FLAC__STREAM_DECODER_TELL_STATUS_UNSUPPORTED
		}

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

		public enum FlacWriteStatus
		{
			FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE,
			FLAC__STREAM_DECODER_WRITE_STATUS_ABORT
		}
	}
}