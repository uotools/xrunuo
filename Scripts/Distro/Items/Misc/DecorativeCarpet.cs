using System;
using Server;
using Server.Engines.Housing.Multis;
using Server.Engines.Housing.Regions;
using Server.Multis;
using Server.Regions;

namespace Server.Items
{
	public class DecorativeCarpet : Item, IDyable
	{
		public override int Hue
		{
			get
			{
				if ( Movable && IsInsideHouse() )
					return 253;

				return base.Hue;
			}
		}

		[Constructable]
		public DecorativeCarpet( int itemId )
			: base( itemId )
		{
			Weight = 1.0;
		}

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );

			if ( Movable && IsInsideHouse() )
				list.Add( 1113267 ); // (Double Click to Lockdown)
		}

		public override bool DisplayWeight { get { return Movable; } }

		public override bool OnDroppedToWorld( Mobile from, Point3D p )
		{
			if ( !base.OnDroppedToWorld( from, p ) )
				return false;

			InvalidateProperties();
			return true;
		}

		public override void OnDoubleClick( Mobile from )
		{
			HouseRegion region = Region.Find( Location, Map ) as HouseRegion;

			if ( region != null )
			{
				BaseHouse house = region.House;

				if ( house.IsOwner( from ) || house.IsCoOwner( from ) )
				{
					Movable = !Movable;

					if ( Movable )
						house.Carpets.Remove( this );
					else
						house.Carpets.Add( this );

					InvalidateProperties();
				}
				else
				{
					// Only the home owner may lock this down.
					from.SendLocalizedMessage( 1113268 );
				}
			}
		}

		public bool IsInsideHouse()
		{
			return Region.Find( Location, Map ).IsPartOf<HouseRegion>();
		}

		public DecorativeCarpet( Serial serial )
			: base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			/*int version = */
			reader.ReadInt();
		}

		#region IDyable Members
		public bool Dye( Mobile from, IDyeTub sender )
		{
			if ( Deleted )
			{
				return false;
			}
			else if ( !IsChildOf( from.Backpack ) )
			{
				// That must be in your pack for you to use it.
				from.SendLocalizedMessage( 1042001 );
				return false;
			}
			else
			{
				Hue = sender.DyedHue;
				return true;
			}
		}
		#endregion
	}
}