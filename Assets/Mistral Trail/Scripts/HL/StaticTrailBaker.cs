using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Utility.Model;

namespace Mistral.Effects.Trail
{
	#if UNITY_EDITOR

	/// <summary>
	/// This Class provides necessary parameters to control the baked trail. 
	/// </summary>
	public class StaticTrailBaker : MonoBehaviour 
	{
		#region Public Variables

		/// <summary>
		/// The name of the trail. 
		/// Influences the final file name. 
		/// </summary>
		public string trailName;

		/// <summary>
		/// The trail to baked. 
		/// However Please feel free to leave this as NULL. 
		/// A default yet univerisal trail will be applied :) 
		/// </summary>
		public TrailBase trailToBake;

		/// <summary>
		/// The Mesh will be stored after the time duration. 
		/// </summary>
		public float duration = 2f;

		public bool emit = false;

		#endregion

		#region MonoBehaviours

		private void Awake()
		{
			if (trailToBake == null)
			{
				if (GetComponent<TrailBase>() != null)
				{
					trailToBake = GetComponent<TrailBase>();
					return;
				}
				Debug.Log("You must assign a trailToBake! ");
				DestroyImmediate(this);
			}
			trailToBake.Emit = false;
		}

		private void Start()
		{
			StartCoroutine(StoreMesh());
		}

		private void Update()
		{
			trailToBake.Emit = emit;
		}

		#endregion 

		#region Private Methods

		private IEnumerator StoreMesh()
		{
			yield return new WaitForSeconds(duration);
			yield return new WaitForEndOfFrame();

			/// Why the Zig-zag
			/// Please Refer to TrailBase.cs in which all trails are generated in the world space.
			/// Hence we need to convert the generated mesh or it will have a strange pivot :)
			/// No Performance Consideration on this Part because it is Editor Only. 
			Matrix4x4 mat = transform.worldToLocalMatrix;
			Mesh toBake = new Mesh();
			Mesh original = trailToBake.GetTrailMesh();
			Vector3[] verts = new Vector3[original.vertices.Length];
			if (toBake != null)
			{
				for (int i = 0; i < original.vertices.Length; i++)
				{
					verts[i] = mat.MultiplyPoint3x4(original.vertices[i]);
				}
				toBake.vertices = verts;
				toBake.uv = original.uv;
				toBake.normals = original.normals;
				Debug.Log(original.GetIndices(0).Length);
				toBake.SetIndices(original.GetIndices(0), MeshTopology.Triangles, 0);
				FBXManager.SaveMeshAsFBX(toBake, trailName);
			}
		}

		#endregion
	}

	#endif
}
