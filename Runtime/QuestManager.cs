using Codice.CM.Client.Differences;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace ToolkitEngine.Quest
{
	public class QuestEventArgs : EventArgs
	{
		#region Properties

		public QuestManager.Quest quest { get; private set; }
		public QuestManager.Task task { get; private set; }
		public QuestManager.State state { get; private set; }
		public int delta { get; private set; }

		#endregion

		#region Constructors

		public QuestEventArgs(QuestManager.Quest quest, QuestManager.State state)
		{
			this.quest = quest;
			this.state = state;
		}

		public QuestEventArgs(QuestManager.Task task, QuestManager.State state)
		{
			this.task = task;
			this.state = state;
		}

		public QuestEventArgs(QuestManager.Task task, int delta)
		{
			this.task = task;
			this.delta = delta;
		}

		#endregion
	}

	public class QuestManager : ConfigurableSubsystem<QuestManager, QuestManagerConfig>
    {
		#region Enumerators

		public enum State
		{
			Inactive,
			Active,
			Completed,
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

		private QuestMap m_activeQuests = new();
		private Dictionary<QuestType, State> m_finishedQuests = new();

#if UNITY_EDITOR
		private static GameObject s_container;
#endif

		public const string QUEST_VAR_NAME = "$quest";
		public const string TASK_VAR_NAME = "$task";

		#endregion

		#region Events

		public event EventHandler<QuestEventArgs> QuestStateChanged;
		public event EventHandler<QuestEventArgs> TaskStateChanged;
		public event EventHandler<QuestEventArgs> TaskCountChanged;

		#endregion

		#region Properties

		public Quest[] activeQuests => m_activeQuests.Values.ToArray();
		public QuestType[] finishedQuests => m_finishedQuests.Keys.ToArray();

#if UNITY_EDITOR
		private static Transform container
		{
			get
			{
				if (s_container == null)
				{
					s_container = new GameObject("Quests");
					UnityEngine.Object.DontDestroyOnLoad(s_container);
				}
				return s_container.transform;
			}
		}
#endif
		#endregion

		#region Methods

		protected override void Initialize()
		{
			base.Initialize();

			m_activeQuests = new();
			m_finishedQuests = new();

#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				s_container = null;
			}
#endif
		}

		public void Activate(QuestType questType)
		{
			// Quest already running, skip
			if (m_activeQuests.ContainsKey(questType))
				return;

			// Quest already finished, skip
			if (m_finishedQuests.ContainsKey(questType))
				return;

			var quest = new Quest(questType);
			quest.StateChanged += Quest_StateChanged;
			quest.TaskStateChanged += Quest_TaskStateChanged;
			quest.TaskCountChanged += Quest_TaskCountChanged;

			// Add quest to dictionary for tracking
			m_activeQuests.Add(questType, quest);
			quest.state = State.Active;
		}

		private void Quest_StateChanged(object sender, QuestEventArgs e)
		{
			QuestStateChanged?.Invoke(sender, e);

			if (e.state == State.Active)
				return;

			if (m_activeQuests.ContainsKey(e.quest.questType))
			{
				var quest = m_activeQuests[e.quest.questType];
				quest.StateChanged -= Quest_StateChanged;
				quest.TaskStateChanged -= Quest_TaskStateChanged;
				quest.TaskCountChanged -= Quest_TaskCountChanged;

				m_activeQuests.Remove(e.quest.questType);
			}

			// Possible to complete / fail quests that are not active
			if (e.state == State.Completed || e.state == State.Failed)
			{
				m_finishedQuests.Add(e.quest.questType, e.state);
			}
		}

		private void Quest_TaskCountChanged(object sender, QuestEventArgs e)
		{
			TaskStateChanged?.Invoke(sender, e);
		}

		private void Quest_TaskStateChanged(object sender, QuestEventArgs e)
		{
			TaskCountChanged?.Invoke(sender, e);
		}

		internal void Finish(QuestType questType, FinishMode finish)
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
					quest.state = State.Inactive;
					break;
			}
		}

		internal bool TryGetQuest(QuestType questType, out Quest quest)
		{
			return m_activeQuests.TryGetValue(questType, out quest);
		}

		public State GetState(QuestType questType)
		{
			if (m_activeQuests.ContainsKey(questType))
				return State.Active;

			if (m_finishedQuests.TryGetValue(questType, out State state))
				return state;

			return State.Inactive;
		}

		public State GetState(BaseQuestType baseType)
		{
			if (baseType is QuestType questType)
			{
				return GetState(questType);
			}
			else if (baseType is TaskType taskType)
			{
				return GetState(taskType);
			}
			return State.Inactive;
		}

		public bool IsActive(QuestType questType) => questType != null && m_activeQuests.ContainsKey(questType);
		public bool IsFinished(QuestType questType) => questType != null && m_finishedQuests.ContainsKey(questType);

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
					task.state = State.Inactive;
					break;
			}
		}

		internal bool TryGetTask(TaskType taskType, out Task task)
		{
			if (taskType == null || !TryGetQuest(taskType.questType, out Quest quest))
			{
				task = default;
				return false;
			}

			return quest.TryGetTask(taskType, out task);
		}

		public State GetState(TaskType taskType)
		{
			if (taskType?.questType == null)
				return State.Inactive;

			if (m_activeQuests.TryGetValue(taskType.questType, out var quest)
				&& quest.TryGetTask(taskType, out var task))
			{
				return task.state;
			}

			if (m_finishedQuests.TryGetValue(taskType.questType, out State state))
				return state;

			return State.Inactive;
		}

		public bool IsActive(TaskType taskType) => taskType != null && GetState(taskType) == State.Active;

		public bool IsFinished(TaskType taskType)
		{
			if (taskType == null)
				return false;

			var state = GetState(taskType);
			return state == State.Completed
				|| state == State.Failed;
		}

		#endregion

		#region Helper Methods

		private static GameObject CreateTracker<T>(string title, ScriptGraphAsset script, string variableName, T value)
		{
			if (script == null)
				return null;

			GameObject tracker = new GameObject(string.Format("{0} Tracker", title));
			UnityEngine.Object.DontDestroyOnLoad(tracker);

#if UNITY_EDITOR
			tracker.transform.SetParent(container);
#endif

			var machine = tracker.AddComponent<ScriptMachine>();
			machine.enabled = false;
			machine.nest.macro = script;
			Variables.Object(tracker).Set(variableName, value);
			machine.enabled = true;

			return tracker;
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
			internal event EventHandler<QuestEventArgs> TaskStateChanged;
			internal event EventHandler<QuestEventArgs> TaskCountChanged;

			#endregion

			#region Properties

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
						tracker = CreateTracker(questType.title, questType.script, QUEST_VAR_NAME, this);
					}

					m_state = value;

					var args = new QuestEventArgs(this, value);
					StateChanged?.Invoke(this, args);

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
						Cleanup();

						var tasks = m_activeTasks.Values.ToArray();
						foreach (var task in tasks)
						{
							Cleanup(task);
						}

						if (state == State.Completed)
						{
							QuestType.Award(questType);
						}

						QuestType.ActiveNextQuests(questType, state);
					}
				}
			}
			internal GameObject tracker { get; private set; }

			public Task[] activeTasks => m_activeTasks.Values.ToArray();

			#endregion

			#region Constructors

			internal Quest(QuestType questType)
			{
				this.questType = questType;
			}

			#endregion

			#region Methods

			internal void Activate(TaskType taskType)
			{
				if (questType.completion != QuestType.Completion.Custom)
					return;

				var task = CreateTask(taskType);
				task.StateChanged += CustomTask_StateChanged;
			}

			private Task CreateTask(TaskType taskType)
			{
				var task = new Task(taskType, this);
				task.StateChanged += Task_StateChanged;
				task.CountChanged += Task_CountChanged;

				// Activate before listening to avoid early callback
				m_activeTasks.Add(taskType, task);
				task.Activate();

				return task;
			}

			internal void Cleanup()
			{
				if (!GameObjectExt.IsNull(tracker))
				{
					UnityEngine.Object.Destroy(tracker);
				}
			}

			private void Cleanup(Task task)
			{
				if (task == null)
					return;

				task.StateChanged -= Task_StateChanged;
				task.CountChanged -= Task_CountChanged;
				m_activeTasks.Remove(task.taskType);

				task.Cleanup();
			}

			private void Task_StateChanged(object sender, QuestEventArgs e)
			{
				TaskStateChanged?.Invoke(sender, e);
			}

			private void Task_CountChanged(object sender, QuestEventArgs e)
			{
				TaskCountChanged?.Invoke(sender, e);
			}

			internal bool TryGetTask(TaskType taskType, out Task task)
			{
				return m_activeTasks.TryGetValue(taskType, out task);
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

			private void CustomTask_StateChanged(object sender, QuestEventArgs e)
			{
				var task = sender as Task;
				if (task.state == State.Active)
					return;

				Cleanup(task);
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
			internal event EventHandler<QuestEventArgs> CountChanged;

			#endregion

			#region Properties

			public Quest quest { get; private set; }
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

					var args = new QuestEventArgs(this, delta);
					CountChanged?.Invoke(this, args);

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

					var args = new QuestEventArgs(this, value);
					StateChanged?.Invoke(this, args);
					CastInstance.TaskStateChanged?.Invoke(this, args);

					if (m_state != State.Active)
					{
						Cleanup();
					}
				}
			}

			internal GameObject tracker { get; private set; }

			#endregion

			#region Constructors

			internal Task(TaskType taskType, Quest quest)
			{
				this.taskType = taskType;
				this.quest = quest;
			}

			#endregion

			#region Methods

			internal void Activate()
			{
				tracker = CreateTracker(taskType.title, taskType.script, TASK_VAR_NAME, this);
				state = State.Active;
			}

			internal void Cleanup()
			{
				if (!GameObjectExt.IsNull(tracker))
				{
					UnityEngine.Object.Destroy(tracker);
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