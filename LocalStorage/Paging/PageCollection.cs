using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace LocalStorage.Paging
{
	internal sealed class PageCollection
		: IInternalPageCollection
		  , IDisposable
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ConcurrentQueue<PageOperation> _ops;

		private readonly List<PageDescriptor> _usedPages;
		private readonly List<PageDescriptor> _unusedPages;
		private readonly List<Page> _pages;

		private readonly Stream _stream;
		private readonly Thread _thread;
		private bool _isDisposed;
		private uint _id;

		public PageCollection(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException("stream");

			_stream = stream;

			_usedPages = new List<PageDescriptor>();
			_unusedPages = new List<PageDescriptor>();
			_pages = new List<Page>();

			_ops = new ConcurrentQueue<PageOperation>();
			_thread = new Thread(ReadWrite)
				{
					Name = "IO Thread"
				};
			_thread.Start();
		}

		public bool IsReadOnly
		{
			get { return !_stream.CanWrite; }
		}

		public void Dispose()
		{
			_isDisposed = true;
			_thread.Join();
		}

		public Task Write(PageDescriptor descriptor, byte[] data)
		{
			PageOperation op = PageOperation.Write(descriptor, data);
			_ops.Enqueue(op);
			return op.Task;
		}

		public Task Read(PageDescriptor descriptor, byte[] data)
		{
			PageOperation op = PageOperation.Read(descriptor, data);
			_ops.Enqueue(op);
			return op.Task;
		}

		private void ReadWrite()
		{
			while (!_isDisposed)
			{
				PageOperation op;
				while (_ops.TryDequeue(out op))
				{
					op.Execute(_stream);
				}
			}
		}

		public Page this[int index]
		{
			get { throw new NotImplementedException(); }
		}

		public Page Allocate(int minimumSize)
		{
			Page page;
			if (!TryGetUnusedPage(minimumSize, out page))
			{
				page = AllocatePage(minimumSize, page);
			}

			return page;
		}

		private Page AllocatePage(int minimumSize, Page page)
		{
			var id = _id++;
			var offset = _stream.Length;
			var length = offset + minimumSize;
			_stream.SetLength(length);
			var descriptor = new PageDescriptor(id, offset, minimumSize);
			_usedPages.Add(descriptor);

			page = new Page(this, descriptor, _stream.CanWrite);
			return page;
		}

		private bool TryGetUnusedPage(int minimumSize, out Page page)
		{
			for (int i = 0; i < _unusedPages.Count; ++i)
			{
				if (_unusedPages[i].Size >= minimumSize)
				{
					var descriptor = _unusedPages[i];
					page = new Page(this, descriptor, _stream.CanWrite);
					_unusedPages.RemoveAt(i);
					return true;
				}
			}

			page = null;
			return false;
		}

		public void Free(Page page)
		{
			
		}
	}
}