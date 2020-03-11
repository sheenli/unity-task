using System.Collections.Generic;
using UnityEngine;

namespace YKFramework.Task
{
    public class TaskList : Task
    {
        public override float Progress
        {
            get
            {
                var cur = 0f;
                for (int i = 0; i < actions.Count; i++)
                {
                    cur += actions[i].Progress;
                }
                return cur / (float)actions.Count;
            }
            protected set
            {
                
            }
        }
        public TaskList(ActionsExecutionMode executionMode)
        {
            this.executionMode = executionMode;
        }
        public enum ActionsExecutionMode
        {
            RunInSequence,
            RunInParallel
        }

        private readonly List<int> finishedIndeces = new List<int>();

        public List<Task> actions = new List<Task>();
        public ActionsExecutionMode executionMode = ActionsExecutionMode.RunInSequence;

        private int mCurIndex;

        protected override void OnExecute()
        {
            mCurIndex = 0;
            finishedIndeces.Clear();
            Progress = 0;
        }

        protected override void OnUpdate()
        {
            if (actions.Count == 0)
            {
                EndAction();
                return;
            }

            switch (executionMode)
            {
                case ActionsExecutionMode.RunInParallel:
                    CheckParallelTask();
                    break;
                case ActionsExecutionMode.RunInSequence:
                    CheckInSequenceTask();
                    break;
            }
        }

        protected override void OnReset()
        {
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].Reset();
            }

            mCurIndex = 0;
            finishedIndeces.Clear();
        }

        private void CheckParallelTask()
        {
            for (var i = 0; i < actions.Count; i++)
            {
                if (finishedIndeces.Contains(i)) continue;
                var status = actions[i].Tick(mOwnerSystem);
                if (status == Status.Failure)
                {
                    mErr = actions[i].ErrInfo;
                    EndAction(false);
                    actions[i].onFinished?.Invoke(false);
                    return;
                }

                if (status == Status.Success)
                {
                    finishedIndeces.Add(i);
                    actions[i].onFinished?.Invoke(true);
                }
            }
            if (finishedIndeces.Count == actions.Count) EndAction();
        }

        private void CheckInSequenceTask()
        {
            for (var i = mCurIndex; i < actions.Count; i++)
            {
                
                var status = actions[i].Tick(mOwnerSystem);
               
                if (status == Status.Failure)
                {
                    EndAction(false);
                    actions[i].onFinished?.Invoke(false);
                    return;
                }

                if (status == Status.Running)
                {
                    mCurIndex = i;
                    return;
                }
                else
                {
                    actions[i].onFinished?.Invoke(true);
                }
            }
            EndAction();
        }

        public void AddTask(Task task)
        {
            this.actions.Add(task);
        }
    }
}