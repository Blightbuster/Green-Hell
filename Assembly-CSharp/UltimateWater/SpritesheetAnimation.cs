using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UltimateWater
{
	public sealed class SpritesheetAnimation : MonoBehaviour
	{
		private void Start()
		{
			Renderer component = base.GetComponent<Renderer>();
			this._Material = component.material;
			this._Material.mainTextureScale = new Vector2(1f / (float)this._Horizontal, 1f / (float)this._Vertical);
			this._Material.mainTextureOffset = new Vector2(0f, 0f);
			this._NextChangeTime = Time.time + this._TimeStep;
		}

		private void Update()
		{
			if (Time.time >= this._NextChangeTime)
			{
				this._NextChangeTime += this._TimeStep;
				if (this._X == this._Horizontal - 1 && this._Y == this._Vertical - 1)
				{
					if (!this._Loop)
					{
						if (this._DestroyGo)
						{
							UnityEngine.Object.Destroy(base.gameObject);
						}
						else
						{
							base.enabled = false;
						}
						return;
					}
					this._X = 0;
					this._Y = 0;
				}
				else
				{
					this._X++;
					if (this._X >= this._Horizontal)
					{
						this._X = 0;
						this._Y++;
					}
				}
				this._Material.mainTextureOffset = new Vector2((float)this._X / (float)this._Horizontal, 1f - (float)(this._Y + 1) / (float)this._Vertical);
			}
		}

		private void OnDestroy()
		{
			if (this._Material != null)
			{
				UnityEngine.Object.Destroy(this._Material);
				this._Material = null;
			}
		}

		[SerializeField]
		[FormerlySerializedAs("horizontal")]
		private int _Horizontal = 2;

		[FormerlySerializedAs("vertical")]
		[SerializeField]
		private int _Vertical = 2;

		[SerializeField]
		[FormerlySerializedAs("timeStep")]
		private float _TimeStep = 0.06f;

		[SerializeField]
		[FormerlySerializedAs("loop")]
		private bool _Loop;

		[SerializeField]
		[FormerlySerializedAs("destroyGo")]
		private bool _DestroyGo;

		private Material _Material;

		private float _NextChangeTime;

		private int _X;

		private int _Y;
	}
}
