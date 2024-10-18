using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Quests")]
	public abstract class BaseTargetQuestUnit<T, TRuntime> : Unit
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

		#endregion

		#region Properties

		protected abstract string VariableName { get; }

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
		}

		protected virtual ControlOutput Trigger(Flow flow)
		{
			Trigger(flow, GetRuntimeTarget(flow));
			return useTarget ? outputTrigger : null;
		}

		protected abstract void Trigger(Flow flow, TRuntime runtime);

		private TRuntime GetRuntimeTarget(Flow flow)
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
			return runtime;
		}

		protected abstract TRuntime GetRuntimeTarget(T type);

		#endregion
	}
}