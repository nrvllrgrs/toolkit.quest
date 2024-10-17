using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Quests"), TypeIcon(typeof(IStateTransition))]
	public abstract class BaseActivateUnit<T> : Unit
		where T : BaseQuestType
	{
		#region Fields

		[DoNotSerialize, PortLabelHidden]
		public ControlInput inputTrigger { get; private set; }

		[DoNotSerialize, PortLabelHidden]
		public ControlOutput outputTrigger { get; private set; }

		[DoNotSerialize]
		public ValueInput type;

		#endregion

		#region Methods

		protected override void Definition()
		{
			inputTrigger = ControlInput(nameof(inputTrigger), Trigger);
			outputTrigger = ControlOutput(nameof(outputTrigger));
			Succession(inputTrigger, outputTrigger);

			type = ValueInput<T>(nameof(type), null);
		}

		private ControlOutput Trigger(Flow flow)
		{
			Activate(flow.GetValue<T>(type));
			return outputTrigger;
		}

		protected abstract void Activate(T value);

		#endregion
	}
}