using System.IO;
using FluentAssertions;
using LocalStorage.Paging;
using NUnit.Framework;

namespace LocalStorage.Test.Paging
{
	[TestFixture]
	public sealed class PageCollectionTest
	{
		private MemoryStream _stream;
		private PageCollection _pages;

		[SetUp]
		public void SetUp()
		{
			_stream = new MemoryStream();
			_pages = new PageCollection(_stream);
		}

		[Test]
		public void TestAllocate1([Values(1, 10, 20, 128, 1024, 2048, 100000)] int length)
		{
			// page sequence number + page length
			int headerSize = sizeof (uint) + sizeof(int);

			_pages.Allocate(length);
			_stream.Length.Should().Be(length + headerSize);
		}
	}
}