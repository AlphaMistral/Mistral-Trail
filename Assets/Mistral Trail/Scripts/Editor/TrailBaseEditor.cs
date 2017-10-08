using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Mistral.Effects.Trail.Editor
{
	public class TrailBaseEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			TrailBase target = (TrailBase)serializedObject.targetObject;
			if (target == null)
				return;
			float defaultLabelWidth = EditorGUIUtility.labelWidth;
			float defaultFieldWidth = EditorGUIUtility.fieldWidth;
			GUILayout.Space(5);

			GUILayout.BeginVertical();
			{
				GUILayout.BeginVertical("", GUI.skin.box);
				{
					GUILayout.Label("核心属性", EditorStyles.boldLabel);
					GUILayout.BeginHorizontal();
					{	
						EditorGUILayout.PropertyField(serializedObject.FindProperty("Emit"));
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					{
						EditorGUILayout.PropertyField(serializedObject.FindProperty("parameter.lifeTime"));
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					{
						EditorGUILayout.PropertyField(serializedObject.FindProperty("parameter.trailMaterial"));
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				GUILayout.BeginVertical("", GUI.skin.box);
				{
					GUILayout.Label("视觉属性", EditorStyles.boldLabel);
					GUILayout.BeginHorizontal();
					{
						EditorGUIUtility.labelWidth = 100f;
						EditorGUIUtility.labelWidth = defaultLabelWidth;
						EditorGUIUtility.fieldWidth = defaultFieldWidth - 80f;
						EditorGUILayout.PropertyField(serializedObject.FindProperty("parameter.sizeOverLife"), new GUIContent("Size Curve"));
						GUILayout.Space(10);
						EditorGUIUtility.labelWidth = defaultLabelWidth - 80f;
						EditorGUILayout.PropertyField(serializedObject.FindProperty("parameter.sizeMultiplier"), new GUIContent("Coef"));
					}
					GUILayout.EndHorizontal();

					EditorGUIUtility.labelWidth = defaultLabelWidth;
					EditorGUIUtility.fieldWidth = defaultFieldWidth;

					EditorGUILayout.PropertyField(serializedObject.FindProperty("parameter.colorOverLife"));

					GUILayout.BeginHorizontal();
					{
						EditorGUILayout.PropertyField(serializedObject.FindProperty("parameter.orientationType"));
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					{
						EditorGUILayout.PropertyField(serializedObject.FindProperty("parameter.trailType"));
					}
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					{
						EditorGUILayout.PropertyField(serializedObject.FindProperty("parameter.forwardOverride"));
					}
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();
			}
			GUILayout.EndVertical();

			DrawSpecificProperties();

			serializedObject.ApplyModifiedProperties();
			serializedObject.UpdateIfDirtyOrScript();
		}

		protected virtual void DrawSpecificProperties()
		{
			
		}
	}
}
