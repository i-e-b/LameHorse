namespace WavReader
{
	public interface IPCMAudio
	{
		int Channels { get; }
		int SampleRateHz { get; }
		int BitDepth { get; }
	}
}