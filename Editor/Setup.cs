using System;
using System.Collections.Generic;
using ToolkitEngine.Quest;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

namespace ToolkitEditor.Quest.VisualScripting
{
	[InitializeOnLoad]
	public static class Setup
	{
		static Setup()
		{
			bool dirty = false;
			var config = BoltCore.Configuration;

			var assemblyName = new LooseAssemblyName("ToolkitEngine.Quest");
			if (!config.assemblyOptions.Contains(assemblyName))
			{
				config.assemblyOptions.Add(assemblyName);
				dirty = true;
			}

			var types = new List<Type>()
			{
				typeof(QuestType),
				typeof(TaskType),
				typeof(QuestManager),
				typeof(QuestManager.Quest),
				typeof(QuestManager.Task),
				typeof(QuestManager.State),
				typeof(QuestEventArgs),
			};

			foreach (var type in types)
			{
				if (!config.typeOptions.Contains(type))
				{
					config.typeOptions.Add(type);
					dirty = true;

					Debug.LogFormat("Adding {0} to Visual Scripting type options.", type.FullName);
				}
			}

			if (dirty)
			{
				var metadata = config.GetMetadata(nameof(config.typeOptions));
				metadata.Save();
				Codebase.UpdateSettings();
				UnitBase.Rebuild();
			}
		}
	}
}