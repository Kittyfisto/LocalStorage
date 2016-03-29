using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace LocalStorage.Paging
{
	/// <summary>
	/// Represents an operation on a page in the source stream.
	/// </summary>
	internal struct PageOperation
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public enum Type
		{
			Read,
			Write
		}

		public readonly byte[] Data;
		public readonly PageDescriptor Descriptor;
		public readonly Type Kind;
		private readonly TaskCompletionSource<int> _taskInfo;

		public override string ToString()
		{
			if (Kind == Type.Read)
			{
				return string.Format("Read @{0}, {1} bytes", Descriptor.Offset, Descriptor.Size);
			}

			return string.Format("Write @{0}, {1} bytes", Descriptor.Offset, Descriptor.Size);
		}

		private PageOperation(Type type, PageDescriptor descriptor, byte[] data)
		{
			Kind = type;
			Descriptor = descriptor;
			Data = data;
			_taskInfo = new TaskCompletionSource<int>();
		}

		public Task Task
		{
			get { return _taskInfo.Task; }
		}

		public static PageOperation Read(PageDescriptor descriptor, byte[] data)
		{
			return new PageOperation(Type.Read, descriptor, data);
		}

		public static PageOperation Write(PageDescriptor descriptor, byte[] data)
		{
			return new PageOperation(Type.Write, descriptor, data);
		}

		public void Execute(Stream sourceStream)
		{
			try
			{
				sourceStream.Position = Descriptor.Offset;

				switch (Kind)
				{
				case Type.Read:
					sourceStream.Read(Data, 0, Data.Length);
					break;

				case Type.Write:
					sourceStream.Write(Data, 0 , Data.Length);
					break;

				default:
					Log.ErrorFormat("Unexpected operation: {0}", Kind);
					break;
				}

				_taskInfo.SetResult(42);
			}
			catch (Exception e)
			{
				_taskInfo.SetException(e);
			}
		}
	}
}