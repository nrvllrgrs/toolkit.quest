using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnTaskCountChangedMessageListener : MessageListener
	{
		#region Fields

		private Journal m_journal;
		private Journal.Task m_task;
		private Journal.Quest m_quest;

		private const string QUEST_VAR = "$quest";
		private const string TASK_VAR = "$task";

		#endregion

		#region Methods

		private void Awake()
		{
			m_journal = GetComponentInParent<Journal>();
			if (m_journal == null)
				return;

			// Task script
			if (Variables.Object(gameObject).IsDefined(TASK_VAR))
			{
				m_task = Variables.Object(gameObject).Get<Journal.Task>(TASK_VAR);
				m_task.CountChanged += Task_CountChanged;
			}
			// Quest script
			else if (Variables.Object(gameObject).IsDefined(QUEST_VAR))
			{
				m_quest = Variables.Object(gameObject).Get<Journal.Quest>(QUEST_VAR);
				m_quest.TaskCountChanged += Task_CountChanged;
			}
			else
			{
				m_journal.onTaskCountChanged.AddListener(Journal_TaskStateChanged);
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
				m_journal.onTaskCountChanged.RemoveListener(Journal_TaskStateChanged);
			}
		}

		private void Task_CountChanged(object sender, QuestEventArgs e)
		{
			Trigger(e);
		}

		private void Journal_TaskStateChanged(QuestEventArgs e)
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
