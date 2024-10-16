using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Quests"), TypeIcon(typeof(IStateTransition))]
	public abstract class BaseFinishUnit<T, TRuntime> : Unit
		where T : BaseQuestType
	{
		#region Fields

		[DoNotSerialize, PortLabelHidden]
		public ControlInput inputTrigger { get; private set; }

		[DoNotSerialize, PortLabelHidden]
		public ControlOutput outputTrigger { get; private set; }

		[UnitHeaderInspectable("Use Target")]
		public bool useTarget = false;

		[DoNotSerialize]
		public ValueInput target;

		[DoNotSerialize]
		public ValueInput finish;

		#endregion

		#region Properties

		protected abstract string VariableName { get;}

		#endregion

		#region Methods

		protected override void Definition()
		{
			inputTrigger = ControlInput(nameof(inputTrigger), Trigger);

			if (useTarget)
			{
				outputTrigger = ControlOutput(nameof(outputTrigger));
				Succession(inputTrigger, outputTrigger);

				target = ValueInput<T>(nameof(target), null);
				Requirement(target, inputTrigger);
			}

			finish = ValueInput(nameof(finish), QuestManager.FinishMode.Complete);
		}

		private ControlOutput Trigger(Flow flow)
		{
			TRuntime runtime;
			if (!useTarget)
			{
				var obj = flow.stack.AsReference().component.gameObject;
				runtime = Variables.Object(obj).Get<TRuntime>(VariableName);
			}
			else
			{
				runtime = GetRuntimeTarget(flow.GetValue<T>(target));
			}

			// Finish task
			Finish(runtime, flow.GetValue<QuestManager.FinishMode>(finish));

			return useTarget ? outputTrigger : null;
		}

		protected abstract TRuntime GetRuntimeTarget(T type);
		protected abstract void Finish(TRuntime runtime, QuestManager.FinishMode finishMode);

		#endregion
	}
}