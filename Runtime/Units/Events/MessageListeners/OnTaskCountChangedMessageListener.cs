using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnTaskCountChangedMessageListener : MessageListener
	{
		private void Start() => GetComponent<Journal>()?.onTaskCountChanged.AddListener((value) =>
		{
            EventBus.Trigger(EventHooks.OnTaskCountChanged, gameObject, value);
		});
	}
}
