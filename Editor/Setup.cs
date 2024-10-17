using System;
using System.Collections.Generic;
using ToolkitEngine.Quest;
using UnityEditor;

namespace ToolkitEditor.Quest.VisualScripting
{
	[InitializeOnLoad]
	public static class Setup
	{
		static Setup()
		{
			var types = new List<Type>()
			{
				typeof(QuestType),
				typeof(TaskType),
				typeof(QuestManager.Quest),
				typeof(QuestManager.Task),
				typeof(QuestManager.State),
				typeof(QuestEventArgs),
			};

			ToolkitEditor.VisualScripting.Setup.Initialize("ToolkitEngine.Quest", types);
		}
	}
}