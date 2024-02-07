using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnQuestStateChangedMessageListener : MessageListener
	{
		#region Fields

		private Journal m_journal;
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

			// Quest script
			if (Variables.Object(gameObject).IsDefined(QUEST_VAR))
			{
				m_quest = Variables.Object(gameObject).Get<Journal.Quest>(QUEST_VAR);
				m_quest.StateChanged += Quest_StateChanged;
			}
			// Task script
			else if (Variables.Object(gameObject).IsDefined(TASK_VAR))
			{
				m_quest = Variables.Object(gameObject).Get<Journal.Task>(TASK_VAR)?.quest;
				m_quest.StateChanged += Quest_StateChanged;
			}
			else
			{
				m_journal.onQuestStateChanged.AddListener(Journal_QuestStateChanged);
			}
		}

		private void OnDestroy()
		{
			if (m_quest != null)
			{
				m_quest.StateChanged -= Quest_StateChanged;
			}
			else if (m_journal != null)
			{
				m_journal.onQuestStateChanged.RemoveListener(Journal_QuestStateChanged);
			}
		}

		private void Quest_StateChanged(object sender, QuestEventArgs e)
		{
			Trigger(e);
		}

		private void Journal_QuestStateChanged(QuestEventArgs e)
		{
			Trigger(e);
		}

		protected void Trigger(QuestEventArgs e)
		{
			EventBus.Trigger(EventHooks.OnQuestStateChanged, gameObject, e);
		}

		#endregion
	}
}
