using System.IO;
using FluentAssertions;
using LocalStorage.Paging;
using LocalStorage.Paging.Views;
using NUnit.Framework;

namespace LocalStorage.Test.Paging.Views
{
	[TestFixture]
	public sealed class StringListViewTest
		: AbstractViewTest
	{
		internal override IPageView CreateView(Page page)
		{
			return new StringListView(page);
		}

		internal override PageType PageType
		{
			get { return PageType.StringList; }
		}

		[Test]
		[Description("Verifies that multiple strings may be stored in one page and be loaded again without a problem")]
		public void TestAddLoad1()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageCollection(stream, 1024))
			using (var page = pages.Allocate(PageType.StringList))
			{
				var view = new StringListView(page);
				view.Add(0, "Foo");
				view.Add(1, "Bar");
				view.Add(2, "Clondyke Bar");

				view.Find(2).Should().Be("Clondyke Bar");
				view.Find(0).Should().Be("Foo");
				view.Find(1).Should().Be("Bar");
			}
		}

		[Test]
		[Description("Verifies that a view actually uses the underlying page to store its data")]
		public void TestAddLoad2()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageCollection(stream, 1024))
			using (var page = pages.Allocate(PageType.StringList))
			{
				var view = new StringListView(page);
				view.Add(0, "Foo");
				view.Add(1, "Bar");
				view.Add(2, "Clondyke Bar");

				// We deliberately create a new view on the same page to test
				// that the view actually uses the page as the data source to store
				// all those strings
				view = new StringListView(page);
				view.Find(2).Should().Be("Clondyke Bar");
				view.Find(0).Should().Be("Foo");
				view.Find(1).Should().Be("Bar");
			}
		}
	}
}