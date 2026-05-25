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
using System.Linq;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	[Desc("A buildable foundation")]
	public class FoundationInfo : TurretedInfo
	{

		[Desc("Damage types that trigger prone state. Defined on the warheads.",
			"If Duration is negative (permanent), you can leave this empty to trigger prone state immediately.")]
		public readonly BitSet<DamageType> DamageTriggers = default;

		public override object Create(ActorInitializer init) { return new Foundation(init, this); }

	}

	public class Foundation : Turreted, INotifyDamage, ISync
	{
		public int BuildProgress { get; protected set; }
		readonly FoundationInfo info;

		public Foundation(ActorInitializer init, FoundationInfo info) : base(init, info)
		{
			this.info = info;
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

		/*
				protected override void Tick(Actor self)
				{
					base.Tick(self);

					if (IsTraitDisabled || info.Duration < 0)
						return;

					if (!IsTraitPaused && remainingDuration > 0)
						remainingDuration--;

					if (isProne && remainingDuration == 0)
						SetProneState(false);
				}
		*/
		public override bool HasAchievedDesiredFacing => true;

		/*
		int IDamageModifier.GetDamageModifier(Actor attacker, Damage damage)
		{

			
			if (damage == null || damage.DamageTypes.IsEmpty)
				return 100;

			var modifierPercentages = info.DamageModifiers.Where(x => damage.DamageTypes.Contains(x.Key)).Select(x => x.Value);
			return Util.ApplyPercentageModifiers(100, modifierPercentages);
			
		}
		*/

		protected override void TraitDisabled(Actor self)
		{
		}

		protected override void TraitEnabled(Actor self)
		{
		}
	}
}
