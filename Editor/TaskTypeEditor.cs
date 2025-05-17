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
		protected string m_name;

		protected SerializedProperty m_id;
		protected SerializedProperty m_title;
		protected SerializedProperty m_description;
		protected SerializedProperty m_script;

		protected SerializedProperty m_useCounter;
		protected SerializedProperty m_count;

		/// <summary>
		/// Indicates whether title has been changed
		/// </summary>
		private bool m_dirty = false;

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
			m_dirty = false;

			m_name = !Equals(m_taskType.name, m_id.stringValue)
				? m_taskType.name
				: string.Empty;
		}

		private void OnDisable()
		{
			RenameAndSave();
		}

		internal void RenameAndSave()
		{
			if (!m_dirty)
				return;

			if (AssetDatabase.IsSubAsset(target))
			{
				var path = AssetDatabase.GetAssetPath(m_taskType);
				var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
				foreach (var asset in assets)
				{
					if (asset != m_taskType)
						continue;

					// Use either name or ID as asset name
					asset.name = !string.IsNullOrWhiteSpace(m_name)
						? m_name
						: m_id.stringValue;
					AssetUtil.LoadImporter(m_taskType)?.SaveAndReimport();
				}
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(m_id, new GUIContent("ID"));
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginChangeCheck();
			{
				m_name = EditorGUILayout.TextField("Name", m_name);
			}
			if (EditorGUI.EndChangeCheck())
			{
				m_dirty = true;
			}

			EditorGUILayout.Space();

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
			{
				EditorGUIRectLayout.PropertyField(ref position, m_id, new GUIContent("ID"));
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginChangeCheck();
			{
				m_name = EditorGUIRectLayout.TextField(ref position, "Name", m_name);
			}
			if (EditorGUI.EndChangeCheck())
			{
				m_dirty = true;
			}

			EditorGUIRectLayout.Space(ref position);

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
				+ (EditorGUIRectLayout.GetSpaceHeight() * 2)
				+ EditorGUIUtility.singleLineHeight // Name
				+ (EditorGUIUtility.standardVerticalSpacing * 6);

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