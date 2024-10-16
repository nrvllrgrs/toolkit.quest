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

		[SerializeField, Tooltip("List of objects that are active while ANY quest matches the state.")]
		private ObjectActivation[] m_objects;

		#endregion

		#region Methods

		private void OnEnable()
		{
			QuestManager.CastInstance.QuestStateChanged += QuestStateChanged;
			QuestManager.CastInstance.TaskStateChanged += TaskStateChanged;
			UpdateLayer<QuestType>(null, CheckQuest);
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

		private void UpdateLayer<T>(T type, Func<T, bool> checkFunc)
			where T : BaseQuestType
		{
			if (type == null)
			{
				// Check all quests
				foreach (var baseType in m_quests)
				{
					if (baseType is QuestType questType && CheckQuest(questType))
					{
						SetObjects();
						break;
					}
					else if (baseType is TaskType taskType && CheckTask(taskType))
					{
						SetObjects();
						break;
					}
				}
			}
			else
			{
				if (!m_quests.Contains(type) || !checkFunc(type))
					return;

				SetObjects();
			}
		}

		private void SetObjects()
		{
			foreach (var obj in m_objects)
			{
				obj.Set();
			}
		}

		private bool CheckQuest(QuestType questType) => QuestManager.CastInstance.GetState(questType) == m_state;
		private bool CheckTask(TaskType taskType) => QuestManager.CastInstance.GetState(taskType) == m_state;

		#endregion
	}
}