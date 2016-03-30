using System.IO;
using System.Linq;
using FluentAssertions;
using LocalStorage.Tables;
using LocalStorage.Test.Classes;
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
			_embeddedStorage = EmbeddedStorage.FromStream(_stream, StorageMode.Create);
		}

		[Test]
		public void TestAddTable1()
		{
			var table = _embeddedStorage.Tables.Add<IntStringType>("Foobar");
			table.Should().NotBeNull();
			table.Name.Should().Be("Foobar");
			table.DataType.Should().Be<IntStringType>();

			_embeddedStorage.Dispose();
			_stream.Position = 0;
			_embeddedStorage = EmbeddedStorage.FromStream(_stream, StorageMode.Open);

			_embeddedStorage.Tables.Count.Should().Be(1);
			table = _embeddedStorage.Tables.First();
			table.Should().BeOfType<Table<IntStringType>>();
			table.Name.Should().Be("Foobar");
			table.DataType.Should().Be<IntStringType>();
		}
	}
}