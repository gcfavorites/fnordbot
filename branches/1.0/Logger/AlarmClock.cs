using System;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;

namespace NielsRask.Logger
{
	public class AlarmClock
	{
		AlarmEngine engine;
		Thread alarmClockThread;

		public AlarmClock() 
		{
			engine = new AlarmEngine(250);
			alarmClockThread = new Thread( new ThreadStart( engine.Start ) );
		}

		public void Start() 
		{
			alarmClockThread.Start();
		}

		public void AddAlarm(IAlarmLauncher launcher) 
		{
			engine.AddAlarm( launcher );
		}

		public void Stop() 
		{
			engine.Running = false;
		}
	}

	public class AlarmEngine 
	{
		bool running = true;

		AlarmCollection alarms;

		public void AddAlarm(IAlarmLauncher launcher ) 
		{
			lock(alarms) 
			{
				alarms.Add( launcher );
			}
		}
		int granularity;
			
		public AlarmEngine(int granularity) 
		{
			this.granularity = granularity;
			alarms = new AlarmCollection();
		}

		public bool Running 
		{
			get { return running; }
			set { running = value; }
		}

		public void Start() 
		{
			while (running) 
			{
				CheckAlarms();
				Thread.Sleep(granularity);
			}
			Console.WriteLine("Engine has stopped");
		}

		private void CheckAlarms()
		{

			lock (alarms) 
			{
				// note that if first alarmtime is larger than now, no other alarms need to be checked, as the list is sorted by date
				if (alarms.Count == 0)
					return;

				if (alarms[0].Time <= DateTime.Now)	// at least one alarm should be triggered
				{
					AlarmCollection killlist = new AlarmCollection();
					for (int i=0; i<alarms.Count; i++) 
					{
						if (alarms[i].Time < DateTime.Now ) 
						{
							ExecuteAlarm( alarms[i] );
							killlist.Add( alarms[i] );
						}
					}
					for (int i=0; i<killlist.Count; i++)
						alarms.Remove(killlist[i]);
				}

			}
		}

		private void ExecuteAlarm(IAlarmLauncher alarm) 
		{
			alarm.Reschedule(this, alarm.Time);
			alarm.Execute();
		}
	}

	public class AlarmCollection : CollectionBase 
	{

		private void BubbleSort()
		{
			int i, j;
			IAlarmLauncher temp;
			for (i = Count - 1; i > 0; i--)
			{
				for (j = 0; j < i; j++)
				{
					if (this[j].Time.CompareTo(this[j + 1].Time) > 0)
					{
						temp = this[j];
						this[j] = this[j + 1];
						this[j + 1] = temp;
					}
				}
			}
		}

		public void Add(IAlarmLauncher alarm) 
		{
			lock(this) 
			{
				List.Add(alarm);
				BubbleSort();
			}
		}

		public void Remove(IAlarmLauncher alarm) 
		{
			List.Remove(alarm);
		}

		public IAlarmLauncher this[int i] 
		{
			get { return (IAlarmLauncher)List[i]; }
			set { List[i] = value; }
		}
	}

	public interface IAlarmLauncher 
	{
		DateTime Time
		{
			get;
		}

		void Reschedule(AlarmEngine engine, DateTime currentAlarmTime);

		void Execute();
	}
}
