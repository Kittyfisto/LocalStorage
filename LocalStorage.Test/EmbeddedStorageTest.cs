using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace LocalStorage.Test
{
	[TestFixture]
	public class EmbeddedStorageTest
	{
		[Test]
		[Description("Verifies that creating a storage on a non-existing file works")]
		public void TestFromFile1()
		{
			var before = DateTime.Now;
			using (var storage = EmbeddedStorage.FromFile(Path.GetTempFileName(), StorageMode.Create))
			{
				var after = DateTime.Now;

				storage.Should().NotBeNull();
				storage.IsReadOnly.Should().BeFalse();
				storage.Descriptor.Should().NotBeNull();
				storage.Descriptor.StorageVersion.Should().Be(StorageDescriptor.CurrentStorageVersion);
				storage.Descriptor.CreationTime.Should().BeOnOrAfter(before);
				storage.Descriptor.CreationTime.Should().BeOnOrBefore(after);
			}
		}

		[Test]
		[Description("Verifies that opening a storage on a previously created file works")]
		public void TestFromFile2()
		{
			IStorageDescriptor descriptor;
			var fname = Path.GetTempFileName();
			using (var storage = EmbeddedStorage.FromFile(fname, StorageMode.Create))
			{
				descriptor = storage.Descriptor;
			}

			using (var storage = EmbeddedStorage.FromFile(fname, StorageMode.Open))
			{
				storage.Should().NotBeNull();
				storage.Descriptor.Should().NotBeNull();
				storage.Descriptor.StorageVersion.Should().Be(descriptor.StorageVersion);
				storage.Descriptor.CreationTime.Should().Be(descriptor.CreationTime);
			}
		}
	}
}