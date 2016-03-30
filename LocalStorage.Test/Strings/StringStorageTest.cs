using System.IO;
using System.Linq;
using FluentAssertions;
using LocalStorage.Paging;
using LocalStorage.Strings;
using NUnit.Framework;

namespace LocalStorage.Test.Strings
{
	[TestFixture]
	public sealed class StringStorageTest
	{
		private MemoryStream _stream;
		private PageStorage _pages;
		private StringStorage _strings;

		[SetUp]
		public void SetUp()
		{
			_stream = new MemoryStream();
			_pages = new PageStorage(_stream, 1024);
			_strings = new StringStorage(_pages);
		}

		[Test]
		[Description("Verifies that a single string can be added and retrieved again")]
		public void TestAdd1()
		{
			_strings.Add("Foobar").Should().Be(1);
			_strings.Load(1).Should().Be("Foobar");
		}

		[Test]
		[Description("Verifies that multiple strings that require at least two pages may be added")]
		public void TestAdd2()
		{
			var value1 = new string(Enumerable.Range(0, 500).Select(i => 'z').ToArray());
			int first = _strings.Add(value1);
			var value2 = new string(Enumerable.Range(0, 500).Select(i => 'a').ToArray());
			int second = _strings.Add(value2);

			_pages.Count.Should().Be(2);

			_strings.Load(first).Should().Be(value1);
			_strings.Load(second).Should().Be(value2);
		}
	}
}