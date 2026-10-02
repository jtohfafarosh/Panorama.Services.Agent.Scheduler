using System;
using System.Diagnostics;
using System.Xml;
using System.Data;
using System.Configuration;
namespace Dpci.PanoramaTaskScheduler
{
	/// <summary>
	/// Summary description for TaskExecutor.
	/// </summary>
	public class TaskExecutor
	{
		public TaskExecutor()
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public void ExecuteTasks()
		{
			DataSet dsScheduler = new DataSet();
			int Interval = 0;
			long TimeDiffInMins = 0; //64 bits
			long Divider = 1;
			long Remainder = 0;
			//Steps
			//load the config file from ConfigFilePath into dataset
			//looop through dataset
			//status = Active
			dsScheduler.ReadXml(ConfigurationSettings.AppSettings["ConfigFilePath"],System.Data.XmlReadMode.Auto);						
			foreach (DataRow dr in dsScheduler.Tables[0].Rows)
			{
				if(dr["Status"].ToString().Equals("Active"))
				{
					Interval = Convert.ToInt32(dr["Interval"].ToString());
					DateTime dt = DateTime.Now;
					TimeSpan tsDifference = dt.Subtract(Convert.ToDateTime(dr["StartOn"].ToString()));
					TimeDiffInMins = Convert.ToInt64(tsDifference.TotalMinutes);
					//(getdate-starton(in Minutes))
					switch(dr["Recurring"].ToString())
					{
						case "Minute":
							Divider = Interval*1;
							break;
						case "Hour":
							Divider = Interval*60;
							break;
						case "Day":
							Divider = Interval*24*60;
							break;
						case "Week":
							Divider = Interval*7*24*60;
							break;
						case "Month":
							Divider = Interval*30*24*60;
							break;
						case "Year":
							Divider = Interval*365*24*60;
							break;
					}
					Remainder = TimeDiffInMins % Divider;
					if(Remainder<5)
					{
						//start the process
						ProcessStartInfo StartInfo = new ProcessStartInfo(dr["Method"].ToString());
						StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
						Process.Start(StartInfo);
						StartInfo.Arguments = dr["Parameters"].ToString();
						StartInfo.Arguments = StartInfo.Arguments.Replace("-","");
						Process.Start(StartInfo);
					}
				}
				//Reset Vars
				Interval = 0;
				Divider = 1;
				Remainder = 0;
			}
					
		}

	}
}
