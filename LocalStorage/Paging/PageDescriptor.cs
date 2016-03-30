using System;

namespace LocalStorage.Paging
{
	internal struct PageDescriptor : IEquatable<PageDescriptor>
	{
		public const int HeaderSize = sizeof (byte) + sizeof (int);
		public const int DefaultSize = 1024;

		public readonly long DataOffset;
		public readonly int Id;
		public readonly PageType Type;

		public PageDescriptor(int id, long dataOffset, PageType type)
		{
			Id = id;
			DataOffset = dataOffset;
			Type = type;
		}

		public PageDescriptor(PageDescriptor page, PageType newType)
		{
			Id = page.Id;
			DataOffset = page.DataOffset;
			Type = newType;
		}

		public bool Equals(PageDescriptor other)
		{
			return Id == other.Id && DataOffset == other.DataOffset && Type == other.Type;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is PageDescriptor && Equals((PageDescriptor) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Id;
				hashCode = (hashCode*397) ^ DataOffset.GetHashCode();
				hashCode = (hashCode*397) ^ (int) Type;
				return hashCode;
			}
		}

		public static bool operator ==(PageDescriptor left, PageDescriptor right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PageDescriptor left, PageDescriptor right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("{0} (#{1})@{2}",
			                     Type,
			                     Id,
			                     DataOffset);
		}
	}
}