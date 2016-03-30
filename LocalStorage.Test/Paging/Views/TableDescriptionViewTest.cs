using System.IO;
using FluentAssertions;
using LocalStorage.Paging;
using LocalStorage.Paging.Views;
using NUnit.Framework;

namespace LocalStorage.Test.Paging.Views
{
	[TestFixture]
	public sealed class TableDescriptionViewTest
		: AbstractViewTest
	{
		internal override IPageView CreateView(Page page)
		{
			return new TableDescriptionView(page);
		}

		internal override PageType PageType
		{
			get { return PageType.TableDescriptor; }
		}

		[Test]
		public void TestProperties()
		{
			using (var stream = new MemoryStream())
			using (var pages = new PageCollection(stream, 1024))
			using (var page = pages.Allocate(PageType.TableDescriptor))
			{
				var view = new TableDescriptionView(page);
				view.ColumnCount = 1;
				view.DataTypeIndex = 2;
				view.NextTableDescriptionId = 3;
				view.TableNameIndex = 4;

				view.ColumnCount.Should().Be(1);
				view.DataTypeIndex.Should().Be(2);
				view.NextTableDescriptionId.Should().Be(3);
				view.TableNameIndex.Should().Be(4);
			}
		}
	}
}