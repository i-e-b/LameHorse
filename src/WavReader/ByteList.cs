namespace WavReader
{
	public class ByteList:IByteArray
	{
		readonly byte[] _src;

		public ByteList(byte[] src)
		{
			_src = src;
		}

		public byte this[long index]
		{
			get
			{
				return _src[index];
			}
		}
	}
}