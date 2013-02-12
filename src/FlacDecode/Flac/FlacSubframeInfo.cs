using System;
using FlacDecode.Audio;
using FlacDecode.MathCoding;

namespace FlacDecode.Flac
{
    unsafe public class FlacSubframeInfo
    {
        public FlacSubframeInfo()
        {
            best = new FlacSubframe();
            lpc_ctx = new LpcContext[LinearPredictiveCoding.MaxLpcWindows];
            for (int i = 0; i < LinearPredictiveCoding.MaxLpcWindows; i++)
                lpc_ctx[i] = new LpcContext();
        }

        public void Init(int* s, int* r, int bps, int w)
        {
            if (w > bps)
                throw new Exception("internal error");
            samples = s;
            obits = bps - w;
            wbits = w;
            best.residual = r;
            best.type = SubframeType.Verbatim;
            best.size = AudioSamples.UINT32_MAX;
            for (int iWindow = 0; iWindow < LinearPredictiveCoding.MaxLpcWindows; iWindow++)
                lpc_ctx[iWindow].Reset();
            done_fixed = 0;
        }

        public FlacSubframe best;
        public int obits;
        public int wbits;
        public int* samples;
        public uint done_fixed;
        public LpcContext[] lpc_ctx;
    };
}
