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
using System.Linq;
using OpenRA.Mods.Common.Effects;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class RefineryInfo : TraitInfo, Requires<WithSpriteBodyInfo>, Requires<IDockHostInfo>
	{
		[Desc("Store resources in silos. Adds cash directly without storing if set to false.")]
		public readonly bool UseStorage = true;

		[Desc("Discard resources once silo capacity has been reached.")]
		public readonly bool DiscardExcessResources = false;

		public readonly bool ShowTicks = true;
		public readonly int TickRate = 10;

		public override object Create(ActorInitializer init) { return new Refinery(init.Self, this); }
	}

	public class Refinery : IAcceptResources, INotifyCreated, ITick, INotifyOwnerChanged
	{
		readonly RefineryInfo info;
		PlayerResources playerResources;
		IEnumerable<int> resourceValueModifiers;

		int currentDisplayTick = 0;
		int currentDisplayValue = 0;
		public Refinery(Actor self, RefineryInfo info)
		{
			this.info = info;
			playerResources = self.Owner.PlayerActor.Trait<PlayerResources>();
			currentDisplayTick = info.TickRate;
		}

		void INotifyCreated.Created(Actor self)
		{
			resourceValueModifiers = self.TraitsImplementing<IResourceValueModifier>().ToArray().Select(m => m.GetResourceValueModifier());
		}

		int IAcceptResources.AcceptResources(Actor self, string resourceType, int count)
		{
			var canAcceptRawResources = playerResources.CanAcceptSpecialRawResources(resourceType);
			if (!playerResources.Info.ResourceValues.TryGetValue(resourceType, out var resourceValue) || !canAcceptRawResources)
				return 0;

			var cappedCount = count;

			// Don't allow resources given to go above the count
			if (canAcceptRawResources && info.UseStorage)
			{
				foreach (var rsv in playerResources.Info.SpecialResourceValues[resourceType])
				{
					var limitedCount = count;
					var capacity = playerResources.SpecialResourcesCapacity[rsv.Key];
					var had = playerResources.HasSpecialResources(rsv.Key);
					var limit = Math.Max(capacity - had, 0);
					var loadValue = limitedCount * rsv.Value;

					// if true, the building will let havesters drop resources even if it can't take them
					if (!info.DiscardExcessResources)
					{
						// Reduce amount if needed until it will fit the available storage
						while (loadValue > limit)
						{
							// HACK: If you have a limit of e.g. 56, but each load gives 100, this will not give you 56
							loadValue = --limitedCount * rsv.Value;
						}
					}

					cappedCount = Math.Min(count, limitedCount);
				}
			}

			var value = Util.ApplyPercentageModifiers(cappedCount * resourceValue, resourceValueModifiers);

			if (info.UseStorage)
			{
				var storageLimit = Math.Max(playerResources.ResourceCapacity - playerResources.Resources, 0);
				if (!info.DiscardExcessResources)
				{
					// Reduce amount if needed until it will fit the available storage
					while (value > storageLimit)
						value = Util.ApplyPercentageModifiers(--cappedCount * resourceValue, resourceValueModifiers);
				}
				else
					value = Math.Min(value, playerResources.ResourceCapacity - playerResources.Resources);

				playerResources.GiveResources(value);
			}
			else
				value = playerResources.ChangeCash(value);

			playerResources.GiveSpecialRawResources(cappedCount, resourceType);

			foreach (var notify in self.World.ActorsWithTrait<INotifyResourceAccepted>())
			{
				if (notify.Actor.Owner != self.Owner)
					continue;

				notify.Trait.OnResourceAccepted(notify.Actor, self, resourceType, cappedCount, value);
			}

			if (info.ShowTicks)
				currentDisplayValue += value;

			return cappedCount;
		}

		void ITick.Tick(Actor self)
		{
			if (info.ShowTicks && currentDisplayValue > 0 && --currentDisplayTick <= 0)
			{
				var temp = currentDisplayValue;
				if (self.Owner.IsAlliedWith(self.World.RenderPlayer))
					self.World.AddFrameEndTask(w => w.Add(new FloatingText(self.CenterPosition, self.OwnerColor(), FloatingText.FormatCashTick(temp), 30)));
				currentDisplayTick = info.TickRate;
				currentDisplayValue = 0;
			}
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			playerResources = newOwner.PlayerActor.Trait<PlayerResources>();
		}
	}
}
