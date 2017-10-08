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
	/// Specifies how the trail should be oriented during the whole process. 
	/// Local: The trail will be generated and oriented regarding a local vector.
	/// World: Always facing a same orientation. 
	/// LookAt: Persistently look at the specified transform. 
	/// </summary>
	public enum TrailOrientation
	{
		Local = 0,
		World = 1,
		LookAt = 2
	}

	/// <summary>
	/// Defines how the trail is oriented regarding the indicated forward direction. 
	/// </summary>
	public enum TrailType
	{
		Vertical = 0,
		Horizontal = 1,
		Cross = 2
	}

	/// <summary>
	/// This Serializable Class Serves as a Parameter Storage for Trail Rendering. 
	/// </summary>
	[System.Serializable]
    public class TrailParameter
    {
        public Material trailMaterial;
        public float lifeTime = 1f;
        public AnimationCurve sizeOverLife = new AnimationCurve();
		public float sizeMultiplier = 1f;
        public Gradient colorOverLife;

		[HideInInspector]
		public float quadScaleFactor = 1f;
		public TrailOrientation orientationType;
		public Vector3 forwardOverride = new Vector3(0f, 0f, 1f);
		public TrailType trailType;

		/// <summary>
		/// Whether the Trail is a Stripe or a Crossed Pillar. 
		/// DON'T TEMPER WITH THIS DURING RUNTIME! 
		/// </summary>
		[HideInInspector]
		public bool isCross;

		/// <summary>
		/// Whether the UV of the Trail is Tiled or not. 
		/// </summary>
		[HideInInspector]
		public bool isTile;
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

		/// <summary>
		/// Time elapsed ever since the TrailPoint is instantiated. 
		/// </summary>
		public float timeSoFar = 0;

		/// <summary>
		/// The distance from the point to the source point -- where it is first instantiated. 
		/// </summary>
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

	/// <summary>
	/// This class specifies all the graphical components of a trail. 
	/// To be honest it doesn't necessarily need to implement IDisposable Interface. 
	/// So why did I do it anyway? -> Zhuangbility. 
	/// </summary>
	public class TrailGraphics : IDisposable
	{
		#region Public Variables

		/// <summary>
		/// The Object Pool Based memory management of all the TrailPoints. 
		/// </summary>
		public RingBuffer<TrailPoint> points;

		/// <summary>
		/// This is the Mesh to be rendered. 
		/// Null in the beginning of course. 
		/// </summary>
		public Mesh mesh;

		public Vector3[] vertices, normals;
		public Vector2[] uvs;
		public Color[] colors;
		public int[] indices;

		/// <summary>
		/// How many vertices are actually active. 
		/// </summary>
		public int activeCount;

		/// <summary>
		/// Is this trail active. 
		/// If it inherits from MonoBehaviour, it is actually the "enabled" attribute. 
		/// </summary>
		public bool activeSelf = false;

		#endregion

		#region ctor

		public TrailGraphics(int number, bool isCross)
		{
			mesh = new Mesh();

			/// This sentence is very important!
			/// We need to change the vertex array and triangle indices very frequently.
			/// Hence for performance issues we need to make the Engine know about the situation.
			/// Post-Comment -> At Least 25% CPU performance increase. 
			mesh.MarkDynamic();

			/// If the trail is a crossed stripe ... 
			/// It is actually a doubled stripe. 
			int coefficient = isCross ? 2 : 1;

			/// A TrailPoint is abstract.
			/// It contains 2 real points actually. 
			vertices = new Vector3[2 * number * coefficient];
			normals = new Vector3[2 * number * coefficient];
			uvs = new Vector2[2 * number * coefficient];
			colors = new Color[2 * number * coefficient];
			indices = new int[6 * number * coefficient];

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
	// [ExecuteInEditMode]
	public abstract class TrailBase : MonoBehaviour
    {
		#region Public Variables

		/// <summary>
		/// All the parameters the system require to render the trail. 
		/// </summary>
        public TrailParameter parameter;

		/// <summary>
		/// Whether the trail should be emitting or not. 
		/// </summary>
		public bool Emit = true;

		/// <summary>
		/// For Animation Only - The size of the trail. 
		/// </summary>
		[HideInInspector]
		public float sizeForAnim = 0f;

		#endregion

		#region Protected Variables

		/// <summary>
		/// Current condition of the emission. 
		/// Please note that it is different from the Emit by indicating the situation rather than definition. 
		/// </summary>
		protected bool isEmitting;
		
		/// <summary>
		/// A Reference to the Transform Component.
		/// All those guys say it is more efficient.
		/// I dunno about it. 
		/// </summary>
		protected Transform m_transform;

		#endregion

		#region Private Variables

		/// <summary>
		/// These are the trails to be rendered. 
		/// </summary>
		private TrailGraphics activeTrail;

		/// <summary>
		/// These trails are being faded out and destroyed. 
		/// </summary>
		private List<TrailGraphics> fadingTrails;

		#endregion

		#region Private Static Variables (Trail Rendering Manager)

		private static Dictionary<Material, List<TrailGraphics>> mat2Trail;
		private static List<Mesh> generatedMeshes;

		private static bool isDrawing = false;
		private static int totalTrailsCount = 0;

		#endregion

		#region MonoBehaviours

		/// <summary>
		/// Serves as a variable-initializer. 
		/// </summary>
		protected virtual void Awake()
		{
			totalTrailsCount++;
			parameter.isCross = parameter.trailType == TrailType.Cross;
			///The only Guy :) Last Titan Standing. 
			if (totalTrailsCount == 1)
			{
				mat2Trail = new Dictionary<Material, List<TrailGraphics>>();
				generatedMeshes = new List<Mesh>();
			}

			fadingTrails = new List<TrailGraphics>();
			m_transform = transform;
			isEmitting = Emit;

			if (isEmitting)
			{
				activeTrail = new TrailGraphics(GetMaxPoints(), parameter.isCross);
				activeTrail.activeSelf = true;
				OnStartEmit();
			}

			SetupMaterialComponent();
		}

		/// <summary>
		/// Subclasses are not encouraged to Override Awake. 
		/// For any initialization operations please Override Start. 
		/// </summary>
		protected virtual void Start()
		{

		}

		/// <summary>
		/// Draw the Meshes in LateUpdate. 
		/// </summary>
		protected virtual void LateUpdate()
		{
			///Management Function. Ensure that it is called only once. 
			if (isDrawing)
				return;
			isDrawing = true;

			foreach(KeyValuePair<Material, List<TrailGraphics>> pair in mat2Trail)
			{
				/// Combine first. 
				/// We generate plenty of meshes but are actually adjacent to each other. 
				/// Although we have some Heap operations here ... It actually worths! 
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

		/// <summary>
		/// Generate the Meshes in Update. 
		/// </summary>
		protected virtual void Update()
		{
			if(!Mathf.Approximately(sizeForAnim, 0f))
			parameter.sizeMultiplier = sizeForAnim;
			/// If we are generating Meshes, we don't want to draw anything. 
			/// So simply destroy any mesh before generating anything. 
			/// Obviously we only want to do this once. 
			if(isDrawing)
			{
				isDrawing = false;
				if (generatedMeshes.Count > 0)
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
				GenerateMesh(activeTrail);
				mat2Trail[parameter.trailMaterial].Add(activeTrail);
			}

			/// IMPROVEMENT: This section is extremely expensive! 
			/// Unless ABOSOLUTELY NECESSARY, do not keeping turning emit ON and OFF! 
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

			CheckEmitChange();
		}

		#endregion

		#region Protected Methods

		protected abstract int GetMaxPoints();

		/// <summary>
		/// De-constructor. 
		/// </summary>
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

		protected virtual void UpdatePoint(TrailPoint point, float deltaTime)
		{

		}

		/// <summary>
		/// Add a new TrailPoint into the activeTrail. 
		/// </summary>
		/// <param name="point"></param>
		/// <param name="position"></param>
		protected void AddPoint(TrailPoint point, Vector3 position)
		{
			if (activeTrail == null)
				return;
			point.position = position;
			point.index = activeTrail.points.Count == 0 ? 0 : activeTrail.points[activeTrail.points.Count - 1].index + 1;
			InitializeNewPoint(point);
			point.distance2Src = activeTrail.points.Count == 0 ? 0 : activeTrail.points[activeTrail.points.Count - 1].distance2Src + 
								 Vector3.Distance(activeTrail.points[activeTrail.points.Count - 1].position, position);

			point.forwardDirection = GetFacing();

			activeTrail.points.Add(point);
		}

		protected Vector3 GetFacing()
		{
			switch (parameter.orientationType)
			{
				case TrailOrientation.LookAt:
					return Camera.current == null ? Vector3.forward : Camera.current.transform.forward;
					break;
				case TrailOrientation.World:
					return parameter.forwardOverride.normalized;
					break;
				case TrailOrientation.Local:
					return m_transform.TransformDirection(parameter.forwardOverride.normalized).normalized; /// Don't Ever Use This! Disaster! 
				default:
					return Vector3.zero;/// Actually Impossible to Happen! 
					break;
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Help a trail generate its own Mesh. 
		/// </summary>
		/// <param name="trail"></param>
		private void GenerateMesh(TrailGraphics trail)
		{
			trail.mesh.Clear(false);
			Vector3 cameraForward = GetFacing();
			trail.activeCount = ActivePointsNumber(trail);

			/// No way to draw a Mesh with only 2 vertices or even less. Exit.
			if (trail.activeCount < 2)
				return;

			int vertIdx = 0;
			Vector3 lastCross = cameraForward;
			if (parameter.trailType == TrailType.Vertical || parameter.isCross)
			{
				for (int i = 0; i < trail.points.Count; i++)
				{
					TrailPoint tp = trail.points[i];
					float timeFraction = tp.timeSoFar / parameter.lifeTime;

					if (timeFraction > 1)
						continue;

					if (parameter.orientationType == TrailOrientation.Local)
						cameraForward = tp.forwardDirection;

					Vector3 cross = Vector3.zero;
					Vector3 moveDir = cameraForward;

					if(i < trail.points.Count - 1)
						moveDir = (trail.points[i + 1].position - tp.position).normalized;
					else 
						moveDir = (tp.position - trail.points[i - 1].position).normalized;
					cross = Vector3.Cross(moveDir, cameraForward).normalized;

					if (cross.magnitude < 0.9f) 
					{
						cross = lastCross.normalized;
					}

					else if (Vector3.Dot(cross, lastCross) < 0f)
					{
						cross = -cross;
						lastCross = -cross;
					}

					else
					{
						lastCross = cross;
					}

					Color c = parameter.colorOverLife.Evaluate(1 - (float)vertIdx / (float)trail.activeCount / 2f);
					float s = parameter.sizeOverLife.Evaluate(timeFraction);

					float uvx = parameter.isTile ? tp.distance2Src / parameter.quadScaleFactor : 1 - timeFraction;
					Vector3 offset = cross * s * parameter.sizeMultiplier;
					trail.vertices[vertIdx] = tp.position + offset;
					trail.uvs[vertIdx] = new Vector2(uvx, 0.0f);
					trail.normals[vertIdx] = cameraForward;
					trail.colors[vertIdx] = c;
					vertIdx++;
					trail.vertices[vertIdx] = tp.position - offset;
					trail.uvs[vertIdx] = new Vector2(uvx, 1.0f);
					trail.normals[vertIdx] = cameraForward;
					trail.colors[vertIdx] = c;
					vertIdx++;
				}
			}

			int oldVertIdx = vertIdx;
			lastCross = cameraForward;

			if (parameter.trailType == TrailType.Horizontal || parameter.isCross)
			{
				for (int i = 0; i < trail.points.Count; i++)
				{
					TrailPoint tp = trail.points[i];
					float timeFraction = tp.timeSoFar / parameter.lifeTime;

					if (timeFraction > 1)
						continue;

					if (parameter.orientationType == TrailOrientation.Local)
						cameraForward = tp.forwardDirection;

					Vector3 cross = cameraForward;

					Color c = parameter.colorOverLife.Evaluate(1 - ((float)vertIdx - oldVertIdx) / (float)trail.activeCount / 2f);
					float s = parameter.sizeOverLife.Evaluate(timeFraction);

					float uvx = parameter.isTile ? tp.distance2Src / parameter.quadScaleFactor : 1 - timeFraction;
					Vector3 offset = cross * s * parameter.sizeMultiplier;
					trail.vertices[vertIdx] = tp.position + offset;
					trail.uvs[vertIdx] = new Vector2(uvx, 0.0f);
					trail.normals[vertIdx] = cameraForward;
					trail.colors[vertIdx] = c;
					vertIdx++;
					trail.vertices[vertIdx] = tp.position - offset;
					trail.uvs[vertIdx] = new Vector2(uvx, 1.0f);
					trail.normals[vertIdx] = cameraForward;
					trail.colors[vertIdx] = c;
					vertIdx++;
				}
			}

			/// "Stack" all redundant vertices into the termination position. 
			Vector3 termination = trail.vertices[vertIdx - 1];
			for (int i = vertIdx; i < trail.vertices.Length; i++)
			{
				trail.vertices[i] = termination;
			}
			 
			/// Now let's focus on triangle array ... 
			int triIdx = 0;
			for (int i = 0, imax = 2 * (trail.activeCount - 1); i < imax; i++)
			{
				/// Ok, start point. 
				if (i % 2 == 0)
				{
					trail.indices[triIdx++] = i;
					trail.indices[triIdx++] = i + 1;
					trail.indices[triIdx++] = i + 2;
				}
				else/// Reverse the process. 
				{
					trail.indices[triIdx++] = i + 2;
					trail.indices[triIdx++] = i + 1;
					trail.indices[triIdx++] = i;
				}
			}

			if (parameter.isCross)
			{
				for (int i = 2 * trail.activeCount, imax = 4 * trail.activeCount - 2; i < imax; i++)
				{
					if (i % 2 == 0)
					{
						trail.indices[triIdx++] = i;
						trail.indices[triIdx++] = i + 1;
						trail.indices[triIdx++] = i + 2;
					}
					else
					{
						trail.indices[triIdx++] = i + 2;
						trail.indices[triIdx++] = i + 1;
						trail.indices[triIdx++] = i;
					}
				}
			}

			/// "Squash" all the redundant vertices and triangle arrays. 
			int termIdx = trail.indices[triIdx - 1];
			for (int i = triIdx; i < trail.indices.Length; i++)
			{
				trail.indices[i] = termIdx;
			}

			/// So now comes the Exciting part. CONG! 
			trail.mesh.vertices = trail.vertices;
			/// Setting Indices array directly to mesh also works. 
			trail.mesh.SetIndices(trail.indices, MeshTopology.Triangles, 0);
			trail.mesh.uv = trail.uvs;
			trail.mesh.normals = trail.normals;
			trail.mesh.colors = trail.colors;
		}

		private void DrawMesh(Mesh trailMesh, Material mat)
		{
			///DrawMesh or DrawMeshNow? Sha sha fen bu qing. 
			Graphics.DrawMesh(trailMesh, Matrix4x4.identity, mat, gameObject.layer);
		}

		/// <summary>
		/// Updates points' positions from time to time. 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="deltaTime"></param>
		private void UpdatePoints(TrailGraphics trail, float deltaTime)
		{
			for (int i = 0; i < trail.points.Count; i++)
			{
				trail.points[i].Update(deltaTime);
			}
		}

		/// <summary>
		/// Count the points whose life time have not passed yet. 
		/// We do nothing to those whose life time have passed. 
		/// Because we are using RingBuffer as an Object Pool :) 
		/// </summary>
		/// <param name="trail"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Just in case the Emission is suddenly shut-down or activated. 
		/// </summary>
		private void CheckEmitChange()
		{
			if (isEmitting != Emit)
			{
				isEmitting = Emit;
				if (isEmitting)
				{
					activeTrail = new TrailGraphics(GetMaxPoints(), parameter.isCross);
					activeTrail.activeSelf = true;
					OnStartEmit();
				}
				else
				{
					if(activeTrail == null)
						return;
					OnStopEmit();
					activeTrail.activeSelf = false;
					fadingTrails.Add(activeTrail);
					activeTrail = null;
				}
			}
		}

		/// <summary>
		/// Setups the material component.
		/// </summary>
		private void SetupMaterialComponent ()
		{
			MeshRenderer thisRenderer = GetComponent<MeshRenderer>();
			if (thisRenderer == null)
			{
				thisRenderer = gameObject.AddComponent<MeshRenderer>();
			}
			thisRenderer.material = parameter.trailMaterial;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Retrieves the Mesh of the TrailGraphics. 
		/// For your own sake, this is READONLY! 
		/// </summary>
		/// <returns>The trail mesh.</returns>
		public Mesh GetTrailMesh()
		{
			return activeTrail.mesh;
		}

		#endregion

	}

	#endregion
}
