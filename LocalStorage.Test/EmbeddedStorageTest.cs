using System;
using System.IO;
using FluentAssertions;
using LocalStorage.Paging.Views;
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
				storage.Header.Should().NotBeNull();
				storage.Header.StorageVersion.Should().Be(StorageHeaderView.CurrentStorageVersion);
				storage.Header.CreationTime.Should().BeOnOrAfter(before);
				storage.Header.CreationTime.Should().BeOnOrBefore(after);
			}
		}

		[Test]
		[Description("Verifies that opening a storage on a previously created file works")]
		public void TestFromFile2()
		{
			IStorageHeader header;
			var fname = Path.GetTempFileName();
			using (var storage = EmbeddedStorage.FromFile(fname, StorageMode.Create))
			{
				header = storage.Header;
			}

			using (var storage = EmbeddedStorage.FromFile(fname, StorageMode.Open))
			{
				storage.Should().NotBeNull();
				storage.Header.Should().NotBeNull();
				storage.Header.StorageVersion.Should().Be(header.StorageVersion);
				storage.Header.CreationTime.Should().Be(header.CreationTime);
			}
		}
	}
}