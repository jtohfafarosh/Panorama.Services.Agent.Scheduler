using System;
using System.IO;
using System.Configuration;

namespace Dpci.PanoramaLogger
{
	/// <summary>
	/// Summary description for PanoramaLog.
	/// </summary>
	public class PanoramaLog
	{
		FileInfo ErrorLog=null; 
		FileInfo InfoLog=null;
		FileInfo SkippedRecordLog=null;
		FileStream Elog=null;
		FileStream Ilog = null;
		FileStream SkippedLog = null;
		public PanoramaLog(string FilePrefix)
		{
			InitLogFile(FilePrefix);
			Elog = ErrorLog.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
			Ilog = InfoLog.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
			SkippedLog = SkippedRecordLog.Open(FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
		}
		~PanoramaLog()
		{
			try
			{
				
			}
			catch{}
		}
		public void InitLogFile(string FilePrefix)
		{
			ErrorLog = new FileInfo(ConfigurationSettings.AppSettings["LogsFolder"] + "\\" + FilePrefix + "_Info.log");
			InfoLog =  new FileInfo(ConfigurationSettings.AppSettings["LogsFolder"] + "\\" + FilePrefix + "_Info.log");
			
			if(File.Exists(ConfigurationSettings.AppSettings["ErrorFolder"] + "\\" + "Error_"+FilePrefix+".log"))
				File.Delete(ConfigurationSettings.AppSettings["ErrorFolder"] + "\\" + "Error_"+FilePrefix+".log");
			SkippedRecordLog = new FileInfo(ConfigurationSettings.AppSettings["ErrorFolder"] + "\\" + "Error_"+FilePrefix+".log");
		}
		public void Close()
		{
			this.WriteError("Closing Streams");
			Elog.Flush();
			Elog.Close();
			Ilog.Flush();
			Ilog.Close();
			SkippedLog.Flush();
			SkippedLog.Close();
		}
		
		public void WriteError(string Error)
		{
			try
			{
				string systemtime = "[" + System.DateTime.Now.ToString() + "] ";
				Elog.Write(System.Text.Encoding.UTF8.GetBytes(systemtime), 0, systemtime.Length);
				Elog.Write(System.Text.Encoding.UTF8.GetBytes(Error.ToString()), 0, Error.Length);
				Elog.Write(System.Text.Encoding.UTF8.GetBytes("\n"), 0, "\n".Length);
				Elog.Flush();
			}
			catch{}
		  
         
		}

		public void WriteInfo(string Info)
		{
			try
			{
				string systemtime = "[" + System.DateTime.Now.ToString() + "] ";
				Ilog.Write(System.Text.Encoding.UTF8.GetBytes(systemtime), 0, systemtime.Length);
				Ilog.Write(System.Text.Encoding.UTF8.GetBytes(Info.ToString()), 0, Info.Length);
				Ilog.Write(System.Text.Encoding.UTF8.GetBytes("\n"), 0, "\n".Length);
				Ilog.Flush();
			}
			catch{}
		}

		public void WriteErrorRecord(string Record)
		{
			try
			{
				SkippedLog.Write(System.Text.Encoding.UTF8.GetBytes(Record.ToString()), 0, Record.Length);
				SkippedLog.Write(System.Text.Encoding.UTF8.GetBytes("\n"), 0, "\n".Length);
				SkippedLog.Flush();
			}
			catch{}
		}

	}
}
