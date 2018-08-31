using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SplineProfile", menuName = "SplineProfile", order = 1)]
public class SplineProfile : ScriptableObject
{
	public Material splineMaterial;

	public AnimationCurve meshCurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f),
		new Keyframe(1f, 0f)
	});

	public float minVal = 0.5f;

	public float maxVal = 0.5f;

	public int vertsInShape = 3;

	public float traingleDensity = 0.2f;

	public float uvScale = 3f;

	public bool uvRotation = true;

	public AnimationCurve flowFlat = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0.025f),
		new Keyframe(0.5f, 0.05f),
		new Keyframe(1f, 0.025f)
	});

	public AnimationCurve flowWaterfall = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0.25f),
		new Keyframe(1f, 0.25f)
	});
}
