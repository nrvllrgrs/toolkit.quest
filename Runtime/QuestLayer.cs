using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToolkitEngine.Quest
{
	public class QuestLayer : MonoBehaviour
    {
		#region Fields

		[SerializeField]
		private List<BaseQuestType> m_quests;

		[SerializeField]
		private QuestManager.State m_state = QuestManager.State.Active;

		[SerializeField]
		private bool m_toggleOnStart = true;

		[SerializeField, Tooltip("List of objects that are active while ANY quest matches the state.")]
		private ObjectActivation[] m_objects;

		#endregion

		#region Methods

		private void Start()
		{
			UpdateLayer<QuestType>(null, CheckQuest, m_toggleOnStart);
		}

		private void OnEnable()
		{
			QuestManager.CastInstance.QuestStateChanged += QuestStateChanged;
			QuestManager.CastInstance.TaskStateChanged += TaskStateChanged;
		}

		private void OnDisable()
		{
			QuestManager.CastInstance.QuestStateChanged -= QuestStateChanged;
			QuestManager.CastInstance.TaskStateChanged -= TaskStateChanged;
		}

		private void QuestStateChanged(object sender, QuestEventArgs e)
		{
			UpdateLayer(e.quest.questType, CheckQuest);
		}

		private void TaskStateChanged(object sender, QuestEventArgs e)
		{
			UpdateLayer(e.task.taskType, CheckTask);
		}

		private void UpdateLayer<T>(T type, Func<T, bool> checkFunc, bool allowInvert = false)
			where T : BaseQuestType
		{
			if (type == null)
			{
				// Check all quests
				foreach (var baseType in m_quests)
				{
					if (baseType is QuestType questType)
					{
						SetObjects(CheckQuest(questType), allowInvert);
						break;
					}
					else if (baseType is TaskType taskType)
					{
						SetObjects(CheckTask(taskType), allowInvert);
						break;
					}
				}
			}
			else
			{
				if (!m_quests.Contains(type))
					return;

				SetObjects(checkFunc(type), allowInvert);
			}
		}

		private void SetObjects(bool value, bool allowInvert = false)
		{
			if (value)
			{
				SetObjects();
			}
			else if (allowInvert)
			{
				InvertObjects();
			}
		}

		private void SetObjects()
		{
			foreach (var obj in m_objects)
			{
				obj.Set();
			}
		}

		private void InvertObjects()
		{
			foreach (var obj in m_objects)
			{
				obj.Invert();
			}
		}

		private bool CheckQuest(QuestType questType) => QuestManager.CastInstance.GetState(questType) == m_state;
		private bool CheckTask(TaskType taskType) => QuestManager.CastInstance.GetState(taskType) == m_state;

		#endregion
	}
}