using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace LocalStorage.Test
{
	[TestFixture]
	public sealed class TablesTest
	{
		private EmbeddedStorage _embeddedStorage;
		private MemoryStream _stream;

		[SetUp]
		public void SetUp()
		{
			_stream = new MemoryStream();
			_embeddedStorage = EmbeddedStorage.FromStream(_stream);
		}

		[Test]
		public void TestAddTable1()
		{
			var table = _embeddedStorage.Tables.Add<string>("Foobar");
			table.Should().NotBeNull();
			table.Name.Should().Be("Foobar");
			table.DataType.Should().Be<string>();

			_embeddedStorage.Dispose();
			_stream.Position = 0;
			_embeddedStorage = EmbeddedStorage.FromStream(_stream);

			_embeddedStorage.Tables.Count.Should().Be(1);
			table = _embeddedStorage.Tables.First();
			table.Should().BeOfType<ITable<string>>();
			table.Name.Should().Be("Foobar");
			table.DataType.Should().Be<string>();
		}
	}
}