using System.Collections.Generic;
using UnityEngine;
using ToolkitEngine.Inventory;
using static ToolkitEngine.Quest.QuestManager;

namespace ToolkitEngine.Quest
{
	[CreateAssetMenu(menuName = "Toolkit/Quest/Quest")]
	public class QuestType : BaseQuestType
    {
		#region Enumerators

		public enum Completion
		{
			Custom,
			Sequence,
			Parallel,
			Any,
		}

		#endregion

		#region Fields

		[SerializeField]
		private CategoryType m_category;

		[SerializeField]
		private int m_order;

		[SerializeField]
		private List<DropEntry> m_rewards;

		[SerializeField]
		private List<TaskType> m_tasks = new();

		[SerializeField]
		private Completion m_completion = Completion.Sequence;

		[SerializeField]
		private List<QuestType> m_questsOnCompleted = new();

		[SerializeField]
		private List<QuestType> m_questsOnFailed = new();

		[SerializeField]
		private List<QuestType> m_questsOnAbandoned = new();

		#endregion

		#region Properties
		public CategoryType category => m_category;
		public int order => m_order;
		public TaskType[] tasks => m_tasks.ToArray();
		public Completion completion => m_completion;

#if UNITY_EDITOR
		internal List<TaskType> taskList { get => m_tasks; set => m_tasks = value; }
#endif
		#endregion

		#region Methods

		public static void Activate(QuestType questType)
		{
			QuestManager.CastInstance.Activate(questType);
		}

		public static void Complete(QuestType questType)
		{
			QuestManager.CastInstance.Finish(questType, FinishMode.Complete);
		}

		public static void Fail(QuestType questType)
		{
			QuestManager.CastInstance.Finish(questType, FinishMode.Fail);
		}

		public static void Abandon(QuestType questType)
		{
			QuestManager.CastInstance.Finish(questType, FinishMode.Abandon);
		}

		internal static void Award(QuestType questType)
		{
			if (questType.m_rewards.Count == 0)
				return;

			if (InventoryManager.CastInstance.TryGetInventory(QuestManager.CastInstance.Config.rewardInventory, out var inventory))
			{
				foreach (var drop in questType.m_rewards)
				{
					inventory.AddDrop(drop);
				}
			}
		}

		internal static void ActiveNextQuests(QuestType questType, State state)
		{
			List<QuestType> nextQuests = null;
			switch (state)
			{
				case State.Completed:
					nextQuests = questType.m_questsOnCompleted;
					break;

				case State.Failed:
					nextQuests = questType.m_questsOnFailed;
					break;

				case State.Inactive:
					nextQuests = questType.m_questsOnAbandoned;
					break;
			}

			if (nextQuests == null)
				return;

			foreach (var quest in nextQuests)
			{
				if (quest == null)
					continue;

				Activate(quest);
			}
		}

		#endregion
	}
}