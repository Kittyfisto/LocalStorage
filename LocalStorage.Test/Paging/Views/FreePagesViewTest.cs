using System.IO;
using FluentAssertions;
using LocalStorage.Paging;
using LocalStorage.Paging.Views;
using NUnit.Framework;

namespace LocalStorage.Test.Paging.Views
{
	[TestFixture]
	public sealed class FreePagesViewTest
		: AbstractViewTest
	{
		[Test]
		public void TestCtor()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, 1024))
			using (var page = pages.Allocate(PageType.FreePageIndex))
			{
				var view = new FreePagesView(page);
				view.PreviousPageId.Should().Be(0);
				view.NextPageId.Should().Be(0);
			}
		}

		[Test]
		public void TestPreviousPageIndex()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, 1024))
			using (var page = pages.Allocate(PageType.FreePageIndex))
			{
				var view = new FreePagesView(page);
				view.PreviousPageId.Should().Be(0);
				for (int i = 1; i < 10000; ++i)
				{
					view.PreviousPageId = i;
					view.PreviousPageId.Should().Be(i);
				}
			}
		}

		[Test]
		public void TestNextPageIndex()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, 1024))
			using (var page = pages.Allocate(PageType.FreePageIndex))
			{
				var view = new FreePagesView(page);
				view.NextPageId.Should().Be(0);
				for (int i = 1; i < 10000; ++i)
				{
					view.NextPageId = i;
					view.NextPageId.Should().Be(i);
				}
			}
		}

		internal override IPageView CreateView(Page page)
		{
			return new FreePagesView(page);
		}

		internal override PageType PageType
		{
			get { return PageType.FreePageIndex; }
		}
	}
}