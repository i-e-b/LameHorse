using System;

namespace LameHorse.LAME
{
	public class Lame
	{
		static readonly bool posix;
		static Lame()
		{
			var p = (int)Environment.OSVersion.Platform;
			posix = (p == 4) || (p == 6) || (p == 128);
		}

		public static IntPtr get_lame_version()
		{
			return (posix) ? (LameSoLinux.get_lame_version()) : (LameDllWindows.get_lame_version());
		}

		public static IntPtr lame_init()
		{
			return (posix) ? (LameSoLinux.lame_init()) : (LameDllWindows.lame_init());
		}

		public static void lame_close(IntPtr lame_global_flags)
		{
			if (posix)
				LameSoLinux.lame_close(lame_global_flags);
			else
				LameDllWindows.lame_close(lame_global_flags);
		}

		public static int lame_set_errorf(IntPtr lame_global_flags, LibMp3Lame.LameInfoCallback func)
		{
			return (posix) ? (LameSoLinux.lame_set_errorf(lame_global_flags, func)) : (LameDllWindows.lame_set_errorf(lame_global_flags, func));
		}

		public static int lame_set_debugf(IntPtr lame_global_flags, LibMp3Lame.LameInfoCallback func)
		{
			return (posix) ? (LameSoLinux.lame_set_debugf(lame_global_flags, func))
				: (LameDllWindows.lame_set_debugf(lame_global_flags, func));
		}

		public static int lame_set_msgf(IntPtr lame_global_flags, LibMp3Lame.LameInfoCallback func)
		{
			return (posix) ? (LameSoLinux.lame_set_msgf(lame_global_flags, func))
				: (LameDllWindows.lame_set_msgf(lame_global_flags, func));
		}

		public static int lame_set_brate(IntPtr lame_global_flags, int brate)
		{
			return (posix) ? (LameSoLinux.lame_set_brate(lame_global_flags, brate))
				: (LameDllWindows.lame_set_brate(lame_global_flags, brate));
		}

		public static int lame_set_mode(IntPtr lame_global_flags, LibMp3Lame.MPEG_mode mode)
		{
			return (posix) ? (LameSoLinux.lame_set_mode(lame_global_flags, mode))
				: (LameDllWindows.lame_set_mode(lame_global_flags, mode));
		}

		public static int lame_set_in_samplerate(IntPtr lame_global_flags, int rateInHz)
		{
			return (posix) ? (LameSoLinux.lame_set_in_samplerate(lame_global_flags, rateInHz))
				: (LameDllWindows.lame_set_in_samplerate(lame_global_flags, rateInHz));
		}

		public static int lame_set_num_channels(IntPtr lame_global_flags, int channels)
		{
			return (posix) ? (LameSoLinux.lame_set_num_channels(lame_global_flags, channels))
				: (LameDllWindows.lame_set_num_channels(lame_global_flags, channels));
		}

		public static int lame_set_quality(IntPtr lame_global_flags, int quality)
		{
			return (posix) ? (LameSoLinux.lame_set_quality(lame_global_flags, quality))
				: (LameDllWindows.lame_set_quality(lame_global_flags, quality));
		}

		public static int lame_init_params(IntPtr lame_global_flags)
		{
			return (posix) ? (LameSoLinux.lame_init_params(lame_global_flags))
				: (LameDllWindows.lame_init_params(lame_global_flags));
		}

		public static int lame_get_lametag_frame(IntPtr lame_global_flags, IntPtr buffer, int size)
		{
			return (posix) ? (LameSoLinux.lame_get_lametag_frame(lame_global_flags, buffer, size))
				: (LameDllWindows.lame_get_lametag_frame(lame_global_flags, buffer, size));
		}

		public static int lame_encode_flush(IntPtr lame_global_flags,
												   IntPtr mp3buf,
												   int mp3buf_size)
		{
			return (posix) ? (LameSoLinux.lame_encode_flush(lame_global_flags, mp3buf, mp3buf_size))
				: (LameDllWindows.lame_encode_flush(lame_global_flags, mp3buf, mp3buf_size));
		}

		public static int lame_encode_buffer(IntPtr lame_global_flags,
													short[] buffer_l,
													short[] buffer_r,
													int nsamples,
													IntPtr mp3buf,
													int mp3buf_size)
		{
			return (posix) ? (LameSoLinux.lame_encode_buffer(lame_global_flags, buffer_l, buffer_r, nsamples, mp3buf, mp3buf_size))
				: (LameDllWindows.lame_encode_buffer(lame_global_flags, buffer_l, buffer_r, nsamples, mp3buf, mp3buf_size));
		}
	}
}