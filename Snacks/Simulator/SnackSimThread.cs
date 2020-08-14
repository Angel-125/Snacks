/**
The MIT License (MIT)
Copyright (c) 2014-2019 by Michael Billard
 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 * */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Snacks
{
    /// <summary>
    /// This class represents a single simulator job. It will check its job list for jobs to process and synchronize with other simulator jobs.
    /// </summary>
    public class SnackSimThread
    {
        #region Housekeeping
        public bool canRun = true;
        public Mutex mutex;
        public List<SimSnacks> jobList;
        public SimSnacks simulator;

        public OnSimulationCompleteDelegate OnSimulationComplete;
        public OnSimulatorCycleCompleteDelegate OnSimulatorCycleComplete;
        public OnSimulatorExceptionDelegate OnSimulatorException;

        private Thread jobThread;
        #endregion

        #region Constructors
        public SnackSimThread(Mutex threadMutex, List<SimSnacks> simulatorList)
        {
            mutex = threadMutex;
            jobList = simulatorList;
            canRun = true;
        }
        #endregion

        #region API
        /// <summary>
        /// Starts the thread.
        /// </summary>
        public void Start()
        {
            jobThread = new Thread(new ThreadStart(ThreadRoutine));
            jobThread.Start();
        }

        /// <summary>
        /// Stops all current and pending jobs and kills the thread.
        /// </summary>
        public void Stop()
        {
            mutex.WaitOne();
            canRun = false;
            jobList.Clear();
            if (simulator != null)
                simulator.exitSimulation = true;
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Adds a simulator job to the job list.
        /// </summary>
        /// <param name="simSnacks">The simulator to add to the jobs list.</param>
        public void AddJob(SimSnacks simSnacks)
        {
            mutex.WaitOne();
            jobList.Add(simSnacks);
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Determines whether or not the simulator thread has jobs.
        /// </summary>
        /// <returns>true if the simulator has job, false if not.</returns>
        public bool HasJobs()
        {
            bool hasJobs = false;
            mutex.WaitOne();
            hasJobs = jobList.Count > 0;
            mutex.ReleaseMutex();
            return hasJobs;
        }

        /// <summary>
        /// Clears all pending and running jobs.
        /// </summary>
        public void ClearJobs()
        {
            mutex.WaitOne();
            jobList.Clear();
            if (simulator != null)
                simulator.exitSimulation = true;
            mutex.ReleaseMutex();
        }
        #endregion

        #region Helpers
        private void ThreadRoutine()
        {
            while (canRun)
            {
                //Make sure that we can run.
                mutex.WaitOne();
                if (!canRun)
                {
                    mutex.ReleaseMutex();
                    Thread.Sleep(100);
                    return;
                }

                //Grab a simulator if one is available.
                if (jobList.Count > 0)
                {
                    simulator = jobList[0];
                    jobList.RemoveAt(0);
                }
                else
                {
                    simulator = null;
                }

                //Done with mutex for now
                mutex.ReleaseMutex();

                //Run the simulator if we have one
                if (simulator != null)
                {
                    //Run a cycle until we are told to exit.
                    while (!simulator.exitSimulation && canRun)
                    {
                        simulator.RunSimulatorCycle();

                        //Check for errors
                        if (simulator.exception != null)
                        {
                            mutex.WaitOne();
                            OnSimulatorException?.Invoke(simulator, simulator.exception);
                            mutex.ReleaseMutex();
                            break;
                        }

                        //Inform delegate that cycle is complete.
                        mutex.WaitOne();
                        OnSimulatorCycleComplete?.Invoke(simulator);

                        //Make sure that we can run.
                        if (!canRun || simulator.exitSimulation)
                        {
                            mutex.ReleaseMutex();
                            OnSimulationComplete?.Invoke(simulator);
                            simulator = null;
                            break;
                        }
                        mutex.ReleaseMutex();
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }

            }
        }
        #endregion
    }
}
