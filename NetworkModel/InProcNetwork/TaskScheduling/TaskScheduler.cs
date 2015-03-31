﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkModel.InProcNetwork.TaskScheduling
{
    public class TaskScheduler :IDisposable
    {
        Thread taskSchedulingThread = null;
        public TaskScheduler()
        {
            StartTaskSchedulerThread();
        }

        void StartTaskSchedulerThread()
        {
            if (Running == false)
            {
                Running = true;
                taskSchedulingThread = new Thread(TaskSchedlulerLoop);
                taskSchedulingThread.IsBackground = true;
                taskSchedulingThread.Start();
            }
        }
        List<TaskSchedlulerTask> tasks = new List<TaskSchedlulerTask>();

        public void SchedluleTask(Action task, TimeSpan timeFromNow)
        {
            //Debug.WriteLine("scheduleTask");
            TaskSchedlulerTask newTask = new TaskSchedlulerTask()
            {
                SchedluledTime = DateTime.Now,
                ExecuteAfter = timeFromNow,
                Function = task
            };
            lock(tasks)
                tasks.Add(newTask);

            
            ev.Set();
        }
        object _sync = new object();

        volatile bool Running = false;
        AutoResetEvent ev = new AutoResetEvent(false);
        void TaskSchedlulerLoop(object o)
        {
            
            while (Running)
            {
                int waitTime = -1;
                
                List<TaskSchedlulerTask> tasksOrderedByTime = null;
                
                // order tasks by run time
                lock (tasks)
                    tasksOrderedByTime = tasks.OrderBy(t => t.RunIn).ToList();

                // pick task closest in time
                TaskSchedlulerTask closestTask = null;
                if (tasksOrderedByTime != null && tasksOrderedByTime.Count > 0)
                    closestTask = tasksOrderedByTime[0];


                if (closestTask != null)
                    waitTime = (int)closestTask.RunIn;

                if (waitTime == -1)
                    ev.WaitOne();
                else
                    ev.WaitOne(waitTime);

                if (!Running)
                    return;
                

                List<TaskSchedlulerTask> tasksToRunNow = null;
                lock (tasks)
                {
                    tasksToRunNow = tasks.Where(t => t.RunIn <= 0).ToList();
                    foreach (var taskToRemove in tasksToRunNow)
                        tasks.Remove(taskToRemove);
                }
                
                foreach (TaskSchedlulerTask t in tasksToRunNow)
                {
                    RunTask(t);
                }
                
                
            }
        }

        bool RunTask(TaskSchedlulerTask task)
        {
            try
            {
                TimeSpan tsFromStart = DateTime.Now - task.SchedluledTime;
                task.Function();
                return true;
            }
            catch(Exception ex)
            {
                Trace.TraceWarning("Failed to run task");
                return false;
            }
        }

        public void Dispose()
        {
            Running = false;
            taskSchedulingThread.Join();
        }
    }
}
