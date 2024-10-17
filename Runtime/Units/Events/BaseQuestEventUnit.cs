using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Events/Quests")]
    public abstract class BaseQuestEventUnit<T> : BaseEventUnit<QuestEventArgs>
		where T : BaseQuestType
    {
		#region Fields

		[UnitHeaderInspectable("Any State")]
		public bool anyState = true;

		[UnitHeaderInspectable("Filtered")]
		public bool filtered = false;

		[DoNotSerialize, PortLabelHidden]
		public ValueInput state { get; private set; }

		[DoNotSerialize]
		public ValueInput filter { get; private set; }

		#endregion

		#region Methods

		protected override void Definition()
		{
			base.Definition();
			if (!anyState)
			{
				state = ValueInput(nameof(state), QuestManager.State.Completed);
			}

			if (filtered)
			{
				filter = ValueInput<T>(nameof(filter), null);
			}
		}

		protected override bool ShouldTrigger(Flow flow, QuestEventArgs args)
		{
			return (anyState || Equals(flow.GetValue<QuestManager.State>(state), args.state))
				&& (!filtered || IsFilterValid(flow.GetValue<T>(filter), args));
		}

		protected abstract bool IsFilterValid(T scriptableObject, QuestEventArgs args);

		#endregion
	}
}