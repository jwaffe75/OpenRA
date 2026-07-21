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

using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Linq;
using OpenRA.Activities;
using OpenRA.Mods.Common.Activities;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("A buildable foundation")]
	public class FoundationInfo : TraitInfo
	{

		[Desc("Damage types that trigger prone state. Defined on the warheads.",
			"If Duration is negative (permanent), you can leave this empty to trigger prone state immediately.")]
		public readonly BitSet<DamageType> DamageTriggers = default;

		[ActorReference]
		//[FieldLoader.Require]
		[Desc("Actor to transform into when the build is done.")]
		public readonly string IntoActor = "napowr";

		[Desc("Offset to spawn the transformed actor relative to the current cell.")]
		public readonly CVec Offset = CVec.Zero;

		[Desc("Sounds to play when transforming.")]
		public readonly ImmutableArray<string> TransformSounds = [];


		public override object Create(ActorInitializer init) { return new Foundation(init, this); }


	}

	public class Foundation : ITick, INotifyDamage, ISync
	{
		public int BuildProgress { get; protected set; }
		readonly FoundationInfo info;

		bool transformed = false;
		readonly string faction;
		readonly Actor self;

		public Foundation(ActorInitializer init, FoundationInfo info)
		{
			this.info = info;
			self = init.Self;
			faction = init.GetValue<FactionInit, string>(self.Owner.Faction.InternalName);
		}

		void INotifyDamage.Damaged(Actor self, AttackInfo e)
		{
			/*
			if (IsTraitPaused || IsTraitDisabled)
				return;

			
			if (e.Damage.Value <= 0 || !e.Damage.DamageTypes.Overlaps(info.DamageTriggers))
				return;
			*/

			BuildProgress += e.Damage.Value;
		}

		
		void ITick.Tick(Actor self)
		{
			if (transformed) {
				return;
			}

			bool done = (BuildProgress >= 100);
			if (done)
			{
				self.QueueActivity(false, GetTransformActivity());
				transformed = true;
			}
		}
		
		public Activity GetTransformActivity()
		{
			return new Transform(info.IntoActor)
			{
				Offset = info.Offset,
				Facing = new(384),
				Sounds = info.TransformSounds,
				Notification = null,
				TextNotification = null,
				Faction = faction
			};
		}

		/*
		int IDamageModifier.GetDamageModifier(Actor attacker, Damage damage)
		{

			
			if (damage == null || damage.DamageTypes.IsEmpty)
				return 100;

			var modifierPercentages = info.DamageModifiers.Where(x => damage.DamageTypes.Contains(x.Key)).Select(x => x.Value);
			return Util.ApplyPercentageModifiers(100, modifierPercentages);
			
		}
		*/

	}
}
