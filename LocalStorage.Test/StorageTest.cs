using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace LocalStorage.Test
{
	[TestFixture]
	public class StorageTest
	{
		[Test]
		public void TestFromFile1()
		{
			using (var storage = EmbeddedStorage.FromFile(Path.GetTempFileName()))
			{
				storage.Should().NotBeNull();
				storage.IsReadOnly.Should().BeFalse();
			}
		}
	}
}