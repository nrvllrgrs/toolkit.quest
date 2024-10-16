using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Events/Quests")]
	public class OnQuestStateChanged : BaseQuestEventUnit<QuestType>
	{
		#region Properties

		public override Type MessageListenerType => typeof(OnQuestStateChangedMessageListener);

		#endregion

		#region Methods

		protected override bool IsFilterValid(QuestType scriptableObject, QuestEventArgs args)
		{
			return Equals(scriptableObject, args.quest.questType);
		}

		#endregion
	}
}