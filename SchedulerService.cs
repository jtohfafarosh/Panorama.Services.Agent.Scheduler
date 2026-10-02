/******************************************************************************
License:

The contents of this file are subject to the TrazerEdge LLC's ("TrazerEdge") 
Panorama Source License (the "License"). You may not copy or use this file, in 
either source code or executable form, except in compliance with the License.

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either express or implied.

You may obtain a copy of the License by requesting it in writing to TrazerEdge.
See the License for the specific language governing rights and limitations
under the License.

Copyrights:

The Source Code is developed by TrazerEdge LLC.
and is covered under Copyright (c) 2026 TrazerEdge LLC. All Rights Reserved.

Contact information for TrazerEdge LLC. is found at https://www.trazeredge.com
******************************************************************************/

using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using Panorama.Utilities;
using Quartz;
using Quartz.Impl;

namespace Panorama.Services.Scheduler
{
    public class SchedulerService : ServiceBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        private System.Timers.Timer tRepeatProcessing;
        private PanoramaLog pLog = new PanoramaLog(typeof(SchedulerService).Namespace);
        private IScheduler oScheduler;

        public SchedulerService()
        {
            // This call is required by the Windows.Forms Component Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitComponent call
        }

        // The main entry point for the process
        private static void Main()
        {
            System.IO.Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            ServiceBase[] ServicesToRun;

            // More than one user Service may run within the same process. To add
            // another service to this process, change the following line to
            // create a second service object. For example,
            //
            //   ServicesToRun = new System.ServiceProcess.ServiceBase[] {new SchedulerService(), new MySecondUserService()};
            //
            ServicesToRun = new ServiceBase[] { new SchedulerService() };

            Run(ServicesToRun);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new Container();
            ServiceName = "Panorama Scheduler Service";
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Set things in motion so your service can do its work.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            pLog.WriteInfo("Panorama Scheduler Service Started. " + PanoramaLog.RunningAssemblyVersion());

            StdSchedulerFactory oSchedulerFactory = new StdSchedulerFactory();
            oScheduler = oSchedulerFactory.GetScheduler();
            oScheduler.Start();

            tRepeatProcessing = new System.Timers.Timer();
            tRepeatProcessing.Interval = (double)(1 * 1000);
            tRepeatProcessing.Elapsed += new System.Timers.ElapsedEventHandler(tRepeatProcessing_Elapsed);
            tRepeatProcessing.Enabled = true;
            tRepeatProcessing.Start();
        }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
        {
            pLog.WriteInfo("Panorama Scheduler Service Stopped.");
            pLog.Close();
            tRepeatProcessing.Stop();
            tRepeatProcessing.Enabled = false;
        }

        private void tRepeatProcessing_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                int iProcessRepeatInterval = Convert.ToInt32(ConfigurationManager.AppSettings["iProcessRepeatInterval"]);
                tRepeatProcessing.Interval = (double)(iProcessRepeatInterval * 60 * 1000);
                tRepeatProcessing.Stop();
                pLog.WriteVerbose("Scheduler check started.");
                TaskExecutor TE = new TaskExecutor(pLog, oScheduler);
                TE.ExecuteTasks();
                TE.ExecuteManualTasks();
                pLog.WriteVerbose("Scheduler check finished.");
            }
            catch (Exception ex)
            {
                pLog.WriteError("Error processing Panorama Scheduler service.");
                pLog.WriteError(ex);
            }
            finally
            {
                if (tRepeatProcessing != null)
                    tRepeatProcessing.Start();
            }
        }
    }
}