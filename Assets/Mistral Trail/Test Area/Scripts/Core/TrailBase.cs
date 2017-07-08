/*
 ### Mistral Trail System ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/08
 */

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Utility.DataStructure;

namespace Mistral.Effects.Trail
{
	#region SubClasses
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
		public bool isForwardOverrided;
		public Vector3 forwardOverride;
		public float quadScaleFactor;
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
		#region Public Variables

		public RingBuffer<TrailPoint> points;
		public Mesh mesh;

		public Vector3[] vertices, normals;
		public Vector2[] uvs;
		public Color[] colors;
		public int[] indices;
		public int activeCount;

		public bool activeSelf = false;

		#endregion

		#region ctor

		public TrailGraphics(int number)
		{
			mesh = new Mesh();
			///This sentence is very important!
			///We need to change the vertex array and triangle indices very frequently.
			///Hence for performance issues we need to make the Engine know about the situation.
			mesh.MarkDynamic();

			///A TrailPoint is abstract.
			///It contains 2 real points actually. 
			vertices = new Vector3[2 * number];
			normals = new Vector3[2 * number];
			uvs = new Vector2[2 * number];
			colors = new Color[2 * number];
			indices = new int[6 * number];

			points = new RingBuffer<TrailPoint>(number);
		}

		#endregion

		public void Dispose()
		{
			if (mesh != null)
			{
#if UNITY_EDITOR
				UnityEngine.Object.DestroyImmediate(mesh, true);
#else
				UnityEngine.Object.Destroy(mesh);
#endif
			}
		}
	}

	#endregion

	#region Main TrailBase Class
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
		
		/// <summary>
		/// A Reference to the Transform Component.
		/// All those guys say it is more efficient.
		/// </summary>
		protected Transform m_transform;

		#endregion

		#region Private Variables

		private TrailGraphics activeTrail;
		private List<TrailGraphics> fadingTrails;

		#endregion

		#region Private Static Variables (Trail Rendering Manager)

		private static Dictionary<Material, List<TrailGraphics>> mat2Trail;
		private static List<Mesh> generatedMeshes;

		private static bool isDrawing = false;
		private static int totalTrailsCount = 0;

		#endregion

		#region MonoBehaviours

		protected virtual void Awake()
		{
			totalTrailsCount++;
			///The only Guy :) Last Titan Standing. 
			if (totalTrailsCount == 1)
			{
				mat2Trail = new Dictionary<Material, List<TrailGraphics>>();
				generatedMeshes = new List<Mesh>();
			}

			m_transform = transform;
			isEmitting = Emit;

			if (isEmitting)
			{
				activeTrail = new TrailGraphics(GetMaxPoints());
				activeTrail.activeSelf = true;
				OnStartEmit();
			}
		}

		protected virtual void LateUpdate()
		{
			if (isDrawing)
				return;
			isDrawing = true;

			foreach(KeyValuePair<Material, List<TrailGraphics>> pair in mat2Trail)
			{
				CombineInstance[] combine = new CombineInstance[pair.Value.Count];
				for(int i = 0;i < pair.Value.Count;i++)
				{
					combine[i] = new CombineInstance
					{
						mesh = pair.Value[i].mesh,
						subMeshIndex = 0,
						transform = Matrix4x4.identity
					};
				}

				Mesh combinedMesh = new Mesh();
				combinedMesh.CombineMeshes(combine, true, false);
				generatedMeshes.Add(combinedMesh);
				DrawMesh(combinedMesh , pair.Key);
				pair.Value.Clear();
			}
		}

		protected virtual void Update()
		{
			if(isDrawing)
			{
				isDrawing = false;
				if (generatedMeshes.Count > 0)
				{
					foreach (Mesh m in generatedMeshes)
					{
#if UNITY_EDITOR
						UnityEngine.Object.DestroyImmediate(m, true);
#else
						UnityEngine.Object.Destroy(m);
#endif
					}
				}
				generatedMeshes.Clear();
			}

			float deltaTime = Time.deltaTime;

			if (!mat2Trail.ContainsKey(parameter.trailMaterial))
			{
				mat2Trail.Add(parameter.trailMaterial, new List<TrailGraphics>());
			}

			if (activeTrail != null)
			{
				UpdatePoints(activeTrail, deltaTime);
				UpdateTrail(activeTrail, deltaTime);
				mat2Trail[parameter.trailMaterial].Add(activeTrail);
			}

			for (int i = fadingTrails.Count - 1; i >= 0; i--)
			{
				if (fadingTrails[i] == null || fadingTrails[i].points.Any(a => a.timeSoFar < parameter.lifeTime) == false)
				{
					if (fadingTrails[i] == null)
						fadingTrails[i].Dispose();
					fadingTrails.RemoveAt(i);
					continue;
				}

				UpdatePoints(fadingTrails[i], deltaTime);
				UpdateTrail(fadingTrails[i], deltaTime);
				GenerateMesh(fadingTrails[i]);
				mat2Trail[parameter.trailMaterial].Add(fadingTrails[i]);
			}

			CheckEmiChange();
		}

		#endregion

		#region Protected Methods

		protected abstract int GetMaxPoints();

		protected virtual void OnDestroy()
		{
			totalTrailsCount--;
			if (totalTrailsCount == 0)
			{
				if (generatedMeshes != null && generatedMeshes.Count > 0)
				{
					foreach (Mesh m in generatedMeshes)
					{
#if UNITY_EDITOR
						DestroyImmediate(m, true);
#else
						Destroy(m);
#endif
					}
				}
				generatedMeshes = null;
				mat2Trail.Clear();
				mat2Trail = null;
			}

			if (activeTrail != null)
			{
				activeTrail.Dispose();
				activeTrail = null;
			}

			if (fadingTrails != null)
			{
				foreach (TrailGraphics fadingTrail in fadingTrails)
				{
					if (fadingTrail != null)
						fadingTrail.Dispose();
				}
				fadingTrails.Clear();
			}
		}

		protected virtual void OnStopEmit()
		{

		}

		protected virtual void OnStartEmit()
		{

		}

		protected virtual void Reset()
		{

		}

		protected virtual void OnTranslate(Vector3 trans)
		{

		}

		protected virtual void InitializeNewPoint(TrailPoint point)
		{


		}

		protected virtual void UpdateTrail(TrailGraphics trail, float deltaTime)
		{

			
		}

		protected void AddPoint(TrailPoint point, Vector3 position)
		{
			if (activeTrail == null)
				return;
			point.position = position;
			point.index = activeTrail.points.Count == 0 ? 0 : activeTrail.points[activeTrail.points.Count - 1].index + 1;
			InitializeNewPoint(point);
			point.distance2Src = activeTrail.points.Count == 0 ? 0 : activeTrail.points[activeTrail.points.Count - 1].distance2Src + Vector3.Distance(activeTrail.points[activeTrail.points.Count - 1].position, position);

			///Override Forward to be implemented in the future. 

			activeTrail.points.Add(point);
		}

