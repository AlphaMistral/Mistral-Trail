/*
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/08
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mistral.Effects.Trail
{
    /// <summary>
    /// This Serializable Class Serves as a Parameter Storage for Trail Rendering. 
    /// </summary>
    [System.Serializable]
    public class TrailParameter
    {
        public Material trailMaterial;
        public float lifeTime;
        public AnimationCurve sizeOverLife = new AnimationCurve();
        public Gradient colorOverLife;
    }

	/// <summary>
	/// This Class Represents a Virtual Point on the Trail. 
	/// </summary>
	public class TrailPoint
	{
		/// <summary>
		/// The Looking-at direction. 
		/// Namely the direction towards which this point is to move in the next frame :)
		/// </summary>
		public Vector3 forwardDirection;

		/// <summary>
		/// The Virtual Position of this TrailPoint.
		/// Please note that a TrailPoint Could not be a GameObject.
		/// Hence, its position should be virtual rather than transform.position. 
		/// </summary>
		public Vector3 position;

		public int index;

		public float timeSoFar = 0;
		public float distance2Src = 0;

		/// <summary>
		/// DISCUSSION: Whether deltaTime should be provided or not. 
		/// HYPOTHESIS: Time.deltaTime is not an O(1) operation. 
		/// </summary>
		/// <param name="deltaTime"></param>
		public virtual void Update(float deltaTime)
		{
			timeSoFar += deltaTime;
		}
	}

	public class TrailGraphics : IDisposable
	{

		public void Dispose()
		{

		}
	}

    /// <summary>
    /// Abstract Class for all kinds of trails. 
    /// It also simultaneously serves as the Trail Rendering Manager! 
    /// Specifically, take the "LateUpdate" function for reference. 
    /// </summary>
    public abstract class TrailBase : MonoBehaviour
    {
        #region Public Variables

        public TrailParameter parameter;
        public bool Emit = false;

		#endregion

		#region Protected Variables

		protected bool isEmitting;

		#endregion

		#region Private Static Variables (Trail Rendering Manager)

		//private static Dictionary<Material, List<>> mat2Trail;

		#endregion

	}
}