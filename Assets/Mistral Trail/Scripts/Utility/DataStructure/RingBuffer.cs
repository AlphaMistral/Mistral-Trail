/*
 ### Mistral Trail System ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/08

 The following code is copied and revised from somewhere else.
 Don't fucking temper with it! 
 */


using System;
using System.Collections;
using System.Collections.Generic;

namespace Mistral.Utility.DataStructure
{
	/// <summary>
	/// The Object Pool based memory manager. 
	/// Please note that currently this RingBuffer has not been fully developed yet and thus runtime errors could occur without any 
	/// helpful exception generated. Good luck and have fun. 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class RingBuffer<T> : IEnumerable, IEnumerable<T>, ICollection<T>, IList<T>
	{
		#region Public Variables

		public int Capacity { get; private set; }
		public int Count { get; private set; }

		#endregion

		#region Private Variables

		private T[] buffer;
		private int index;
		private long version;

		#endregion

		#region ctor

		public RingBuffer(int capacity)
		{
			if (capacity > 0)
			{
				Capacity = capacity;
				buffer = new T[capacity];
			}
			else
			{
				throw new ArgumentException("RingBuffer的容量不能小于等于0!");
			}
		}

		#endregion

		#region RingBuffer Interfaces

		public void Add(T item)
		{
			buffer[index++ % Capacity] = item;
			if (Count < Capacity) Count++;
			version++;
		}

		public void Clear()
		{
			for (int i = 0; i < Count; i++)
			{
				buffer[i] = default(T);
			}
			index = 0;
			Count = 0;
			//Be aware that the version is never cleared to 0 ... 
			version++;
		}

		public bool Contains(T item)
		{
			return IndexOf(item) >= 0;
		}

		/// <summary>
		/// Don't you Ever query IndexOf(NULL). 
		/// Otherwise I will hate you.
		/// Trust me, it won't work! 
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf(T item)
		{
			/// 2 b continued ... 
			if (item == null)
			{

			}
			for (int i = 0; i < Count; i++)
			{
				T temp = buffer[(index - Count + i) % Capacity];
				if (item != null && item.Equals(temp))
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Copies the whole Ring Buffer to an array. 
		/// However if the array's capacity is too small ... 
		/// I dunno what 2 do. (i am lazy :( )
		/// </summary>
		/// <param name="target"></param>
		/// <param name="startIndex"></param>
		public void CopyTo(T[] target, int startIndex)
		{
			for (int i = 0; i < Count; i++)
			{
				target[i + startIndex] = buffer[(index - Count + i) % Capacity];
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			long storedVersion = version;
			for (int i = 0; i < Count; i++)
			{
				if (version != storedVersion)
				{
					throw new InvalidOperationException("Ring Buffer 被外部修改, 退出!");
				}
				yield return this[i];
			}
		}

		public T this[int idx]
		{
			get
			{
				return buffer[(index - Count + idx) % Capacity];
			}
			set
			{
				Insert(idx, value);
			}
		}

		public void Insert(int idx, T item)
		{
			if (idx < 0 || idx > Count)
			{
				throw new IndexOutOfRangeException();
			}

			if (index == Count)
			{
				Add(item);
				return;
			}

			int count = Math.Min(Count, Capacity - 1) - idx;

			int id = (index - Count + idx) % Capacity;

			for (int i = id + count; i > id; i--)
			{
				buffer[i % Capacity] = buffer[(i - 1) % Capacity];
			}

			buffer[id] = item;
			if (Count < Capacity)
			{
				Count++;
				index++;
			}

			version++;
		}

		public bool Remove(T item)
		{
			int position = IndexOf(item);
			if (index < 0)
				return false;
			RemoveAt(position);
			return true;
		}

		public void RemoveAt(int idx)
		{
			if (idx < 0 || idx > Count)
			{
				throw new IndexOutOfRangeException();
			}
			for (int i = idx; i < Count - 1; i++)
			{
				buffer[(index - Count + i) % Capacity] = buffer[(index - Count + i + 1) % Capacity];
			}
			buffer[(index - 1) % Capacity] = default(T);
			index--;
			Count--;
			version++;
		}

		/// <summary>
		/// Why did I implement this? 
		/// Holy something and son of someone. 
		/// </summary>
		bool ICollection<T>.IsReadOnly { get { return false; } }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		#endregion
	}
}