using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Pathfinding/Seeker")]
[HelpURL("http://arongranberg.com/astar/docs/class_seeker.php")]
public class Seeker : VersionedMonoBehaviour
{
	public Seeker()
	{
		this.onPathDelegate = new OnPathDelegate(this.OnPathComplete);
		this.onPartialPathDelegate = new OnPathDelegate(this.OnPartialPathComplete);
	}

	protected override void Awake()
	{
		base.Awake();
		this.startEndModifier.Awake(this);
	}

	public Path GetCurrentPath()
	{
		return this.path;
	}

	public void CancelCurrentPathRequest(bool pool = true)
	{
		if (!this.IsDone())
		{
			this.path.Error();
			if (pool)
			{
				this.path.Claim(this.path);
				this.path.Release(this.path, false);
			}
		}
	}

	public void OnDestroy()
	{
		this.ReleaseClaimedPath();
		this.startEndModifier.OnDestroy(this);
	}

	public void ReleaseClaimedPath()
	{
		if (this.prevPath != null)
		{
			this.prevPath.Release(this, true);
			this.prevPath = null;
		}
	}

	public void RegisterModifier(IPathModifier modifier)
	{
		this.modifiers.Add(modifier);
		this.modifiers.Sort((IPathModifier a, IPathModifier b) => a.Order.CompareTo(b.Order));
	}

	public void DeregisterModifier(IPathModifier modifier)
	{
		this.modifiers.Remove(modifier);
	}

	public void PostProcess(Path p)
	{
		this.RunModifiers(Seeker.ModifierPass.PostProcess, p);
	}

	public void RunModifiers(Seeker.ModifierPass pass, Path p)
	{
		if (pass == Seeker.ModifierPass.PreProcess && this.preProcessPath != null)
		{
			this.preProcessPath(p);
		}
		else if (pass == Seeker.ModifierPass.PostProcess && this.postProcessPath != null)
		{
			this.postProcessPath(p);
		}
		for (int i = 0; i < this.modifiers.Count; i++)
		{
			MonoModifier monoModifier = this.modifiers[i] as MonoModifier;
			if (!(monoModifier != null) || monoModifier.enabled)
			{
				if (pass == Seeker.ModifierPass.PreProcess)
				{
					this.modifiers[i].PreProcess(p);
				}
				else if (pass == Seeker.ModifierPass.PostProcess)
				{
					this.modifiers[i].Apply(p);
				}
			}
		}
	}

	public bool IsDone()
	{
		return this.path == null || this.path.PipelineState >= PathState.Returned;
	}

	private void OnPathComplete(Path p)
	{
		this.OnPathComplete(p, true, true);
	}

	private void OnPathComplete(Path p, bool runModifiers, bool sendCallbacks)
	{
		if (p != null && p != this.path && sendCallbacks)
		{
			return;
		}
		if (this == null || p == null || p != this.path)
		{
			return;
		}
		if (!this.path.error && runModifiers)
		{
			this.RunModifiers(Seeker.ModifierPass.PostProcess, this.path);
		}
		if (sendCallbacks)
		{
			p.Claim(this);
			this.lastCompletedNodePath = p.path;
			this.lastCompletedVectorPath = p.vectorPath;
			if (this.tmpPathCallback != null)
			{
				this.tmpPathCallback(p);
			}
			if (this.pathCallback != null)
			{
				this.pathCallback(p);
			}
			if (this.prevPath != null)
			{
				this.prevPath.Release(this, true);
			}
			this.prevPath = p;
			if (!this.drawGizmos)
			{
				this.ReleaseClaimedPath();
			}
		}
	}

	private void OnPartialPathComplete(Path p)
	{
		this.OnPathComplete(p, true, false);
	}

	private void OnMultiPathComplete(Path p)
	{
		this.OnPathComplete(p, false, true);
	}

	[Obsolete("Use ABPath.Construct(start, end, null) instead")]
	public ABPath GetNewPath(Vector3 start, Vector3 end)
	{
		return ABPath.Construct(start, end, null);
	}

	public Path StartPath(Vector3 start, Vector3 end)
	{
		return this.StartPath(start, end, null, -1);
	}

