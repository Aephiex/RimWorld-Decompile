using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class Building_Turret : Building, IAttackTarget, IAttackTargetSearcher, ILoadReferenceable
	{
		protected StunHandler stunner;

		protected LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;

		private LocalTargetInfo lastAttackedTarget;

		private int lastAttackTargetTick;

		private const float SightRadiusTurret = 13.4f;

		Thing IAttackTarget.Thing
		{
			get
			{
				return this;
			}
		}

		Thing IAttackTargetSearcher.Thing
		{
			get
			{
				return this;
			}
		}

		public abstract LocalTargetInfo CurrentTarget
		{
			get;
		}

		public abstract Verb AttackVerb
		{
			get;
		}

		public LocalTargetInfo TargetCurrentlyAimingAt
		{
			get
			{
				return this.CurrentTarget;
			}
		}

		public Verb CurrentEffectiveVerb
		{
			get
			{
				return this.AttackVerb;
			}
		}

		public LocalTargetInfo LastAttackedTarget
		{
			get
			{
				return this.lastAttackedTarget;
			}
		}

		public int LastAttackTargetTick
		{
			get
			{
				return this.lastAttackTargetTick;
			}
		}

		public Building_Turret()
		{
			this.stunner = new StunHandler(this);
		}

		public override void Tick()
		{
			base.Tick();
			this.stunner.StunHandlerTick();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_TargetInfo.Look(ref this.forcedTarget, "forcedTarget");
			Scribe_TargetInfo.Look(ref this.lastAttackedTarget, "lastAttackedTarget");
			Scribe_Deep.Look<StunHandler>(ref this.stunner, "stunner", new object[1]
			{
				this
			});
			Scribe_Values.Look<int>(ref this.lastAttackTargetTick, "lastAttackTargetTick", 0, false);
		}

		public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(dinfo, out absorbed);
			if (!absorbed)
			{
				this.stunner.Notify_DamageApplied(dinfo, true);
				absorbed = false;
			}
		}

		public abstract void OrderAttack(LocalTargetInfo targ);

		public bool ThreatDisabled()
		{
			CompPowerTrader comp = base.GetComp<CompPowerTrader>();
			if (comp != null && !comp.PowerOn)
			{
				return true;
			}
			CompMannable comp2 = base.GetComp<CompMannable>();
			if (comp2 != null && !comp2.MannedNow)
			{
				return true;
			}
			return false;
		}

		protected void OnAttackedTarget(LocalTargetInfo target)
		{
			this.lastAttackTargetTick = Find.TickManager.TicksGame;
			this.lastAttackedTarget = target;
		}
	}
}
