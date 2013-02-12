using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Integration.Tests
{
    [TestFixture]
    public class SimpleIntegrationTests
    {

        [Test]
        public void can_get_wav_headers ()
        {
            using (var fs = new FileStream(".\\res\\dtmf.wav", FileMode.Open))
            {
                var subject = new WavReader.WavReader(fs);

                Assert.That(subject.BitDepth, Is.EqualTo(16), "Bit depth wrong");
                Assert.That(subject.Channels, Is.EqualTo(2), "channels wrong");
                Assert.That(subject.SampleRateHz, Is.EqualTo(44100), "sample rate wrong");
            }
        }
    }
}