	public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback)
	{
		return this.StartPath(start, end, callback, -1);
	}

	public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback, int graphMask)
	{
		return this.StartPath(ABPath.Construct(start, end, null), callback, graphMask);
	}

	public Path StartPath(Path p, OnPathDelegate callback = null, int graphMask = -1)
	{
		MultiTargetPath multiTargetPath = p as MultiTargetPath;
		if (multiTargetPath != null)
		{
			OnPathDelegate[] array = new OnPathDelegate[multiTargetPath.targetPoints.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = this.onPartialPathDelegate;
			}
			multiTargetPath.callbacks = array;
			p.callback = (OnPathDelegate)Delegate.Combine(p.callback, new OnPathDelegate(this.OnMultiPathComplete));
		}
		else
		{
			p.callback = (OnPathDelegate)Delegate.Combine(p.callback, this.onPathDelegate);
		}
		p.enabledTags = this.traversableTags;
		p.tagPenalties = this.tagPenalties;
		p.nnConstraint.graphMask = graphMask;
		this.StartPathInternal(p, callback);
		return p;
	}

	private void StartPathInternal(Path p, OnPathDelegate callback)
	{
		if (this.path != null && this.path.PipelineState <= PathState.Processing && this.path.CompleteState != PathCompleteState.Error && this.lastPathID == (uint)this.path.pathID)
		{
			this.path.Error();
			this.path.LogError("Canceled path because a new one was requested.\nThis happens when a new path is requested from the seeker when one was already being calculated.\nFor example if a unit got a new order, you might request a new path directly instead of waiting for the now invalid path to be calculated. Which is probably what you want.\nIf you are getting this a lot, you might want to consider how you are scheduling path requests.");
		}
		this.path = p;
		this.tmpPathCallback = callback;
		this.lastPathID = (uint)this.path.pathID;
		this.RunModifiers(Seeker.ModifierPass.PreProcess, this.path);
		AstarPath.StartPath(this.path, false);
	}

	public MultiTargetPath StartMultiTargetPath(Vector3 start, Vector3[] endPoints, bool pathsForAll, OnPathDelegate callback = null, int graphMask = -1)
	{
		MultiTargetPath multiTargetPath = MultiTargetPath.Construct(start, endPoints, null, null);
		multiTargetPath.pathsForAll = pathsForAll;
		this.StartPath(multiTargetPath, callback, graphMask);
		return multiTargetPath;
	}

	public MultiTargetPath StartMultiTargetPath(Vector3[] startPoints, Vector3 end, bool pathsForAll, OnPathDelegate callback = null, int graphMask = -1)
	{
		MultiTargetPath multiTargetPath = MultiTargetPath.Construct(startPoints, end, null, null);
		multiTargetPath.pathsForAll = pathsForAll;
		this.StartPath(multiTargetPath, callback, graphMask);
		return multiTargetPath;
	}

	[Obsolete("You can use StartPath instead of this method now. It will behave identically.")]
	public MultiTargetPath StartMultiTargetPath(MultiTargetPath p, OnPathDelegate callback = null, int graphMask = -1)
	{
		this.StartPath(p, callback, graphMask);
		return p;
	}

	public void OnDrawGizmos()
	{
		if (this.lastCompletedNodePath == null || !this.drawGizmos)
		{
			return;
		}
		if (this.detailedGizmos)
		{
			Gizmos.color = new Color(0.7f, 0.5f, 0.1f, 0.5f);
			if (this.lastCompletedNodePath != null)
			{
				for (int i = 0; i < this.lastCompletedNodePath.Count - 1; i++)
				{
					Gizmos.DrawLine((Vector3)this.lastCompletedNodePath[i].position, (Vector3)this.lastCompletedNodePath[i + 1].position);
				}
			}
		}
		Gizmos.color = new Color(0f, 1f, 0f, 1f);
		if (this.lastCompletedVectorPath != null)
		{
			for (int j = 0; j < this.lastCompletedVectorPath.Count - 1; j++)
			{
				Gizmos.DrawLine(this.lastCompletedVectorPath[j], this.lastCompletedVectorPath[j + 1]);
			}
		}
	}

	protected override int OnUpgradeSerializedData(int version)
	{
		if (version == 0 && this.traversableTagsCompatibility != null && this.traversableTagsCompatibility.tagsChange != -1)
		{
			this.traversableTags = this.traversableTagsCompatibility.tagsChange;
			this.traversableTagsCompatibility = new TagMask(-1, -1);
		}
		return 1;
	}

	public bool drawGizmos = true;

	public bool detailedGizmos;

	public StartEndModifier startEndModifier = new StartEndModifier();

	[HideInInspector]
	public int traversableTags = -1;

	[FormerlySerializedAs("traversableTags")]
	[SerializeField]
	[HideInInspector]
	protected TagMask traversableTagsCompatibility = new TagMask(-1, -1);

	[HideInInspector]
	public int[] tagPenalties = new int[32];

	public OnPathDelegate pathCallback;

	public OnPathDelegate preProcessPath;

	public OnPathDelegate postProcessPath;

	[NonSerialized]
	private List<Vector3> lastCompletedVectorPath;

	[NonSerialized]
	private List<GraphNode> lastCompletedNodePath;

	[NonSerialized]
	protected Path path;

	[NonSerialized]
	private Path prevPath;

	private readonly OnPathDelegate onPathDelegate;

	private readonly OnPathDelegate onPartialPathDelegate;

	private OnPathDelegate tmpPathCallback;

	protected uint lastPathID;

	private readonly List<IPathModifier> modifiers = new List<IPathModifier>();

	public enum ModifierPass
	{
		PreProcess,
		PostProcess = 2
	}
}
