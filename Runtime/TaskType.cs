using System;
using UnityEngine;

namespace ToolkitEngine.Quest
{
	[Serializable]
	public class TaskType : BaseQuestType
	{
		#region Fields

		[SerializeField]
		private QuestType m_questType;

		[SerializeField]
		private bool m_useCounter;

		[SerializeField]
		private int m_count;

		#endregion

		#region Properties

		public QuestType questType { get => m_questType; internal set => m_questType = value; }
		public bool useCounter => m_useCounter;
		public int count => m_count;

		#endregion
	}
}