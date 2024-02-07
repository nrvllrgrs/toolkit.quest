using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;
using ToolkitEngine.Quest;

namespace ToolkitEditor.Quest
{
	[CustomEditor(typeof(TaskType))]
	public class TaskTypeEditor : Editor
    {
		#region Fields

		protected TaskType m_taskType;

		protected SerializedProperty m_id;
		protected SerializedProperty m_title;
		protected SerializedProperty m_description;
		protected SerializedProperty m_script;

		protected SerializedProperty m_useCounter;
		protected SerializedProperty m_count;

		#endregion

		#region Methods

		private void OnEnable()
		{
			if (target == null)
				return;

			m_taskType = target as TaskType;

			m_id = serializedObject.FindProperty(nameof(m_id));
			m_title = serializedObject.FindProperty(nameof(m_title));
			m_description = serializedObject.FindProperty(nameof(m_description));
			m_script = serializedObject.FindProperty(nameof(m_script));

			m_useCounter = serializedObject.FindProperty(nameof(m_useCounter));
			m_count = serializedObject.FindProperty(nameof(m_count));
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(m_id, new GUIContent("ID"));
			EditorGUI.EndDisabledGroup();

			EditorGUILayout.PropertyField(m_title);
			EditorGUILayout.PropertyField(m_description);
			EditorGUILayoutUtility.ScriptableObjectField<ScriptGraphAsset>(m_script, m_taskType);

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(m_useCounter);
			if (m_useCounter.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(m_count);
				--EditorGUI.indentLevel;
			}

			serializedObject.ApplyModifiedProperties();
		}

		public void OnEmbeddedGUI(Rect position)
		{
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUIRectLayout.PropertyField(ref position, m_id, new GUIContent("ID"));
			EditorGUI.EndDisabledGroup();

			EditorGUIRectLayout.PropertyField(ref position, m_title);
			EditorGUIRectLayout.PropertyField(ref position, m_description);
			EditorGUIRectLayout.ScriptableObjectField<ScriptGraphAsset>(ref position, m_script, m_taskType);

			EditorGUIRectLayout.Space(ref position);

			EditorGUIRectLayout.PropertyField(ref position, m_useCounter);
			if (m_useCounter.boolValue)
			{
				++EditorGUI.indentLevel;
				EditorGUIRectLayout.PropertyField(ref position, m_count);
				--EditorGUI.indentLevel;
			}

			serializedObject.ApplyModifiedProperties();
		}

		public float GetEmbeddedHeight()
		{
			float height = EditorGUI.GetPropertyHeight(m_id)
				+ EditorGUI.GetPropertyHeight(m_title)
				+ EditorGUI.GetPropertyHeight(m_description)
				+ EditorGUI.GetPropertyHeight(m_script)
				+ EditorGUI.GetPropertyHeight(m_useCounter)
				+ EditorGUIRectLayout.GetSpaceHeight()
				+ (EditorGUIUtility.standardVerticalSpacing * 5);

			if (m_useCounter.boolValue)
			{
				height += EditorGUI.GetPropertyHeight(m_count)
					+ EditorGUIUtility.standardVerticalSpacing;
			}

			return height;
		}

		#endregion
	}
}