using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using LocalStorage.Paging;
using NUnit.Framework;

namespace LocalStorage.Test.Paging
{
	[TestFixture]
	public sealed class PageCollectionTest
	{
		[Test]
		[Repeat(100)]
		[Description("Verifies that creating a page collection on a previously used stream restores all page descriptors")]
		public void TestOpen([Values(1, 12, 1024, 10000)] int numPages, [Values(1024, 2048)] int pageLength)
		{
			var stream = new MemoryStream();
			var pages = new PageCollection(stream, pageLength);

			var descriptors = new List<PageDescriptor>();
			for (int i = 0; i < numPages; ++i)
			{
				var page = pages.Allocate(PageType.TableDescriptor);
				descriptors.Add(page.Descriptor);
			}

			pages.Dispose();
			stream.Position.Should().Be(numPages*pageLength);

			stream.Position = 0;
			pages = new PageCollection(stream, pageLength);
			pages.Pages.Count().Should().Be(numPages);
			pages.Pages.Should().Equal(descriptors);
		}

		[Test]
		public void TestAllocateOnePage([Values(10, 20, 128, 1024, 2048, 100000)] int pageLength)
		{
			// page sequence number + page length + page type
			int headerSize = sizeof(uint) + sizeof(PageType);

			var stream = new MemoryStream();
			var pages = new PageCollection(stream, pageLength);

			Page page = pages.Allocate(PageType.TableDescriptor);
			page.Wait();
			stream.Length.Should().Be(pageLength);
			page.Descriptor.Id.Should().Be(0);
			page.Descriptor.DataOffset.Should().Be(headerSize);
			page.Descriptor.Type.Should().Be(PageType.TableDescriptor);

			page.Position.Should()
			    .Be(0, "Because the read/write pointer of a page should point to the start of the page data segment");
			page.Length.Should().Be(pageLength - PageDescriptor.HeaderSize, "Because a page's readable/writable area excludes its header region");

			var data = new byte[pageLength - PageDescriptor.HeaderSize];
			page.Read(data, 0, data.Length).Should().Be(data.Length);
			data.Should().Equal(Enumerable.Range(0, data.Length).Select(unused => (byte) 0).ToArray(),
			                    "Because the page's memory should be zeroed out by default");

			stream.Position = 0;
			using (var reader = new BinaryReader(stream))
			{
				reader.ReadInt32()
				      .Should()
				      .Be(page.Descriptor.Id, "Because the first 4 bytes should represent the page sequence number");
				reader.ReadByte().Should().Be((byte) page.Descriptor.Type, "Because the next byte should represent the page type");
			}
		}

		[Test]
		public void TestAllocateTwoPages([Values(128, 1024, 2048)] int pageLength)
		{
			var stream = new MemoryStream();
			var pages = new PageCollection(stream, pageLength);

			var first = pages.Allocate(PageType.TableDescriptor);
			first.Should().NotBeNull();
			first.Descriptor.DataOffset.Should().Be(PageDescriptor.HeaderSize);

			var second = pages.Allocate(PageType.TableDescriptor);
			second.Should().NotBeNull();
			second.Descriptor.DataOffset.Should().Be(pageLength + PageDescriptor.HeaderSize);

			using (var reader = new BinaryReader(stream))
			{
				first.Wait();
				second.Wait();

				stream.Position = 0;
				reader.ReadInt32()
					  .Should()
					  .Be(first.Descriptor.Id, "Because the first 4 bytes should represent the page sequence number");
				reader.ReadByte().Should().Be((byte)first.Descriptor.Type, "Because the next byte should represent the page type");

				stream.Position = pageLength;
				reader.ReadInt32()
					  .Should()
					  .Be(second.Descriptor.Id, "Because the first 4 bytes should represent the page sequence number");
				reader.ReadByte().Should().Be((byte)second.Descriptor.Type, "Because the next byte should represent the page type");
			}
		}

		[Test]
		[Ignore("TODO")]
		public void TestAllocateFreeAllocate()
		{
			
		}
	}
}