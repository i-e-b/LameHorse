using System;
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

		public void DecodeFlacToWav(string flacFilePath, string wavFilePath)
		{
			dll.FLAC__stream_decoder_init_file(_streamDecoder, flacFilePath, (d, f, buffer, c) => {
				Console.WriteLine("Asked to write data");

				return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE;
			}, null, (d, error,c) => {
				Console.WriteLine("decoding error");
			}, IntPtr.Zero);

			dll.FLAC__stream_decoder_process_until_end_of_stream(_streamDecoder);
		}

		~LibFlacDecode()
		{
			Dispose();
		}

		public void Dispose()
		{
			var local = Interlocked.Exchange(ref _streamDecoder, IntPtr.Zero);
			if (local == IntPtr.Zero) return;

			dll.FLAC__stream_decoder_delete(local);
		}
	}
}