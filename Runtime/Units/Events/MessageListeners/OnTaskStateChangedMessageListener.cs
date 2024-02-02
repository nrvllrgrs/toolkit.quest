using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnTaskStateChangedMessageListener : MessageListener
	{
		private void Start() => GetComponent<Journal>()?.onTaskStateChanged.AddListener((value) =>
		{
            EventBus.Trigger(EventHooks.OnTaskStateChanged, gameObject, value);
		});
	}
}
