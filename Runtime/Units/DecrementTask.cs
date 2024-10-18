using Unity.VisualScripting;
using static ToolkitEngine.Quest.QuestManager;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Quests"), TypeIcon(typeof(Subtract<>))]
	public class DecrementTask : BaseTargetQuestUnit<TaskType, Task>
	{
		#region Properties

		protected override string VariableName => TASK_VAR_NAME;

		#endregion

		#region Methods

		protected override Task GetRuntimeTarget(TaskType type)
		{
			return QuestManager.CastInstance.TryGetTask(type, out var task)
				? task
				: null;
		}

		protected override void Trigger(Flow flow, Task runtime)
		{
			--runtime.count;
		}

		#endregion
	}
}