using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnQuestStateChangedMessageListener_Tracker : MessageListener
	{
		private void Start() => GetComponent<Journal>()?.onQuestStateChanged.AddListener((value) =>
		{
			EventBus.Trigger(EventHooks.OnQuestStateChanged, value.quest.tracker, value);
		});
	}
}
