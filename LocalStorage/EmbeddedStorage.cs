using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using LocalStorage.Paging;
using LocalStorage.Paging.Views;
using LocalStorage.Strings;
using LocalStorage.Tables;
using LocalStorage.Types;

namespace LocalStorage
{
	public sealed class EmbeddedStorage
		: IEmbdeddedStorage
		, IDisposable
	{
		private readonly string _fileName;
		private readonly PageCollection _pages;
		private readonly TableStorage _tables;
		private readonly StorageHeaderView _header;
		private readonly bool _disposeStream;
		private readonly Stream _stream;
		private readonly StringStorage _strings;
		private readonly TypeStorage _types;

		private EmbeddedStorage(Stream stream, string fileName, bool create, bool disposeStream)
		{
			if (stream == null) throw new ArgumentNullException("stream");

			_stream = stream;
			_fileName = fileName;
			_disposeStream = disposeStream;
			_pages = new PageCollection(stream, PageDescriptor.DefaultSize);

			if (create)
			{
				using (var page = _pages.Allocate(PageType.StorageHeader))
				{
					_header = new StorageHeaderView(page)
						{
							StorageVersion = StorageHeaderView.CurrentStorageVersion,
							CreationTime = DateTime.Now
						};
					_header.Commit();
				}
			}
			else
			{
				var descriptor = _pages.Pages.FirstOrDefault();
				if (descriptor.Type != PageType.StorageHeader)
					throw new InvalidDataException("The given stream is not a valid storage or has been corrupted");

				using (var page = _pages.Load(descriptor))
				{
					_header = new StorageHeaderView(page);
				}
			}

			_strings = new StringStorage(_pages);
			_types = new TypeStorage(_pages, _strings);
			_tables = new TableStorage(_pages, _strings, _types);
		}

		public override string ToString()
		{
			return string.Format("File: {0}", _fileName);
		}

		public string FileName
		{
			get { return _fileName; }
		}

		public IStorageHeader Header
		{
			get { return _header; }
		}

		public ITableCollection Tables
		{
			get { return _tables; }
		}

		public bool IsReadOnly
		{
			get { return _pages.IsReadOnly; }
		}

		public void Dispose()
		{
			_pages.Dispose();
			if (_disposeStream)
			{
				_stream.Dispose();
			}
		}

		public static EmbeddedStorage FromFile(string fileName, StorageMode mode)
		{
			FileStream stream;
			switch (mode)
			{
				case StorageMode.Open:
					stream = File.Open(fileName, FileMode.Open);
					return new EmbeddedStorage(stream, fileName, create: false, disposeStream: true);

				case StorageMode.Create:
					stream = File.Open(fileName, FileMode.Create);
					return new EmbeddedStorage(stream, fileName, create: true, disposeStream: true);

				case StorageMode.OpenOrCreate:
					stream = File.Open(fileName, FileMode.OpenOrCreate);
					return new EmbeddedStorage(stream, fileName, create: stream.Length == 0, disposeStream: true);

				default:
					throw new InvalidEnumArgumentException("mode", (int)mode, typeof(StorageMode));
			}
		}

		public static EmbeddedStorage FromStream(Stream stream, StorageMode mode)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			if (!stream.CanRead) throw new ArgumentException("stream must be readable", "stream");

			switch (mode)
			{
				case StorageMode.Open:
					return new EmbeddedStorage(stream, null, create: false, disposeStream: false);

				case StorageMode.Create:
					return new EmbeddedStorage(stream, null, create: true, disposeStream: false);

				case StorageMode.OpenOrCreate:
					var length = stream.Length - stream.Position;
					return new EmbeddedStorage(stream, null, create: length == 0, disposeStream: false);

				default:
					throw new InvalidEnumArgumentException("mode", (int)mode, typeof(StorageMode));
			}
		}
	}
}