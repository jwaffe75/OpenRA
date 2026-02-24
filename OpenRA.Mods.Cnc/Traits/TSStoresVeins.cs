#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Diagnostics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Cnc.Traits
{
	public class TSStoresVeinsInfo : TraitInfo
	{
		public override object Create(ActorInitializer init) { return new TSStoresVeins(init.Self, this); }
	}

	public class TSStoresVeins : INotifyRemovedFromWorld
	{
		readonly TSStoresVeinsInfo info;
		TSPlayerResources player;

		public int Stored
		{
			get
			{
				return player.Veins;
			}
		}

		public TSStoresVeins(Actor self, TSStoresVeinsInfo info)
		{
			this.info = info;
			player = self.Owner.PlayerActor.Trait<TSPlayerResources>();
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			// It looks like this event happens when the building is either destroyed or sold
			// This building cannot be captured, so there is no need to handle the captured events.
			//
			// the reason why this building is marked as not capturable is because there's a
			// maximum of one building allowed, and I'm not sure how to make it so that engineers
			// won't enter it if there's already a waste silo under ownership of the player trying
			// to capture it.
			player.Veins = 0;
		}
	}
}
