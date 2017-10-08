/*
 ### Mistral Model ###
 Author: Jingping Yu
 RTX: joshuayu
 Created on: 2017/07/10
 */

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mistral.Utility.Model
{
	/// <summary>
	/// This class provides a mesh saving/loading functionality. 
	/// All static functions of course :)
	/// BTW my Chinese typewritting on my MacBook is broken while I am writing this class. 
	/// So I wish u can understand all the shit below. 
	/// GL & HF. 
	/// </summary>
	public static class MeshSerializer 
	{
		#region CONSTANTS

		private static int MAX_VERT_NUMBER = 65000;

		#endregion

		#region Read Mesh

		public static Mesh ReadMesh (byte[] bytes)
		{
			if (bytes == null || bytes.Length < 5)
			{
				throw new Exception("u r not giving me a MESH FILE dude! ");
			}

			BinaryReader buffer = new BinaryReader(new MemoryStream(bytes));

			int vertexNumber = buffer.ReadUInt16();
			int triangleNumber = buffer.ReadUInt16();
			byte format = buffer.ReadByte();

			if (vertexNumber < 0 || vertexNumber > MAX_VERT_NUMBER)
			{
				throw new Exception("The vertex number stored in the Mesh is fucking kidding. ");
			}
			if (triangleNumber < 0 || triangleNumber > MAX_VERT_NUMBER)
			{
				throw new Exception("The triangle number stored in the Mesh is fucking kidding. ");
			}
			/// Copied From Unity Forum. I dunno what it means. 
			/// 2 b honest I don't think it gonna be called anyway if u don't fuck around with my script. 
			if (format < 1 || (format & 1) == 0 || format > 15)
			{
				throw new Exception("Format Error! ");
			}

			/// This is the guy we wanna return :) 
			Mesh resultMesh = new Mesh();

			Vector3[] vertices = new Vector3[vertexNumber];
			ReadVector3Array16Bit(vertices, buffer);
			resultMesh.vertices = vertices;

			/// Here comes the Xuanxue part. 
			/// Don't u ever ask me to explain it.
			if ((format & 2) != 0)
			{
				Vector3[] normals = new Vector3[vertexNumber];
				ReadVector3ArrayBytes(normals, buffer);
				resultMesh.normals = normals;
			}

			if ((format & 4) != 0)
			{
				Vector4[] tangents = new Vector4[vertexNumber];
				ReadVector4ArrayBytes(tangents, buffer);
				resultMesh.tangents = tangents;
			}

			if ((format & 8) != 0)
			{
				Vector2[] uvs = new Vector2[vertexNumber];
				ReadVector2Array16Bit(uvs, buffer);
			}

			int[] triangles = new int[triangleNumber * 3];
			for (int i = 0; i < triangleNumber; i++)
			{
				triangles[i * 3] = buffer.ReadInt16();
				triangles[i * 3 + 1] = buffer.ReadInt16();
				triangles[i * 3 + 2] = buffer.ReadInt16();
			}
			resultMesh.triangles = triangles;

			buffer.Close();

			return resultMesh;
		}

		#endregion

		#region Write Mesh

		public static byte[] WriteMesh(Mesh mesh, bool saveTangents)
		{
			if (!mesh)
			{
				throw new Exception("OK I c u wanna kid me but it's not fun. Next one. ");
			}

			Vector3[] vertices = mesh.vertices;
			Vector3[] normals = mesh.normals;
			Vector4[] tangents = mesh.tangents;
			Vector2[] uvs = mesh.uv;
			int[] triangles = mesh.triangles;

			byte format = 1;

			if (normals.Length > 0)
				format |= 2;
			if (saveTangents && tangents.Length > 0)
				format |= 4;
			if (uvs.Length > 0)
				format |= 8;

			MemoryStream stream = new MemoryStream();
			BinaryWriter buffer = new BinaryWriter(stream);

			var vertCount = (ushort)vertices.Length;
			var triCount = (ushort)(triangles.Length / 3);
			buffer.Write(vertCount);
			buffer.Write(triCount);
			buffer.Write(format);

			WriteVector3Array16Bit(vertices, buffer);
			WriteVector3ArrayBytes(normals, buffer);

			if (saveTangents)
				WriteVector4ArrayBytes(tangents, buffer);
			WriteVector2Array16Bit(uvs, buffer);

			foreach (int idx in triangles)
			{
				buffer.Write((ushort)idx);
			}

			buffer.Close();

			return stream.ToArray();
		}

		#endregion

		#region Read&Write Vectors (ctrl+c & ctrl+v from the forum)

		static void ReadVector3Array16Bit(Vector3[] arr, BinaryReader buf)
		{
			var n = arr.Length;
			if (n == 0)
				return;

			// read bounding box
			Vector3 bmin;
			Vector3 bmax;
			bmin.x = buf.ReadSingle();
			bmax.x = buf.ReadSingle();
			bmin.y = buf.ReadSingle();
			bmax.y = buf.ReadSingle();
			bmin.z = buf.ReadSingle();
			bmax.z = buf.ReadSingle();

			// decode vectors as 16 bit integer components between the bounds
			for (var i = 0; i < n; ++i)
			{
				ushort ix = buf.ReadUInt16();
				ushort iy = buf.ReadUInt16();
				ushort iz = buf.ReadUInt16();
				float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
				float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
				float zz = iz / 65535.0f * (bmax.z - bmin.z) + bmin.z;
				arr[i] = new Vector3(xx, yy, zz);
			}
		}

		static void WriteVector3Array16Bit(Vector3[] arr, BinaryWriter buf)
		{
			if (arr.Length == 0)
				return;

			// calculate bounding box of the array
			var bounds = new Bounds(arr[0], new Vector3(0.001f, 0.001f, 0.001f));
			foreach (var v in arr)
				bounds.Encapsulate(v);

			// write bounds to stream
			var bmin = bounds.min;
			var bmax = bounds.max;
			buf.Write(bmin.x);
			buf.Write(bmax.x);
			buf.Write(bmin.y);
			buf.Write(bmax.y);
			buf.Write(bmin.z);
			buf.Write(bmax.z);

			// encode vectors as 16 bit integer components between the bounds
			foreach (var v in arr)
			{
				var xx = Mathf.Clamp((v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f, 0.0f, 65535.0f);
				var yy = Mathf.Clamp((v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f, 0.0f, 65535.0f);
				var zz = Mathf.Clamp((v.z - bmin.z) / (bmax.z - bmin.z) * 65535.0f, 0.0f, 65535.0f);
				var ix = (ushort)xx;
				var iy = (ushort)yy;
				var iz = (ushort)zz;
				buf.Write(ix);
				buf.Write(iy);
				buf.Write(iz);
			}
		}

		static void ReadVector2Array16Bit(Vector2[] arr, BinaryReader buf)
		{
			var n = arr.Length;
			if (n == 0)
				return;

			// Read bounding box
			Vector2 bmin;
			Vector2 bmax;
			bmin.x = buf.ReadSingle();
			bmax.x = buf.ReadSingle();
			bmin.y = buf.ReadSingle();
			bmax.y = buf.ReadSingle();

			// Decode vectors as 16 bit integer components between the bounds
			for (var i = 0; i < n; ++i)
			{
				ushort ix = buf.ReadUInt16();
				ushort iy = buf.ReadUInt16();
				float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
				float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
				arr[i] = new Vector2(xx, yy);
			}
		}

		static void WriteVector2Array16Bit(Vector2[] arr, BinaryWriter buf)
		{
			if (arr.Length == 0)
				return;

			// Calculate bounding box of the array
			Vector2 bmin = arr[0] - new Vector2(0.001f, 0.001f);
			Vector2 bmax = arr[0] + new Vector2(0.001f, 0.001f);
			foreach (var v in arr)
			{
				bmin.x = Mathf.Min(bmin.x, v.x);
				bmin.y = Mathf.Min(bmin.y, v.y);
				bmax.x = Mathf.Max(bmax.x, v.x);
				bmax.y = Mathf.Max(bmax.y, v.y);
			}

			// Write bounds to stream
			buf.Write(bmin.x);
			buf.Write(bmax.x);
			buf.Write(bmin.y);
			buf.Write(bmax.y);

			// Encode vectors as 16 bit integer components between the bounds
			foreach (var v in arr)
			{
				var xx = (v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f;
				var yy = (v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f;
				var ix = (ushort)xx;
				var iy = (ushort)yy;
				buf.Write(ix);
				buf.Write(iy);
			}
		}

		static void ReadVector3ArrayBytes(Vector3[] arr, BinaryReader buf)
		{
			// decode vectors as 8 bit integers components in -1.0f .. 1.0f range
			var n = arr.Length;
			for (var i = 0; i < n; ++i)
			{
				byte ix = buf.ReadByte();
				byte iy = buf.ReadByte();
				byte iz = buf.ReadByte();
				float xx = (ix - 128.0f) / 127.0f;
				float yy = (iy - 128.0f) / 127.0f;
				float zz = (iz - 128.0f) / 127.0f;
				arr[i] = new Vector3(xx, yy, zz);
			}
		}

		static void WriteVector3ArrayBytes(Vector3[] arr, BinaryWriter buf)
		{
			// encode vectors as 8 bit integers components in -1.0f .. 1.0f range
			foreach (var v in arr)
			{
				var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
				var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
				var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
				buf.Write(ix);
				buf.Write(iy);
				buf.Write(iz);
			}
		}

		static void ReadVector4ArrayBytes(Vector4[] arr, BinaryReader buf)
		{
			// Decode vectors as 8 bit integers components in -1.0f .. 1.0f range
			var n = arr.Length;
			for (var i = 0; i < n; ++i)
			{
				byte ix = buf.ReadByte();
				byte iy = buf.ReadByte();
				byte iz = buf.ReadByte();
				byte iw = buf.ReadByte();
				float xx = (ix - 128.0f) / 127.0f;
				float yy = (iy - 128.0f) / 127.0f;
				float zz = (iz - 128.0f) / 127.0f;
				float ww = (iw - 128.0f) / 127.0f;
				arr[i] = new Vector4(xx, yy, zz, ww);
			}
		}

		static void WriteVector4ArrayBytes(Vector4[] arr, BinaryWriter buf)
		{
			// Encode vectors as 8 bit integers components in -1.0f .. 1.0f range
			foreach (var v in arr)
			{
				var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
				var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
				var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
				var iw = (byte)Mathf.Clamp(v.w * 127.0f + 128.0f, 0.0f, 255.0f);
				buf.Write(ix);
				buf.Write(iy);
				buf.Write(iz);
				buf.Write(iw);
			}
		}

		#endregion
	}

}
