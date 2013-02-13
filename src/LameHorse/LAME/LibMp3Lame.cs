using System;
using System.Runtime.InteropServices;

namespace LameHorse.LAME
{
	/// <remarks>
    /// Originally from http://sixfeetsix.blogspot.co.uk/2012/04/managed-code-to-interact-with.html
    /// </remarks>
	public class LibMp3Lame : IDisposable
	{
		/* MPEG modes */
		public enum MPEG_mode
		{
			STEREO = 0,
			JOINT_STEREO,
			DUAL_CHANNEL,   /* LAME doesn't supports this! */
			MONO,
			NOT_SET,
			MAX_INDICATOR   /* Don't use this! It's used for sanity checks. */
		}

		public delegate void LameInfoCallback(string format, params object[] args);
		IntPtr lame_global_flags;

		public static string GetLameVersion()
		{
			return Marshal.PtrToStringAnsi(Lame.get_lame_version());
		}

		public void LameInit()
		{
			IntPtr ret = Lame.lame_init();
			if (ret == default(IntPtr))
				throw new LibMp3LameException(
					"Unable to create the lame struct possibly due to no memory being left");
			lame_global_flags = ret;
		}

		public void LameSetErrorFunction(LameInfoCallback callback)
		{
			int ret = Lame.lame_set_errorf(lame_global_flags, callback);
			if (ret < 0)
				throw new LibMp3LameException("lame_set_errorf returned an error");
		}

		public void LameSetDebugFunction(LameInfoCallback callback)
		{
			if (Lame.lame_set_debugf(lame_global_flags, callback) < 0)
				throw new LibMp3LameException("lame_set_errorf returned an error");
		}

		public void LameSetMessageFunction(LameInfoCallback callback)
		{
			if (Lame.lame_set_msgf(lame_global_flags, callback) < 0)
				throw new LibMp3LameException("lame_set_errorf returned an error");
		}

		public void LameSetBRate(int brate)
		{
			if (Lame.lame_set_brate(lame_global_flags, brate) != 0)
				throw new LibMp3LameException("lame_set_brate returned an error");
		}

		public void LameSetMode(MPEG_mode mode)
		{
			if (Lame.lame_set_mode(lame_global_flags, mode) != 0)
				throw new LibMp3LameException("lame_set_mode returned an error");
		}

		public void LameSetInSampleRate(int rateInHz)
		{
			if (Lame.lame_set_in_samplerate(lame_global_flags, rateInHz) != 0)
				throw new LibMp3LameException("lame_set_in_samplerate returned an error");
		}

		public void LameSetNumChannels(int numChannels)
		{
			if (Lame.lame_set_num_channels(lame_global_flags, numChannels) != 0)
				throw new LibMp3LameException("lame_set_num_channels returned an error");
		}

		public void LameSetQuality(int quality)
		{
			if (Lame.lame_set_quality(lame_global_flags, quality) != 0)
				throw new LibMp3LameException("lame_set_quality returned an error");
		}

		public void LameInitParams()
		{
			if (Lame.lame_init_params(lame_global_flags) < 0)
				throw new LibMp3LameException("lame_init_params returned an error");
		}

		public int LameEncodeBuffer(short[] bufferL, short[] bufferR,
									int nsamples, byte[] mp3Buffer)
		{
			GCHandle pinnedArray = GCHandle.Alloc(mp3Buffer, GCHandleType.Pinned);
			IntPtr p = pinnedArray.AddrOfPinnedObject();
			int ret = Lame.lame_encode_buffer(lame_global_flags, bufferL, bufferR,
				nsamples, p, mp3Buffer.Length);
			pinnedArray.Free();
			if (ret < 0)
				throw new LibMp3LameException("lame_encode_buffer returned an error (" + ret + ")");
			return ret;
		}

		public int LameEncodeFlush(byte[] mp3Buffer)
		{
			GCHandle pinnedArray = GCHandle.Alloc(mp3Buffer, GCHandleType.Pinned);
			IntPtr p = pinnedArray.AddrOfPinnedObject();
			int ret = Lame.lame_encode_flush(lame_global_flags, p, mp3Buffer.Length);
			pinnedArray.Free();
			if (ret < 0)
				throw new LibMp3LameException("lame_encode_flush returned an error (" + ret + ")");
			return ret;
		}

		public int LameGetLameTagFrame(byte[] buffer)
		{
			GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IntPtr p = pinnedArray.AddrOfPinnedObject();
			int ret = Lame.lame_get_lametag_frame(lame_global_flags, p, buffer.Length);
			pinnedArray.Free();
			if (ret < 0)
				throw new LibMp3LameException("lame_encode_flush returned an error (" + ret + ")");
			if (ret > buffer.Length)
				throw new LibMp3LameException("lame_encode_flush failed due to buffer being to small"
									+ " as it should have been at least " + ret
									+ " bytes rather than " + buffer.Length);
			return ret;
		}

		public void LameClose()
		{
			Lame.lame_close(lame_global_flags);
			lame_global_flags = default(IntPtr);
		}

		public void Dispose()
		{
			if (lame_global_flags != default(IntPtr))
				LameClose();
		}
	}
}
