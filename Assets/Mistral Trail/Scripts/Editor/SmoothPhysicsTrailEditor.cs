using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Mistral.Effects.Trail.Editor
{
	[CustomEditor(typeof(SmoothPhysicsTrail))]
	[CanEditMultipleObjects]
	public class SmoothPhysicsTrailEditor : SmoothTrailEditor
	{
		protected override void DrawSpecificProperties()
		{
			base.DrawSpecificProperties();
			GUILayout.BeginVertical("", GUI.skin.box);
			{
				GUILayout.Label("物理参数", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("force"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("inheritVelocity"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("drag"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("frequency"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("amplitude"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("turbulenceStrength"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("velocityByDistance"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("aproximatedFlyDistance"));
			}
			GUILayout.EndVertical();
		}
	}
}
