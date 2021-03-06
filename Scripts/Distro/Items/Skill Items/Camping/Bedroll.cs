using System;
using Server;
using Server.Network;
using Server.Mobiles;
using Server.Gumps;

namespace Server.Items
{
	public class Bedroll : Item
	{
		[Constructable]
		public Bedroll()
			: base( 0xA57 )
		{
			Weight = 5.0;
		}

		public Bedroll( Serial serial )
			: base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( this.Parent != null || !this.VerifyMove( from ) )
				return;

			if ( !from.InRange( this, 2 ) )
			{
				from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019045 ); // I can't reach that.
				return;
			}

			if ( this.ItemID == 0xA57 ) // rolled
			{
				Direction dir = PlayerMobile.GetDirection4( from.Location, this.Location );

				if ( dir == Direction.North || dir == Direction.South )
					this.ItemID = 0xA55;
				else
					this.ItemID = 0xA56;
			}
			else // unrolled
			{
				this.ItemID = 0xA57;

				if ( !from.HasGump<LogoutGump>() )
				{
					CampfireEntry entry = Campfire.GetEntry( from );

					if ( entry != null && entry.Safe )
						from.SendGump( new LogoutGump( entry, this ) );
				}
			}
		}

		private class LogoutGump : Gump
		{
			public override int TypeID { get { return 0x8AE; } }

			private Timer m_CloseTimer;

			private CampfireEntry m_Entry;
			private Bedroll m_Bedroll;

			public LogoutGump( CampfireEntry entry, Bedroll bedroll )
				: base( 100, 0 )
			{
				m_Entry = entry;
				m_Bedroll = bedroll;

				m_CloseTimer = Timer.DelayCall( TimeSpan.FromSeconds( 10.0 ), new TimerCallback( Close ) );

				AddPage( 0 );

				AddBackground( 0, 0, 400, 350, 0xA28 );

				AddHtmlLocalized( 100, 20, 200, 35, 1011015, false, false ); // <center>Logging out via camping</center>

				/* Using a bedroll in the safety of a camp will log you out of the game safely.
				 * If this is what you wish to do choose CONTINUE and you will be logged out.
				 * Otherwise, select the CANCEL button to avoid logging out at this time.
				 * The camp will remain secure for 10 seconds at which time this window will close
				 * and you not be logged out.
				 */
				AddHtmlLocalized( 50, 55, 300, 140, 1011016, true, true );

				AddHtmlLocalized( 80, 300, 110, 35, 1011011, false, false ); // CONTINUE
				AddHtmlLocalized( 235, 300, 110, 35, 1011012, false, false ); // CANCEL

				AddButton( 45, 298, 0xFA5, 0xFA7, 2, GumpButtonType.Reply, 0 );
				AddButton( 200, 298, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0 );
			}

			public override void OnResponse( NetState sender, RelayInfo info )
			{
				PlayerMobile pm = m_Entry.Player;

				m_CloseTimer.Stop();

				if ( Campfire.GetEntry( pm ) != m_Entry )
					return;

				if ( info.ButtonID == 2 && m_Entry.Safe && m_Bedroll.Parent == null && m_Bedroll.IsAccessibleTo( pm )
					&& m_Bedroll.VerifyMove( pm ) && m_Bedroll.Map == pm.Map && pm.InRange( m_Bedroll, 2 ) )
				{
					pm.PlaceInBackpack( m_Bedroll );

					pm.BedrollLogout = true;
					sender.Dispose();
				}

				Campfire.RemoveEntry( m_Entry );
			}

			private void Close()
			{
				Campfire.RemoveEntry( m_Entry );
				m_Entry.Player.CloseGump<LogoutGump>();
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadEncodedInt();
		}
	}
}
