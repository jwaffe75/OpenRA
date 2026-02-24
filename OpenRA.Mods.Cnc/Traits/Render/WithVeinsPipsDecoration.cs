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

using System;
using System.Collections.Generic;
using Linguini.Bundle.Errors;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Cnc.Traits.Render
{
	public class WithVeinsPipsDecorationInfo : WithResourceStoragePipsDecorationInfoBase
	{
		public override object Create(ActorInitializer init)
		{
			return new WithVeinsPipsDecoration(init.Self, this);
		}
	}

	public class WithVeinsPipsDecoration : WithResourceStoragePipsDecorationBase<WithResourceStoragePipsDecorationInfoBase>, INotifyOwnerChanged
	{
		protected TSPlayerResources player;

		public WithVeinsPipsDecoration(Actor self, WithVeinsPipsDecorationInfo info)
		: base(self, info)
		{
			player = self.Owner.PlayerActor.Trait<TSPlayerResources>();
		}

		public override int Capacity
		{
			get => player.Info.TriggerChemicalMissileOnVeinsAmount;
		}

		public override int Amount
		{
			get => player.Veins;
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			player = newOwner.PlayerActor.Trait<TSPlayerResources>();
		}
	}
}
