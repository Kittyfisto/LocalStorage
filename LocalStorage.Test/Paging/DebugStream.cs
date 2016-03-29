using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LocalStorage.Test.Paging
{
	sealed class DebugStream
		: Stream
	{
		private struct Operation
		{
			public enum Type
			{
				Read,
				Write,
				SetLength,
			}

			public Type Kind;
			public Thread Thread;
			public long Position;
			public byte[] Data;
			public byte[] Stream;

			public override string ToString()
			{
				return string.Format("{0}: {1}@{2}, {3} bytes", Thread.ManagedThreadId, Kind, Position, Data.Length);
			}
		}

		private readonly List<Operation> _ops;
		private readonly Stream _innerStream;

		public DebugStream(Stream innerStream)
		{
			_innerStream = innerStream;
			_ops = new List<Operation>();
		}

		public override void Flush()
		{
			_innerStream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _innerStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_innerStream.SetLength(value);
			_ops.Add(new Operation
				{
					Kind = Operation.Type.SetLength,
					Data = new byte[value],
					Stream = ((MemoryStream)_innerStream).ToArray(),
					Position = _innerStream.Position,
					Thread = Thread.CurrentThread
				});
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int read = _innerStream.Read(buffer, offset, count);
			var data = new byte[count];
			Array.Copy(buffer, offset, data, 0, count);
			_ops.Add(new Operation
			{
				Kind = Operation.Type.Read,
				Data = data,
				Stream = ((MemoryStream)_innerStream).ToArray(),
				Position = _innerStream.Position,
				Thread = Thread.CurrentThread
			});
			return read;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			var data = new byte[count];
			Array.Copy(buffer, offset, data, 0, count);

			_innerStream.Write(buffer, offset, count);

			_ops.Add(new Operation
			{
				Kind = Operation.Type.Write,
				Data = data,
				Stream = ((MemoryStream)_innerStream).ToArray(),
				Position = _innerStream.Position,
				Thread = Thread.CurrentThread
			});
		}

		public override bool CanRead
		{
			get { return _innerStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _innerStream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _innerStream.CanWrite; }
		}

		public override long Length
		{
			get { return _innerStream.Length; }
		}

		public override long Position
		{
			get { return _innerStream.Position; }
			set { _innerStream.Position = value; }
		}

		public Stream InnerStream
		{
			get { return _innerStream; }
		}
	}
}