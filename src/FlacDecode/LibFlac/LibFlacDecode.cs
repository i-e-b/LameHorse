using System;
using System.Runtime.InteropServices;
using System.Threading;
using FlacDecode.LibFlac.Interop;
using dll = FlacDecode.LibFlac.Interop.LibFlacDllWindows;

namespace FlacDecode.LibFlac
{
	public class LibFlacDecode : IDisposable
	{
		IntPtr _streamDecoder;

		public LibFlacDecode()
		{
			_streamDecoder = dll.FLAC__stream_decoder_new();
			if (_streamDecoder == IntPtr.Zero) throw new OutOfMemoryException("Could not allocate lib flac stream decoder");
		}

		public unsafe void DecodeFlacToWav(string flacFilePath, string wavFilePath)
		{
			dll.FLAC__stream_decoder_init_file(_streamDecoder, flacFilePath, WriteCallback, null, (d, error,c) => {
				throw new Exception("decoding error");
			}, IntPtr.Zero);

			dll.FLAC__stream_decoder_process_until_end_of_stream(_streamDecoder);
		}

		static unsafe Callbacks.FlacWriteStatus WriteCallback(IntPtr d, FrameHeader *frame, Int32** buffer, IntPtr c)
		{
			FrameHeader f = *frame;
			Console.WriteLine("Asked to write "+f.blockSize+" samples of "+f.channels+" channels, "+f.sample_rate+" ss; "+f.bitsPerSample+" bps");


			return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE;
		}

		~LibFlacDecode()
		{
			Dispose();
		}

		public void Dispose()
		{
			var local = Interlocked.Exchange(ref _streamDecoder, IntPtr.Zero);
			if (local == IntPtr.Zero) return;

			dll.FLAC__stream_decoder_finish(local);
			dll.FLAC__stream_decoder_delete(local);
		}
	}
}