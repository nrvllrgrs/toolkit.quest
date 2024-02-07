using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Events/Quests"), TypeIcon(typeof(IStateTransition))]
	public class FinishTask : Unit
	{
		#region Fields

		[DoNotSerialize, PortLabelHidden]
		public ControlInput inputTrigger { get; private set; }

		[DoNotSerialize]
		public ValueInput finish;

		#endregion

		#region Methods

		protected override void Definition()
		{
			inputTrigger = ControlInput(nameof(inputTrigger), Trigger);
			finish = ValueInput(nameof(finish), Journal.FinishMode.Complete);
		}

		private ControlOutput Trigger(Flow flow)
		{
			// Get object
			var obj = flow.stack.AsReference().component.gameObject;
			var task = Variables.Object(obj).Get<Journal.Task>("$task");

			// Finish task
			switch (flow.GetValue<Journal.FinishMode>(finish))
			{
				case Journal.FinishMode.Complete:
					task.state = Journal.State.Completed;
					break;

				case Journal.FinishMode.Fail:
					task.state = Journal.State.Failed;
					break;

				case Journal.FinishMode.Abandon:
					task.state = Journal.State.Inactive;
					break;
			}

			return null;
		}

		#endregion
	}
}