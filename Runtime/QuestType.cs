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
			Finish(questType, FinishMode.Complete, questType.m_questsOnCompleted);

			if (InventoryManager.CastInstance.TryGetInventory(QuestManager.CastInstance.Config.rewardInventory, out var inventory))
			{
				foreach (var drop in questType.m_rewards)
				{
					inventory.AddDrop(drop);
				}
			}
		}

		public static void Fail(QuestType questType)
		{
			Finish(questType, FinishMode.Fail, questType.m_questsOnFailed);
		}

		public static void Abandon(QuestType questType)
		{
			Finish(questType, FinishMode.Abandon, questType.m_questsOnAbandoned);
		}

		private static void Finish(QuestType questType, FinishMode finishMode, IEnumerable<QuestType> nextQuests)
		{
			if (nextQuests == null)
				return;

			QuestManager.CastInstance.Finish(questType, finishMode);

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