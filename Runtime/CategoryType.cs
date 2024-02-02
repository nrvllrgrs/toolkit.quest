using System;
using UnityEngine;

namespace ToolkitEngine.Quest
{
	[CreateAssetMenu(menuName = "Toolkit/Quest/Category")]
	public class CategoryType : ScriptableObject
    {
		#region Fields

		[SerializeField]
		private string m_id = Guid.NewGuid().ToString();

		[SerializeField]
		private string m_name;

		[SerializeField]
		private Sprite m_icon;

		[SerializeField]
		private Color m_color = Color.white;

		#endregion

		#region Properties

		public string id => m_id;
		public new string name => m_name;
		public Sprite icon => m_icon;
		public Color color => m_color;

		#endregion
	}
}