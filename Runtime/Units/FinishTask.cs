using static ToolkitEngine.Quest.QuestManager;

namespace ToolkitEngine.Quest.VisualScripting
{
	public class FinishTask : BaseFinishUnit<TaskType, Task>
	{
		#region Properties

		protected override string VariableName => TASK_VAR_NAME;

		#endregion

		#region Methods

		protected override Task GetRuntimeTarget(TaskType taskType)
		{
			return QuestManager.CastInstance.TryGetTask(taskType, out var task)
				? task
				: null;
		}

		protected override void Finish(Task runtime, FinishMode finishMode)
		{
			// Finish quest
			switch (finishMode)
			{
				case FinishMode.Complete:
					runtime.state = State.Completed;
					break;

				case FinishMode.Fail:
					runtime.state = State.Failed;
					break;

				case FinishMode.Abandon:
					runtime.state = State.Inactive;
					break;
			}
		}

		#endregion
	}
}