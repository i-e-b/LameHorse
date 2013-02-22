using System.IO;
using FileTranscoder;
using NUnit.Framework;

namespace Integration.Tests
{
 [TestFixture]
    public class FlacToWavTests
    {
        [Test]
        public void can_transcode_to_wav ()
        {
			string @in = @"./res/dtmf.flac";
            string @out = @"./res/dtmf_out_flake.wav";
			
			File.Delete(@out);

            Decode.FlacToWav(@in, @out);

            Assert.That(File.Exists(@out));
			Assert.That(new FileInfo(@out).Length, Is.EqualTo(882044));
        }

     
        [Test]
        public void can_transcode_to_wav_using_lib_flac ()
        {
			string @in = @"./res/dtmf_R.flac";
            string @out = @"./res/dtmf_out_libflac.wav";
			
			File.Delete(@out);

            Decode.FlacToWav_ForceLibFlac(@in, @out);

            Assert.That(File.Exists(@out));
			Assert.That(new FileInfo(@out).Length, Is.EqualTo(882044));
        }
    }
}