#endregion

#region Private Methods

		private void GenerateMesh(TrailGraphics trail)
		{
			trail.mesh.Clear(false);
			Vector3 cameraForward = Camera.main.transform.forward;
			if (parameter.isForwardOverrided)
				cameraForward = parameter.forwardOverride;
			trail.activeCount = ActivePointsNumber(trail);

			///No way to draw a Mesh with only 2 vertices or even less. Exit.
			if (trail.activeCount < 2)
				return;

			int vertIdx = 0;
			for (int i = 0; i < trail.points.Count; i++)
			{
				TrailPoint tp = trail.points[i];
				float timeFraction = tp.timeSoFar / parameter.lifeTime;

				if (timeFraction > 1)
					continue;

				Vector3 cross = Vector3.zero;

				if (i < trail.points.Count - 1)
				{
					cross = Vector3.Cross((trail.points[i + 1].position - trail.points[i].position).normalized, cameraForward).normalized;
				}
				else
				{
					cross = Vector3.Cross((tp.position - trail.points[i - 1].position).normalized, cameraForward).normalized;
				}

				Color c = parameter.colorOverLife.Evaluate(1 - (float)vertIdx / (float)trail.activeCount / 2f);
				float s = parameter.sizeOverLife.Evaluate(timeFraction);

				trail.vertices[vertIdx] = tp.position + cross * s;
				trail.uvs[vertIdx] = new Vector2(tp.distance2Src / parameter.quadScaleFactor, 0.0f);
				trail.normals[vertIdx] = cameraForward;
				trail.colors[vertIdx] = c;
				vertIdx++;
				trail.vertices[vertIdx] = tp.position - cross * s;
				trail.uvs[vertIdx] = new Vector2(tp.distance2Src / parameter.quadScaleFactor, 1.0f);
				trail.normals[vertIdx] = cameraForward;
				trail.colors[vertIdx] = c;
				vertIdx++;
			}

			///"Stack" all redundant vertices into the termination position. 
			Vector2 termination = trail.vertices[vertIdx - 1];
			for (int i = vertIdx; i < trail.vertices.Length; i++)
			{
				trail.vertices[i] = termination;
			}

			///Now let's focus on triangle array ... 
			int triIdx = 0;
			for (int i = 0, imax = 2 * (trail.activeCount - 1); i < imax; i++)
			{
				///Ok, start point. 
				if (i % 2 == 0)
				{
					trail.indices[triIdx++] = i;
					trail.indices[triIdx++] = i + 1;
					trail.indices[triIdx++] = i + 2;
				}
				else///Reverse the process. 
				{
					trail.indices[triIdx++] = i + 2;
					trail.indices[triIdx++] = i + 1;
					trail.indices[triIdx++] = i;
				}
			}

			///"Squash" all the redundant vertices and triangle arrays. 
			int termIdx = trail.indices[triIdx - 1];
			for (int i = triIdx; i < trail.indices.Length; i++)
			{
				trail.indices[i] = termIdx;
			}

			///So now comes the Exciting part. CONG! 
			trail.mesh.vertices = trail.vertices;
			trail.mesh.SetIndices(trail.indices, MeshTopology.Triangles, 0);
			trail.mesh.uv = trail.uvs;
			trail.mesh.normals = trail.normals;
			trail.mesh.colors = trail.colors;
		}

		private void DrawMesh(Mesh trailMesh, Material mat)
		{

		}

		private void UpdatePoints(TrailGraphics line, float deltaTime)
		{


		}

		private int ActivePointsNumber(TrailGraphics trail)
		{
			int count = 0;

			for (int i = 0; i < trail.points.Count; i++)
			{
				if (trail.points[i].timeSoFar < parameter.lifeTime)
					count++;
			}

			return count;
		}

		private void CheckEmiChange()
		{
			if (isEmitting != Emit)
			{
				isEmitting = Emit;
				if (isEmitting)
				{
					activeTrail = new TrailGraphics(GetMaxPoints());
					activeTrail.activeSelf = true;
					OnStartEmit();
				}
				else
				{
					OnStopEmit();
					activeTrail.activeSelf = false;
					fadingTrails.Add(activeTrail);
					activeTrail = null;
				}
			}
		}
		

#endregion

	}

#endregion
}
