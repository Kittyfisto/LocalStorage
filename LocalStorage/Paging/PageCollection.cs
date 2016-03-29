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

		private readonly long _streamStart;
		private readonly BinaryReader _reader;
		private readonly PageOperation _restoreIndex;
		private readonly Stream _stream;
		private readonly Thread _thread;
		private readonly List<PageDescriptor> _unusedPages;
		private readonly List<PageDescriptor> _usedPages;
		private readonly Dictionary<int, Page> _workingSet;
		private readonly BinaryWriter _writer;

		private int _previousPageId;
		private bool _isDisposed;
		private long _streamEnd;

		public PageCollection(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException("stream");

			_stream = stream;
			_streamStart = stream.Position;
			_streamEnd = stream.Length;
			_writer = new BinaryWriter(_stream);
			_reader = new BinaryReader(_stream);

			_usedPages = new List<PageDescriptor>();
			_unusedPages = new List<PageDescriptor>();
			_workingSet = new Dictionary<int, Page>();

			_ops = new ConcurrentQueue<PageOperation>();
			_restoreIndex = PageOperation.RestoreIndex();
			_ops.Enqueue(_restoreIndex);

			_previousPageId = -1;

			_thread = new Thread(ReadWrite)
				{
					Name = "IO Thread"
				};
			_thread.Start();
		}

		public IEnumerable<PageDescriptor> Pages
		{
			get
			{
				_restoreIndex.Task.Wait();
				return _usedPages;
			}
		}

		public Task IndexRestoreTask
		{
			get { return _restoreIndex.Task; }
		}

		public bool CanWrite
		{
			get { return _stream.CanWrite; }
		}

		public bool IsReadOnly
		{
			get { return !_stream.CanWrite; }
		}

		public Page this[int index]
		{
			get { throw new NotImplementedException(); }
		}

		public void Dispose()
		{
			_isDisposed = true;
			_thread.Join();

			if (_ops.Count > 0)
			{
				Log.FatalFormat("The IO thread neded before writing all operations to disk - this should not happen!");
			}
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

		public Page Get(PageDescriptor descriptor)
		{
			var page = Page.ReadAndCreate(this, descriptor);
			_workingSet.Add(descriptor.Id, page);
			return page;
		}

		public void Remove(Page page)
		{
			_workingSet.Remove(page.Descriptor.Id);
		}

		public Page Allocate(PageType type, int minimumSize)
		{
			_restoreIndex.Task.Wait();

			Page page;
			if (!TryGetFreedPage(type, minimumSize, out page))
			{
				page = AllocatePage(type, minimumSize);
			}

			return page;
		}

		public void Free(Page page)
		{
			_restoreIndex.Task.Wait();
		}

		private void ReadWrite()
		{
			while (!_isDisposed)
			{
				ExecuteAllPendingOperations();
			}

			ExecuteAllPendingOperations();
		}

		private void ExecuteAllPendingOperations()
		{
			PageOperation op;
			while (_ops.TryDequeue(out op))
			{
				Execute(op);
			}
		}

		private void Execute(PageOperation op)
		{
			try
			{
				var kind = op.Kind;
				switch (kind)
				{
					case PageOperation.Type.Nop:
						break;

					case PageOperation.Type.Read:
						ExecuteRead(op);
						break;

					case PageOperation.Type.Write:
						ExecuteWrite(op);
						break;

					case PageOperation.Type.RestoreIndex:
						ExecuteRestoreIndex();
						break;

					default:
						Log.ErrorFormat("Unexpected operation: {0}", kind);
						break;
				}

				op.SetFinished();
			}
			catch (Exception e)
			{
				op.SetException(e);
			}
		}

		private void ExecuteRestoreIndex()
		{
			while (_stream.Position < _stream.Length)
			{
				var id = _reader.ReadInt32();
				var dataSize = _reader.ReadInt32();
				var type = _reader.ReadByte();
				var offset = _stream.Position - _streamStart;
				var descriptor = new PageDescriptor(id, offset, dataSize, (PageType)type);
				_usedPages.Add(descriptor);

				// Advance to the next page
				_stream.Seek(dataSize, SeekOrigin.Current);
			}
		}

		private void ExecuteWrite(PageOperation op)
		{
			_writer.BaseStream.Position = op.Descriptor.DataOffset - PageDescriptor.HeaderSize + _streamStart;
			_writer.Write(op.Descriptor.Id);
			_writer.Write(op.Descriptor.DataSize);
			_writer.Write((byte) op.Descriptor.Type);
			_writer.Write(op.Data, 0, op.Data.Length);
			_writer.Flush();
		}

		private void ExecuteRead(PageOperation op)
		{
			_reader.BaseStream.Position = op.Descriptor.DataOffset + _streamStart;
			_reader.Read(op.Data, 0, op.Data.Length);
		}

		private Page AllocatePage(PageType type, int minimumDataSize)
		{
			int id = Interlocked.Increment(ref _previousPageId);

			long pageOffset = _streamEnd - _streamStart;
			long pageDataOffset = pageOffset + PageDescriptor.HeaderSize;

			var pageSize = minimumDataSize + PageDescriptor.HeaderSize;
			Interlocked.Add(ref _streamEnd, pageSize);

			var descriptor = new PageDescriptor(id, pageDataOffset, minimumDataSize, type);
			_usedPages.Add(descriptor);

			Page page = Page.WriteAndCreate(this, descriptor);
			return page;
		}

		/// <summary>
		///     Tries to locate a previously (but no longer) used page of sufficient size.
		///     Changes the page's type to the given type, if necessary.
		/// </summary>
		/// <remarks>
		///     The returned page may have a bigger data that is required.
		/// </remarks>
		/// <param name="type"></param>
		/// <param name="minimumSize"></param>
		/// <param name="page"></param>
		/// <returns></returns>
		private bool TryGetFreedPage(PageType type, int minimumSize, out Page page)
		{
			for (int i = 0; i < _unusedPages.Count; ++i)
			{
				if (_unusedPages[i].DataSize >= minimumSize)
				{
					var descriptor = new PageDescriptor(_unusedPages[i], type);
					page = Page.WriteAndCreate(this, descriptor);
					_unusedPages.RemoveAt(i);
					_usedPages.Add(descriptor);
					_workingSet.Add(descriptor.Id, page);
					return true;
				}
			}

			page = null;
			return false;
		}

		public void Wait()
		{
			var op = PageOperation.Nop();
			_ops.Enqueue(op);
			op.Task.Wait();
		}
	}
}