﻿using System;
using System.Collections;
using UnityEngine;
using YKFramework.Services;

namespace YKFramework.Task.Example
{
    public class TaskTest : MonoBehaviour
    {
        public class LogTask : Task
        {
            private string msg;

            public LogTask(string msg)
            {
                this.msg = msg;
            }

            protected override void OnExecute()
            {
                MonoManager.current.StartCoroutine(Test(1));
            }

            IEnumerator Test(float waittime)
            {
                yield return new WaitForSeconds(waittime);
                Debug.Log(msg);
                EndAction();
            }
        }

        private TaskList task;
        private void Awake()
        {
            TaskList sequenceTask = new TaskList(TaskList.ActionsExecutionMode.RunInSequence);
            var log1 = new LogTask("顺序测试1");
            log1.onFinished = a =>
            {
                Debug.Log("子任务完成");
            };
            sequenceTask.AddTask(log1);
            sequenceTask.AddTask(new LogTask("顺序测试2"));
            sequenceTask.AddTask(new LogTask("顺序测试3"));
            
            TaskList parallelTask = new TaskList(TaskList.ActionsExecutionMode.RunInParallel);
            parallelTask.AddTask(new LogTask("并行任务测试1"));
            parallelTask.AddTask(new LogTask("并行任务测试2"));
            parallelTask.AddTask(new LogTask("并行任务测试3"));
            
//            TaskList task = new TaskList(TaskList.ActionsExecutionMode.RunInSequence);//顺序执行
            task = new TaskList(TaskList.ActionsExecutionMode.RunInParallel);//并行执行
            task.AddTask(sequenceTask);
            task.AddTask(parallelTask);
            task.Execute(this, success =>
            {
                Debug.Log("所有任务执行完毕"+success);
            });
            
        }

        private void Update()
        {
            Debug.Log("当前进度："+task.Progress);
        }
    }
}