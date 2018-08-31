using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
	[AddComponentMenu("Scripts/RootMotion.FinalIK/IK/Leg IK")]
	public class LegIK : IK
	{
		[ContextMenu("User Manual")]
		protected override void OpenUserManual()
		{
			Debug.Log("No User Manual page for this component yet, sorry.");
		}

		[ContextMenu("Scrpt Reference")]
		protected override void OpenScriptReference()
		{
			Debug.Log("No Script Reference for this component yet, sorry.");
		}

		[ContextMenu("Support Group")]
		private void SupportGroup()
		{
			Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
		}

		[ContextMenu("Asset Store Thread")]
		private void ASThread()
		{
			Application.OpenURL("http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
		}

		public override IKSolver GetIKSolver()
		{
			return this.solver;
		}

		public IKSolverLeg solver = new IKSolverLeg();
	}
}
