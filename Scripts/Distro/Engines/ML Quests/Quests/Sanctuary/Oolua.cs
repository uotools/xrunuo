using System;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Engines.Quests
{
	public class PixieDustToDustQuest : BaseQuest
	{
		/* Pixie dust to dust */
		public override object Title { get { return 1073661; } }

		/* Is there anything more foul than a pixie? They have cruel eyes and a mind for mischief, I say. I don't 
		care if some think they're cute -- I say kill them and let the Avatar sort them out. In fact, if you were 
		to kill a few pixies, I'd make sure you had a few coins to rub together, if you get my meaning. */
		public override object Description { get { return 1073700; } }

		/* Perhaps you'll change your mind and return at some point. */
		public override object Refuse { get { return 1073733; } }

		/* There's too much cuteness in the world -- kill those pixies! */
		public override object Uncomplete { get { return 1073741; } }

		public PixieDustToDustQuest()
			: base()
		{
			AddObjective( new SlayObjective( typeof( Pixie ), "pixies", 10 ) );

			AddReward( new BaseReward( typeof( LargeTreasureBag ), 1072706 ) );
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

	public class Oolua : MondainQuester
	{
		public override Type[] Quests
		{
			get
			{
				return new Type[] 
			{ 
				typeof( ParoxysmusSuccubiQuest ),
				typeof( ParoxysmusMolochQuest ),
				typeof( ParoxysmusDaemonsQuest ),
				typeof( ParoxysmusArcaneDaemonsQuest ),
				typeof( CausticComboQuest ),
				typeof( PlagueLordQuest ),
				typeof( OrcSlayingQuest ),
				typeof( DreadhornQuest ),
				typeof( PixieDustToDustQuest )
			};
			}
		}

		[Constructable]
		public Oolua()
			: base( "Lorekeeper Oolua", "the keeper of tradition" )
		{
		}

		public Oolua( Serial serial )
			: base( serial )
		{
		}

		public override void InitBody()
		{
			InitStats( 100, 100, 25 );

			Female = true;
			CantWalk = true;
			Race = Race.Elf;

			Hue = 0x853F;
			HairItemID = 0x2FCC;
			HairHue = 0x388;
		}

		public override void InitOutfit()
		{
			AddItem( new ElvenBoots( 0x70E ) );
			AddItem( new WildStaff() );
			AddItem( new GemmedCirclet() );
			AddItem( new Cloak( 0x1BB ) );
			AddItem( new Skirt( 0x3 ) );
			AddItem( new FancyShirt( 0x70A ) );
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