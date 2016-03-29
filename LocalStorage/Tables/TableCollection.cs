using System;
using System.Collections;
using System.Collections.Generic;
using LocalStorage.Paging;

namespace LocalStorage.Tables
{
	internal sealed class TableCollection
		: ITableCollection
	{
		private readonly Dictionary<string, ITable> _tables;
		private readonly PageCollection _pages;

		public TableCollection(PageCollection pages)
		{
			if (pages == null) throw new ArgumentNullException("pages");

			_pages = pages;
			//_tableIndexPage = pages.Allocate()
			_tables = new Dictionary<string, ITable>();
		}

		public IEnumerator<ITable> GetEnumerator()
		{
			return _tables.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count
		{
			get { return _tables.Count; }
		}

		public ITable Add<T>(string tableName)
		{
			if (tableName == null) throw new ArgumentNullException("tableName");

			var header = new TableHeader(tableName, typeof(T));
			var page = _pages.Allocate(header.Size);
			header.WriteTo(page);
			page.Flush();

			var table = new Table<T>(_pages, header);
			_tables.Add(tableName, table);
			return table;
		}
	}
}