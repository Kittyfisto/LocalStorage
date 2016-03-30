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
		[Description("Verifies that a simple table can be added to the storage")]
		public void TestAddTable1()
		{
			var table = _embeddedStorage.Tables.Add<IntStringType>("Foobar");
			table.Should().NotBeNull();
			table.Name.Should().Be("Foobar");
			table.DataType.Should().Be<IntStringType>();
			table.Columns.Should().NotBeNull();

			table.Columns.ElementAt(0).Should().NotBeNull();
			table.Columns.ElementAt(0).Name.Should().Be("Index");
			table.Columns.ElementAt(0).DataType.Should().Be<int>();
			table.Columns.ElementAt(0).Should().BeOfType<Column<int>>();

			table.Columns.ElementAt(1).Should().NotBeNull();
			table.Columns.ElementAt(1).Name.Should().Be("Value");
			table.Columns.ElementAt(1).DataType.Should().Be<string>();
			table.Columns.ElementAt(1).Should().BeOfType<Column<string>>();
		}

		[Test]
		[Description("Verifies that a table can be added and read from the storage again")]
		public void TestAddTable2()
		{
			_embeddedStorage.Tables.Add<IntStringType>("Foobar");
			_embeddedStorage.Dispose();
			_stream.Position = 0;
			_embeddedStorage = EmbeddedStorage.FromStream(_stream, StorageMode.Open);

			_embeddedStorage.Tables.Count.Should().Be(1);
			var table = _embeddedStorage.Tables.First();
			table.Should().BeOfType<Table<IntStringType>>();
			table.Name.Should().Be("Foobar");
			table.DataType.Should().Be<IntStringType>();
		}
	}
}