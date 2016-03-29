// ReSharper disable CheckNamespace

using System;
using System.IO;

namespace LocalStorage
// ReSharper restore CheckNamespace
{
	internal static class ReaderWriterExtensions
	{
		public static void Write(this BinaryWriter writer, DateTime value)
		{
			writer.Write((byte)value.Kind);
			writer.Write(value.Ticks);
		}

		public static DateTime ReadDateTime(this BinaryReader reader)
		{
			var kind = (DateTimeKind)reader.ReadByte();
			var ticks = reader.ReadInt64();
			return new DateTime(ticks, kind);
		}
	}
}