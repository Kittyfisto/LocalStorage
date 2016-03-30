using System;
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
		[Repeat(100)]
		[Description("Verifies that the contents of a page are only written to the base stream after the page has been disposed of")]
		public void TestPageWriteAndDispose()
		{
			const int dataLength = 128;
			const int pageLength = dataLength + PageDescriptor.HeaderSize;

			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, pageLength))
			{
				var data = Enumerable.Range(0, dataLength).Select(x => (byte)x).ToArray();
				var streamData = new byte[dataLength];

				using (var page = pages.Allocate(PageType.Invalid))
				{
					new Action(() => page.Write(data, 0, data.Length))
						.ShouldNotThrow("Because writing data to a page may never fail");

					page.Position = 0;
					var actualData = new byte[dataLength];
					page.Read(actualData, 0, actualData.Length).Should().Be(
						dataLength, "Because reading data from a page should always work in one go");
					actualData.Should().Equal(data, "Because the data written to the page should've immediately appeared in the page's buffer");

					stream.Length.Should().Be(dataLength + PageDescriptor.HeaderSize,
						"Because the actual stream should've been resized to accomodate the page's data");
					stream.Read(streamData, 0, dataLength);
					streamData.Should().Equal(new byte[dataLength], "Because the page has not yet been persisted yet");

					page.Commit();
				}

				stream.Position = PageDescriptor.HeaderSize;
				stream.Read(streamData, 0, dataLength).Should().Be(dataLength);
				streamData.Should().Equal(data, "Because the page should've been written to the base stream after having been disposed of");
			}
		}

		[Test]
		[Repeat(100)]
		[Description("Verifies that reading the contents of a previously written page works")]
		public void TestWriteAndRead()
		{
			const int dataLength = 128;
			const int pageLength = dataLength + PageDescriptor.HeaderSize;

			using (var stream = new MemoryStream())
			using (var pages = new PageStorage(stream, pageLength))
			{
				var data = Enumerable.Range(0, dataLength).Select(x => (byte)x).ToArray();
				PageDescriptor descriptor;

				using (var page = pages.Allocate(PageType.Invalid))
				{
					descriptor = page.Descriptor;

					new Action(() => page.Write(data, 0, data.Length))
						.ShouldNotThrow("Because writing data to a page may never fail");

					page.Commit();
				}

				using (var page = pages.Load(descriptor))
				{
					var actualData = new byte[dataLength];
					page.Read(actualData, 0, actualData.Length).Should().Be(
						dataLength, "Because reading data from a page should always work in one go");
					actualData.Should().Equal(data, "Because the data written to the page should've immediately appeared in the page's buffer");
				}
			}
		}

		[Test]
		[Description("Verifies that creating a page collection on a previously used stream restores all page descriptors")]
		public void TestOpen([Values(1, 32, 1024)] int numPages, [Values(1024, 2048)] int pageLength)
		{
			var descriptors = new List<PageDescriptor>();
			using (var stream = new MemoryStream())
			{
				using (var pages = new PageStorage(stream, pageLength))
				{
					for (int i = 0; i < numPages; ++i)
					{
						var page = pages.Allocate(PageType.TableDescriptor);
						descriptors.Add(page.Descriptor);
					}
				}

				stream.Position.Should().Be(numPages * pageLength);
				stream.Position = 0;

				using (var pages = new PageStorage(stream, pageLength))
				{
					pages.Pages.Count().Should().Be(numPages);
					pages.Pages.Should().Equal(descriptors);
				}
			}
		}
	}
}