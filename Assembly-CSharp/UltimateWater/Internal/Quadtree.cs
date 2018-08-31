using System;
using UnityEngine;

namespace UltimateWater.Internal
{
	public class Quadtree<T> where T : class, IPoint2D
	{
		public Quadtree(Rect rect, int maxElementsPerNode, int maxTotalElements)
		{
			this._Root = this;
			this.Rect = rect;
			this._Elements = new T[maxElementsPerNode];
			this._NumElements = 0;
			this._FreeSpace = maxTotalElements;
		}

		private Quadtree(Quadtree<T> root, Rect rect, int maxElementsPerNode) : this(rect, maxElementsPerNode, 0)
		{
			this._Root = root;
		}

		public Rect Rect
		{
			get
			{
				return this._Rect;
			}
			set
			{
				this._Rect = value;
				this._Center = this._Rect.center;
				this._MarginRect = this._Rect;
				float num = this._Rect.width * 0.0025f;
				this._MarginRect.xMin = this._MarginRect.xMin - num;
				this._MarginRect.yMin = this._MarginRect.yMin - num;
				this._MarginRect.xMax = this._MarginRect.xMax + num;
				this._MarginRect.yMax = this._MarginRect.yMax + num;
				if (this._A != null)
				{
					float width = this._Rect.width * 0.5f;
					float height = this._Rect.height * 0.5f;
					this._A.Rect = new Rect(this._Rect.xMin, this._Center.y, width, height);
					this._B.Rect = new Rect(this._Center.x, this._Center.y, width, height);
					this._C.Rect = new Rect(this._Rect.xMin, this._Rect.yMin, width, height);
					this._D.Rect = new Rect(this._Center.x, this._Rect.yMin, width, height);
				}
			}
		}

		public int Count
		{
			get
			{
				return (this._A == null) ? this._NumElements : (this._A.Count + this._B.Count + this._C.Count + this._D.Count);
			}
		}

		public int FreeSpace
		{
			get
			{
				return this._Root._FreeSpace;
			}
		}

		public bool AddElement(T element)
		{
			Vector2 position = element.Position;
			if (this._Root._FreeSpace <= 0 || float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsInfinity(position.x) || float.IsInfinity(position.y))
			{
				element.Destroy();
				return false;
			}
			if (this._Root == this && !this._Rect.Contains(element.Position))
			{
				this.ExpandToContain(element.Position);
			}
			return this.AddElementInternal(element);
		}

		public void UpdateElements(Quadtree<T> root)
		{
			if (this._A != null)
			{
				this._A.UpdateElements(root);
				this._B.UpdateElements(root);
				this._C.UpdateElements(root);
				this._D.UpdateElements(root);
			}
			if (this._Elements != null)
			{
				for (int i = 0; i < this._Elements.Length; i++)
				{
					T t = this._Elements[i];
					if (t != null && !this._MarginRect.Contains(t.Position))
					{
						this.RemoveElementAt(i);
						root.AddElement(t);
					}
				}
			}
		}

		public void ExpandToContain(Vector2 point)
		{
			Rect rect = this._Rect;
			do
			{
				float num = rect.width * 0.5f;
				rect.xMin -= num;
				rect.yMin -= num;
				rect.xMax += num;
				rect.yMax += num;
			}
			while (!rect.Contains(point));
			this.Rect = rect;
			this.UpdateElements(this._Root);
		}

		public virtual void Destroy()
		{
			this._Elements = null;
			if (this._A != null)
			{
				this._A.Destroy();
				this._B.Destroy();
				this._C.Destroy();
				this._D.Destroy();
				this._A = (this._B = (this._C = (this._D = null)));
			}
		}

		private bool AddElementInternal(T element)
		{
			if (element == null)
			{
				throw new ArgumentException("Element null");
			}
			if (this._A != null)
			{
				Vector2 position = element.Position;
				if (position.x < this._Center.x)
				{
					if (position.y > this._Center.y)
					{
						return this._A.AddElementInternal(element);
					}
					return this._C.AddElementInternal(element);
				}
				else
				{
					if (position.y > this._Center.y)
					{
						return this._B.AddElementInternal(element);
					}
					return this._D.AddElementInternal(element);
				}
			}
			else
			{
				if (this._NumElements != this._Elements.Length)
				{
					while (this._LastIndex < this._Elements.Length)
					{
						if (this._Elements[this._LastIndex] == null)
						{
							this.AddElementAt(element, this._LastIndex);
							return true;
						}
						this._LastIndex++;
					}
					this._LastIndex = 0;
					while (this._LastIndex < this._Elements.Length)
					{
						if (this._Elements[this._LastIndex] == null)
						{
							this.AddElementAt(element, this._LastIndex);
							return true;
						}
						this._LastIndex++;
					}
					throw new InvalidOperationException("UltimateWater: Code supposed to be unreachable.");
				}
				if (this._Depth < 80)
				{
					T[] elements = this._Elements;
					this.SpawnChildNodes();
					this._A._Depth = (this._B._Depth = (this._C._Depth = (this._D._Depth = this._Depth + 1)));
					this._Elements = null;
					this._NumElements = 0;
					this._Root._FreeSpace += elements.Length;
					for (int i = 0; i < elements.Length; i++)
					{
						this.AddElementInternal(elements[i]);
					}
					return this.AddElementInternal(element);
				}
				throw new Exception("UltimateWater: Quadtree depth limit reached.");
			}
		}

		protected virtual void AddElementAt(T element, int index)
		{
			this._NumElements++;
			this._Root._FreeSpace--;
			this._Elements[index] = element;
		}

		protected virtual void RemoveElementAt(int index)
		{
			this._NumElements--;
			this._Root._FreeSpace++;
			this._Elements[index] = (T)((object)null);
		}

		protected virtual void SpawnChildNodes()
		{
			float width = this._Rect.width * 0.5f;
			float height = this._Rect.height * 0.5f;
			this._A = new Quadtree<T>(this._Root, new Rect(this._Rect.xMin, this._Center.y, width, height), this._Elements.Length);
			this._B = new Quadtree<T>(this._Root, new Rect(this._Center.x, this._Center.y, width, height), this._Elements.Length);
			this._C = new Quadtree<T>(this._Root, new Rect(this._Rect.xMin, this._Rect.yMin, width, height), this._Elements.Length);
			this._D = new Quadtree<T>(this._Root, new Rect(this._Center.x, this._Rect.yMin, width, height), this._Elements.Length);
		}

		protected Rect _Rect;

		protected Rect _MarginRect;

		protected Vector2 _Center;

		protected Quadtree<T> _A;

		protected Quadtree<T> _B;

		protected Quadtree<T> _C;

		protected Quadtree<T> _D;

		protected Quadtree<T> _Root;

		protected T[] _Elements;

		protected int _NumElements;

		private int _FreeSpace;

		private int _LastIndex;

		private int _Depth;
	}
}
