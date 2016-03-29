using System;

namespace LocalStorage.Paging
{
	internal struct PageDescriptor : IEquatable<PageDescriptor>
	{
		public static readonly int HeaderSize = sizeof (PageType) + sizeof (int) + sizeof (int);
		public readonly long DataOffset;
		public readonly int DataSize;
		public readonly int Id;
		public readonly PageType Type;

		public PageDescriptor(int id, long dataOffset, int dataSize, PageType type)
		{
			if (dataSize < 0)
				throw new ArgumentOutOfRangeException("dataSize");

			Id = id;
			DataOffset = dataOffset;
			DataSize = dataSize;
			Type = type;
		}

		public PageDescriptor(PageDescriptor page, PageType newType)
		{
			Id = page.Id;
			DataOffset = page.DataOffset;
			DataSize = page.DataSize;
			Type = newType;
		}

		public bool Equals(PageDescriptor other)
		{
			return Id == other.Id && DataOffset == other.DataOffset && DataSize == other.DataSize && Type == other.Type;
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
				var hashCode = (int) Id;
				hashCode = (hashCode*397) ^ DataOffset.GetHashCode();
				hashCode = (hashCode*397) ^ DataSize;
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
			return string.Format("{0} (#{1})@{2}, {3} bytes",
			                     Type,
			                     Id,
			                     DataOffset,
			                     DataSize);
		}
	}
}