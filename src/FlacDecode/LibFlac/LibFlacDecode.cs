using System;
using FlacDecode.LibFlac.Interop;
using dll = FlacDecode.LibFlac.Interop.LibFlacDllWindows;

namespace FlacDecode.LibFlac
{
	public class LibFlacDecode
	{

		public unsafe void DecodeFlacToWav(string flacFilePath, string wavFilePath)
		{
			var streamDecoder = dll.FLAC__stream_decoder_new();
			if (streamDecoder == IntPtr.Zero) throw new OutOfMemoryException("Could not allocate lib flac stream decoder");

			Callbacks.Write writeCallback = WriteCallback;
			Callbacks.Error errorCallback = ErrorCallback;
			try
			{
				dll.FLAC__stream_decoder_init_file(streamDecoder, flacFilePath, writeCallback, null, errorCallback, IntPtr.Zero);
				dll.FLAC__stream_decoder_process_until_end_of_stream(streamDecoder);
			}
			finally
			{
				GC.KeepAlive(errorCallback);
				GC.KeepAlive(writeCallback);
				dll.FLAC__stream_decoder_finish(streamDecoder);
				dll.FLAC__stream_decoder_delete(streamDecoder);
			}
		}

		void ErrorCallback(IntPtr decoder, Callbacks.FlacErrorStatus status, IntPtr clientdata)
		{
			Console.WriteLine("Decoding error");
		}

		static unsafe Callbacks.FlacWriteStatus WriteCallback(IntPtr d, FrameHeader* frame, Int32** buffer, IntPtr c)
		{
			if (frame == null) return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE;
			var f = *frame;
			Console.WriteLine("Asked to write " + f.blockSize + " samples of " + f.channels + " channels, " + f.sample_rate + " ss; " + f.bitsPerSample + " bps");



			return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE;
		}
	}
}