using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Effects.Trail;
using Mistral.Utility.Model;

public class TestStoreMesh : MonoBehaviour 
{
	public Mesh mesh;

	private void Start ()
	{
		mesh = GetComponent<SmoothTrail>().GetTrailMesh();
		StartCoroutine(Test());
	}

	private IEnumerator Test ()
	{
		yield return new WaitForSeconds(2f);
		MeshManager.SaveMeshToResource(mesh, "Fucking");
	}
}
