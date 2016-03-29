using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using LocalStorage.Paging;
using LocalStorage.Tables;

namespace LocalStorage
{
	public sealed class EmbeddedStorage
		: IEmbdeddedStorage
		, IDisposable
	{
		private readonly string _fileName;
		private readonly PageCollection _pages;
		private readonly TableCollection _tables;
		private readonly StorageDescriptor _descriptor;
		private readonly bool _disposeStream;
		private readonly Stream _stream;

		private EmbeddedStorage(Stream stream, string fileName, bool create, bool disposeStream)
		{
			if (stream == null) throw new ArgumentNullException("stream");

			_stream = stream;
			_fileName = fileName;
			_disposeStream = disposeStream;
			_pages = new PageCollection(stream);

			if (create)
			{
				using (var header = _pages.Allocate(PageType.StorageDescriptor, StorageDescriptor.HeaderSize))
				{
					_descriptor = new StorageDescriptor
						{
							StorageVersion = StorageDescriptor.CurrentStorageVersion,
							CreationTime = DateTime.Now
						};
					_descriptor.WriteTo(header);
				}
			}
			else
			{
				var descriptor = _pages.Pages.FirstOrDefault();
				if (descriptor.Type != PageType.StorageDescriptor)
					throw new InvalidDataException("The given stream is not a valid storage or has been corrupted");

				using (var page = _pages.Get(descriptor))
				{
					_descriptor = new StorageDescriptor();
					_descriptor.ReadFrom(page);
				}
			}

			_tables = new TableCollection(_pages);
		}

		public override string ToString()
		{
			return string.Format("File: {0}", _fileName);
		}

		public string FileName
		{
			get { return _fileName; }
		}

		public IStorageDescriptor Descriptor
		{
			get { return _descriptor; }
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