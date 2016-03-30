using System;
using System.IO;
using System.Threading.Tasks;

namespace LocalStorage.Paging
{
	internal sealed class Page
		: Stream
	{
		private readonly bool _canWrite;
		private readonly byte[] _data;
		private readonly PageDescriptor _descriptor;
		private readonly Task _initTask;
		private readonly PageStorage _pages;

		private bool _isDirty;
		private int _position;

		/// <summary>
		/// </summary>
		/// <param name="pages"></param>
		/// <param name="descriptor"></param>
		/// <param name="zeroOut">When true, then the page's content must be zeroed out before any read operation may continue</param>
		private Page(PageStorage pages, PageDescriptor descriptor, bool zeroOut)
		{
			_pages = pages;
			_descriptor = descriptor;
			_data = new byte[pages.PageSize - PageDescriptor.HeaderSize];
			_pages = pages;

			_initTask = zeroOut
							? _pages.Write(Descriptor, _data)
							: _pages.Read(descriptor, _data);

			if (pages.CanWrite)
			{
				_canWrite = true;
			}
		}

		/// <summary>
		/// Used for testing.
		/// </summary>
		internal void Wait()
		{
			_initTask.Wait();
		}

		public PageDescriptor Descriptor
		{
			get { return _descriptor; }
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

		/// <summary>
		///     Creates a new page and flushes it to the page collection.
		/// </summary>
		/// <returns></returns>
		public static Page WriteAndCreate(PageStorage pages, PageDescriptor descriptor)
		{
			return new Page(pages, descriptor, true);
		}

		public static Page ReadAndCreate(PageStorage pages, PageDescriptor descriptor)
		{
			return new Page(pages, descriptor, false);
		}

		public override void Flush()
		{
			
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
					Position = _data.Length + offset;
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
			_initTask.Wait();

			Array.Copy(_data, _position, buffer, offset, count);
			_position += count;
			return count;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (count == 0)
				return;

			Array.Copy(buffer, offset, _data, _position, count);
			_position += count;
			_isDirty = true;
		}

		public void Commit()
		{
			if (_isDirty)
			{
				Task task = _pages.Write(_descriptor, _data);
				task.Wait();

				_isDirty = false;
			}
		}
	}
}