using System;
using System.ComponentModel;
using System.IO;
using FluentAssertions;
using LocalStorage.Paging;
using LocalStorage.Paging.Views;
using NUnit.Framework;

namespace LocalStorage.Test.Paging.Pages
{
	[TestFixture]
	public sealed class FreePagesViewTest
		: AbstractViewTest
	{
		[Test]
		public void TestCtor()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageCollection(stream, 1024))
			using (var page = pages.Allocate(PageType.FreePageIndex))
			using (var view = new FreePagesView(page))
			{
				view.PreviousPageIndex.Should().Be(0);
				view.NextPageIndex.Should().Be(0);
			}
		}

		[Test]
		public void TestPreviousPageIndex()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageCollection(stream, 1024))
			using (var page = pages.Allocate(PageType.FreePageIndex))
			using (var view = new FreePagesView(page))
			{
				view.PreviousPageIndex.Should().Be(0);
				for (int i = 1; i < 10000; ++i)
				{
					view.PreviousPageIndex = i;
					view.PreviousPageIndex.Should().Be(i);
				}
			}
		}

		[Test]
		public void TestNextPageIndex()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageCollection(stream, 1024))
			using (var page = pages.Allocate(PageType.FreePageIndex))
			using (var view = new FreePagesView(page))
			{
				view.NextPageIndex.Should().Be(0);
				for (int i = 1; i < 10000; ++i)
				{
					view.NextPageIndex = i;
					view.NextPageIndex.Should().Be(i);
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