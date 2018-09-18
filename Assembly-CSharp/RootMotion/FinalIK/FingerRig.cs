using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	public class FingerRig : SolverManager
	{
		public bool initiated { get; private set; }

		public bool IsValid(ref string errorMessage)
		{
			foreach (Finger finger in this.fingers)
			{
				if (!finger.IsValid(ref errorMessage))
				{
					return false;
				}
			}
			return true;
		}

		[ContextMenu("Auto-detect")]
		public void AutoDetect()
		{
			this.fingers = new Finger[0];
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform[] array = new Transform[0];
				this.AddChildrenRecursive(base.transform.GetChild(i), ref array);
				if (array.Length == 3 || array.Length == 4)
				{
					Finger finger = new Finger();
					finger.bone1 = array[0];
					finger.bone2 = array[1];
					if (array.Length == 3)
					{
						finger.tip = array[2];
					}
					else
					{
						finger.bone3 = array[2];
						finger.tip = array[3];
					}
					finger.weight = 1f;
					Array.Resize<Finger>(ref this.fingers, this.fingers.Length + 1);
					this.fingers[this.fingers.Length - 1] = finger;
				}
			}
		}

		public void AddFinger(Transform bone1, Transform bone2, Transform bone3, Transform tip, Transform target = null)
		{
			Finger finger = new Finger();
			finger.bone1 = bone1;
			finger.bone2 = bone2;
			finger.bone3 = bone3;
			finger.tip = tip;
			finger.target = target;
			Array.Resize<Finger>(ref this.fingers, this.fingers.Length + 1);
			this.fingers[this.fingers.Length - 1] = finger;
			this.initiated = false;
			finger.Initiate(base.transform, this.fingers.Length - 1);
			if (this.fingers[this.fingers.Length - 1].initiated)
			{
				this.initiated = true;
			}
		}

		public void RemoveFinger(int index)
		{
			if ((float)index < 0f || index >= this.fingers.Length)
			{
				Warning.Log("RemoveFinger index out of bounds.", base.transform, false);
				return;
			}
			if (this.fingers.Length == 1)
			{
				this.fingers = new Finger[0];
				return;
			}
			Finger[] array = new Finger[this.fingers.Length - 1];
			int num = 0;
			for (int i = 0; i < this.fingers.Length; i++)
			{
				if (i != index)
				{
					array[num] = this.fingers[i];
					num++;
				}
			}
			this.fingers = array;
		}

		private void AddChildrenRecursive(Transform parent, ref Transform[] array)
		{
			Array.Resize<Transform>(ref array, array.Length + 1);
			array[array.Length - 1] = parent;
			if (parent.childCount != 1)
			{
				return;
			}
			this.AddChildrenRecursive(parent.GetChild(0), ref array);
		}

		protected override void InitiateSolver()
		{
			this.initiated = true;
			for (int i = 0; i < this.fingers.Length; i++)
			{
				this.fingers[i].Initiate(base.transform, i);
				if (!this.fingers[i].initiated)
				{
					this.initiated = false;
				}
			}
		}

		public void UpdateFingerSolvers()
		{
			if (this.weight <= 0f)
			{
				return;
			}
			foreach (Finger finger in this.fingers)
			{
				finger.Update(this.weight);
			}
		}

		public void FixFingerTransforms()
		{
			foreach (Finger finger in this.fingers)
			{
				finger.FixTransforms();
			}
		}

		protected override void UpdateSolver()
		{
			this.UpdateFingerSolvers();
		}

		protected override void FixTransforms()
		{
			this.FixFingerTransforms();
		}

		[Range(0f, 1f)]
		[Tooltip("The master weight for all fingers.")]
		public float weight = 1f;

		public Finger[] fingers = new Finger[0];
	}
}
