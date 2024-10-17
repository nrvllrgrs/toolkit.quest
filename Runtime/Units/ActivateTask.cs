namespace ToolkitEngine.Quest.VisualScripting
{
	public class ActivateTask : BaseActivateUnit<TaskType>
	{
		#region Methods

		protected override void Activate(TaskType value)
		{
			QuestManager.CastInstance.Activate(value);
		}

		#endregion
	}
}