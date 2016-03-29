using System;
using System.Threading.Tasks;

namespace LocalStorage.Paging
{
	/// <summary>
	/// Represents an operation on a page in the source stream.
	/// </summary>
	internal struct PageOperation
	{
		public enum Type
		{
			Read,
			Write,
			RestoreIndex
		}

		public readonly byte[] Data;
		public readonly PageDescriptor Descriptor;
		public readonly Type Kind;
		private readonly TaskCompletionSource<int> _taskInfo;

		public override string ToString()
		{
			if (Kind == Type.Read)
			{
				return string.Format("Read @{0}, {1} bytes", Descriptor.DataOffset, Descriptor.DataSize);
			}

			return string.Format("Write @{0}, {1} bytes", Descriptor.DataOffset, Descriptor.DataSize);
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

		public static PageOperation RestoreIndex()
		{
			return new PageOperation(Type.RestoreIndex, new PageDescriptor(), null);
		}

		public void SetFinished()
		{
			_taskInfo.SetResult(42);
		}

		public void SetException(Exception exception)
		{
			_taskInfo.SetException(exception);
		}
	}
}