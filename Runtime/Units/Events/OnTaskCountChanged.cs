using System;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Events/Quests")]
	public class OnTaskCountChanged : BaseQuestEventUnit<TaskType>
	{
		#region Fields

		[DoNotSerialize]
		public ValueInput taskType { get; private set; }

		#endregion

		#region Properties

		public override Type MessageListenerType
		{
			get => owner == Owner.Default
				? typeof(OnTaskCountChangedMessageListener)
				: typeof(OnTaskCountChangedMessageListener_Tracker);
		}

		#endregion

		#region Methods

		protected override bool IsFilterValid(TaskType scriptableObject, QuestEventArgs args)
		{
			return Equals(scriptableObject, args.task.taskType);
		}

		#endregion
	}
}