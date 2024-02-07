using System;
using System.Collections.Generic;
using UnityEngine;
using ToolkitEngine.Inventory;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest
{
	[CreateAssetMenu(menuName = "Toolkit/Quest/Quest")]
	public class QuestType : ScriptableObject
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
		private string m_id = Guid.NewGuid().ToString();

		[SerializeField]
		private CategoryType m_category;

		[SerializeField]
		private int m_order;

		[SerializeField]
		private string m_title;

		[SerializeField, TextArea]
		private string m_description;

		[SerializeField]
		private List<DropEntry> m_rewards;

		[SerializeField]
		private List<TaskType> m_tasks = new();

		[SerializeField]
		private Completion m_completion = Completion.Sequence;

		[SerializeField]
		private ScriptGraphAsset m_script;

		[SerializeField]
		private List<QuestType> m_questsOnCompleted = new();

		[SerializeField]
		private List<QuestType> m_questsOnFailed = new();

		[SerializeField]
		private List<QuestType> m_questsOnAbandoned = new();

		#endregion

		#region Properties

		public string id => m_id;
		public CategoryType category => m_category;
		public int order => m_order;
		public string title => m_title;
		public string description => m_description;
		public TaskType[] tasks => m_tasks.ToArray();
		public Completion completion => m_completion;
		public ScriptGraphAsset script => m_script;

#if UNITY_EDITOR
		internal List<TaskType> taskList { get => m_tasks; set => m_tasks = value; }
#endif
		#endregion

		#region Methods

		public void Complete(Journal journal)
		{
			Finish(journal, m_questsOnCompleted);

			var inventory = journal?.GetComponentInParent<Inventory.Inventory>();
			if (inventory != null)
			{
				foreach (var drop in m_rewards)
				{
					inventory.AddDrop(drop);
				}
			}
		}

		public void Fail(Journal journal)
		{
			Finish(journal, m_questsOnFailed);
		}

		public void Abandon(Journal journal)
		{
			Finish(journal, m_questsOnAbandoned);
		}

		private void Finish(Journal journal, IEnumerable<QuestType> nextQuests)
		{
			if (journal == null || nextQuests == null)
				return;

			foreach (var quest in nextQuests)
			{
				if (quest == null)
					continue;

				journal.Activate(quest);
			}
		}

		#endregion
	}
}