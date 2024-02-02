using ToolkitEngine.Quest;
using UnityEditor;
using UnityEngine;

namespace ToolkitEditor.Quest
{
	[CustomEditor(typeof(CategoryType))]
	public class CategoryTypeEditor : Editor
	{
		#region Fields

		protected SerializedProperty m_id;
		protected SerializedProperty m_name;
		protected SerializedProperty m_icon;
		protected SerializedProperty m_color;

		#endregion

		#region Properties

		private void OnEnable()
		{
			m_id = serializedObject.FindProperty(nameof(m_id));
			m_name = serializedObject.FindProperty(nameof(m_name));
			m_icon = serializedObject.FindProperty(nameof(m_icon));
			m_color = serializedObject.FindProperty(nameof(m_color));
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(m_id, new GUIContent("ID"));
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.PropertyField(m_name, new GUIContent("Name"));
			EditorGUILayout.ObjectField(m_icon, typeof(Sprite), GUILayout.Height(64), GUILayout.Width(64 + EditorGUIUtility.labelWidth));
			EditorGUILayout.PropertyField(m_color);

			serializedObject.ApplyModifiedProperties();
		}

		#endregion
	}

}
