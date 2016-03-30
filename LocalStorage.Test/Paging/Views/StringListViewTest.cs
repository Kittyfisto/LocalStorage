using System;
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
			using (var pages = new PageStorage(stream, 1024))
			using (var page = pages.Allocate(PageType.StringList))
			{
				var view = new StringListView(page);
				view.TryAdd(1, "Foo").Should().BeTrue();
				view.TryAdd(2, "Bar").Should().BeTrue();
				view.TryAdd(3, "Clondyke Bar").Should().BeTrue();

				string value;
				view.TryFind(3, out value).Should().BeTrue();
				value.Should().Be("Clondyke Bar");

				view.TryFind(1, out value).Should().BeTrue();
				value.Should().Be("Foo");

				view.TryFind(2, out value).Should().BeTrue();
				value.Should().Be("Bar");
			}
		}

		[Test]
		[Description("Verifies that a view actually uses the underlying page to store its data")]
		public void TestAddLoad2()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, 1024))
			using (var page = pages.Allocate(PageType.StringList))
			{
				var view = new StringListView(page);
				view.TryAdd(1, "Foo").Should().BeTrue();
				view.TryAdd(2, "Bar").Should().BeTrue();
				view.TryAdd(3, "Clondyke Bar").Should().BeTrue();

				// We deliberately create a new view on the same page to test
				// that the view actually uses the page as the data source to store
				// all those strings
				view = new StringListView(page);

				string value;
				view.TryFind(3, out value).Should().BeTrue();
				value.Should().Be("Clondyke Bar");

				view.TryFind(1, out value).Should().BeTrue();
				value.Should().Be("Foo");

				view.TryFind(2, out value).Should().BeTrue();
				value.Should().Be("Bar");
			}
		}

		[Test]
		[Description("Verifies that using an index of 0 is not allowed")]
		public void TestAdd1()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, 1024))
			using (var page = pages.Allocate(PageType.StringList))
			{
				var view = new StringListView(page);
				new Action(() => view.TryAdd(0, "FOobar"))
					.ShouldThrow<ArgumentOutOfRangeException>()
					.WithMessage("An index must be greater than 0\r\nParameter name: index");
			}
		}
	}
}