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
		[SetUp]
		public void SetUp()
		{
			_stream = new MemoryStream();
			_pages = new PageCollection(_stream);
		}

		private MemoryStream _stream;
		private PageCollection _pages;

		[Test]
		[Repeat(100)]
		[Description("Verifies that creating a page collection on a previously used stream restores all page descriptors")]
		public void TestOpen([Values(1, 12, 1024, 10000)] int numPages, [Values(1024, 2048)] int pageLength)
		{
			var descriptors = new List<PageDescriptor>();
			for (int i = 0; i < numPages; ++i)
			{
				var page = _pages.Allocate(PageType.TableDescriptor, pageLength);
				descriptors.Add(page.Descriptor);
			}

			_pages.Dispose();
			_stream.Position.Should().Be(numPages*pageLength + numPages*PageDescriptor.HeaderSize);

			_stream.Position = 0;
			_pages = new PageCollection(_stream);
			_pages.Pages.Count().Should().Be(numPages);
			_pages.Pages.Should().Equal(descriptors);
		}

		[Test]
		public void TestAllocateOnePage([Values(1, 10, 20, 128, 1024, 2048, 100000)] int length)
		{
			// page sequence number + page length + page type
			int headerSize = sizeof (uint) + sizeof (int) + sizeof (PageType);

			Page page = _pages.Allocate(PageType.TableDescriptor, length);
			page.Wait();
			_stream.Length.Should().Be(length + headerSize);
			page.Descriptor.Id.Should().Be(0);
			page.Descriptor.DataOffset.Should().Be(headerSize);
			page.Descriptor.DataSize.Should().Be(length);
			page.Descriptor.Type.Should().Be(PageType.TableDescriptor);

			page.Position.Should()
			    .Be(0, "Because the read/write pointer of a page should point to the start of the page data segment");
			page.Length.Should().Be(length);

			var data = new byte[length];
			page.Read(data, 0, length).Should().Be(length);
			data.Should().Equal(Enumerable.Range(0, length).Select(unused => (byte) 0).ToArray(),
			                    "Because the page's memory should be zeroed out by default");

			_stream.Position = 0;
			using (var reader = new BinaryReader(_stream))
			{
				reader.ReadInt32()
				      .Should()
				      .Be(page.Descriptor.Id, "Because the first 4 bytes should represent the page sequence number");
				reader.ReadInt32().Should().Be(length, "Because the next 4 bytes should represent the page data size");
				reader.ReadByte().Should().Be((byte) page.Descriptor.Type, "Because the next byte should represent the page type");
			}
		}

		[Test]
		public void TestAllocateTwoPages([Values(128, 1024, 2048)] int firstPageLength,
		                                 [Values(128, 2014, 2048)] int secondPageLength)
		{
			var first = _pages.Allocate(PageType.TableDescriptor, firstPageLength);
			first.Should().NotBeNull();
			first.Descriptor.DataOffset.Should().Be(PageDescriptor.HeaderSize);
			first.Descriptor.DataSize.Should().Be(firstPageLength);

			var second = _pages.Allocate(PageType.TableDescriptor, secondPageLength);
			second.Should().NotBeNull();
			second.Descriptor.DataOffset.Should().Be(PageDescriptor.HeaderSize + firstPageLength + PageDescriptor.HeaderSize);
			second.Descriptor.DataSize.Should().Be(secondPageLength);

			using (var reader = new BinaryReader(_stream))
			{
				first.Wait();
				second.Wait();

				_stream.Position = 0;
				reader.ReadInt32()
					  .Should()
					  .Be(first.Descriptor.Id, "Because the first 4 bytes should represent the page sequence number");
				reader.ReadInt32().Should().Be(firstPageLength, "Because the next 4 bytes should represent the page data size");
				reader.ReadByte().Should().Be((byte)first.Descriptor.Type, "Because the next byte should represent the page type");

				_stream.Position = PageDescriptor.HeaderSize + firstPageLength;
				reader.ReadInt32()
					  .Should()
					  .Be(second.Descriptor.Id, "Because the first 4 bytes should represent the page sequence number");
				reader.ReadInt32().Should().Be(secondPageLength, "Because the next 4 bytes should represent the page data size");
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