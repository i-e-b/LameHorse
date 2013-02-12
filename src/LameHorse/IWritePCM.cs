namespace LameHorse
{
	public interface IWritePCM
	{
        void Write(short[] leftSamples, short[] rightSamples, int length);
        void Flush();
	}
}
