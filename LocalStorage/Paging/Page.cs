using System;
using System.IO;
using System.Threading.Tasks;

namespace LocalStorage.Paging
{
	internal class Page
		: Stream
	{
		private readonly bool _canWrite;
		private readonly byte[] _data;
		private readonly PageDescriptor _descriptor;
		private readonly BinaryWriter _writer;

		protected readonly PageCollection Pages;
		private readonly Task _readTask;

		private bool _isDirty;
		private bool _isDisposed;
		private int _position;

		public Page(PageCollection pages, PageDescriptor descriptor, bool canWrite)
		{
			Pages = pages;
			_descriptor = descriptor;
			_canWrite = canWrite;
			_data = new byte[descriptor.Size];
			_readTask = Pages.Read(descriptor, _data);

			if (canWrite)
			{
				_writer = new BinaryWriter(this);
			}
		}

		public BinaryWriter Writer
		{
			get { return _writer; }
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return _canWrite; }
		}

		public override long Length
		{
			get { return _data.Length; }
		}

		public override long Position
		{
			get { return _position; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException();
				if (value >= _data.Length)
					throw new ArgumentOutOfRangeException();

				_position = (int) value;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_isDisposed = true;
		}

		public override void Flush()
		{
			if (_isDirty)
			{
				Task task = Pages.Write(_descriptor, _data);
				task.Wait();

				_isDirty = false;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					Position = offset;
					break;

				case SeekOrigin.Current:
					Position += offset;
					break;

				case SeekOrigin.End:
					Position = _descriptor.Size + offset;
					break;
			}

			return Position;
		}

		public override void SetLength(long value)
		{
			throw new InvalidOperationException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			_readTask.Wait();

			Array.Copy(_data, _position, buffer, offset, count);
			return count;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count == 0)
				return;

			Array.Copy(buffer, offset, _data, _position, count);
			_isDirty = true;
		}
	}
}