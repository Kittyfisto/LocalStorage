using System;
using System.IO;
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

		private EmbeddedStorage(Stream stream, string fileName)
		{
			if (stream == null) throw new ArgumentNullException("stream");

			_fileName = fileName;
			_pages = new PageCollection(stream);
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
		}

		public static EmbeddedStorage FromFile(string fileName)
		{
			return new EmbeddedStorage(File.Open(fileName, FileMode.OpenOrCreate), fileName);
		}

		public static EmbeddedStorage FromStream(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException("stream");
			if (!stream.CanRead) throw new ArgumentException("stream must be readable", "stream");

			return new EmbeddedStorage(stream, null);
		}
	}
}