using System;
using UnityEngine;

public interface IBulletParent
{
	void OnHit(GameObject hit_object);

	float GetDamage();
}
