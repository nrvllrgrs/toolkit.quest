using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnQuestStateChangedMessageListener : MessageListener
	{
		#region Fields

		private QuestManager.Quest m_quest;

		private const string QUEST_VAR = "$quest";
		private const string TASK_VAR = "$task";

		#endregion

		#region Methods

		private void Awake()
		{
			// Quest script
			if (Variables.Object(gameObject).IsDefined(QUEST_VAR))
			{
				m_quest = Variables.Object(gameObject).Get<QuestManager.Quest>(QUEST_VAR);
				m_quest.StateChanged += Quest_StateChanged;
			}
			// Task script
			else if (Variables.Object(gameObject).IsDefined(TASK_VAR))
			{
				m_quest = Variables.Object(gameObject).Get<QuestManager.Task>(TASK_VAR)?.quest;
				m_quest.StateChanged += Quest_StateChanged;
			}
			else
			{
				QuestManager.CastInstance.QuestStateChanged += Quest_StateChanged;
			}
		}

		private void OnDestroy()
		{
			if (m_quest != null)
			{
				m_quest.StateChanged -= Quest_StateChanged;
			}
			else
			{
				QuestManager.CastInstance.QuestStateChanged -= Quest_StateChanged;
			}
		}

		private void Quest_StateChanged(object sender, QuestEventArgs e)
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
