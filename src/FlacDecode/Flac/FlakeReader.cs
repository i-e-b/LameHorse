/**
 * CUETools.Flake: pure managed FLAC audio encoder
 * Copyright (c) 2009 Grigory Chudov
 * Based on Flake encoder, http://flake-enc.sourceforge.net/
 * Copyright (c) 2006-2009 Justin Ruggles
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.IO;
using System.Threading;
using FlacDecode.Audio;
using FlacDecode.Bitwise;
using FlacDecode.Flac;
using FlacDecode.MathCoding;

namespace FlacDecode
{
	public class FlakeReader : IDisposable
	{
		Stream _IO;
		readonly byte[] _framesBuffer;
		readonly string _path;
		readonly Crc16 _crc16;
		readonly Crc8 _crc8;
		readonly FlacFrame _frame;
		readonly BitReader _framereader;
		readonly int[] _residualBuffer;
		readonly int[] _samplesBuffer;
		int _framesBufferLength, _framesBufferOffset;
		long _sampleCount, _sampleOffset;
		int _samplesBufferOffset;
		int _samplesInBuffer;

		bool _doCRC = true;
		long _firstFrameOffset;
		uint _maxFrameSize;
		AudioPCMConfig _pcm;
		SeekPoint[] _seekTable;

		public FlakeReader(string path, Stream outputStream = null)
		{
			_path = path;
			_IO = outputStream ?? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 0x10000);

			_crc8 = new Crc8();
			_crc16 = new Crc16();

			_framesBuffer = new byte[0x20000];
			decode_metadata();

			_frame = new FlacFrame(PCM.ChannelCount);
			_framereader = new BitReader();

			if (((int)_maxFrameSize * PCM.BitsPerSample * PCM.ChannelCount * 2 >> 3) > _framesBuffer.Length)
			{
				byte[] temp = _framesBuffer;
				_framesBuffer = new byte[((int)_maxFrameSize * PCM.BitsPerSample * PCM.ChannelCount * 2 >> 3)];
				if (_framesBufferLength > 0)
					Array.Copy(temp, _framesBufferOffset, _framesBuffer, 0, _framesBufferLength);
				_framesBufferOffset = 0;
			}
			_samplesInBuffer = 0;

			if (PCM.BitsPerSample != 16 && PCM.BitsPerSample != 24)
				throw new Exception("invalid flac file");

			_samplesBuffer = new int[Flake.MaxBlocksize * PCM.ChannelCount];
			_residualBuffer = new int[Flake.MaxBlocksize * PCM.ChannelCount];
		}

		public FlakeReader(AudioPCMConfig _pcm)
		{
			this._pcm = _pcm;
			_crc8 = new Crc8();
			_crc16 = new Crc16();

			_samplesBuffer = new int[Flake.MaxBlocksize * PCM.ChannelCount];
			_residualBuffer = new int[Flake.MaxBlocksize * PCM.ChannelCount];
			_frame = new FlacFrame(PCM.ChannelCount);
			_framereader = new BitReader();
		}

		public bool DoCRC
		{
			get { return _doCRC; }
			set { _doCRC = value; }
		}

		public int[] Samples
		{
			get { return _samplesBuffer; }
		}

		public long Length
		{
			get { return _sampleCount; }
		}

		public long Remaining
		{
			get { return Length - Position; }
		}

		public long Position
		{
			get { return _sampleOffset - _samplesInBuffer; }
			set
			{
				if (value > _sampleCount)
					throw new Exception("seeking past end of stream");
				if (value < Position || value > _sampleOffset)
				{
					if (_seekTable != null && _IO.CanSeek)
					{
						int best_st = -1;
						for (int st = 0; st < _seekTable.Length; st++)
						{
							if (_seekTable[st].Number <= value &&
								(best_st == -1 || _seekTable[st].Number > _seekTable[best_st].Number))
								best_st = st;
						}
						if (best_st != -1)
						{
							_framesBufferLength = 0;
							_samplesInBuffer = 0;
							_samplesBufferOffset = 0;
							_IO.Position = _seekTable[best_st].Offset + _firstFrameOffset;
							_sampleOffset = _seekTable[best_st].Number;
						}
					}
					if (value < Position)
						throw new Exception("cannot seek backwards without seek table");
				}
				while (value > _sampleOffset)
				{
					_samplesInBuffer = 0;
					_samplesBufferOffset = 0;

					fill_frames_buffer();
					if (_framesBufferLength == 0)
						throw new Exception("seek failed");

					int bytesDecoded = DecodeFrame(_framesBuffer, _framesBufferOffset, _framesBufferLength);
					_framesBufferLength -= bytesDecoded;
					_framesBufferOffset += bytesDecoded;

					_sampleOffset += _samplesInBuffer;
				}

				int diff = _samplesInBuffer - (int)(_sampleOffset - value);
				_samplesInBuffer -= diff;
				_samplesBufferOffset += diff;
			}
		}

		public AudioPCMConfig PCM
		{
			get { return _pcm; }
		}

		public string Path
		{
			get { return _path; }
		}

		unsafe void interlace(AudioBuffer buff, int offset, int count)
		{
			if (PCM.ChannelCount == 2)
			{
				fixed (int* src = &_samplesBuffer[_samplesBufferOffset])
					buff.Interlace(offset, src, src + Flake.MaxBlocksize, count);
			}
			else
			{
				for (int ch = 0; ch < PCM.ChannelCount; ch++)
					fixed (int* res = &buff.Samples[offset, ch], src = &_samplesBuffer[_samplesBufferOffset + ch * Flake.MaxBlocksize])
					{
						int* psrc = src;
						for (int i = 0; i < count; i++)
							res[i * PCM.ChannelCount] = *(psrc++);
					}
			}
		}

		public int Read(AudioBuffer buff, int maxLength)
		{
			buff.Prepare(this, maxLength);

			int offset = 0;
			int sampleCount = buff.Length;

			while (_samplesInBuffer < sampleCount)
			{
				if (_samplesInBuffer > 0)
				{
					interlace(buff, offset, _samplesInBuffer);
					sampleCount -= _samplesInBuffer;
					offset += _samplesInBuffer;
					_samplesInBuffer = 0;
					_samplesBufferOffset = 0;
				}

				fill_frames_buffer();

				if (_framesBufferLength == 0)
				{
					buff.Length = offset;
					return offset;
				}

				int bytesDecoded = DecodeFrame(_framesBuffer, _framesBufferOffset, _framesBufferLength);
				_framesBufferLength -= bytesDecoded;
				_framesBufferOffset += bytesDecoded;

				_samplesInBuffer -= _samplesBufferOffset; // can be set by Seek, otherwise zero
				_sampleOffset += _samplesInBuffer;
			}

			interlace(buff, offset, sampleCount);
			_samplesInBuffer -= sampleCount;
			_samplesBufferOffset += sampleCount;
			if (_samplesInBuffer == 0)
				_samplesBufferOffset = 0;
			return offset + sampleCount;
		}

		unsafe void fill_frames_buffer()
		{
			if (_framesBufferLength == 0)
				_framesBufferOffset = 0;
			else if (_framesBufferLength < _framesBuffer.Length / 2 && _framesBufferOffset >= _framesBuffer.Length / 2)
			{
				fixed (byte* buff = _framesBuffer)
					AudioSamples.MemCpy(buff, buff + _framesBufferOffset, _framesBufferLength);
				_framesBufferOffset = 0;
			}
			while (_framesBufferLength < _framesBuffer.Length / 2)
			{
				int read = _IO.Read(_framesBuffer, _framesBufferOffset + _framesBufferLength, _framesBuffer.Length - _framesBufferOffset - _framesBufferLength);
				_framesBufferLength += read;
				if (read == 0)
					break;
			}
		}

		unsafe void decode_frame_header(BitReader bitreader, FlacFrame frame)
		{
			int header_start = bitreader.Position;

			if (bitreader.readbits(15) != 0x7FFC)
				throw new Exception("invalid frame");
			bitreader.readbit();
			frame.bs_code0 = (int)bitreader.readbits(4);
			uint sr_code0 = bitreader.readbits(4);
			frame.ch_mode = (ChannelMode)bitreader.readbits(4);
			uint bps_code = bitreader.readbits(3);
			if (Flake.flac_bitdepths[bps_code] != PCM.BitsPerSample)
				throw new Exception("unsupported bps coding");
			uint t1 = bitreader.readbit(); // == 0?????
			if (t1 != 0)
				throw new Exception("unsupported frame coding");
			frame.frame_number = (int)bitreader.read_utf8();

			// custom block size
			if (frame.bs_code0 == 6)
			{
				frame.bs_code1 = (int)bitreader.readbits(8);
				frame.blocksize = frame.bs_code1 + 1;
			}
			else if (frame.bs_code0 == 7)
			{
				frame.bs_code1 = (int)bitreader.readbits(16);
				frame.blocksize = frame.bs_code1 + 1;
			}
			else
				frame.blocksize = Flake.flac_blocksizes[frame.bs_code0];

			// custom sample rate
			if (sr_code0 < 4 || sr_code0 > 11)
			{
				// sr_code0 == 12 -> sr == bitreader.readbits(8) * 1000;
				// sr_code0 == 13 -> sr == bitreader.readbits(16);
				// sr_code0 == 14 -> sr == bitreader.readbits(16) * 10;
				throw new Exception("invalid sample rate mode");
			}

			int frame_channels = (int)frame.ch_mode + 1;
			if (frame_channels > 11)
				throw new Exception("invalid channel mode");
			if (frame_channels == 2 || frame_channels > 8) // Mid/Left/Right Side Stereo
				frame_channels = 2;
			else
				frame.ch_mode = ChannelMode.NotStereo;
			if (frame_channels != PCM.ChannelCount)
				throw new Exception("invalid channel mode");

			// CRC-8 of frame header
			byte crc = _doCRC ? _crc8.ComputeChecksum(bitreader.Buffer, header_start, bitreader.Position - header_start) : (byte)0;
			frame.crc8 = (byte)bitreader.readbits(8);
			if (_doCRC && frame.crc8 != crc)
				throw new Exception("header crc mismatch");
		}

		unsafe void decode_subframe_constant(BitReader bitreader, FlacFrame frame, int ch)
		{
			int obits = frame.subframes[ch].obits;
			frame.subframes[ch].best.residual[0] = bitreader.readbits_signed(obits);
		}

		unsafe void decode_subframe_verbatim(BitReader bitreader, FlacFrame frame, int ch)
		{
			int obits = frame.subframes[ch].obits;
			for (int i = 0; i < frame.blocksize; i++)
				frame.subframes[ch].best.residual[i] = bitreader.readbits_signed(obits);
		}

		unsafe void decode_residual(BitReader bitreader, FlacFrame frame, int ch)
		{
			// rice-encoded block
			// coding method
			frame.subframes[ch].best.rc.CodingMethod = (int)bitreader.readbits(2); // ????? == 0
			if (frame.subframes[ch].best.rc.CodingMethod != 0 && frame.subframes[ch].best.rc.CodingMethod != 1)
				throw new Exception("unsupported residual coding");
			// partition order
			frame.subframes[ch].best.rc.PartitionOrder = (int)bitreader.readbits(4);
			if (frame.subframes[ch].best.rc.PartitionOrder > 8)
				throw new Exception("invalid partition order");
			int psize = frame.blocksize >> frame.subframes[ch].best.rc.PartitionOrder;
			int res_cnt = psize - frame.subframes[ch].best.order;

			int rice_len = 4 + frame.subframes[ch].best.rc.CodingMethod;
			// residual
			int j = frame.subframes[ch].best.order;
			int* r = frame.subframes[ch].best.residual + j;
			for (int p = 0; p < (1 << frame.subframes[ch].best.rc.PartitionOrder); p++)
			{
				if (p == 1) res_cnt = psize;
				int n = Math.Min(res_cnt, frame.blocksize - j);

				int k = frame.subframes[ch].best.rc.RiceParams[p] = (int)bitreader.readbits(rice_len);
				if (k == (1 << rice_len) - 1)
				{
					k = frame.subframes[ch].best.rc.BpsIfUsingEscape[p] = (int)bitreader.readbits(5);
					for (int i = n; i > 0; i--)
						*(r++) = bitreader.readbits_signed(k);
				}
				else
				{
					bitreader.read_rice_block(n, k, r);
					r += n;
				}
				j += n;
			}
		}

		unsafe void decode_subframe_fixed(BitReader bitreader, FlacFrame frame, int ch)
		{
			// warm-up samples
			int obits = frame.subframes[ch].obits;
			for (int i = 0; i < frame.subframes[ch].best.order; i++)
				frame.subframes[ch].best.residual[i] = bitreader.readbits_signed(obits);

			// residual
			decode_residual(bitreader, frame, ch);
		}

		unsafe void decode_subframe_lpc(BitReader bitreader, FlacFrame frame, int ch)
		{
			// warm-up samples
			int obits = frame.subframes[ch].obits;
			for (int i = 0; i < frame.subframes[ch].best.order; i++)
				frame.subframes[ch].best.residual[i] = bitreader.readbits_signed(obits);

			// LPC coefficients
			frame.subframes[ch].best.cbits = (int)bitreader.readbits(4) + 1; // lpc_precision
			frame.subframes[ch].best.shift = bitreader.readbits_signed(5);
			if (frame.subframes[ch].best.shift < 0)
				throw new Exception("negative shift");
			for (int i = 0; i < frame.subframes[ch].best.order; i++)
				frame.subframes[ch].best.coefs[i] = bitreader.readbits_signed(frame.subframes[ch].best.cbits);

			// residual
			decode_residual(bitreader, frame, ch);
		}

		unsafe void decode_subframes(BitReader bitreader, FlacFrame frame)
		{
			fixed (int* r = _residualBuffer, s = _samplesBuffer)
				for (int ch = 0; ch < PCM.ChannelCount; ch++)
				{
					// subframe header
					uint t1 = bitreader.readbit(); // ?????? == 0
					if (t1 != 0)
						throw new Exception("unsupported subframe coding (ch == " + ch.ToString() + ")");
					var type_code = (int)bitreader.readbits(6);
					frame.subframes[ch].wbits = (int)bitreader.readbit();
					if (frame.subframes[ch].wbits != 0)
						frame.subframes[ch].wbits += (int)bitreader.read_unary();

					frame.subframes[ch].obits = PCM.BitsPerSample - frame.subframes[ch].wbits;
					switch (frame.ch_mode)
					{
						case ChannelMode.MidSide:
							frame.subframes[ch].obits += ch;
							break;
						case ChannelMode.LeftSide:
							frame.subframes[ch].obits += ch;
							break;
						case ChannelMode.RightSide:
							frame.subframes[ch].obits += 1 - ch;
							break;
					}

					frame.subframes[ch].best.type = (SubframeType)type_code;
					frame.subframes[ch].best.order = 0;

					if ((type_code & (uint)SubframeType.Lpc) != 0)
					{
						frame.subframes[ch].best.order = (type_code - (int)SubframeType.Lpc) + 1;
						frame.subframes[ch].best.type = SubframeType.Lpc;
					}
					else if ((type_code & (uint)SubframeType.Fixed) != 0)
					{
						frame.subframes[ch].best.order = (type_code - (int)SubframeType.Fixed);
						frame.subframes[ch].best.type = SubframeType.Fixed;
					}

					frame.subframes[ch].best.residual = r + ch * Flake.MaxBlocksize;
					frame.subframes[ch].samples = s + ch * Flake.MaxBlocksize;

					// subframe
					switch (frame.subframes[ch].best.type)
					{
						case SubframeType.Constant:
							decode_subframe_constant(bitreader, frame, ch);
							break;
						case SubframeType.Verbatim:
							decode_subframe_verbatim(bitreader, frame, ch);
							break;
						case SubframeType.Fixed:
							decode_subframe_fixed(bitreader, frame, ch);
							break;
						case SubframeType.Lpc:
							decode_subframe_lpc(bitreader, frame, ch);
							break;
						default:
							throw new Exception("invalid subframe type");
					}
				}
		}

		unsafe void restore_samples_fixed(FlacFrame frame, int ch)
		{
			FlacSubframeInfo sub = frame.subframes[ch];

			AudioSamples.MemCpy(sub.samples, sub.best.residual, sub.best.order);
			int* data = sub.samples + sub.best.order;
			int* residual = sub.best.residual + sub.best.order;
			int data_len = frame.blocksize - sub.best.order;
			int s1;
			switch (sub.best.order)
			{
				case 0:
					AudioSamples.MemCpy(data, residual, data_len);
					break;
				case 1:
					s1 = data[-1];
					for (int i = data_len; i > 0; i--)
					{
						s1 += *(residual++);
						*(data++) = s1;
					}
					break;
				case 2:
					int s2 = data[-2];
					s1 = data[-1];
					for (int i = data_len; i > 0; i--)
					{
						int s0 = *(residual++) + (s1 << 1) - s2;
						*(data++) = s0;
						s2 = s1;
						s1 = s0;
					}
					break;
				case 3:
					for (int i = 0; i < data_len; i++)
						data[i] = residual[i] + (((data[i - 1] - data[i - 2]) << 1) + (data[i - 1] - data[i - 2])) + data[i - 3];
					break;
				case 4:
					for (int i = 0; i < data_len; i++)
						data[i] = residual[i] + ((data[i - 1] + data[i - 3]) << 2) - ((data[i - 2] << 2) + (data[i - 2] << 1)) - data[i - 4];
					break;
			}
		}

		unsafe void restore_samples_lpc(FlacFrame frame, int ch)
		{
			FlacSubframeInfo sub = frame.subframes[ch];
			ulong csum = 0;
			fixed (int* coefs = sub.best.coefs)
			{
				for (int i = sub.best.order; i > 0; i--)
					csum += (ulong)Math.Abs(coefs[i - 1]);
				if ((csum << sub.obits) >= 1UL << 32)
					LinearPredictiveCoding.decode_residual_long(sub.best.residual, sub.samples, frame.blocksize, sub.best.order, coefs, sub.best.shift);
				else
					LinearPredictiveCoding.decode_residual(sub.best.residual, sub.samples, frame.blocksize, sub.best.order, coefs, sub.best.shift);
			}
		}

		unsafe void restore_samples(FlacFrame frame)
		{
			for (int ch = 0; ch < PCM.ChannelCount; ch++)
			{
				switch (frame.subframes[ch].best.type)
				{
					case SubframeType.Constant:
						AudioSamples.MemSet(frame.subframes[ch].samples, frame.subframes[ch].best.residual[0], frame.blocksize);
						break;
					case SubframeType.Verbatim:
						AudioSamples.MemCpy(frame.subframes[ch].samples, frame.subframes[ch].best.residual, frame.blocksize);
						break;
					case SubframeType.Fixed:
						restore_samples_fixed(frame, ch);
						break;
					case SubframeType.Lpc:
						restore_samples_lpc(frame, ch);
						break;
				}
				if (frame.subframes[ch].wbits != 0)
				{
					int* s = frame.subframes[ch].samples;
					int x = frame.subframes[ch].wbits;
					for (int i = frame.blocksize; i > 0; i--)
						*(s++) <<= x;
				}
			}
			if (frame.ch_mode != ChannelMode.NotStereo)
			{
				int* l = frame.subframes[0].samples;
				int* r = frame.subframes[1].samples;
				switch (frame.ch_mode)
				{
					case ChannelMode.LeftRight:
						break;
					case ChannelMode.MidSide:
						for (int i = frame.blocksize; i > 0; i--)
						{
							int mid = *l;
							int side = *r;
							mid <<= 1;
							mid |= (side & 1); /* i.e. if 'side' is odd... */
							*(l++) = (mid + side) >> 1;
							*(r++) = (mid - side) >> 1;
						}
						break;
					case ChannelMode.LeftSide:
						for (int i = frame.blocksize; i > 0; i--)
						{
							int _l = *(l++), _r = *r;
							*(r++) = _l - _r;
						}
						break;
					case ChannelMode.RightSide:
						for (int i = frame.blocksize; i > 0; i--)
							*(l++) += *(r++);
						break;
				}
			}
		}

		public unsafe int DecodeFrame(byte[] buffer, int pos, int len)
		{
			fixed (byte* buf = buffer)
			{
				_framereader.Reset(buf, pos, len);
				decode_frame_header(_framereader, _frame);
				decode_subframes(_framereader, _frame);
				_framereader.flush();
				ushort crc_1 = _doCRC ? _crc16.ComputeChecksum(_framereader.Buffer + pos, _framereader.Position - pos) : (ushort)0;
				var crc_2 = (ushort)_framereader.readbits(16);
				if (_doCRC && crc_1 != crc_2)
					throw new Exception("frame crc mismatch");
				restore_samples(_frame);
				_samplesInBuffer = _frame.blocksize;
				return _framereader.Position - pos;
			}
		}


		bool skip_bytes(int bytes)
		{
			for (int j = 0; j < bytes; j++)
				if (0 == _IO.Read(_framesBuffer, 0, 1))
					return false;
			return true;
		}

		unsafe void decode_metadata()
		{
			int i, id;
			var FLAC__STREAM_SYNC_STRING = new[] { (byte)'f', (byte)'L', (byte)'a', (byte)'C' };
			var ID3V2_TAG_ = new[] { (byte)'I', (byte)'D', (byte)'3' };

			for (i = id = 0; i < 4; )
			{
				if (_IO.Read(_framesBuffer, 0, 1) == 0)
					throw new Exception("FLAC stream not found");
				byte x = _framesBuffer[0];
				if (x == FLAC__STREAM_SYNC_STRING[i])
				{
					i++;
					id = 0;
					continue;
				}
				if (id < 3 && x == ID3V2_TAG_[id])
				{
					id++;
					i = 0;
					if (id == 3)
					{
						if (!skip_bytes(3))
							throw new Exception("FLAC stream not found");
						int skip = 0;
						for (int j = 0; j < 4; j++)
						{
							if (0 == _IO.Read(_framesBuffer, 0, 1))
								throw new Exception("FLAC stream not found");
							skip <<= 7;
							skip |= (_framesBuffer[0] & 0x7f);
						}
						if (!skip_bytes(skip))
							throw new Exception("FLAC stream not found");
					}
					continue;
				}
				if (x == 0xff) /* MAGIC NUMBER for the first 8 frame sync bits */
				{
					do
					{
						if (_IO.Read(_framesBuffer, 0, 1) == 0)
							throw new Exception("FLAC stream not found");
						x = _framesBuffer[0];
					} while (x == 0xff);
					if (x >> 2 == 0x3e) /* MAGIC NUMBER for the last 6 sync bits */
					{
						throw new Exception("headerless file unsupported");
					}
				}
				throw new Exception("FLAC stream not found");
			}

			do
			{
				fill_frames_buffer();
				fixed (byte* buf = _framesBuffer)
				{
					var bitreader = new BitReader(buf, _framesBufferOffset, _framesBufferLength - _framesBufferOffset);
					bool is_last = bitreader.readbit() != 0;
					var type = (MetadataType)bitreader.readbits(7);
					var len = (int)bitreader.readbits(24);

					if (type == MetadataType.StreamInfo)
					{

						bitreader.readbits(Flake.flacStreamMetadataStreaminfoMinBlockSizeLen);
						bitreader.readbits(Flake.flacStreamMetadataStreaminfoMaxBlockSizeLen);
						bitreader.readbits(Flake.flacStreamMetadataStreaminfoMinFrameSizeLen);
						_maxFrameSize = bitreader.readbits(Flake.flacStreamMetadataStreaminfoMaxFrameSizeLen);
						var sample_rate = (int)bitreader.readbits(Flake.flacStreamMetadataStreaminfoSampleRateLen);
						int channels = 1 + (int)bitreader.readbits(Flake.flacStreamMetadataStreaminfoChannelsLen);
						int bits_per_sample = 1 + (int)bitreader.readbits(Flake.flacStreamMetadataStreaminfoBitsPerSampleLen);
						_pcm = new AudioPCMConfig(bits_per_sample, channels, sample_rate);
						_sampleCount = (long)bitreader.readbits64(Flake.flacStreamMetadataStreaminfoTotalSamplesLen);
						bitreader.skipbits(Flake.flacStreamMetadataStreaminfoMd5SumLen);
					}
					else if (type == MetadataType.Seektable)
					{
						int num_entries = len / 18;
						_seekTable = new SeekPoint[num_entries];
						for (int e = 0; e < num_entries; e++)
						{
							_seekTable[e].Number = (long)bitreader.readbits64(Flake.FlacStreamMetadataSeekpointSampleNumberLen);
							_seekTable[e].Offset = (long)bitreader.readbits64(Flake.FlacStreamMetadataSeekpointStreamOffsetLen);
							_seekTable[e].Framesize = (int)bitreader.readbits24(Flake.FlacStreamMetadataSeekpointFrameSamplesLen);
						}
					}
					if (_framesBufferLength < 4 + len)
					{
						_IO.Position += 4 + len - _framesBufferLength;
						_framesBufferLength = 0;
					}
					else
					{
						_framesBufferLength -= 4 + len;
						_framesBufferOffset += 4 + len;
					}
					if (is_last)
						break;
				}
			} while (true);
			_firstFrameOffset = _IO.Position - _framesBufferLength;
		}

		~FlakeReader()
		{
			Dispose();
		}

		public void Dispose()
		{
			var lio = Interlocked.Exchange(ref _IO, null);
			if (lio == null) return;
			lio.Flush();
			lio.Close();
			lio.Dispose();
		}
	}
}