using System;
using Unity.VisualScripting;
using UnityEngine;

namespace ToolkitEngine.Quest
{
	public abstract class BaseQuestType : ScriptableObject
    {
		#region Fields

		[SerializeField]
		protected string m_id = Guid.NewGuid().ToString();

		[SerializeField]
		protected string m_title;

		[SerializeField, TextArea]
		protected string m_description;

		[SerializeField]
		protected ScriptGraphAsset m_script;

		#endregion

		#region Properties

		public string id => m_id;
		public string title => m_title;
		public string description => m_description;
		public ScriptGraphAsset script => m_script;

		#endregion
	}
}