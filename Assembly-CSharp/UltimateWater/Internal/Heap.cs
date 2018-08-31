using System;
using System.Collections;
using System.Collections.Generic;

namespace UltimateWater.Internal
{
	public sealed class Heap<T> : IEnumerable<T>, IEnumerable where T : IComparable<T>
	{
		public Heap() : this(8)
		{
		}

		public Heap(int capacity)
		{
			this._Elements = new T[capacity];
		}

		public int Count
		{
			get
			{
				return this._NumElements;
			}
		}

		public T Max
		{
			get
			{
				return this._Elements[0];
			}
		}

		public T ExtractMax()
		{
			if (this._NumElements == 0)
			{
				throw new InvalidOperationException("Heap is empty.");
			}
			T result = this._Elements[0];
			this._Elements[0] = this._Elements[--this._NumElements];
			this._Elements[this._NumElements] = default(T);
			this.BubbleDown(0);
			return result;
		}

		public void Insert(T element)
		{
			if (this._Elements.Length == this._NumElements)
			{
				this.Resize(this._Elements.Length * 2);
			}
			this._Elements[this._NumElements++] = element;
			this.BubbleUp(this._NumElements - 1, element);
		}

		public void Remove(T element)
		{
			for (int i = 0; i < this._NumElements; i++)
			{
				if (this._Elements[i].Equals(element))
				{
					this._Elements[i] = this._Elements[--this._NumElements];
					this._Elements[this._NumElements] = default(T);
					this.BubbleDown(i);
					return;
				}
			}
		}

		public void Clear()
		{
			this._NumElements = 0;
		}

		public T[] GetUnderlyingArray()
		{
			return this._Elements;
		}

		public void Resize(int len)
		{
			Array.Resize<T>(ref this._Elements, len);
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (this._Elements.Length != this._NumElements)
			{
				Array.Resize<T>(ref this._Elements, this._NumElements);
			}
			return this._Elements.GetEnumerator();
		}

		private void BubbleUp(int index, T element)
		{
			while (index != 0)
			{
				int num = index - 1 >> 1;
				T other = this._Elements[num];
				if (element.CompareTo(other) <= 0)
				{
					return;
				}
				this._Elements[index] = this._Elements[num];
				this._Elements[num] = element;
				index = num;
			}
		}

		private void BubbleDown(int index)
		{
			T t = this._Elements[index];
			for (int i = (index << 1) + 1; i < this._NumElements; i = (index << 1) + 1)
			{
				T t2 = this._Elements[i];
				int num;
				if (i + 1 < this._NumElements)
				{
					T t3 = this._Elements[i + 1];
					if (t2.CompareTo(t3) > 0)
					{
						num = i;
					}
					else
					{
						t2 = t3;
						num = i + 1;
					}
				}
				else
				{
					num = i;
				}
				if (t.CompareTo(t2) >= 0)
				{
					return;
				}
				this._Elements[num] = t;
				this._Elements[index] = t2;
				index = num;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (this._Elements.Length != this._NumElements)
			{
				Array.Resize<T>(ref this._Elements, this._NumElements);
			}
			return this._Elements.GetEnumerator();
		}

		private T[] _Elements;

		private int _NumElements;
	}
}
