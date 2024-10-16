using static ToolkitEngine.Quest.QuestManager;

namespace ToolkitEngine.Quest.VisualScripting
{
	public class FinishQuest : BaseFinishUnit<QuestType, QuestManager.Quest>
	{
		#region Properties

		protected override string VariableName => QUEST_VAR_NAME;

		#endregion

		#region Methods

		protected override QuestManager.Quest GetRuntimeTarget(QuestType questType)
		{
			return QuestManager.CastInstance.TryGetQuest(questType, out var quest)
				? quest
				: null;
		}

		protected override void Finish(QuestManager.Quest runtime, FinishMode finishMode)
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