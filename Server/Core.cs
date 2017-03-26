﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

using Server.Events;
using Server.Network;
using Server.Profiler;

namespace Server
{
	public static class Core
	{
		private static readonly ILog log = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

		private static bool m_Crashed;
		private static bool m_Closing;

		public static bool Closing { get { return m_Closing; } }

		private static TimerThread m_TimerThread;

		/* current time */
		private static DateTime m_Now = DateTime.UtcNow;
		public static DateTime Now
		{
			get
			{
				return m_Now;
			}
		}

		/* main loop profiler */
		private static MainProfile m_TotalProfile;
		private static MainProfile m_CurrentProfile;

		public static MainProfile TotalProfile
		{
			get { return m_TotalProfile; }
		}

		public static MainProfile CurrentProfile
		{
			get { return m_CurrentProfile; }
		}

		public static void ResetCurrentProfile()
		{
			m_CurrentProfile = new MainProfile( m_Now );
		}

		private static void ClockProfile( MainProfile.TimerId id )
		{
			DateTime prev = m_Now;
			m_Now = DateTime.UtcNow;

			TimeSpan diff = m_Now - prev;
			m_TotalProfile.Add( id, diff );
			m_CurrentProfile.Add( id, diff );
		}

		public static void Run()
		{
			EventSink.Instance = new EventSink();

			if ( !ScriptCompiler.Compile( Environment.Debug ) )
			{
				log.Fatal( "Fatal: Compilation failed. Press any key to exit." );
				Console.ReadLine();
				return;
			}

			ScriptCompiler.VerifyLibraries();

			// This instance is shared among timer scheduler and timer executor,
			// and accessed from both core & timer threads.
			Queue<Timer> timerQueue = new Queue<Timer>();

			// Timer scheduler must be set up before world load, since world load
			// could schedule timers on entity deserialization.
			var timerScheduler = TimerScheduler.Instance = new TimerScheduler( timerQueue );
			m_TimerThread = new TimerThread( timerScheduler );

			TimerExecutor timerExecutor = new TimerExecutor( timerQueue );

			PacketHandlers.Instance = new PacketHandlers();

			try
			{
				ScriptCompiler.Configure();

				TileData.Configure();
			}
			catch ( TargetInvocationException e )
			{
				log.Fatal( "Fatal: Configure exception: {0}", e.InnerException );
				return;
			}

			Environment.SaveConfig();

			Region.Load();
			World.Instance.Load();

			try
			{
				ScriptCompiler.Initialize();
			}
			catch ( TargetInvocationException e )
			{
				log.Fatal( "Initialize exception: {0}", e.InnerException );
				return;
			}

			m_TimerThread.Start();

			NetServer netServer = new NetServer( new Listener( Listener.Port ) );
			netServer.Initialize();

			GameServer.Instance = new GameServer( netServer, PacketHandlers.Instance );
			GameServer.Instance.Initialize();

			EventSink.InvokeServerStarted();

			PacketDispatcher.Initialize();

			m_Now = DateTime.UtcNow;
			m_TotalProfile = new MainProfile( m_Now );
			m_CurrentProfile = new MainProfile( m_Now );

			try
			{
				while ( !m_Closing )
				{
					m_Now = DateTime.UtcNow;

					Thread.Sleep( 1 );

					ClockProfile( MainProfile.TimerId.Idle );

					Mobile.ProcessDeltaQueue();

					ClockProfile( MainProfile.TimerId.MobileDelta );

					Item.ProcessDeltaQueue();

					ClockProfile( MainProfile.TimerId.ItemDelta );

					timerExecutor.Slice();

					ClockProfile( MainProfile.TimerId.Timers );

					netServer.Slice();

					ClockProfile( MainProfile.TimerId.Network );

					// Done with this iteration.
					m_TotalProfile.Next();
					m_CurrentProfile.Next();
				}
			}
			catch ( Exception e )
			{
				HandleCrashed( e );
			}

			m_TimerThread.Stop();
		}

		public static void HandleCrashed( Exception e )
		{
			log.Error( "Error: {0}", e );

			m_Crashed = true;

			bool close = false;

			try
			{
				CrashedEventArgs args = new CrashedEventArgs( e );

				EventSink.InvokeCrashed( args );

				close = args.Close;
			}
			catch
			{
			}

			if ( !close && !Environment.Service )
			{
				log.Error( "This exception is fatal, press return to exit" );
				Console.ReadLine();
			}

			m_Closing = true;
		}

		public static void Kill( bool restart = false )
		{
			HandleClosed();

			if ( restart )
				Process.Start( Environment.ExePath );

			Environment.Process.Kill();
		}

		public static void HandleClosed()
		{
			if ( m_Closing )
				return;

			m_Closing = true;

			log.Info( "Exiting..." );

			if ( !m_Crashed )
				EventSink.InvokeShutdown( new ShutdownEventArgs() );

			if ( m_TimerThread != null )
				m_TimerThread.Stop();

			log.Info( "done" );
		}
	}
}
