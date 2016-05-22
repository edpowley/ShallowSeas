using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShallowNet
{
	public abstract class Base64Array<T> : List<T>
	{
		protected abstract int ValueSize { get; }
		protected abstract byte[] valueToBytes(T value);
		protected abstract T bytesToValue(byte[] bytes, int index);

		private string serialise()
		{
			byte[] bytes = new byte[Count * ValueSize];
			for(int i=0;i< Count;i++)
			{
				byte[] valueBytes = valueToBytes(this[i]);
				System.Diagnostics.Trace.Assert(valueBytes.Length == ValueSize);
				valueBytes.CopyTo(bytes, i * ValueSize);
			}

			return Convert.ToBase64String(bytes);
		}

		private void deserialise(string data)
		{
			Clear();
			byte[] bytes = Convert.FromBase64String(data);
			for (int index = 0; index < bytes.Length; index += ValueSize)
			{
				T value = bytesToValue(bytes, index);
				Add(value);
			}
		}

		public static void register<S>() where S : Base64Array<T>, new()
		{
			fastJSON.JSON.RegisterCustomType(typeof(Base64Array<T>), serialise, deserialise<S>);
		}

		private static string serialise(object data)
		{
			Base64Array<T> ob = (Base64Array<T>)data;
			return ob.serialise();
		}

		private static object deserialise<S>(string data) where S : Base64Array<T>, new()
		{
			S result = new S();
			result.deserialise(data);
			return result;
		}
	}

	public class Base64Array_float : Base64Array<float>
	{
		static Base64Array_float()
		{
			register<Base64Array_float>();
		}

		protected override int ValueSize { get { return sizeof(float); } }

		protected override float bytesToValue(byte[] bytes, int index)
		{
			return BitConverter.ToSingle(bytes, index);
		}

		protected override byte[] valueToBytes(float value)
		{
			return BitConverter.GetBytes(value);
		}
	}
}
