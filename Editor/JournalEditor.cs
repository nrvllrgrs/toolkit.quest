using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using ToolkitEngine.Quest;
using UnityEngine;

namespace ToolkitEditor.Quest
{
	[CustomEditor(typeof(Journal))]
    public class JournalEditor : BaseToolkitEditor
    {
		#region Fields

		protected Journal m_journal;

		protected SerializedProperty m_activeQuests;
		protected SerializedProperty m_finishedQuests;

		protected SerializedProperty m_onQuestStateChanged;
		protected SerializedProperty m_onTaskStateChanged;
		protected SerializedProperty m_onTaskCountChanged;

		private ReorderableList m_activeQuestList;
		private ReorderableList m_finishedQuestList;

		private const float INFO_WIDTH = 78f;

		#endregion

		#region Methods

		private void OnEnable()
		{
			m_journal = target as Journal;

			m_activeQuests = serializedObject.FindProperty(nameof(m_activeQuests));
			m_finishedQuests = serializedObject.FindProperty(nameof(m_finishedQuests));

			m_onQuestStateChanged = serializedObject.FindProperty(nameof(m_onQuestStateChanged));
			m_onTaskStateChanged = serializedObject.FindProperty(nameof(m_onTaskStateChanged));
			m_onTaskCountChanged = serializedObject.FindProperty(nameof(m_onTaskCountChanged));

			m_activeQuestList = new ReorderableList(new List<Journal.Quest>(m_journal.activeQuests), typeof(Journal.Quest), false, false, false, false);
			m_activeQuestList.drawElementCallback += (rect, index, isActive, isFocused) =>
			{
				if (!index.Between(0, m_journal.activeQuests.Length - 1))
					return;

				++EditorGUI.indentLevel;

				var quest = m_journal.activeQuests[index];
				var questProp = m_activeQuests.FindPropertyRelative("keys").GetArrayElementAtIndex(index);

				if (EditorGUIRectLayout.Foldout(ref rect, questProp, new GUIContent(quest.questType.title)))
				{
					++EditorGUI.indentLevel;
					EditorGUI.BeginDisabledGroup(true);

					foreach (var task in quest.activeTasks)
					{
						var labelRect = new Rect(rect);
						if (task.taskType.useCounter)
						{
							labelRect.width -= INFO_WIDTH + EditorGUIUtility.standardVerticalSpacing;

							var counterRect = new Rect(rect);
							counterRect.width = INFO_WIDTH;
							counterRect.x = labelRect.width + EditorGUIUtility.standardVerticalSpacing;

							EditorGUIRectLayout.LabelField(ref counterRect, new GUIContent(string.Format("{0}/{1}", task.count, task.taskType.count)));
						}

						EditorGUIRectLayout.LabelField(ref labelRect, new GUIContent(task.taskType.title));
						rect.y = labelRect.y;
					}

					EditorGUI.EndDisabledGroup();
					--EditorGUI.indentLevel;
				}

				--EditorGUI.indentLevel;
			};
			m_activeQuestList.elementHeightCallback += (index) =>
			{
				float height = EditorGUIUtility.singleLineHeight;

				if (index.Between(0, m_journal.activeQuests.Length - 1))
				{
					var quest = m_journal.activeQuests[index];
					var questProp = m_activeQuests.FindPropertyRelative("keys").GetArrayElementAtIndex(index);

					if (questProp.isExpanded)
					{
						height += quest.activeTasks.Length * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
					}
				}

				return height;
			};

			m_finishedQuestList = new ReorderableList(new List<QuestType>(m_journal.finishedQuests), typeof(QuestType), false, false, false, false);
			m_finishedQuestList.drawElementCallback += (rect, index, isActive, isFocused) =>
			{
				if (!index.Between(0, m_journal.finishedQuests.Length - 1))
					return;

				++EditorGUI.indentLevel;
				var questType = m_journal.finishedQuests[index];

				var labelRect = new Rect(rect);
				labelRect.width -= INFO_WIDTH + EditorGUIUtility.standardVerticalSpacing;

				var stateRect = new Rect(rect);
				stateRect.width = INFO_WIDTH;
				stateRect.x = labelRect.width + EditorGUIUtility.standardVerticalSpacing;

				EditorGUIRectLayout.LabelField(ref labelRect, questType.title);
				EditorGUIRectLayout.LabelField(ref stateRect, m_journal.GetState(questType).ToString());

				--EditorGUI.indentLevel;
			};

			m_journal.onQuestStateChanged.AddListener(QuestChanged);
			m_journal.onTaskStateChanged.AddListener(QuestChanged);
			m_journal.onTaskCountChanged.AddListener(QuestChanged);
		}

		private void OnDisable()
		{
			m_journal.onQuestStateChanged.RemoveListener(QuestChanged);
			m_journal.onTaskStateChanged.RemoveListener(QuestChanged);
			m_journal.onTaskCountChanged.RemoveListener(QuestChanged);
		}

		protected override void DrawProperties()
		{
			EditorGUILayout.LabelField("Active Quests");
			m_activeQuestList.DoLayoutList();

			EditorGUILayout.LabelField("Finished Quests");
			m_finishedQuestList.DoLayoutList();
		}

		protected override void DrawEvents()
		{
			if (EditorGUILayoutUtility.Foldout(m_onQuestStateChanged, "Events"))
			{
				EditorGUILayout.PropertyField(m_onQuestStateChanged);
				EditorGUILayout.PropertyField(m_onTaskStateChanged);
				EditorGUILayout.PropertyField(m_onTaskCountChanged);
			}
		}

		private void QuestChanged(QuestEventArgs e)
		{
			m_activeQuestList.list = m_journal.activeQuests;
			m_finishedQuestList.list = m_journal.finishedQuests;

			Repaint();
		}

		#endregion
	}
}