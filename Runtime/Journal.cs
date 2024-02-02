using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace ToolkitEngine.Quest
{
	public class QuestEventArgs : EventArgs
	{
		#region Properties

		public Journal journal { get; private set; }
		public Journal.Quest quest { get; private set; }
		public Journal.Task task { get; private set; }
		public Journal.State state { get; private set; }
		public int delta { get; private set; }

		#endregion

		#region Constructors

		public QuestEventArgs(Journal journal, Journal.Quest quest, Journal.State state)
		{
			this.journal = journal;
			this.quest = quest;
			this.state = state;
		}

		public QuestEventArgs(Journal journal, Journal.Task task, Journal.State state)
		{
			this.journal = journal;
			this.task = task;
			this.state = state;
		}

		public QuestEventArgs(Journal journal, Journal.Task task, int delta)
		{
			this.journal = journal;
			this.task = task;
			this.delta = delta;
		}

		#endregion
	}

	public class Journal : MonoBehaviour
    {
		#region Enumerators

		public enum State
		{
			Inactive,
			Active,
			Completed,
			Abandoned,
			Failed,
		}

		public enum FinishMode
		{
			Complete,
			Fail,
			Abandon
		}

		#endregion

		#region Fields

		[SerializeField]
		private QuestMap m_activeQuests = new();

		[SerializeField]
		private Dictionary<QuestType, State> m_finishedQuests = new();

		#endregion

		#region Events

		[SerializeField]
		private UnityEvent<QuestEventArgs> m_onQuestStateChanged;

		[SerializeField]
		private UnityEvent<QuestEventArgs> m_onTaskStateChanged;

		[SerializeField]
		private UnityEvent<QuestEventArgs> m_onTaskCountChanged;

		#endregion

		#region Properties

		public Quest[] activeQuests => m_activeQuests.Values.ToArray();
		public QuestType[] finishedQuests => m_finishedQuests.Keys.ToArray();
		public UnityEvent<QuestEventArgs> onQuestStateChanged => m_onQuestStateChanged;
		public UnityEvent<QuestEventArgs> onTaskStateChanged => m_onTaskStateChanged;
		public UnityEvent<QuestEventArgs> onTaskCountChanged => m_onTaskCountChanged;

		#endregion

		#region Methods

		public void Activate(QuestType questType)
		{
			// Quest already running, skip
			if (m_activeQuests.ContainsKey(questType))
				return;

			// Quest already finished, skip
			if (m_finishedQuests.ContainsKey(questType))
				return;

			var quest = new Quest(questType, this);
			quest.StateChanged += Quest_StateChanged;
			quest.state = State.Active;
		}

		private void Quest_StateChanged(object sender, QuestEventArgs e)
		{
			if (e.state == State.Active)
			{
				// Add quest to dictionary for tracking
				m_activeQuests.Add(e.quest.questType, e.quest);
			}
			else
			{
				if (m_activeQuests.ContainsKey(e.quest.questType))
				{
					var quest = m_activeQuests[e.quest.questType];
					quest.StateChanged -= Quest_StateChanged;

					m_activeQuests.Remove(e.quest.questType);
				}

				if (e.state == State.Completed || e.state == State.Failed)
				{
					m_finishedQuests.Add(e.quest.questType, e.state);
				}
			}
		}

		public void Finish(QuestType questType, FinishMode finish)
		{
			if (!TryGetQuest(questType, out Quest quest))
				return;

			switch (finish)
			{
				case FinishMode.Complete:
					quest.state = State.Completed;
					break;

				case FinishMode.Fail:
					quest.state = State.Failed;
					break;

				case FinishMode.Abandon:
					quest.state = State.Abandoned;
					break;
			}
		}

		private bool TryGetQuest(QuestType questType, out Quest quest)
		{
			return m_activeQuests.TryGetValue(questType, out quest);
		}

		public bool IsActive(QuestType questType) => questType != null && m_activeQuests.ContainsKey(questType);
		public bool IsFinished(QuestType questType) => questType != null && m_finishedQuests.ContainsKey(questType);

		public State GetState(QuestType questType)
		{
			if (m_activeQuests.ContainsKey(questType))
				return State.Active;

			if (m_finishedQuests.TryGetValue(questType, out State state))
				return state;

			return State.Inactive;
		}

		private static GameObject CreateTracker<T>(Journal journal, string title, ScriptGraphAsset script, string variableName, T value)
		{
			if (script == null)
				return null;

			GameObject tracker = new GameObject(string.Format("{0} Tracker", title));
			//obj.hideFlags |= HideFlags.HideAndDontSave;
			tracker.transform.SetParent(journal.transform);

			var machine = tracker.AddComponent<ScriptMachine>();
			machine.enabled = false;
			machine.nest.macro = script;
			Variables.Object(tracker).Set(variableName, value);
			machine.enabled = true;

			return tracker;
		}

		#endregion

		#region Task Methods

		public void Activate(TaskType taskType)
		{
			if (taskType == null)
				return;

			// Quest not running, skip
			if (!TryGetQuest(taskType.questType, out Quest quest))
				return;

			// Task already running, skip
			if (quest.TryGetTask(taskType, out Task task))
				return;

			quest.Activate(taskType);
		}

		public void Increment(TaskType taskType)
		{
			Modify(taskType, 1);
		}

		public void Decrement(TaskType taskType)
		{
			Modify(taskType, -1);
		}

		public void Modify(TaskType taskType, int delta)
		{
			if (!TryGetTask(taskType, out Task task))
				return;

			task.count += delta;
		}

		public void Finish(TaskType taskType, FinishMode finish)
		{
			if (taskType == null)
				return;

			if (!TryGetTask(taskType, out Task task))
				return;

			switch (finish)
			{
				case FinishMode.Complete:
					task.state = State.Completed;
					break;

				case FinishMode.Fail:
					task.state = State.Failed;
					break;

				case FinishMode.Abandon:
					task.state = State.Abandoned;
					break;
			}
		}

		private bool TryGetTask(TaskType taskType, out Task task)
		{
			if (taskType == null || !TryGetQuest(taskType.questType, out Quest quest))
			{
				task = default;
				return false;
			}

			return quest.TryGetTask(taskType, out task);
		}

		#endregion

		#region Strutures

		[Serializable]
		public class Quest
		{
			#region Fields

			private State m_state = State.Inactive;

			private int m_taskIndex = -1;
			private Dictionary<TaskType, Task> m_activeTasks = new();

			#endregion

			#region Events

			internal event EventHandler<QuestEventArgs> StateChanged;

			#endregion

			#region Properties

			public Journal journal { get; private set; }
			public QuestType questType { get; private set; }

			public State state
			{
				get => m_state;
				internal set
				{
					// No change, skip
					if (m_state == value)
						return;

					// Tracker needs to exist before state updates
					if (value == State.Active)
					{
						tracker = CreateTracker(journal, questType.title, questType.script, "$quest", this);
					}

					m_state = value;

					var args = new QuestEventArgs(journal, this, value);
					StateChanged?.Invoke(this, args);
					journal.onQuestStateChanged?.Invoke(args);

					if (value == State.Active)
					{
						switch (questType.completion)
						{
							case QuestType.Completion.Sequence:
								NextTask();
								break;

							case QuestType.Completion.Parallel:
								foreach (var t in questType.tasks)
								{
									var task = CreateTask(t);
									task.StateChanged += ParallelTask_StateChanged;
								}
								break;

							case QuestType.Completion.Any:
								foreach (var t in questType.tasks)
								{
									var task = CreateTask(t);
									task.StateChanged += AnyTask_StateChanged;
								}
								break;
						}
					}
					else
					{
						if (questType.autoClean)
						{
							Cleanup();
						}

						var tasks = m_activeTasks.Values.ToArray();
						foreach (var task in tasks)
						{
							Cleanup(task);
						}
					}
				}
			}
			internal GameObject tracker { get; private set; }

			public Task[] activeTasks => m_activeTasks.Values.ToArray();

			#endregion

			#region Constructors

			internal Quest(QuestType questType, Journal journal)
			{
				this.questType = questType;
				this.journal = journal;
			}

			#endregion

			#region Methods

			internal void Activate(TaskType taskType)
			{
				if (questType.completion != QuestType.Completion.Custom)
					return;

				CreateTask(taskType);
			}

			private Task CreateTask(TaskType taskType)
			{
				var task = new Task(taskType, journal);

				// Activate before listening to avoid early callback
				m_activeTasks.Add(taskType, task);
				task.Activate();

				return task;
			}

			internal bool TryGetTask(TaskType taskType, out Task task)
			{
				return m_activeTasks.TryGetValue(taskType, out task);
			}

			internal void Cleanup()
			{
				if (!GameObjectExt.IsNull(tracker))
				{
					Destroy(tracker);
				}
			}

			private void Cleanup(Task task)
			{
				if (task == null)
					return;

				m_activeTasks.Remove(task.taskType);
				task.Cleanup();
			}

			private void NextTask()
			{
				++m_taskIndex;

				if (m_taskIndex < questType.tasks.Length)
				{
					var task = CreateTask(questType.tasks[m_taskIndex]);
					task.StateChanged += SequenceTask_StateChanged;
				}
				else
				{
					state = State.Completed;
				}
			}

			private void SequenceTask_StateChanged(object sender, EventArgs e)
			{
				var task = sender as Task;
				task.StateChanged -= SequenceTask_StateChanged;

				if (AttemptComplete(task))
				{
					NextTask();
				}

				Cleanup(task);
			}

			private void ParallelTask_StateChanged(object sender, EventArgs e)
			{
				var task = sender as Task;
				task.StateChanged -= ParallelTask_StateChanged;

				if (AttemptComplete(task))
				{
					if (!m_activeTasks.Any())
					{
						state = State.Completed;
					}
				}
				
				Cleanup(task);
			}

			private void AnyTask_StateChanged(object sender, EventArgs e)
			{
				var task = sender as Task;
				if (task.state == State.Active)
					return;

				var tasks = m_activeTasks.ToArray();
				foreach (var t in tasks)
				{
					task.StateChanged -= AnyTask_StateChanged;
					Cleanup(t.Value);
				}

				state = task.state;
			}

			private bool AttemptComplete(Task task)
			{
				if (task == null)
					return false;

				if (task.state != State.Completed)
				{
					// Match quest state to incomplete task state
					state = task.state;
					return false;
				}

				return true;
			}

			#endregion
		}

		[Serializable]
		public class Task
		{
			#region Fields

			private State m_state = State.Inactive;
			private int m_count;

			#endregion

			#region Events

			internal event EventHandler<QuestEventArgs> StateChanged;

			#endregion

			#region Properties

			public Journal journal { get; private set; }
			public TaskType taskType { get; private set; }

			public int count
			{
				get => m_count;
				set
				{
					value = Mathf.Clamp(value, 0, taskType.count);

					// No change, skip
					if (m_count == value)
						return;

					int delta = value - count;
					m_count = value;

					var args = new QuestEventArgs(journal, this, delta);
					journal.onTaskCountChanged?.Invoke(args);

					if (count == taskType.count)
					{
						state = State.Completed;
					}
				}
			}

			public State state
			{
				get => m_state;
				internal set
				{
					// No change, skip
					if (m_state == value)
						return;

					m_state = value;

					var args = new QuestEventArgs(journal, this, value);
					StateChanged?.Invoke(this, args);
					journal.onTaskStateChanged?.Invoke(args);

					if (m_state != State.Active)
					{
						Cleanup();
					}
				}
			}

			internal GameObject tracker { get; private set; }

			#endregion

			#region Constructors

			internal Task(TaskType taskType, Journal journal)
			{
				this.taskType = taskType;
				this.journal = journal;
			}

			#endregion

			#region Methods

			internal void Activate()
			{
				tracker = CreateTracker(journal, taskType.title, taskType.script, "$task", this);
				state = State.Active;
			}

			internal void Cleanup()
			{
				if (!GameObjectExt.IsNull(tracker))
				{
					Destroy(tracker);
					tracker = null;
				}
			}

			#endregion
		}

		[Serializable]
		public class QuestMap : SerializableDictionary<QuestType, Quest>
		{ }

		#endregion
	}
}