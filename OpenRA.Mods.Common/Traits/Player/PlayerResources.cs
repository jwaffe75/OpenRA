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
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[TraitLocation(SystemActors.Player | SystemActors.EditorPlayer)]
	public class PlayerResourcesInfo : PlayerResourcesBaseInfo
	{
		public override object Create(ActorInitializer init) { return new PlayerResources(init.Self, this); }
	}

	public class PlayerResources : PlayerResourcesBase
	{
		public readonly PlayerResourcesInfo Info;

		public PlayerResources(Actor self, PlayerResourcesInfo info)
		: base(self, info)
		{
			Info = info;
		}
	}
}
