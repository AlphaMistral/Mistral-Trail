/*
 ### Mistral Trail System ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/09
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Utility.MathX;
using Mistral.Utility.DataStructure;

namespace Mistral.Effects.Trail
{
	public class SmoothRopeTrail : TrailBase 
	{

		#region Public Variables

		/// <summary>
		/// The exact distance between every two joints. 
		/// </summary>
		public float jointDistance = 0.3f;

		/// <summary>
		/// The total number of the joints.
		/// Joints could only be added or refreshed but never deleted!  
		/// </summary>
		public int maxJointNumber = 50;

		/// <summary>
		/// The follow scale.
		/// </summary>
		public float followScale = 1f;

		[HideInInspector]
		public Vector3 up = Vector3.up;

		[HideInInspector]
		public Vector3 right = Vector3.right;

		[HideInInspector]
		public Vector3 forward = Vector3.forward;

		#endregion

		#region Private Variables

		private Vector3 lastPosition;

		/// <summary>
		/// This distance is never accumulated - We are talking about ropes! 
		/// </summary>
		private float distanceMoved;

		/// <summary>
		/// Whether the Rope should be updated now or not. 
		/// </summary>
		private bool updateNow = false;

		#endregion

		#region MonoBehaviours

		protected override void Start ()
		{
			base.Start ();
			lastPosition = m_transform.position;
		}

		protected override void Update()
		{
			if (isEmitting) 
			{
				distanceMoved += Vector3.Distance(m_transform.position, lastPosition);

				if (distanceMoved != 0f && distanceMoved >= jointDistance)
				{
					updateNow = true;
					AddPoint(new TrailPoint(), m_transform.position);
					distanceMoved = 0.0f;
				}
				lastPosition = m_transform.position;
			}
			base.Update();
		}

		#endregion

		#region Override Methods

		protected override void OnStartEmit()
		{
			lastPosition = m_transform.position;
			distanceMoved = 0f;
		}

		protected override int GetMaxPoints()
		{
			return maxJointNumber;
		}

		protected override void OnTranslate(Vector3 trans)
		{
			lastPosition += trans;
		}

		protected override void UpdateTrail(TrailGraphics trail, float deltaTime)
		{ 
			if (trail.points.Count <= 0 || !updateNow)
				return;
			Segment(trail.points.Count - 1, m_transform.position, trail);
			for (int i = trail.points.Count - 1; i > 0; i--)
			{
				Segment(i - 1, trail.points[i].position, trail);
			}
			updateNow = false;
		}

		#endregion

		#region Private Methods

		private void Segment (int i, Vector3 pos, TrailGraphics trail)
		{
			Vector3 d = pos - trail.points[i].position;
			float mag = d.magnitude;
			if (mag <= 0.001f)
				mag = 1f;
			Vector3 temp = Vector3.zero;
			temp.x = pos.x - Vector3.Dot(right, d) * jointDistance * followScale / mag;
			temp.y = pos.y - Vector3.Dot(up, d) * jointDistance * followScale / mag;
			temp.z = pos.z - Vector3.Dot(forward, d) * jointDistance * followScale / mag;
			trail.points[i].position = temp;
		}

		#endregion
	}
}
