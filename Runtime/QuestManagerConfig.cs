using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolkitEngine.Quest
{
	[CreateAssetMenu(menuName = "Toolkit/Config/QuestManager Config")]
	public class QuestManagerConfig : ScriptableObject
    {
		#region Fields

		[SerializeField]
		private string m_rewardInventory;

		#endregion

		#region Properties

		public string rewardInventory => m_rewardInventory;

		#endregion
	}
}