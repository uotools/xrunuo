using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class WondersOfTheNaturalWorldQuest : BaseQuest
	{
		/* Wonders of the Natural World */
		public override object Title { get { return 1074444; } }

		/* Step right up!  Step right up!  Lords and Ladies, this is your opportunity to view the find of a lifetime!  
		What magical energy caused the fascinating play of light and darkness within these subterranean passageways?  
		What mysterious forces are at work deep within the Prism of Light?  Admission tickets are good for a full day 
		of adventure and excitement and well worth the price at 10,000 gold. Whadda ya say? */
		public override object Description { get { return 1074445; } }

		/* C'mon now Lords and Ladies -- you're passing up the opportunity of a lifetime.  Is 10,000 gold too much 
		to pay for your enlightenment? */
		public override object Refuse { get { return 1074446; } }

		/* Dig into those pockets Lords and Ladies!  Just ten-thousand-shiny-gold-coins and you'll be walking in the 
		bootsteps of the famous Lord Denthe himself! */
		public override object Uncomplete { get { return 1074447; } }

		/* Step right up!  Thank you, enjoy the amazing caverns. */
		public override object Complete { get { return 1074448; } }

		public WondersOfTheNaturalWorldQuest()
			: base()
		{
			AddObjective( new ObtainObjective( typeof( Gold ), "gold coins", 10000, 0xEED ) );

			AddReward( new BaseReward( typeof( PrismOfLightAdmissionTicket ), 1074340 ) ); // Prism of Light Admission Ticket
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}

	public class Lefty : MondainQuester
	{
		public override Type[] Quests
		{
			get
			{
				return new Type[] 
			{ 
				typeof( WondersOfTheNaturalWorldQuest )
			};
			}
		}

		[Constructable]
		public Lefty()
			: base( "Lefty", "the ticket seller" )
		{
		}

		public Lefty( Serial serial )
			: base( serial )
		{
		}

		public override void InitBody()
		{
			InitStats( 100, 100, 25 );

			Female = false;
			CantWalk = true;
			Race = Race.Human;

			Hue = 0x83F4;
			HairItemID = 0x203B;
			HairHue = 0x470;
		}

		public override void InitOutfit()
		{
			AddItem( new ThighBoots( 0x901 ) );
			AddItem( new LongPants( 0x70D ) );
			AddItem( new Tunic( 0x30 ) );
			AddItem( new Cloak( 0x30 ) );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}