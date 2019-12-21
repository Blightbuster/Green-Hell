using System;
using UnityEngine;

public interface IMSMultisampleController
{
	void UpdateSpatialBlend(out float blend_value, out Vector3? inner_pos, out float distance);
}
