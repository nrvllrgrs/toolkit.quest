namespace ToolkitEngine.Quest.VisualScripting
{
	public class ActivateQuest : BaseActivateUnit<QuestType>
    {
		#region Methods

		protected override void Activate(QuestType value)
		{
			QuestManager.CastInstance.Activate(value);
		}

		#endregion
	}
}