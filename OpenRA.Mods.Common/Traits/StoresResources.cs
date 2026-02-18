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
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("Allows the storage of resources.")]
	public class StoresResourcesInfo : TraitInfo, IStoresResourcesInfo
	{
		[FieldLoader.Require]
		[Desc("The amounts of resources that can be stored.")]
		public readonly int Capacity = 28;

		[Desc("Which resources can be stored.")]
		public readonly ImmutableArray<string> Resources = [];

		ImmutableArray<string> IStoresResourcesInfo.ResourceTypes => Resources;

		public override object Create(ActorInitializer init) { return new StoresResources(init.Self, this); }
	}

	public class StoresResources : IStoresResources, ISync, INotifyKilled, INotifyAddedToWorld, INotifyRemovedFromWorld, INotifyCapture, INotifyOwnerChanged
	{
		readonly Dictionary<string, int> contents = [];
		readonly StoresResourcesInfo info;

		[VerifySync]
		public int ContentHash
		{
			get
			{
				var value = 0;
				foreach (var c in contents)
					value += c.Value << c.Key.Length;

				return value;
			}
		}

		public int ContentsSum { get; private set; } = 0;
		public IReadOnlyDictionary<string, int> Contents { get; }
		int IStoresResources.Capacity => info.Capacity;

		public StoresResources(Actor self, StoresResourcesInfo info)
		{
			this.info = info;

			foreach (var r in info.Resources)
				contents[r] = 0;

			Contents = new ReadOnlyDictionary<string, int>(contents);
		}

		public bool HasType(string resourceType)
		{
			return info.Resources.Contains(resourceType);
		}

		int IStoresResources.AddResource(string resourceType, int value)
		{
			if (!HasType(resourceType))
				return value;

			if (ContentsSum + value > info.Capacity)
			{
				var added = info.Capacity - ContentsSum;
				contents[resourceType] += added;
				ContentsSum = info.Capacity;
				return value - added;
			}

			contents[resourceType] += value;
			ContentsSum += value;
			return 0;
		}

		int IStoresResources.RemoveResource(string resourceType, int value)
		{
			if (!HasType(resourceType))
				return value;

			if (contents[resourceType] < value)
			{
				var leftover = value - contents[resourceType];
				ContentsSum -= contents[resourceType];
				contents[resourceType] = 0;
				return leftover;
			}

			contents[resourceType] -= value;
			ContentsSum -= value;
			return 0;
		}

		static void PrintBeforeAfter(int beforeAmount, int beforeCapacity, int afterAmount, int afterCapacity)
		{
			Debug.WriteLine($"before amount={beforeAmount},before capacity={beforeCapacity}, after amount={afterAmount}, after capacity={afterCapacity}");
		}

		void OnRemoval(Actor self, Player oldOwner, Player newOwner)
		{

			Debug.WriteLine($"OnRemoval: {self.Info.Name}, oldOwner={oldOwner.PlayerName}");
			var oldOwnerSpecialResources = oldOwner.PlayerActor.Trait<PlayerResources>();
			foreach (var specialResourceType in oldOwnerSpecialResources.SpecialResourcesTypes)
			{
				Debug.WriteLine($"Resource type: {specialResourceType}");
				if (!HasType(specialResourceType))
				{
					Debug.WriteLine("Is not contained by this building.");
					continue;
				}

				var oldOwnerBeforeAmount = oldOwnerSpecialResources.HasSpecialResources(specialResourceType);
				var oldOwnerBeforeCapacity = oldOwnerSpecialResources.SpecialResourcesCapacity[specialResourceType];

				var amountTaken = oldOwnerSpecialResources.TakeSpecialResource(specialResourceType, info.Capacity);
				oldOwnerSpecialResources.RemoveSpecialStorage(info.Capacity, specialResourceType);

				var oldOwnerAfterAmount = oldOwnerSpecialResources.HasSpecialResources(specialResourceType);
				var oldOwnerAfterCapacity = oldOwnerSpecialResources.SpecialResourcesCapacity[specialResourceType];

				Debug.WriteLine("Old owner:");
				PrintBeforeAfter(oldOwnerBeforeAmount, oldOwnerBeforeCapacity, oldOwnerAfterAmount, oldOwnerAfterCapacity);

				if (newOwner != null)
				{
					var newOwnerSpecialResources = newOwner.PlayerActor.Trait<PlayerResources>();
					var newOwnerBeforeAmount = newOwnerSpecialResources.HasSpecialResources(specialResourceType);
					var newOwnerBeforeCapacity = newOwnerSpecialResources.SpecialResourcesCapacity[specialResourceType];
					newOwnerSpecialResources.GiveSpecialResources(amountTaken, specialResourceType);
					newOwnerSpecialResources.AddSpecialStorage(info.Capacity, specialResourceType);
					var newOwnerAfterAmount = newOwnerSpecialResources.HasSpecialResources(specialResourceType);
					var newOwnerAfterCapacity = newOwnerSpecialResources.SpecialResourcesCapacity[specialResourceType];

					Debug.WriteLine($"New Owner: {newOwner.PlayerName}");
					PrintBeforeAfter(newOwnerBeforeAmount, newOwnerBeforeCapacity, newOwnerAfterAmount, newOwnerAfterCapacity);
				}
			}
		}

		void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			OnRemoval(self, oldOwner, newOwner);
		}

		void INotifyCapture.OnCapture(Actor self, Actor captor, Player oldOwner, Player newOwner, BitSet<CaptureType> captureTypes)
		{
			OnRemoval(self, oldOwner, newOwner);
		}

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			OnRemoval(self, self.Owner, null);
		}

		void INotifyAddedToWorld.AddedToWorld(Actor self)
		{
			Debug.WriteLine($"OnAddedToWorld: {self.Info.Name}");
			// TODO: Does this run if the building is already in the map when it starts?
			// HACK: It seems like there's only one Capacity field, for all the
			// resources this might contain?
			var playerResources = self.Owner.PlayerActor.Trait<PlayerResources>();

			foreach (var specialResourceType in playerResources.SpecialResourcesTypes)
			{
				Debug.WriteLine($"Resource {specialResourceType}");
				if (!HasType(specialResourceType))
				{
					Debug.WriteLine("Is not contained by this building");
					continue;
				}

				var oldStorage = playerResources.SpecialResourcesCapacity[specialResourceType];
				playerResources.AddSpecialStorage(info.Capacity, specialResourceType);
				var newStorage = playerResources.SpecialResourcesCapacity[specialResourceType];
				Debug.WriteLine($"Before storage: {oldStorage}, After storage: {newStorage}");
			}
		}

		void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
		{
			OnRemoval(self, self.Owner, null);
		}
	}
}
