using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnTaskCountChangedMessageListener : MessageListener
	{
		#region Fields

		private QuestManager m_journal;
		private QuestManager.Task m_task;
		private QuestManager.Quest m_quest;

		private const string QUEST_VAR = "$quest";
		private const string TASK_VAR = "$task";

		#endregion

		#region Methods

		private void Awake()
		{
			// Task script
			if (Variables.Object(gameObject).IsDefined(TASK_VAR))
			{
				m_task = Variables.Object(gameObject).Get<QuestManager.Task>(TASK_VAR);
				m_task.CountChanged += Task_CountChanged;
			}
			// Quest script
			else if (Variables.Object(gameObject).IsDefined(QUEST_VAR))
			{
				m_quest = Variables.Object(gameObject).Get<QuestManager.Quest>(QUEST_VAR);
				m_quest.TaskCountChanged += Task_CountChanged;
			}
			else
			{
				QuestManager.CastInstance.TaskCountChanged += Task_CountChanged;
			}
		}

		private void OnDestroy()
		{
			if (m_task != null)
			{
				m_task.CountChanged -= Task_CountChanged;
			}
			else if (m_quest != null)
			{
				m_quest.TaskCountChanged -= Task_CountChanged;
			}
			else if (m_journal != null)
			{
				QuestManager.CastInstance.TaskCountChanged -= Task_CountChanged;
			}
		}

		private void Task_CountChanged(object sender, QuestEventArgs e)
		{
			Trigger(e);
		}

		protected void Trigger(QuestEventArgs e)
		{
			EventBus.Trigger(EventHooks.OnTaskCountChanged, gameObject, e);
		}

		#endregion
	}
}
