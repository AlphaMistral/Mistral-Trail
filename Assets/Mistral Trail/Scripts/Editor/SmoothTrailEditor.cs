using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Mistral.Effects.Trail.Editor
{
	[CustomEditor(typeof(SmoothTrail))]
	[CanEditMultipleObjects]
	public class SmoothTrailEditor : TrailBaseEditor
	{
		protected override void DrawSpecificProperties()
		{
			GUILayout.BeginVertical("", GUI.skin.box);
			{
				GUILayout.Label("性能与顶点疏密", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("minVertexDistance"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("maxPointNumber"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("pointsInMiddle"));
			}
			GUILayout.EndVertical();
		}
	}
}
