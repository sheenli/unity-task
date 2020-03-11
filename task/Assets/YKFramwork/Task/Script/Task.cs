using System;
using System.Collections;
using UnityEngine;
using YKFramework.Services;

namespace YKFramework.Task
{

    public abstract class Task
    {
        public virtual float Progress { get; protected set; }
        private bool latch;
        protected string mErr = string.Empty;
        protected object mOwnerSystem;
        protected float mStartedTime;

        public float ElapsedTime => Time.time - mStartedTime;
        public Status status { get; private set; }
        public virtual string TaskName { get; }
        public virtual string ErrInfo { get; }

        public bool isRunning => status == Status.Running;

        internal Action<bool> onFinished;

        public void Execute(object owner,Action<bool> callback = null)
        {
            this.onFinished = callback;
            Progress = 0;
            if (!isRunning) MonoManager.current.StartCoroutine(Updater(owner,callback));
        }

        private IEnumerator Updater(object owner,Action<bool> callback)
        {
            while (Tick(owner) == Status.Running) yield return null;
           
            if (callback != null)
            {
                callback(status == Status.Success);
            }
        }
        
        
        internal Status Tick(object owner)
        {
            if (status == Status.Running)
            {
                OnUpdate();
                latch = false;
                return status;
            }
            //如果任务结束了跳过这一帧//
            if ( latch ) { 
                latch = false;
                return status;
            }
            mStartedTime = Time.time;
            status = Status.Running;
            mOwnerSystem = owner;
            OnExecute();
            if (status == Status.Running)
                OnUpdate();
            return status;
        }

        public void EndAction(bool success = true)
        {
            if (status != Status.Running)
            {
                OnForcedStop();
                return;
            }

            latch = true;
            status = success ? Status.Success : Status.Failure;
            Progress = status == Status.Success ? 1 : 0;
            OnStop();
        }

        public void Reset()
        {
            status = Status.None;
            OnReset();
        }


        protected virtual void OnExecute()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void OnReset()
        {
            
        }
        protected virtual void OnForcedStop()
        {
        }
    }
}