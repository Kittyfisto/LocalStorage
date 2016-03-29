using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using LocalStorage.Paging;
using NUnit.Framework;

namespace LocalStorage.Test.Paging
{
	[TestFixture]
	public sealed class PageCollectionAcceptanceTest
	{
		[Test]
		[Description("Verifies that creating a page collection on a previously used stream restores all page descriptors")]
		public void TestOpen([Values(1, 32, 1024)] int numPages, [Values(1024, 2048)] int pageLength)
		{
			for (int x = 0; x < 1000; ++x)
			{
				var descriptors = new List<PageDescriptor>();
				using (var stream = new MemoryStream())
				{
					using (var pages = new PageCollection(stream))
					{
						for (int i = 0; i < numPages; ++i)
						{
							var page = pages.Allocate(PageType.TableDescriptor, pageLength);
							descriptors.Add(page.Descriptor);
						}
					}

					stream.Position.Should().Be(numPages * pageLength + numPages * PageDescriptor.HeaderSize);
					stream.Position = 0;

					using (var pages = new PageCollection(stream))
					{
						pages.Pages.Count().Should().Be(numPages);
						pages.Pages.Should().Equal(descriptors);
					}
				}
			}
		}
	}
}