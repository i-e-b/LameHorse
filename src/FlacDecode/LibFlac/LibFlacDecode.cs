using System;
using System.Runtime.InteropServices;
using System.Threading;
using FlacDecode.LibFlac.Interop;
using dll = FlacDecode.LibFlac.Interop.LibFlacDllWindows;

namespace FlacDecode.LibFlac
{
	public class LibFlacDecode
	{
		readonly string _flacFilePath;
		readonly string _wavFilePath;
		object _swapToken = new Object();
		WavWriter _writer;

		public LibFlacDecode(string flacFilePath, string wavFilePath)
		{
			_flacFilePath = flacFilePath;
			_wavFilePath = wavFilePath;
		}

		public unsafe void DecodeFlacToWav()
		{
			var token = Interlocked.Exchange(ref _swapToken, null);
			if (token == null) throw new Exception("Only one decode can be at once per instance");

			IntPtr streamDecoder;
			try
			{
				streamDecoder = dll.FLAC__stream_decoder_new();
				if (streamDecoder == IntPtr.Zero) throw new OutOfMemoryException("Could not allocate lib flac stream decoder");
			}
			finally
			{
				_swapToken = token;
			}

			Callbacks.Write writeCallback = WriteCallback;
			Callbacks.Error errorCallback = ErrorCallback;
			Callbacks.Metadata metadataCallback = MetadataCallback;
			try
			{
				dll.FLAC__stream_decoder_init_file(streamDecoder, _flacFilePath, writeCallback, metadataCallback, errorCallback, IntPtr.Zero);
				dll.FLAC__stream_decoder_process_until_end_of_stream(streamDecoder);
			}
			finally
			{
				GC.KeepAlive(errorCallback);
				GC.KeepAlive(writeCallback);
				GC.KeepAlive(metadataCallback);
				dll.FLAC__stream_decoder_finish(streamDecoder);
				dll.FLAC__stream_decoder_delete(streamDecoder);
				_swapToken = token;
				if (_writer != null)
				{
					_writer.FlushAndClose();
					_writer.Dispose();
				}
			}
		}

		void MetadataCallback(IntPtr decoder, ref FlacMetadataHeader metadata, IntPtr clientdata)
		{
			if (_writer != null) return;
			if (metadata.Type != FlacMetadataType.FLAC__METADATA_TYPE_STREAMINFO)
			{
				Console.WriteLine("Unsupported metadata. Skipping");
				return;
			}

			var estimatedFileSize = Math.Ceiling(
				(
				metadata.streamInfo.Channels
				* metadata.streamInfo.BitsPerSample
				* metadata.streamInfo.TotalSamples)
					/ 8.0
				);

			_writer = new WavWriter(_wavFilePath, (uint)estimatedFileSize,
				(int)metadata.streamInfo.Channels, (int)metadata.streamInfo.SampleRate);

			//Console.WriteLine("Metadata "+metadata.Type.ToString()+", "+metadata.streamInfo.TotalSamples+ " total samples");
			//Console.WriteLine("expecting total filesize of "+estimatedFileSize+" bytes");
		}

		void ErrorCallback(IntPtr decoder, Callbacks.FlacErrorStatus status, IntPtr clientdata)
		{
			Console.WriteLine("Decoding error");
		}

		unsafe Callbacks.FlacWriteStatus WriteCallback(IntPtr d, FrameHeader* frame, IntPtr buffer, IntPtr c)
		{
			if (_writer == null)
			{
				Console.WriteLine("No metadata recevied before samples sent");
				return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_ABORT;
			}
			if (frame == null) return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE;
			var f = *frame;
			Console.WriteLine("Asked to write " + f.blockSize + " samples of " + f.channels + " channels, " + f.sample_rate + " ss; " + f.bitsPerSample + " bps");

			var bufferSize = (int)((f.bitsPerSample*f.channels*f.blockSize) / 8);
			var buf = new byte[bufferSize];

			Marshal.Copy(buffer, buf, 0, bufferSize);

			_writer.WriteSamples(buf, 0, bufferSize);

			return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE;
		}
	}
}