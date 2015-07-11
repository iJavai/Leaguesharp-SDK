// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Kalista.cs" company="">
//   
// </copyright>
// <summary>
//   The champion class
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista
{
    using System;
    using System.Linq;

    using iKalista.Helpers;

    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Events;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.UI.IMenu;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using LeagueSharp.SDK.Core.Utils;
    using LeagueSharp.SDK.Core.Wrappers;

    using SharpDX;

    /// <summary>
    ///     The champion class
    /// </summary>
    public class Kalista
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Kalista" /> class.
        /// </summary>
        public Kalista()
        {
            MenuGenerator.Generate();
            Game.OnUpdate += this.OnUpdate;
            Spellbook.OnCastSpell += (sender, args) =>
                {
                    if (sender.Owner.IsMe && args.Slot == SpellSlot.Q && ObjectManager.Player.IsDashing())
                    {
                        args.Process = false;
                    }
                };
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
                {
                    if (!sender.IsMe)
                    {
                        return;
                    }

                    if (args.SData.Name == "KalistaExpungeWrapper")
                    {
                        DelayAction.Add(0x7D, Orbwalker.ResetAutoAttackTimer);
                    }
                };
            Drawing.OnDraw += this.OnDraw;
        }

        /// <summary>
        ///     The Drawing Function
        /// </summary>
        /// <param name="args">
        ///     The Arguments
        /// </param>
        private void OnDraw(EventArgs args)
        {
            if (this.Menu["com.kalista.drawing"]["drawQ"].GetValue<MenuBool>().Value)
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, SpellManager.Spell[SpellSlot.Q].Range, System.Drawing.Color.Crimson);
            }

            if (this.Menu["com.kalista.drawing"]["drawE"].GetValue<MenuBool>().Value)
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, SpellManager.Spell[SpellSlot.E].Range, System.Drawing.Color.Crimson);
            }

            if (this.Menu["com.kalista.drawing"]["drawELeaving"].GetValue<MenuBool>().Value)
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, SpellManager.Spell[SpellSlot.E].Range - 200, System.Drawing.Color.Crimson);
            }

            if (this.Menu["com.kalista.drawing"]["drawPercentage"].GetValue<MenuBool>().Value)
            {
                foreach (var source in
                            GameObjects.EnemyHeroes.Where(x => ObjectManager.Player.Distance(x) <= 2000f && !x.IsDead))
                {
                    var currentPercentage = Helper.GetRendDamage(source) * 100 / source.GetHealthWithShield();

                    Drawing.DrawText(
                        Drawing.WorldToScreen(source.Position)[0],
                        Drawing.WorldToScreen(source.Position)[1],
                        currentPercentage >= 100 ? System.Drawing.Color.DarkRed : System.Drawing.Color.White,
                        currentPercentage >= 100 ? "Killable With E" : "Current Damage: " + currentPercentage + "%");
                }
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the menu.
        /// </summary>
        public Menu Menu
        {
            get
            {
                return MenuGenerator.RootMenu;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Sends sentinels to either dragon or baron pit!
        /// </summary>
        private void AutoSentinel()
        {
            if (!SpellManager.Spell[SpellSlot.W].IsReady())
            {
                return;
            }

            var baronPosition = new Vector3(4944, 10388, -712406f);
            var dragonPosition = new Vector3(9918f, 4474f, -71.2406f);

            if (this.Menu["com.kalista.misc"]["sentinelDrake"].GetValue<MenuKeyBind>().Active
                && ObjectManager.Player.Distance(dragonPosition) <= SpellManager.Spell[SpellSlot.W].Range)
            {
                SpellManager.Spell[SpellSlot.W].Cast(dragonPosition);
            }

            if (this.Menu["com.kalista.misc"]["sentinelBaron"].GetValue<MenuKeyBind>().Active
                && ObjectManager.Player.Distance(baronPosition) <= SpellManager.Spell[SpellSlot.W].Range)
            {
                SpellManager.Spell[SpellSlot.W].Cast(baronPosition);
            }
        }

        /// <summary>
        ///     Steals Jungle Mobs
        /// </summary>
        private void JungleSteal()
        {
            if (!SpellManager.Spell[SpellSlot.E].IsReady())
            {
                return;
            }

            var jungleMinion =
                GameObjects.Jungle.FirstOrDefault(
                    x =>
                    !x.Name.Contains("Mini") && x.IsMobKillable()
                    && x.IsValidTarget(SpellManager.Spell[SpellSlot.E].Range));
            var siegeMinion =
                GameObjects.EnemyMinions.FirstOrDefault(
                    x =>
                    (x.SkinName.Contains("siege") || x.SkinName.Contains("super")) && x.IsMobKillable()
                    && x.IsValidTarget(SpellManager.Spell[SpellSlot.E].Range));

            switch (this.Menu["com.kalista.misc"]["stealMode"].GetValue<MenuList<string>>().Index)
            {
                case 0: // Both

                    if (jungleMinion != null || siegeMinion != null)
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }

                    break;
                case 1: // Jungle Minions

                    if (jungleMinion != null)
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }

                    break;
                case 2: // siege minions

                    if (siegeMinion != null)
                    {
                        SpellManager.Spell[SpellSlot.E].Cast();
                    }

                    break;
            }
        }

        /// <summary>
        ///     The Combo Function
        /// </summary>
        private void OnCombo()
        {
            if (SpellManager.Spell[SpellSlot.Q].IsReady()
                && this.Menu["com.kalista.combo"]["useQ"].GetValue<MenuBool>().Value)
            {
                var target = TargetSelector.GetTarget(SpellManager.Spell[SpellSlot.Q].Range);

                if (!ObjectManager.Player.IsWindingUp && !ObjectManager.Player.IsDashing())
                {
                    var prediction = SpellManager.Spell[SpellSlot.Q].GetPrediction(target);
                    if (prediction.Hitchance >= HitChance.Medium)
                    {
                        SpellManager.Spell[SpellSlot.Q].Cast(prediction.CastPosition);
                    }
                }
            }

            if (SpellManager.Spell[SpellSlot.E].IsReady()
                && this.Menu["com.kalista.combo"]["useE"].GetValue<MenuBool>().Value)
            {
                var rendTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            x =>
                            x.IsEnemy && x.IsValidTarget(950) && x.HasRendBuff()
                            && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield))
                        .OrderByDescending(Helper.GetRendDamage)
                        .FirstOrDefault();

                if (rendTarget == null)
                {
                    return;
                }

                var damage = Math.Ceiling(Helper.GetRendDamage(rendTarget) * 100 / rendTarget.GetHealthWithShield());

                if (this.Menu["com.kalista.combo"]["useELeaving"].GetValue<MenuBool>().Value && damage >= this.Menu["com.kalista.combo"]["eLeavePercent"].GetValue<MenuSlider>().Value
                    && rendTarget.HealthPercent > 20
                    && rendTarget.ServerPosition.Distance(ObjectManager.Player.ServerPosition)
                    > Math.Pow(SpellManager.Spell[SpellSlot.E].Range * 0.8, 2) && !rendTarget.IsFacing(ObjectManager.Player))
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
                }

                if (this.Menu["com.kalista.combo"]["autoE"].GetValue<MenuBool>().Value && damage >= this.Menu["com.kalista.combo"]["minStacks"].GetValue<MenuSlider>().Value)
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
                }

                if (rendTarget.IsRendKillable())
                {
                    Console.WriteLine("Killable: " + rendTarget.IsRendKillable());
                    SpellManager.Spell[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        ///     On Flee Function
        /// </summary>
        private void OnFlee()
        {
            // TODO
        }

        /// <summary>
        ///     The Harass Function
        /// </summary>
        private void OnHarass()
        {
            if (SpellManager.Spell[SpellSlot.Q].IsReady()
                && this.Menu["com.kalista.harass"]["useQ"].GetValue<MenuBool>().Value)
            {
                var target = TargetSelector.GetTarget(SpellManager.Spell[SpellSlot.Q].Range);

                if (!ObjectManager.Player.IsWindingUp && !ObjectManager.Player.IsDashing())
                {
                    SpellManager.Spell[SpellSlot.Q].CastIfHitchanceMinimum(target, HitChance.Medium);
                }
            }

            if (SpellManager.Spell[SpellSlot.E].IsReady()
                && this.Menu["com.kalista.harass"]["useE"].GetValue<MenuBool>().Value)
            {
                var rendTarget =
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            x =>
                            x.IsEnemy && x.IsValidTarget(950) && x.HasRendBuff()
                            && !x.HasBuffOfType(BuffType.Invulnerability) && !x.HasBuffOfType(BuffType.SpellShield))
                        .OrderByDescending(Helper.GetRendDamage)
                        .FirstOrDefault();

                if (rendTarget == null)
                {
                    return;
                }

                var currentPercentDamage = Math.Ceiling(Helper.GetRendDamage(rendTarget) * 100 / rendTarget.Health);

                if (rendTarget.IsRendKillable()
                    || currentPercentDamage >= this.Menu["com.kalista.harass"]["minStacks"].GetValue<MenuSlider>().Value)
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        ///     The lane clearing function
        /// </summary>
        private void OnLaneclear()
        {
            if (this.Menu["com.kalista.laneclear"]["useQ"].GetValue<MenuBool>().Value)
            {
                var currentMana = ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100;
                if (currentMana < this.Menu["com.kalista.laneclear"]["qMana"].GetValue<MenuSlider>().Value)
                {
                    return;
                }

                var minions =
                    GameObjects.EnemyMinions.Where(
                        x => ObjectManager.Player.Distance(x) <= SpellManager.Spell[SpellSlot.Q].Range && x.IsValid)
                        .ToList();
                if (minions.Count < 0)
                {
                    return;
                }

                foreach (
                    var minion in minions.Where(x => x.Health <= ObjectManager.Player.GetSpellDamage(x, SpellSlot.Q)))
                {
                    var killableMinions =
                        Helper.GetCollisionMinions(
                            ObjectManager.Player, 
                            ObjectManager.Player.ServerPosition.Extend(
                                minion.ServerPosition, 
                                SpellManager.Spell[SpellSlot.Q].Range))
                            .Count(
                                collisionMinion =>
                                collisionMinion.Health
                                <= ObjectManager.Player.GetSpellDamage(collisionMinion, SpellSlot.Q));

                    if (killableMinions >= this.Menu["com.kalista.laneclear"]["qClear"].GetValue<MenuSlider>().Value
                        && !ObjectManager.Player.IsWindingUp)
                    {
                        SpellManager.Spell[SpellSlot.Q].Cast(minion.ServerPosition);
                    }
                }
            }

            if (this.Menu["com.kalista.laneclear"]["useE"].GetValue<MenuBool>().Value)
            {
                var currentMana = ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100;
                if (currentMana < this.Menu["com.kalista.laneclear"]["eMana"].GetValue<MenuSlider>().Value)
                {
                    return;
                }

                var minions =
                    GameObjects.EnemyMinions.Where(
                        x => ObjectManager.Player.Distance(x) <= SpellManager.Spell[SpellSlot.E].Range && x.IsValid)
                        .ToList();

                var killableMinions =
                    minions.Count(
                        x => SpellManager.Spell[SpellSlot.E].CanCast(x) && x.Health <= Helper.GetRendDamage(x));

                if (killableMinions >= this.Menu["com.kalista.laneclear"]["eClear"].GetValue<MenuSlider>().Value
                    && !ObjectManager.Player.IsWindingUp)
                {
                    SpellManager.Spell[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        ///     The update function
        /// </summary>
        /// <param name="args">
        ///     The Arguments
        /// </param>
        private void OnUpdate(EventArgs args)
        {
            switch (Orbwalker.ActiveMode)
            {
                case OrbwalkerMode.Orbwalk:
                    this.OnCombo();
                    break;
                case OrbwalkerMode.Hybrid:
                    this.OnHarass();
                    break;
                case OrbwalkerMode.LaneClear:
                    this.OnLaneclear();
                    break;
            }

            if (this.Menu["com.kalista.flee"]["fleeActive"].GetValue<MenuKeyBind>().Active)
            {
                this.OnFlee();
            }

            this.UpdateFunctions();
        }

        /// <summary>
        ///     Process the kill steal
        /// </summary>
        private void ProcessKillsteal()
        {
            if (!SpellManager.Spell[SpellSlot.E].IsReady())
            {
                return;
            }

            var rendTarget =
                ObjectManager.Get<Obj_AI_Hero>()
                    .FirstOrDefault(
                        x => x.IsEnemy && x.IsRendKillable() && x.IsValidTarget(SpellManager.Spell[SpellSlot.E].Range));

            if (rendTarget != null)
            {
                SpellManager.Spell[SpellSlot.E].Cast();
            }
        }

        /// <summary>
        ///     The things we always want to process
        /// </summary>
        private void UpdateFunctions()
        {
            this.ProcessKillsteal();
            this.JungleSteal();
            this.AutoSentinel();
        }

        #endregion
    }
}