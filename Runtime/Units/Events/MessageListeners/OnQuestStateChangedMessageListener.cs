using UnityEngine;
using Unity.VisualScripting;

namespace ToolkitEngine.Quest.VisualScripting
{
	[AddComponentMenu("")]
	public class OnQuestStateChangedMessageListener : MessageListener
	{
		private void Start() => GetComponent<Journal>()?.onQuestStateChanged.AddListener((value) =>
		{
			EventBus.Trigger(EventHooks.OnQuestStateChanged, gameObject, value);
		});
	}
}
