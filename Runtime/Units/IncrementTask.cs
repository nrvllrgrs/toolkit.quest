using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Quests"), TypeIcon(typeof(Add<>))]
	public class IncrementTask : Unit
	{
		#region Properties

		[DoNotSerialize, PortLabelHidden]
		public ControlInput inputTrigger { get; private set; }

		#endregion

		#region Methods

		protected override void Definition()
		{
			inputTrigger = ControlInput(nameof(inputTrigger), Trigger);
		}

		private ControlOutput Trigger(Flow flow)
		{
			// Get object
			var obj = flow.stack.AsReference().component.gameObject;
			var task = Variables.Object(obj).Get<QuestManager.Task>("task");

			// Increment task
			++task.count;

			return null;
		}

		#endregion
	}
}