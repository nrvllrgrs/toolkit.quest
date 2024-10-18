using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Quests"), TypeIcon(typeof(IStateTransition))]
	public abstract class BaseFinishUnit<T, TRuntime> : BaseTargetQuestUnit<T,TRuntime>
		where T : BaseQuestType
	{
		#region Fields

		[DoNotSerialize]
		public ValueInput finish;

		#endregion

		#region Methods

		protected override void Definition()
		{
			base.Definition();
			finish = ValueInput(nameof(finish), QuestManager.FinishMode.Complete);
		}

		protected override void Trigger(Flow flow, TRuntime runtime)
		{
			Finish(runtime, flow.GetValue<QuestManager.FinishMode>(finish));
		}

		protected abstract void Finish(TRuntime runtime, QuestManager.FinishMode finishMode);

		#endregion
	}
}