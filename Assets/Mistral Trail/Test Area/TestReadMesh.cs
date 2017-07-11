using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Utility.Model;

public class TestReadMesh : MonoBehaviour 
{
	private void Start()
	{
		byte[] bytes = File.ReadAllBytes(Application.dataPath + "/MTR Generated/Fucking.mtrmesh");
		Mesh mesh = MeshSerializer.ReadMesh(bytes);
		GetComponent<MeshFilter>().mesh = mesh;
	}
}
