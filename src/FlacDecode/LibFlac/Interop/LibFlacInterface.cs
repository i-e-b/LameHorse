using System;
using Interfaces;
using win = FlacDecode.LibFlac.Interop.LibFlacDllWindows;
using nix = FlacDecode.LibFlac.Interop.LibFlacDllWindows;

namespace FlacDecode.LibFlac.Interop
{
	public unsafe class LibFlacInterface
	{
		readonly bool _posix;

		public LibFlacInterface()
		{
			_posix = AddLocalPathForLinuxLibrarySearch.Setup();
		}

		public IntPtr FLAC__stream_decoder_new()
		{
			return (_posix) ?
				nix.FLAC__stream_decoder_new()
				: win.FLAC__stream_decoder_new();
		}

		public void FLAC__stream_decoder_delete(IntPtr decoder)
		{
			if (_posix) nix.FLAC__stream_decoder_delete(decoder);
			else win.FLAC__stream_decoder_delete(decoder);
		}

		public bool FLAC__stream_decoder_finish(IntPtr decoder)
		{
			return (_posix) ?
				nix.FLAC__stream_decoder_finish(decoder)
				: win.FLAC__stream_decoder_finish(decoder);
		}

		public void FLAC__stream_decoder_init_stream(
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
			)
		{
			if (_posix) nix.FLAC__stream_decoder_init_stream(decoder,readCallback,seekCallback,tellCallback,lengthCallback,eofCallback,writeCallback,metadataCallback,errorCallback,clientData);
			else win.FLAC__stream_decoder_init_stream(decoder,readCallback,seekCallback,tellCallback,lengthCallback,eofCallback,writeCallback,metadataCallback,errorCallback,clientData);
		}

		/// <summary>
		/// Initialize the decoder instance to decode native FLAC files
		/// </summary>
		public void FLAC__stream_decoder_init_file(
			IntPtr decoder,
			string filename,
			Callbacks.Write writeCallback, // not null
			Callbacks.Metadata metadataCallback, // can be  null
			Callbacks.Error errorCallback, //not null
			IntPtr clientData)
		{
			if (_posix) nix.FLAC__stream_decoder_init_file(decoder,filename,writeCallback,metadataCallback,errorCallback,clientData);
			else win.FLAC__stream_decoder_init_file(decoder,filename,writeCallback,metadataCallback,errorCallback,clientData);
		}


		/// <summary>
		/// Decode until the end of the stream. This version instructs the decoder to decode from the current position and continue until the end of stream (the read callback returns FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM), or until the callbacks return a fatal error.
		/// &#13;&#13;
		/// As the decoder needs more input it will call the read callback. As each metadata block and frame is decoded, the metadata or write callback will be called with the decoded metadata or frame.
		/// </summary>
		/// <param name="decoder">An initialized decoder instance. Not null.</param>
		public void FLAC__stream_decoder_process_until_end_of_stream(IntPtr decoder)
		{
			if (_posix) nix.FLAC__stream_decoder_process_until_end_of_stream(decoder);
			else win.FLAC__stream_decoder_process_until_end_of_stream(decoder);
		}

	
	}
}
