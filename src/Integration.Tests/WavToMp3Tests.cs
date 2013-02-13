using System.IO;
using Interfaces;
using LameHorse;
using NUnit.Framework;
using WavReader;

namespace Integration.Tests
{
    [TestFixture]
    public class WavToMp3Tests
    {
        [Test]
        public void can_get_wav_headers ()
        {
            using (var fs = new FileStream(@"./res/dtmf.wav", FileMode.Open))
            {
                IPCMAudio subject = new WavFromFile(fs);

                Assert.That(subject.BitDepth, Is.EqualTo(16), "Bit depth wrong");
                Assert.That(subject.Channels, Is.EqualTo(2), "channels wrong");
                Assert.That(subject.SampleRateHz, Is.EqualTo(44100), "sample rate wrong");
            }
        }

        [Test]
        public void Can_write_an_MP3_file ()
        {
            using (var @in = new FileStream(@"./res/dtmf.wav", FileMode.Open))
            using (var @out = new FileStream(@"./res/dtmf_out.mp3", FileMode.Create))
            {
                var reader = new WavFromFile(@in);
                var writer = new Mp3Writer(@out, 256, reader);

                var left = new short[512];
                var right = new short[512];

                int len;
                while ((len = reader.Read(left,right)) > 0)
                {
                    writer.Write(left, right, len);
                }
                writer.Flush();
            }

            Assert.That(File.Exists(@"./res/dtmf_out.mp3"));
        }
    }
}
