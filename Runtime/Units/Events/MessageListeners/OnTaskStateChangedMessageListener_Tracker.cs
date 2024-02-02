using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnTaskStateChangedMessageListener_Tracker : MessageListener
	{
		private void Start() => GetComponent<Journal>()?.onTaskStateChanged.AddListener((value) =>
		{
            EventBus.Trigger(EventHooks.OnTaskStateChanged, value.task.tracker, value);
		});
	}
}
