using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;

namespace ToolkitEngine.Quest
{
	public class QuestEvents : MonoBehaviour
	{
		#region Fields

		[SerializeField]
		private List<BaseQuestType> m_quests;

		[SerializeField]
		private QuestManager.State m_state = QuestManager.State.Active;

		[SerializeField, Tooltip("Invoked when ANY quest matches the state."), Foldout("Events")]
		private UnityEvent m_onStateMatched;

		[SerializeField, Tooltip("Invoked when ALL quests don't match the state."), Foldout("Events")]
		private UnityEvent m_onStateMismatched;

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
						InvokeStateMatched();
						break;
					}
					else if (baseType is TaskType taskType && CheckTask(taskType))
					{
						InvokeStateMatched();
						break;
					}
				}
			}
			else if (m_quests.Contains(type) && checkFunc(type))
			{
				InvokeStateMatched();
			}
			else
			{
				m_onStateMismatched?.Invoke();
			}
		}

		private void InvokeStateMatched()
		{
			m_onStateMatched?.Invoke();
		}

		private bool CheckQuest(QuestType questType) => QuestManager.CastInstance.GetState(questType) == m_state;
		private bool CheckTask(TaskType taskType) => QuestManager.CastInstance.GetState(taskType) == m_state;

		#endregion
	}
}