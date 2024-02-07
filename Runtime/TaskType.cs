using System;
using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest
{
	[Serializable]
	public class TaskType : ScriptableObject
	{
		#region Fields

		[SerializeField]
		private string m_id = Guid.NewGuid().ToString();

		[SerializeField]
		private string m_title;

		[SerializeField, TextArea]
		private string m_description;

		[SerializeField]
		private ScriptGraphAsset m_script;

		[SerializeField]
		private bool m_useCounter;

		[SerializeField]
		private int m_count;

		#endregion

		#region Properties

		public QuestType questType { get; internal set; }
		public string id => m_id;
		public string title => m_title;
		public string description => m_description;
		public ScriptGraphAsset script => m_script;
		public bool useCounter => m_useCounter;
		public int count => m_count;

		#endregion
	}
}