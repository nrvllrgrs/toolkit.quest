using UnityEngine;
using Unity.VisualScripting;
using static ToolkitEngine.Quest.Journal;

namespace ToolkitEngine.Quest.VisualScripting
{
	[UnitCategory("Events/Quests")]
    public abstract class BaseQuestEventUnit<T> : BaseEventUnit<QuestEventArgs>
		where T : ScriptableObject
    {
		#region Enumerators

		public enum Owner
		{
			Default,
			Quest,
			Task
		}

		#endregion

		#region Fields

		[UnitHeaderInspectable("Graph")]
		public Owner owner = Owner.Default;

		[UnitHeaderInspectable("Any State")]
		public bool anyState = true;

		[UnitHeaderInspectable("Filtered")]
		public bool filtered = false;

		[DoNotSerialize, PortLabelHidden]
		public ValueInput state { get; private set; }

		[DoNotSerialize]
		public ValueInput filter { get; private set; }

		#endregion

		#region Methods

		protected override void Definition()
		{
			base.Definition();
			if (!anyState)
			{
				state = ValueInput(nameof(state), Journal.State.Completed);
			}

			if (filtered)
			{
				filter = ValueInput<T>(nameof(filter), null);
			}
		}

		public override void StartListening(GraphStack stack)
		{
			if (owner == Owner.Default)
			{
				base.StartListening(stack);
			}
			else
			{
				var data = stack.GetElementData<Data>(this);
				GameObject target = null;

				switch (owner)
				{
					case Owner.Quest:
						target = ((Journal.Quest)Variables.Object(stack.gameObject).Get("$quest")).journal.gameObject;
						break;

					case Owner.Task:
						target = ((Task)Variables.Object(stack.gameObject).Get("$task")).journal.gameObject;
						break;
				}

				if (target != null)
				{
					data.target = target;
					StartListening(stack, false);
				}
			}
		}

		protected override bool ShouldTrigger(Flow flow, QuestEventArgs args)
		{
			return (anyState || Equals(flow.GetValue<Journal.State>(state), args.state))
				&& (!filtered || IsFilterValid(flow.GetValue<T>(filter), args));
		}

		protected abstract bool IsFilterValid(T scriptableObject, QuestEventArgs args);

		#endregion
	}
}