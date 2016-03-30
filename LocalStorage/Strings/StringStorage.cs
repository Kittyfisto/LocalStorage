using System;
using System.Linq;
using System.Threading;
using LocalStorage.Paging;
using LocalStorage.Paging.Views;

namespace LocalStorage.Strings
{
	/// <summary>
	/// Responsible for storing variable-length strings in a <see cref="PageCollection"/>.
	/// </summary>
	internal sealed class StringStorage
	{
		private readonly PageCollection _pages;
		private readonly StringListView _firstPage;
		private int _currentIndex;

		public StringStorage(PageCollection pages)
		{
			if (pages == null) throw new ArgumentNullException("pages");

			_pages = pages;

			var descriptor = _pages.Pages.FirstOrDefault(x => x.Type == PageType.StringList);
			_firstPage = descriptor.Id == 0
				               ? new StringListView(_pages.Allocate(PageType.StringList))
				               : new StringListView(_pages.Load(descriptor));
			_currentIndex = -1;
		}

		/// <summary>
		/// Stores the given string in this collection and returns a unique index to represent and lookup the string,
		/// when needed.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int Allocate(string value)
		{
			var index = Interlocked.Increment(ref _currentIndex);
			_firstPage.Add(index, value);
			_firstPage.Commit();
			return index;
		}

		public string Load(int index)
		{
			return _firstPage.Find(index);
		}
	}
}