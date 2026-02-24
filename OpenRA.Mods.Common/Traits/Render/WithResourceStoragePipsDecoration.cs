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

using System.Collections.Generic;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits.Render
{
	public class WithResourceStoragePipsDecorationInfo : WithResourceStoragePipsDecorationInfoBase
	{
		public override object Create(ActorInitializer init) { return new WithResourceStoragePipsDecoration(init.Self, this); }
	}

	public class WithResourceStoragePipsDecoration : WithResourceStoragePipsDecorationBase<WithResourceStoragePipsDecorationInfo>, INotifyOwnerChanged
	{
		protected PlayerResources player;
		public WithResourceStoragePipsDecoration(Actor self, WithResourceStoragePipsDecorationInfo info)
			: base(self, info)
		{
			player = self.Owner.PlayerActor.Trait<PlayerResources>();
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			player = newOwner.PlayerActor.Trait<PlayerResources>();
		}

		public override int Capacity
		{
			get => player.ResourceCapacity;
		}

		public override int Amount
		{
			get => player.Resources;
		}
	}
}
