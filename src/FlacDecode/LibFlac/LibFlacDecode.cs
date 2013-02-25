using System;
using System.Runtime.InteropServices;
using System.Threading;
using FlacDecode.LibFlac.Interop;

namespace FlacDecode.LibFlac
{
	public unsafe class LibFlacDecode
	{
		readonly string _flacFilePath;
		readonly string _wavFilePath;
		object _swapToken = new Object();
		WavWriter _writer;
		readonly LibFlacInterface _libFlac;

		public LibFlacDecode(string flacFilePath, string wavFilePath)
		{
			_flacFilePath = flacFilePath;
			_wavFilePath = wavFilePath;
			_libFlac = new LibFlacInterface();
		}

		public void DecodeFlacToWav()
		{
			var token = Interlocked.Exchange(ref _swapToken, null);
			if (token == null) throw new Exception("Only one decode can be at once per instance");

			IntPtr streamDecoder;
			try
			{
				streamDecoder = _libFlac.FLAC__stream_decoder_new();
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
				_libFlac.FLAC__stream_decoder_init_file(streamDecoder, _flacFilePath, writeCallback, metadataCallback, errorCallback, IntPtr.Zero);
				_libFlac.FLAC__stream_decoder_process_until_end_of_stream(streamDecoder);
			}
			finally
			{
				GC.KeepAlive(errorCallback);
				GC.KeepAlive(writeCallback);
				GC.KeepAlive(metadataCallback);
				_libFlac.FLAC__stream_decoder_finish(streamDecoder);
				_libFlac.FLAC__stream_decoder_delete(streamDecoder);
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
		}

		static void ErrorCallback(IntPtr decoder, Callbacks.FlacErrorStatus status, IntPtr clientdata)
		{
			throw new Exception("Decoding error " + status.ToString());
		}

		Callbacks.FlacWriteStatus WriteCallback(IntPtr d, FrameHeader* frame, IntPtr buffer, IntPtr c)
		{
			if (_writer == null)
			{
				Console.WriteLine("No metadata recevied before samples sent");
				return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_ABORT;
			}
			if (frame == null) return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE;
			var f = *frame;


			if (f.channels == 1)
			{
				var sampleCount = (int)(f.blockSize);

				var channel_A_Ptr = (IntPtr)Marshal.PtrToStructure(buffer, typeof(IntPtr));
				var channel_A = new int[sampleCount];
				Marshal.Copy(channel_A_Ptr, channel_A, 0, sampleCount);

				for (int i = 0; i < sampleCount; i++)
				{
					_writer.WriteSample((short)channel_A[i]);
				}
			}
			else if (f.channels > 2) throw new Exception("Wav does not support more than 2 channels");
			else
			{
				var sampleCount = (int)(f.blockSize);

				var channel_A_Ptr = (IntPtr)Marshal.PtrToStructure(buffer, typeof(IntPtr));
				var channel_A = new int[sampleCount];
				Marshal.Copy(channel_A_Ptr, channel_A, 0, sampleCount);

				var channel_B_Ptr = (IntPtr)Marshal.PtrToStructure(buffer + IntPtr.Size, typeof(IntPtr));
				var channel_B = new int[sampleCount];
				Marshal.Copy(channel_B_Ptr, channel_B, 0, sampleCount);

				for (int i = 0; i < sampleCount; i++)
				{
					var A = channel_A[i];
					var B = channel_B[i];

					_writer.WriteSample((short)A);
					_writer.WriteSample((short)B);
				}
			}

			return Callbacks.FlacWriteStatus.FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE;
		}
	}
}