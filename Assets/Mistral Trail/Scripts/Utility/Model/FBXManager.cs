/*
 ### Mistral Model ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/11
 */

using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mistral.Utility.FileX;
using UTJ.FbxExporter;

namespace Mistral.Utility.Model
{
	/// <summary>
	/// This class Serves to manipulate FBX Files. 
	/// </summary>
	public static class FBXManager
	{
		#region Paths

		private static string FBX_PATH = Application.dataPath + "/MTR Generated/FBX Models/";

		#endregion

		#region Private Variables

		private static bool isInitialized = false;

		#endregion

		#region Public Interfaces

		public static void SaveGameObjectAsFBX(GameObject go, string fileName)
		{
			if (!isInitialized)
			{
				Initialize();
			}
			FbxExporter.ExportOptions options = FbxExporter.ExportOptions.defaultValue;
			/// Make sure it is in meter. 
			options.scale_factor = 100f;
			FbxExporter exporter = new FbxExporter(options);
			exporter.CreateScene(fileName);
			exporter.AddNode(go);
			var ret = exporter.Write(FBX_PATH + fileName + ".fbx", FbxExporter.Format.FbxBinary);
			exporter.Release();
		}

		public static void SaveMeshAsFBX(Mesh mesh, string fileName)
		{
			GameObject go = new GameObject(fileName);
			go.AddComponent<MeshFilter>();
			go.AddComponent<MeshRenderer>();
			go.GetComponent<MeshFilter>().mesh = mesh;
			SaveGameObjectAsFBX(go, fileName);
			MonoBehaviour.DestroyImmediate(go);
		}

		#endregion

		#region Private Methods

		private static void Initialize()
		{
			FileManager.TryCreateDirectory(FBX_PATH);
			isInitialized = true;
		}

		#endregion
	}

}