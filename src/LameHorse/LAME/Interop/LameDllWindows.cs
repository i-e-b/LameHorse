using System;
using System.Runtime.InteropServices;

namespace LameHorse.LAME.Interop
{
	public class LameDllWindows
	{
		// const char* CDECL get_lame_version(void);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr get_lame_version();

		// lame_global_flags * CDECL lame_init(void);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lame_init();

		// void lame_close(lame_global_flags *);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void lame_close(IntPtr lame_global_flags);

		/*
		 * OPTIONAL:
		 * Set printf like error/debug/message reporting functions.
		 * The second argument has to be a pointer to a function which looks like
		 *   void my_debugf(const char *format, va_list ap)
		 *   {
		 *       (void) vfprintf(stdout, format, ap);
		 *   }
		 * If you use NULL as the value of the pointer in the set function, the
		 * lame buildin function will be used (prints to stderr).
		 * To quiet any output you have to replace the body of the example function
		 * with just "return;" and use it in the set function.
		 */

		// int CDECL lame_set_errorf(lame_global_flags *, lame_report_function);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_set_errorf(IntPtr lame_global_flags, LibMp3Lame.LameInfoCallback func);

		// int CDECL lame_set_debugf(lame_global_flags *, lame_report_function);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_set_debugf(IntPtr lame_global_flags, LibMp3Lame.LameInfoCallback func);

		// int CDECL lame_set_msgf  (lame_global_flags *, lame_report_function);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_set_msgf(IntPtr lame_global_flags, LibMp3Lame.LameInfoCallback func);

		// int CDECL lame_set_brate(lame_global_flags *, int);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_set_brate(IntPtr lame_global_flags, int brate);

		// int CDECL lame_set_mode(lame_global_flags *, MPEG_mode);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_set_mode(IntPtr lame_global_flags, LibMp3Lame.MPEG_mode mode);

		// int CDECL lame_set_in_samplerate(lame_global_flags *, int);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_set_in_samplerate(IntPtr lame_global_flags, int rateInHz);

		// int CDECL lame_set_num_channels(lame_global_flags *, int); 
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_set_num_channels(IntPtr lame_global_flags, int channels);

		// int CDECL lame_set_quality(lame_global_flags *, int);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_set_quality(IntPtr lame_global_flags, int quality);

		// int CDECL lame_init_params(lame_global_flags *);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_init_params(IntPtr lame_global_flags);

		/*
		 * input pcm data, output (maybe) mp3 frames.
		 * This routine handles all buffering, resampling and filtering for you.
		 *
		 * return code     number of bytes output in mp3buf. Can be 0
		 *                 -1:  mp3buf was too small
		 *                 -2:  malloc() problem
		 *                 -3:  lame_init_params() not called
		 *                 -4:  psycho acoustic problems                                                                                                    
		 *
		 * The required mp3buf_size can be computed from num_samples,
		 * samplerate and encoding rate, but here is a worst case estimate:
		 *
		 * mp3buf_size in bytes = 1.25*num_samples + 7200
		 *
		 * I think a tighter bound could be:  (mt, March 2000)
		 * MPEG1:
		 *    num_samples*(bitrate/8)/samplerate + 4*1152*(bitrate/8)/samplerate + 512
		 * MPEG2:
		 *    num_samples*(bitrate/8)/samplerate + 4*576*(bitrate/8)/samplerate + 256
		 *
		 * but test first if you use that!
		 *
		 * set mp3buf_size = 0 and LAME will not check if mp3buf_size is
		 * large enough.
		 *
		 * NOTE:
		 * if gfp->num_channels=2, but gfp->mode = 3 (mono), the L & R channels
		 * will be averaged into the L channel before encoding only the L channel
		 * This will overwrite the data in buffer_l[] and buffer_r[].
		 *
		 */
		// int CDECL lame_encode_buffer (                                                                                                                      
		//     lame_global_flags*  gfp,           /* global context handle                 */
		//     const short int     buffer_l [],   /* PCM data for left channel             */
		//     const short int     buffer_r [],   /* PCM data for right channel            */
		//     const int           nsamples,      /* number of samples per channel         */
		//     unsigned char*      mp3buf,        /* pointer to encoded MP3 stream         */
		//     const int           mp3buf_size ); /* number of valid octets in this stream */
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_encode_buffer(IntPtr lame_global_flags,
		                                            short[] buffer_l,
		                                            short[] buffer_r,
		                                            int nsamples,
		                                            IntPtr mp3buf,
		                                            int mp3buf_size);

		// int CDECL lame_encode_flush(
		//     lame_global_flags *  gfp,    /* global context handle                 */
		//     unsigned char*       mp3buf, /* pointer to encoded MP3 stream         */
		//     int                  size);  /* number of valid octets in this stream */
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_encode_flush(IntPtr lame_global_flags,
		                                           IntPtr mp3buf,
		                                           int mp3buf_size);

		// size_t CDECL lame_get_lametag_frame(
		//     const lame_global_flags *, unsigned char* buffer, size_t size);
		[DllImport("libmp3lame.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int lame_get_lametag_frame(IntPtr lame_global_flags, IntPtr buffer, int size);
	}
}