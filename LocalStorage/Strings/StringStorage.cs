using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using LocalStorage.Paging;
using LocalStorage.Paging.Views;

namespace LocalStorage.Strings
{
	/// <summary>
	/// Responsible for storing variable-length strings in a <see cref="PageStorage"/>.
	/// </summary>
	internal sealed class StringStorage
	{
		private readonly PageStorage _pages;
		private readonly StringListView _firstPage;
		private int _currentIndex;
		private StringListView _currentPage;

		public StringStorage(PageStorage pages)
		{
			if (pages == null) throw new ArgumentNullException("pages");

			_pages = pages;

			var descriptor = _pages.Pages.FirstOrDefault(x => x.Type == PageType.StringList);
			_firstPage = descriptor.Id == 0
				               ? new StringListView(_pages.Allocate(PageType.StringList))
				               : new StringListView(_pages.Load(descriptor));
			_currentPage = _firstPage;
			_currentIndex = 0;
		}

		/// <summary>
		/// Stores the given string in this collection and returns a unique index to represent and lookup the string,
		/// when needed.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int Add(string value)
		{
			var index = Interlocked.Increment(ref _currentIndex);
			// TODO: Find a way to store huge strings over multiple pages because otherwise why even bother...
			while (!_currentPage.TryAdd(index, value))
			{
				var page = _pages.Allocate(PageType.StringList);
				_currentPage.NextPageId = page.Descriptor.Id;
				_currentPage.Commit();

				_currentPage = new StringListView(page);
			}
			_currentPage.Commit();
			return index;
		}

		[Pure]
		public string Load(int index)
		{
			var view = _firstPage;
			while(true)
			{
				string value;
				if (view.TryFind(index, out value))
					return value;

				var next = view.NextPageId;
				if (next != 0)
				{
					var page = _pages.Load(next, PageType.StringList);
					// TODO: The page of a view should be replacable instead of having to create a new view every time...
					view = new StringListView(page);
				}
				else
				{
					throw new ArgumentException(string.Format("No such string '#{0}'", index));
				}
			} 
		}
	}
}