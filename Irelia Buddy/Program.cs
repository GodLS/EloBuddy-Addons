using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace Irelia_Buddy
{
    class Program
    {
        private static readonly AIHeroClient Player = ObjectManager.Player;
        private static int rcount;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.BaseSkinName != "Irelia") return;

            IreliaMenu.Initialize();
            Spells.Initialize();
            Bootstrap.Init(null);

            Game.OnTick += OnTick;
            Orbwalker.OnPreAttack += OnPreAttack;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += OnGapcloser;
            Interrupter.OnInterruptableSpell += OnInterruptableSpell;
            //Obj_AI_Base.OnAggro += OnAggro; // find equivalent
            //Obj_AI_Base.OnProcessSpellCast += (sender, eventArgs) => //causes crash
            //    {
            //        if (sender.IsMe && eventArgs.SData.Name == Spells.E.Name)
            //            Core.DelayAction(Orbwalker.ResetAutoAttack, 260);

            //        if (sender.IsMe && eventArgs.SData.Name == Spells.Q.Name)
            //            Core.DelayAction(Orbwalker.ResetAutoAttack, 260);

            //    };
            Orbwalker.OnPostAttack += (unit, target) =>
            {
                if (IreliaMenu.ComboMenu["combo.items"].Cast<CheckBox>().CurrentValue && unit.IsMe && target != null && Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo)
                {
                    if (Spells.Tiamat.IsReady())
                        Spells.Tiamat.Cast();

                    if (Spells.Hydra.IsReady())
                        Spells.Hydra.Cast();
                }
            };
        }

        static void OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (!IreliaMenu.MiscMenu["misc.interrupt"].Cast<CheckBox>().CurrentValue) return;
            if (sender.IsValidTarget(Spells.E.Range) && Player.HealthPercent <= sender.HealthPercent)
                if (Spells.E.IsReady())
                    Spells.E.Cast(sender);
        }

        private static void OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!IreliaMenu.MiscMenu["misc.ag.e"].Cast<CheckBox>().CurrentValue) return;
            if (sender.IsValidTarget(Spells.E.Range) && e.End.Distance(Player.Position) <= Spells.E.Range
                && Player.HealthPercent <= sender.HealthPercent)
            {
                if (Spells.E.IsReady())
                    Spells.E.Cast(sender);
            }
        }

        //private static bool Selected()
        //{
        //    if (!IreliaMenu.ComboMenu["force.target"].Cast<CheckBox>().CurrentValue) return false;

        //    var target = (Obj_AI_Base)Hud.SelectedTarget;
        //    float range = IreliaMenu.ComboMenu[("force.target.range")].Cast<Slider>().CurrentValue;
        //    if (target == null || target.IsDead || target.IsZombie) return false;

        //    return !(Player.Distance(target) > range);
        //}

        //private static Obj_AI_Base GetTarget(float range)
        //{
        //    return (Selected() ? (Obj_AI_Base)Hud.SelectedTarget : TargetSelector.GetTarget(range, DamageType.Physical));
        //}


        private static void OnDraw(EventArgs args)
        {
            if (IreliaMenu.DrawingsMenu["drawings.q"].Cast<CheckBox>().CurrentValue)
            {
                if (Spells.Q.IsReady()) new Circle { Color = Color.FloralWhite, Radius = Spells.Q.Range }.Draw(Player.Position);
                else if (Spells.Q.IsOnCooldown) new Circle { Color = Color.Maroon, Radius = Spells.Q.Range }.Draw(Player.Position);
            }

            if (IreliaMenu.DrawingsMenu["drawings.e"].Cast<CheckBox>().CurrentValue)
            {
                if (Spells.E.IsReady()) new Circle { Color = Color.FloralWhite, Radius = Spells.E.Range }.Draw(Player.Position);
                else if (Spells.E.IsOnCooldown) new Circle { Color = Color.Maroon, Radius = Spells.E.Range }.Draw(Player.Position);
            }

            if (IreliaMenu.DrawingsMenu["drawings.r"].Cast<CheckBox>().CurrentValue)
            {
                if (Spells.R.IsReady()) new Circle { Color = Color.FloralWhite, Radius = Spells.R.Range }.Draw(Player.Position);
                else if (Spells.R.IsOnCooldown) new Circle { Color = Color.Maroon, Radius = Spells.R.Range }.Draw(Player.Position);
            }

        }

        private static void OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (IreliaMenu.ComboMenu["combo.w"].Cast<CheckBox>().CurrentValue &&
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) &&
                target != null && target.IsValidTarget())
                    if (Spells.W.IsReady())
                        Spells.W.Cast();

            if (IreliaMenu.HarassMenu["harass.w"].Cast<CheckBox>().CurrentValue &&
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) &&
                target != null &&
                target.Type == Player.Type &&
                target.IsValidTarget())
                    if (Spells.W.IsReady())
                        Spells.W.Cast();
        }

        private static void OnTick(EventArgs args)
        {
            Killsteal();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {

            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                Clear();
            }
            RCount();
        }

        private static void Clear()
        {
            //if (Player.ManaPercent <= IreliaMenu.LaneClearMenu["laneclear.mana"].Cast<Slider>().CurrentValue && Player.HasBuff("ireliatranscendentbladesspell") && rcount >= 1) goto castr;
            if (Player.ManaPercent <= IreliaMenu.LaneClearMenu["laneclear.mana"].Cast<Slider>().CurrentValue) return;

            var qminion =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Spells.Q.Range)
                    .FirstOrDefault(
                        m =>
                            m.Distance(Player) <= Spells.Q.Range &&
                            m.Health <= QDamage(m) + ExtraWDamage() + SheenDamage(m) - 10 &&
                            m.IsValidTarget());

            if (Spells.Q.IsReady() && IreliaMenu.LaneClearMenu["laneclear.q"].Cast<CheckBox>().CurrentValue && qminion != null && !Orbwalker.IsAutoAttacking)
            {
                Spells.Q.Cast(qminion);
            }

            //castr:
            //var rminions = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position.To2D(), Spells.R.Range);
            //if (Spells.R.IsReady() && IreliaMenu.MainMenu["laneclear.r"].Cast<CheckBox>().CurrentValue && rminions.Count != 0)
            //{
            //    var location = Spells.R.GetLineFarmLocation(rminions);

            //    if (location.MinionsHit >=
            //        IreliaMenu.MainMenu.Item("laneclear.r.minimum").GetValue<Slider>().Value)
            //        Spells.R.Cast(location.Position);
            //}
        }

        private static void Killsteal()
        {
            foreach (
                var enemy in
                    HeroManager.Enemies.Where(e => e.Distance(Player) <= Spells.R.Range && e.IsValidTarget() && !e.IsInvulnerable))
            {
                if (Spells.Q.IsReady() && IreliaMenu.MiscMenu["misc.ks.q"].Cast<CheckBox>().CurrentValue &&
                    Spells.E.IsReady() && IreliaMenu.MiscMenu["misc.ks.e"].Cast<CheckBox>().CurrentValue &&
                    EDamage(enemy) + QDamage(enemy) + ExtraWDamage() + SheenDamage(enemy) >=
                            enemy.Health)
                {
                    if (enemy.Distance(Player) <= Spells.Q.Range && enemy.Distance(Player) > Spells.E.Range)
                    {
                        Spells.Q.Cast(enemy);
                        var enemy1 = enemy;
                        Core.DelayAction(() => Spells.E.Cast(enemy1), (int)(1000 * Player.Distance(enemy) / 2200));
                    }
                    else if (enemy.Distance(Player) <= Spells.Q.Range)
                    {
                        Spells.E.Cast(enemy);
                        var enemy1 = enemy;
                        Core.DelayAction(() => Spells.Q.Cast(enemy1), 250);
                    }

                }

                if (IreliaMenu.MiscMenu["misc.ks.q"].Cast<CheckBox>().CurrentValue && Spells.Q.IsReady() &&
                    QDamage(enemy) + ExtraWDamage() + SheenDamage(enemy)>= enemy.Health &&
                    enemy.Distance(Player) <= Spells.Q.Range)
                {
                    Spells.Q.Cast(enemy);
                    return;
                }

                if (IreliaMenu.MiscMenu["misc.ks.e"].Cast<CheckBox>().CurrentValue && Spells.E.IsReady() &&
                    EDamage(enemy) >= enemy.Health && enemy.Distance(Player) <= Spells.E.Range)
                {
                    Spells.E.Cast(enemy);
                    return;
                }

                if (IreliaMenu.MiscMenu["misc.ks.r"].Cast<CheckBox>().CurrentValue && Spells.R.IsReady() &&
                    RDamage(enemy) >= enemy.Health)
                {
                    Spells.R.Cast(enemy);
                }

            }
        }

        private static void RCount()
        {
            if (rcount == 0 && Spells.R.IsReady())
                rcount = 4;

            if (!Spells.R.IsReady() & rcount != 0)
                rcount = 0;

            foreach (
                var buff in
                    Player.Buffs.Where(b => b.Name == "ireliatranscendentbladesspell" && b.IsValid))
            {
                rcount = buff.Count;
            }
        }

        private static bool UnderTheirTower(Obj_AI_Base target)
        {
            var tower =
                ObjectManager
                    .Get<Obj_AI_Turret>()
                    .FirstOrDefault(turret => turret != null && turret.Distance(target) <= 775 && turret.IsValid && turret.Health > 0 && !turret.IsAlly);

            return tower != null;
        }

        private static void Combo()
        {
            var gctarget = TargetSelector.GetTarget(Spells.Q.Range * 2.5f, DamageType.Physical);
            var target = TargetSelector.GetTarget(Spells.Q.Range, DamageType.Physical);
            if (gctarget == null) return;

            var qminion =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Spells.Q.Range + 350)
                    .Where(
                        m =>
                        m.IsValidTarget()
                        && Prediction.Health.GetPrediction(m, 1000 * (int)(m.Distance(Player) / 2200))
                        <= QDamage(m) + ExtraWDamage() + SheenDamage(m) - 10)
                    .OrderBy(m => m.Distance(gctarget))
                    .FirstOrDefault();

            if (Spells.Q.IsReady())
            {
                if (IreliaMenu.ComboMenu["combo.q.gc"].Cast<CheckBox>().CurrentValue && qminion != null &&
                    gctarget.Distance(Player) >= Player.GetAutoAttackRange(gctarget) &&
                    qminion.Distance(gctarget) <= Player.Distance(gctarget) &&
                    qminion.Distance(Player) <= Spells.Q.Range)
                {
                    Spells.Q.Cast(qminion);
                }

                if (IreliaMenu.ComboMenu["combo.q"].Cast<CheckBox>().CurrentValue && target != null &&
                    target.Distance(Player) <= Spells.Q.Range &&
                    Player.Distance(target) >=
                    IreliaMenu.ComboMenu["combo.q.minrange"].Cast<Slider>().CurrentValue)
                {
                    if (UnderTheirTower(target))
                        if (target.HealthPercent >=
                            IreliaMenu.ComboMenu["combo.q.undertower"].Cast<Slider>().CurrentValue) return;

                    if (IreliaMenu.ComboMenu["combo.w"].Cast<CheckBox>().CurrentValue)
                        Spells.W.Cast();

                    if (qminion != null && qminion.Distance(target) <= Player.Distance(target)) return;

                    Spells.Q.Cast(target);
                }

                if (IreliaMenu.ComboMenu["combo.q"].Cast<CheckBox>().CurrentValue &&
                    IreliaMenu.ComboMenu["combo.q.lastsecond"].Cast<CheckBox>().CurrentValue && target != null)
                {
                    var buff = Player.Buffs.FirstOrDefault(b => b.Name == "ireliahitenstylecharged" && b.IsValid);
                    if (buff != null && buff.EndTime - Game.Time <= (Player.Distance(target) / 2200 + .500 + Player.AttackCastDelay) && !Orbwalker.IsAutoAttacking)
                    {
                        if (UnderTheirTower(target))
                            if (target.HealthPercent >=
                                IreliaMenu.ComboMenu["combo.q.undertower"].Cast<Slider>().CurrentValue) return;

                        Spells.Q.Cast(target);
                    }
                }
            }

            if (Spells.E.IsReady() && IreliaMenu.ComboMenu["combo.e"].Cast<CheckBox>().CurrentValue && target != null)
            {
                if (IreliaMenu.ComboMenu["combo.e.logic"].Cast<CheckBox>().CurrentValue &&
                    target.Distance(Player) <= Spells.E.Range)
                {
                    if (target.HealthPercent >= Player.HealthPercent && !Player.IsDashing())
                    {
                        Spells.E.Cast(target);
                    }
                    else if (!target.IsAttackingPlayer && !Player.IsDashing() && target.Distance(Player) >= Spells.E.Range * .5)
                    {
                        Spells.E.Cast(target);
                    }
                }
                else if (target.Distance(Player) <= Spells.E.Range)
                {
                    Spells.E.Cast(target);
                }
            }

            if (Spells.R.IsReady() && IreliaMenu.ComboMenu["combo.r"].Cast<CheckBox>().CurrentValue && !IreliaMenu.ComboMenu["combo.r.selfactivated"].Cast<CheckBox>().CurrentValue)
            {
                if (IreliaMenu.ComboMenu["combo.r.weave"].Cast<CheckBox>().CurrentValue)
                {
                    if (Item.HasItem((int)ItemId.Sheen, Player) && Item.CanUseItem((int)ItemId.Sheen) || Item.HasItem((int)ItemId.Trinity_Force, Player) && Item.CanUseItem((int)ItemId.Trinity_Force))
                        if (target != null && !Player.HasBuff("sheen") &&
                            target.Distance(Player) <= Spells.R.Range)
                        {
                            Spells.R.Cast(target);
                        }   
                }
                else if (!IreliaMenu.ComboMenu["combo.r.weave"].Cast<CheckBox>().CurrentValue && target != null)
                {
                    Spells.R.Cast(target);
                    // Set to Q range because we are already going to combo them at this point most likely, no stupid long range R initiations
                }
            }
            else if (Spells.R.IsReady() && IreliaMenu.ComboMenu["combo.r"].Cast<CheckBox>().CurrentValue && IreliaMenu.ComboMenu["combo.r.selfactivated"].Cast<CheckBox>().CurrentValue && rcount <= 3)
            {
                if (IreliaMenu.ComboMenu["combo.r.weave"].Cast<CheckBox>().CurrentValue)
                {
                    if (Item.HasItem((int)ItemId.Sheen, Player) && Item.CanUseItem((int)ItemId.Sheen) || Item.HasItem((int)ItemId.Trinity_Force, Player) && Item.CanUseItem((int)ItemId.Trinity_Force))
                        if (target != null && !Player.HasBuff("sheen") &&
                            target.Distance(Player) <= Spells.R.Range)
                        {
                            Spells.R.Cast(target);
                        }
                }
                else if (!IreliaMenu.ComboMenu["combo.r.weave"].Cast<CheckBox>().CurrentValue && target != null)
                {
                    Spells.R.Cast(target);
                    // Set to Q range because we are already going to combo them at this point most likely, no stupid long range R initiations
                }
            }

            if (IreliaMenu.ComboMenu["combo.ignite"].Cast<CheckBox>().CurrentValue && target != null)
            {
                if (Player.Distance(target) <= 600 && ComboDamage(target) >= target.Health)
                    Player.Spellbook.CastSpell(Spells.Ignite, target);
            }

            if (IreliaMenu.ComboMenu["combo.items"].Cast<CheckBox>().CurrentValue && target != null)
            {   
                if (Spells.Youmuu.IsReady() && target.IsValidTarget(Spells.Q.Range))
                {
                    Spells.Youmuu.Cast();
                }

                if (Player.Distance(target) <= 450 && Spells.Cutlass.IsReady())
                {
                    Spells.Cutlass.Cast(target);
                }

                if (Player.Distance(target) <= 450 && Spells.Blade.IsReady())
                {
                    Spells.Blade.Cast(target);
                }
            }
        }
        private static void Harass()
        {
            var gctarget = TargetSelector.GetTarget(Spells.Q.Range * 2.5f, DamageType.Physical);
            var target = TargetSelector.GetTarget(Spells.Q.Range, DamageType.Physical);
            if (gctarget == null) return;

            var qminion =
                EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Position, Spells.Q.Range + 350)
                    .Where(
                        m =>
                        m.IsValidTarget()
                        && Prediction.Health.GetPrediction(m, (int)(m.Distance(Player) / 2200))
                        <= QDamage(m) + ExtraWDamage() + SheenDamage(m) - 10)
                    .OrderBy(m => m.Distance(gctarget))
                    .FirstOrDefault();

            if (Spells.Q.IsReady())
            {
                if (IreliaMenu.HarassMenu["harass.q.gc"].Cast<CheckBox>().CurrentValue && qminion != null &&
                    gctarget.Distance(Player) >= Player.GetAutoAttackRange(gctarget) &&
                    qminion.Distance(gctarget) <= Player.Distance(gctarget) &&
                    qminion.Distance(Player) <= Spells.Q.Range)
                {
                    Spells.Q.Cast(qminion);
                }

                if (IreliaMenu.HarassMenu["harass.q"].Cast<CheckBox>().CurrentValue && target != null &&
                    target.Distance(Player) <= Spells.Q.Range &&
                    Player.Distance(target) >=
                    IreliaMenu.HarassMenu["harass.q.minrange"].Cast<Slider>().CurrentValue)
                {
                    if (UnderTheirTower(target))
                        if (target.HealthPercent >=
                            IreliaMenu.HarassMenu["harass.q.undertower"].Cast<Slider>().CurrentValue) return;

                    if (IreliaMenu.HarassMenu["harass.w"].Cast<CheckBox>().CurrentValue)
                        Spells.W.Cast();

                    if (qminion != null && qminion.Distance(target) <= Player.Distance(target)) return;

                    Spells.Q.Cast(target);
                }

                if (IreliaMenu.HarassMenu["harass.q"].Cast<CheckBox>().CurrentValue &&
                    IreliaMenu.HarassMenu["harass.q.lastsecond"].Cast<CheckBox>().CurrentValue && target != null)
                {
                    var buff = Player.Buffs.FirstOrDefault(b => b.Name == "ireliahitenstylecharged" && b.IsValid);
                    if (buff != null && buff.EndTime - Game.Time <= (Player.Distance(target) / 2200 + .500 + Player.AttackCastDelay) && !Orbwalker.IsAutoAttacking)
                    {
                        if (UnderTheirTower(target))
                            if (target.HealthPercent >=
                                IreliaMenu.HarassMenu["harass.q.undertower"].Cast<Slider>().CurrentValue) return;

                        Spells.Q.Cast(target);
                    }
                }
            }

            if (Spells.E.IsReady() && IreliaMenu.HarassMenu["harass.e"].Cast<CheckBox>().CurrentValue && target != null)
            {
                if (IreliaMenu.HarassMenu["harass.e.logic"].Cast<CheckBox>().CurrentValue &&
                    target.Distance(Player) <= Spells.E.Range)
                {
                    if (target.HealthPercent >= Player.HealthPercent && !Player.IsDashing())
                    {
                        Spells.E.Cast(target);
                    }
                    else if (!target.IsAttackingPlayer && !Player.IsDashing() && target.Distance(Player) >= Spells.E.Range * .5)
                    {
                        Spells.E.Cast(target);
                    }
                }
                else if (target.Distance(Player) <= Spells.E.Range)
                {
                    Spells.E.Cast(target);
                }
            }

            if (Spells.R.IsReady() && IreliaMenu.HarassMenu["harass.r"].Cast<CheckBox>().CurrentValue && !IreliaMenu.HarassMenu["harass.r.selfactivated"].Cast<CheckBox>().CurrentValue)
            {
                if (IreliaMenu.HarassMenu["harass.r.weave"].Cast<CheckBox>().CurrentValue)
                {
                    if (Item.HasItem((int)ItemId.Sheen, Player) && Item.CanUseItem((int)ItemId.Sheen) || Item.HasItem((int)ItemId.Trinity_Force, Player) && Item.CanUseItem((int)ItemId.Trinity_Force))
                        if (target != null && !Player.HasBuff("sheen") &&
                            target.Distance(Player) <= Spells.R.Range)
                        {
                            Spells.R.Cast(target);
                        }
                }
                else if (!IreliaMenu.HarassMenu["harass.r.weave"].Cast<CheckBox>().CurrentValue && target != null)
                {
                    Spells.R.Cast(target);
                }
            }
            else if (Spells.R.IsReady() && IreliaMenu.HarassMenu["harass.r"].Cast<CheckBox>().CurrentValue && IreliaMenu.HarassMenu["harass.r.selfactivated"].Cast<CheckBox>().CurrentValue && rcount <= 3)
            {
                if (IreliaMenu.HarassMenu["harass.r.weave"].Cast<CheckBox>().CurrentValue)
                {
                    if (Item.HasItem((int)ItemId.Sheen, Player) && Item.CanUseItem((int)ItemId.Sheen) || Item.HasItem((int)ItemId.Trinity_Force, Player) && Item.CanUseItem((int)ItemId.Trinity_Force))
                        if (target != null && !Player.HasBuff("sheen") &&
                            target.Distance(Player) <= Spells.R.Range)
                        {
                            Spells.R.Cast(target);
                        }
                }
                else if (!IreliaMenu.HarassMenu["harass.r.weave"].Cast<CheckBox>().CurrentValue && target != null)
                {
                    Spells.R.Cast(target);
                }
            }
        }


        private static void JungleClear()
        {

        }


        public static float ComboDamage(Obj_AI_Base hero) // Thanks honda
        {
            var result = 0d;

            if (Spells.Q.IsReady())
            {
                result += QDamage(hero) + ExtraWDamage() + SheenDamage(hero);
            }
            if (Spells.W.IsReady() || Player.HasBuff("ireliahitenstylecharged"))
            {
                result += (ExtraWDamage() + Player.CalculateDamageOnUnit(hero, DamageType.Physical, Player.TotalAttackDamage)) * 3; // 3 autos
            }
            if (Spells.E.IsReady())
            {
                result += EDamage(hero);
            }
            if (Spells.R.IsReady())
            {
                result += RDamage(hero);
            }

            return (float)result;
        }

        private static double SheenDamage(Obj_AI_Base target) // Thanks princer007 for the basic idea
        {
            var result = 0d;
            foreach (var item in Player.InventoryItems)
                switch ((int)item.Id)
                {
                    case 3057: //Sheen
                        if (Item.CanUseItem((int)ItemId.Sheen))
                            result += Player.CalculateDamageOnUnit(target, DamageType.Physical, Player.BaseAttackDamage);
                                //Player.GetItemDamage(target, ItemId.Sheen);
                        break;
                    case 3078: //Triforce
                        if (Item.CanUseItem((int)ItemId.Trinity_Force))
                            result += Player.CalculateDamageOnUnit(target, DamageType.Physical, Player.BaseAttackDamage * 2);
                                //Player.GetItemDamage(target, ItemId.Trinity_Force);
                        break;
                }
            return result;
        }

        private static double ExtraWDamage()
        {
            // tried some stuff with if buff == null but the damage will be enough then cast W and it worked.. but meh, idk will look at later

            var extra = 0d;
            var buff = Player.Buffs.FirstOrDefault(b => b.Name == "ireliahitenstylecharged" && b.IsValid);
            if (buff != null) // && buff.EndTime > (1000 * Player.Distance(target) / 2200))
                extra += new double[] { 15, 30, 45, 60, 75 }[Spells.W.Level - 1];

            return extra;
        }
        private static double QDamage(Obj_AI_Base target)
        {
            return Spells.Q.IsReady()
                ? Player.CalculateDamageOnUnit(
                    target,
                    DamageType.Physical,
                    new float[] { 20, 50, 80, 110, 140 }[Spells.Q.Level - 1]
                    + Player.TotalAttackDamage)
                : 0d;
        }

        private static double EDamage(Obj_AI_Base target)
        {
            return Spells.E.IsReady()
                ? Player.CalculateDamageOnUnit(
                    target,
                    DamageType.Magical,
                    new float[] { 80, 120, 160, 200, 240 }[Spells.E.Level - 1]
                    + .5f * Player.TotalMagicalDamage)
                : 0d;
        }

        private static double RDamage(Obj_AI_Base target)
        {
            return Spells.R.IsReady()
                ? Player.CalculateDamageOnUnit(
                    target,
                    DamageType.Physical,
                    (new float[] { 80, 120, 160 }[Spells.R.Level - 1]
                    + .5f * Player.TotalMagicalDamage
                    + .6f * Player.FlatPhysicalDamageMod
                    ) * rcount)
                : 0d;
        }
    }
}
