# Panorama Agent Scheduler

Windows service (`Service.exe`, service name **Panorama Agent Scheduler**) that loads active tasks for this server from Panorama and runs them on a Quartz cron schedule.

## Purpose

Scheduled console tasks, including the Entermedia reports, are rows in the Panorama `TASK` table. This service refreshes that list and starts each task's executable when its cron is due.

## Usage

Installed as the Windows service **Panorama Agent Scheduler**. It is not run from a console for the daily jobs.

On job-23 the task log is under `D:\Panorama\Logs\Agents`.

| File | What it shows |
|------|----------------|
| `Panorama.Services.Scheduler_{yyyy-MM-dd}.log` | Service start (with assembly version, for example `Started. v26.9.29.1`) and stop, schedule changes, and each task launch (`Launching` with file and arguments, then `Launched` with process id, then next run for a cron job) |

Each timer tick also writes "Scheduler check started" and "Scheduler check finished". Those lines are `WriteVerbose` and appear only when `LoggingLevel` is `Verbose`.

## Flow

1. On start, create a Quartz scheduler and a timer.
2. Each tick, load active tasks whose `TaskServerName` is this machine.
3. Add, update, or remove Quartz jobs so they match those rows.
4. When a job fires, or a Waiting row is on the Panorama Task Queue for this server, start `TaskMethod` with `TaskParameters` (`~` becomes a space, `-` is removed).
5. `Process.Start` uses CreateProcess (`UseShellExecute` false) with the executable's folder as the working directory. The call runs on a background thread. The scheduler waits 15 seconds.
6. Record `TaskLastRunOn` and `TaskNextRunOn` only after the process has started. A manual queue row is set to Completed at that point. If `Process.Start` has not returned, the row stays Waiting.

## Configuration

| Key | Purpose |
|-----|---------|
| `ConnectionString` | Panorama database that holds `TASK` |
| `LogsFolder` | `D:\Panorama\Logs\Agents` |
| `LoggingLevel` | `Info` for normal operation. `Verbose` includes every scheduler check. |
| `iProcessRepeatInterval` | Minutes between scheduler checks (deployed value is typically 5) |
| `ConfigFilePath` | Legacy XML path. Live jobs come from the database. |

## Requirements

- .NET Framework 4.6.2
- The service account can start the task executables and write `LogsFolder`

## Notes

The service process must be restarted after deploying `Service.exe` and `Panorama.Utilities.dll` before the new check lines appear. A service that stays running past midnight rolls its log to the new day's file on the next write.

A normal launch writes `Launching {TaskName}` and, a moment later, `Launched {TaskName}. Process id {id}.` If `Process.Start` is still waiting after 15 seconds, the log gets `Launch of {TaskName} has not returned after 15 seconds. Process.Start is still waiting on {path}.` That error means the start is blocked. The scheduler thread continues so other jobs are not stuck behind it. CreateProcess does not show a Mark-of-the-Web prompt. A file downloaded from the Internet can still be unblocked with `Unblock-File` if another tool shells it.
