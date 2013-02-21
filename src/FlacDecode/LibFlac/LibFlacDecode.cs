using System;
using System.Threading;
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