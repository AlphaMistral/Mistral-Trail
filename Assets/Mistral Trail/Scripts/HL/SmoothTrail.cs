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
	/// <summary>
	/// Creates a smooth trail with discrete control points. 
	/// Currently SmoothTrail is not optimized and thus if and performance issue is detected, 
	/// please turn it off or convert to other trail types. 
	/// </summary>
	public class SmoothTrail : TrailBase 
	{
		#region SubClasses

		/// <summary>
		/// Newly generated control points during runtime to ensure smoothness. 
		/// </summary>
		public class AdditionalPoint
		{
			public Vector3 position;
			public Vector3 forward;
			public Vector3 velocity = Vector3.zero;
			public float distance2Src = 0f;
		}

		#endregion

		#region Public Variables

		public float minVertexDistance = 0.1f;

		/// <summary>
		/// Please note that since additional control points are automatically added during runtime. 
		/// It is unnecessary to assign a large point number. 
		/// </summary>
		public int maxPointNumber = 150;

		/// <summary>
		/// Number of points to be inserted between two control points. 
		/// </summary>
		public int pointsInMiddle = 4;

		#endregion

		#region Private Variables

		private Vector3 lastPosition;
		private float distanceMoved;
		private RingBuffer<AdditionalPoint> controlPoints;

		#endregion

		#region MonoBehaviours

		protected override void Start()
		{
			base.Start();
			lastPosition = m_transform.position;
		}

		protected override void Update()
		{
			if (isEmitting)
			{
				distanceMoved += Vector3.Distance(m_transform.position, lastPosition);
				if (!Mathf.Approximately(distanceMoved, 0.0f) && distanceMoved >= minVertexDistance)
				{
					AddControlPoint(m_transform.position);
					distanceMoved = 0.0f;
				}
				else
				{
					controlPoints[controlPoints.Count - 1].position = m_transform.position;
					controlPoints[controlPoints.Count - 1].forward = GetFacing();
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
			distanceMoved = 0.0f;
			controlPoints = new RingBuffer<AdditionalPoint>(maxPointNumber);
			controlPoints.Add(new AdditionalPoint { position = lastPosition });

			controlPoints[0].forward = GetFacing();

			AddPoint(new TrailPoint(), lastPosition);
			AddControlPoint(lastPosition);
		}

		protected override int GetMaxPoints()
		{
			return maxPointNumber * (pointsInMiddle + 1);
		}

		protected override void UpdateTrail(TrailGraphics trail, float deltaTime)
		{
			if (!trail.activeSelf)
				return;
			
			int trailPointIdx = 0;
			for (int i = 0; i < controlPoints.Count; i++)
			{
				trail.points[trailPointIdx].position = controlPoints[i].position;

				if(parameter.orientationType != TrailOrientation.World)
				trail.points[trailPointIdx].forwardDirection = controlPoints[i].forward;

				trailPointIdx++;
				if (i < controlPoints.Count - 1)
				{
					Vector3 cp1, cp2;
					float distance = Vector3.Distance(controlPoints[i].position, controlPoints[i + 1].position) / 2;
					if (i == 0)
					{
						cp1 = controlPoints[i].position + (controlPoints[i + 1].position - controlPoints[i].position).normalized * distance;
					}
					else
					{
						cp1 = controlPoints[i].position + (controlPoints[i + 1].position - controlPoints[i - 1].position).normalized * distance;
					}

					int nextIdx = i + 1;

					if (nextIdx == controlPoints.Count - 1)
					{
						cp2 = controlPoints[nextIdx].position + (controlPoints[nextIdx - 1].position - controlPoints[nextIdx].position).normalized * distance;
					}
					else
					{
						cp2 = controlPoints[nextIdx].position + (controlPoints[nextIdx - 1].position - controlPoints[nextIdx + 1].position).normalized * distance;
					}

					TrailPoint current = trail.points[trailPointIdx - 1];
					TrailPoint next = trail.points[trailPointIdx + pointsInMiddle];

					for (int j = 0; j < pointsInMiddle; j++)
					{
						float t = (((float)j + 1) / ((float)pointsInMiddle + 1));
						trail.points[trailPointIdx].position = Bezier.CalculateCubic(t, controlPoints[i].position, cp1, cp2, controlPoints[i + 1].position, 0.3f);
						trail.points[trailPointIdx].timeSoFar = Mathf.Lerp(current.timeSoFar, next.timeSoFar, t);

						if (parameter.orientationType != TrailOrientation.World)
							trail.points[trailPointIdx].forwardDirection = Vector3.Lerp(current.forwardDirection, next.forwardDirection, t);

						trailPointIdx++;
					}
				}
				else
				{
					for (int j = 0; j < pointsInMiddle; j++)
					{
						if (trail.points[trailPointIdx - pointsInMiddle].position == null || trail.points[trailPointIdx] == null)
							continue;
						float t = (((float)j + 1) / ((float)pointsInMiddle + 1));
						trail.points[trailPointIdx].position = controlPoints[controlPoints.Count - 1].position;
						trail.points[trailPointIdx].timeSoFar = trail.points[trailPointIdx - pointsInMiddle].timeSoFar;

						if (parameter.orientationType != TrailOrientation.World)
							trail.points[trailPointIdx].forwardDirection = trail.points[trailPointIdx - pointsInMiddle].forwardDirection;

						trailPointIdx++;
					}
				}
				if (i == 0)
					controlPoints[i].distance2Src = 0f;
				else controlPoints[i].distance2Src = controlPoints[i - 1].distance2Src + Vector3.Distance(controlPoints[i].position, controlPoints[i - 1].position);
			}

            CustomChanges(controlPoints, deltaTime);

			int lastCPIdx = (pointsInMiddle + 1) * (controlPoints.Count - 1);
			int prevCPIdx = lastCPIdx - pointsInMiddle - 1;
			int activeCount = lastCPIdx + 1;

			float distance2Src = trail.points[prevCPIdx].distance2Src;
			for (int i = prevCPIdx + 1; i < activeCount; i++)
			{
				distance2Src += Vector3.Distance(trail.points[i - 1].position, trail.points[i].position);
				trail.points[i].distance2Src = distance2Src;
			}

		}

		#endregion

		#region Private Methods

		private void AddControlPoint(Vector3 pos)
		{
			for (int i = 0; i < pointsInMiddle; i++)
			{
				AddPoint(new TrailPoint(), pos);
			}
			AddPoint(new TrailPoint(), pos);
			AdditionalPoint ap = new AdditionalPoint { position = pos };

			ap.forward = GetFacing();

			controlPoints.Add(ap);
		}

        #endregion

        #region Override

        protected virtual void CustomChanges(RingBuffer<AdditionalPoint> cps, float deltatime)
        {

        }

        #endregion
    }
}
