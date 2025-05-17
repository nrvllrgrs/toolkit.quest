using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using ToolkitEngine.Quest;
using Unity.VisualScripting;
using System;

namespace ToolkitEditor.Quest
{
	[CustomEditor(typeof(QuestType))]
    public class QuestTypeEditor : Editor
    {
        #region Fields

		protected QuestType m_questType;

		protected SerializedProperty m_id;
		protected SerializedProperty m_category;
		protected SerializedProperty m_order;

		protected SerializedProperty m_title;
		protected SerializedProperty m_description;
		protected SerializedProperty m_rewards;
		protected SerializedProperty m_script;

		protected SerializedProperty m_tasks;
		protected SerializedProperty m_completion;

		protected SerializedProperty m_questsOnCompleted;
		protected SerializedProperty m_questsOnFailed;
		protected SerializedProperty m_questsOnAbandoned;

        // Used to map reorderable list to evaluator
        private ReorderableList m_reorderableList;

		/// <summary>
		/// Embedded task editors
		/// </summary>
		private Dictionary<TaskType, TaskTypeEditor> m_taskEditors = new();

		#endregion

		#region Methods

		private void OnEnable()
        {
            m_questType = target as QuestType;

            m_id = serializedObject.FindProperty(nameof(m_id));
            m_category = serializedObject.FindProperty(nameof(m_category));
            m_order = serializedObject.FindProperty(nameof(m_order));

            m_title = serializedObject.FindProperty(nameof(m_title));
            m_description = serializedObject.FindProperty(nameof(m_description));
			m_rewards = serializedObject.FindProperty(nameof(m_rewards));
			m_script = serializedObject.FindProperty(nameof(m_script));

			m_tasks = serializedObject.FindProperty(nameof(m_tasks));
			m_completion = serializedObject.FindProperty(nameof(m_completion));

			m_questsOnCompleted = serializedObject.FindProperty(nameof(m_questsOnCompleted));
			m_questsOnAbandoned = serializedObject.FindProperty(nameof(m_questsOnAbandoned));
			m_questsOnFailed = serializedObject.FindProperty(nameof(m_questsOnFailed));

            m_reorderableList = new ReorderableList(new List<TaskType>(m_questType.taskList), typeof(TaskType), true, false, true, true);
			m_taskEditors.Clear();

			m_reorderableList.drawElementCallback += (rect, index, isActive, isFocused) =>
			{
				if (!index.Between(0, m_questType.taskList.Count - 1))
					return;

				var taskProp = m_tasks.GetArrayElementAtIndex(index);
				if (taskProp == null)
					return;

				var serializedTaskType = new SerializedObject(taskProp.objectReferenceValue);
				var taskType = serializedTaskType.targetObject as TaskType;

				string name = !Equals(taskType.name, taskType.id)
					? taskType.name
					: string.Format("Task {0}", index + 1);

				++EditorGUI.indentLevel;
				bool expanded = EditorGUIRectLayout.Foldout(ref rect, taskProp, new GUIContent(name));
				--EditorGUI.indentLevel;

				if (expanded)
				{
					var taskEditor = GetTaskEditor(taskType);
					taskEditor.OnEmbeddedGUI(rect);
				}
			};
			m_reorderableList.elementHeightCallback += ElementHeightCallback;

            m_reorderableList.onCanAddCallback += OnCanAddCallback;
			m_reorderableList.onAddDropdownCallback += OnAddDropdownCallback;
			m_reorderableList.onReorderCallbackWithDetails += OnReorderCallback;
			m_reorderableList.onCanRemoveCallback += OnCanRemoveCallback;
			m_reorderableList.onRemoveCallback += OnRemoveCallback;
		}

		private void OnDisable()
		{
			foreach (var taskEditor in m_taskEditors.Values)
			{
				taskEditor.RenameAndSave();
			}
		}

		public override void OnInspectorGUI()
        {
            serializedObject.Update();

			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(m_id, new GUIContent("ID"));
			EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(m_category);
            EditorGUILayout.PropertyField(m_order);

			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_title);
            EditorGUILayout.PropertyField(m_description);
			EditorGUILayout.PropertyField(m_rewards);
			EditorGUILayoutUtility.ScriptableObjectField<ScriptGraphAsset>(m_script, m_questType);

			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Tasks",  EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(m_completion);

			m_reorderableList.DoLayoutList();

			EditorGUILayout.Separator();

			EditorGUILayout.LabelField("Next Quests", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(m_questsOnCompleted, new GUIContent("On Completed"));
			EditorGUILayout.PropertyField(m_questsOnFailed, new GUIContent("On Failed"));
			EditorGUILayout.PropertyField(m_questsOnAbandoned, new GUIContent("On Abandoned"));

			serializedObject.ApplyModifiedProperties();
		}

		private TaskTypeEditor GetTaskEditor(TaskType taskType)
		{
			if (!m_taskEditors.TryGetValue(taskType, out var editor))
			{
				editor = CreateEditor(taskType, typeof(TaskTypeEditor)) as TaskTypeEditor;
				m_taskEditors.Add(taskType, editor);
			}
			return editor;
		}

		#endregion

		#region ReorderableList Callbacks

		private float ElementHeightCallback(int index)
		{
			if (!index.Between(0, m_questType.taskList.Count - 1))
				return 0f;

			var taskProp = m_tasks.GetArrayElementAtIndex(index);
			if (taskProp == null)
				return 0f;

			float height = EditorGUIUtility.singleLineHeight
				+ EditorGUIUtility.standardVerticalSpacing;

			if (taskProp.isExpanded)
			{
				var serializedTaskType = new SerializedObject(taskProp.objectReferenceValue);
				var taskType = serializedTaskType.targetObject as TaskType;

				var taskEditor = GetTaskEditor(taskType);
				height += taskEditor.GetEmbeddedHeight();
			}

			return height;

		}

		private bool OnCanAddCallback(ReorderableList list)
		{
			return true;
		}

		private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
		{
			var taskType = CreateInstance<TaskType>();
			taskType.questType = m_questType;
			taskType.name = Guid.NewGuid().ToString();

			m_questType.taskList.Add(taskType);
			AssetDatabase.AddObjectToAsset(taskType, m_questType);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			m_reorderableList.list = m_questType.taskList;
			Repaint();
		}

		private void OnReorderCallback(ReorderableList list, int oldIndex, int newIndex)
		{
			m_questType.taskList = list.list as List<TaskType>;
		}

		private bool OnCanRemoveCallback(ReorderableList list)
		{
			return m_questType.taskList.Count > 0;
		}

		private void OnRemoveCallback(ReorderableList list)
		{
			var taskType = m_questType.taskList[list.index];

			// Remove from embedded editor map
			m_taskEditors.Remove(taskType);

			m_questType.taskList.RemoveAt(list.index);
			list.list = m_questType.taskList;
			list.index = Mathf.Clamp(list.index, 0, m_questType.taskList.Count - 1);

			if (taskType.script != null)
			{
				AssetDatabase.RemoveObjectFromAsset(taskType.script);
			}

			AssetDatabase.RemoveObjectFromAsset(taskType);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		#endregion
	}
}