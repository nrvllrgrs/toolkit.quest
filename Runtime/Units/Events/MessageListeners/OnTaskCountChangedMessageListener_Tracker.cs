using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnTaskCountChangedMessageListener_Tracker : MessageListener
	{
		private void Start() => GetComponent<Journal>()?.onTaskCountChanged.AddListener((value) =>
		{
            EventBus.Trigger(EventHooks.OnTaskCountChanged, value.task.tracker, value);
		});
	}
}
