using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Utility.Model;
using Mistral.Effects.Trail;

public class TestFBX : MonoBehaviour
{
	private void Start()
	{
		//FBXManager.SaveMeshAsFBX(GetComponent<MeshFilter>().mesh, "OMG");
		StartCoroutine(Fuck());
	}

	private IEnumerator Fuck()
	{
		yield return new WaitForSeconds(2f);
		Mesh mesh = GetComponent<SmoothTrail>().GetTrailMesh();
		Vector3[] verts = mesh.vertices;
		
		for (int i = 0, imax = mesh.vertices.Length; i < imax; i++)
		{
			verts[i] *= 100f;
		}

		mesh.vertices = verts;
		FBXManager.SaveMeshAsFBX(mesh, "I love Lily! ");
	}
}
