// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Kalista.cs" company="LeagueSharp">
//   Copyright (C) 2015 LeagueSharp
//   
//             This program is free software: you can redistribute it and/or modify
//             it under the terms of the GNU General Public License as published by
//             the Free Software Foundation, either version 3 of the License, or
//             (at your option) any later version.
//   
//             This program is distributed in the hope that it will be useful,
//             but WITHOUT ANY WARRANTY; without even the implied warranty of
//             MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//             GNU General Public License for more details.
//   
//             You should have received a copy of the GNU General Public License
//             along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   The champion class
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using iKalista.Helpers;
    using iKalista.Modules;

    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Events;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.UI.IMenu;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;
    using LeagueSharp.SDK.Core.UI.INotifications;
    using LeagueSharp.SDK.Core.Utils;
    using LeagueSharp.SDK.Core.Wrappers;

    using SharpDX;

    using Color = System.Drawing.Color;

    /// <summary>
    ///     The champion class
    /// </summary>
    public class Kalista
    {
        #region Static Fields

        /// <summary>
        ///     The Modules
        /// </summary>
        public static readonly List<IModule> Modules = new List<IModule>
                                                           {
                                                               new KillstealModule(), new AutoEModule(), 
                                                               new AutoHarassModule(), new LeavingModule(), 
                                                               new MobStealModule()
                                                           };

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="Kalista" /> class.
        /// </summary>
        public Kalista()
        {
            MenuGenerator.Generate();
            Game.OnUpdate += this.OnUpdate;
            Drawing.OnDraw += this.OnDraw;
            TickLimiter.Add("ModulesLimiter", 0x7D);
            this.LoadModules();
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
            Notifications.Add(
                new Notification(
                    "iKalista 2.0 - Even Better Then Before", 
                    "Please Note this is currently a work in progress assembly\n"
                    + "Please leave any feedback / suggestion in the thread\n" + "Made by Corey, Protected under Australian IP Laws"));
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
        ///     Loads all the modules
        /// </summary>
        private void LoadModules()
        {
            foreach (var module in Modules.Where(x => x.ShouldBeLoaded()))
            {
                try
                {
                    module.OnLoad();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error loading module: " + module.GetModuleName() + " Exception: " + e);
                }
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
                Drawing.DrawCircle(ObjectManager.Player.Position, SpellManager.Spell[SpellSlot.Q].Range, Color.Crimson);
            }

            if (this.Menu["com.kalista.drawing"]["drawE"].GetValue<MenuBool>().Value)
            {
                Drawing.DrawCircle(ObjectManager.Player.Position, SpellManager.Spell[SpellSlot.E].Range, Color.Crimson);
            }

            if (this.Menu["com.kalista.drawing"]["drawELeaving"].GetValue<MenuBool>().Value)
            {
                Drawing.DrawCircle(
                    ObjectManager.Player.Position, 
                    SpellManager.Spell[SpellSlot.E].Range - 200, 
                    Color.Crimson);
            }

            if (this.Menu["com.kalista.drawing"]["drawPercentage"].GetValue<MenuBool>().Value)
            {
                foreach (var source in
                    GameObjects.EnemyHeroes.Where(x => !x.IsDead && x.Position.IsOnScreen()))
                {
                    var currentPercentage = Helper.GetRendDamage(source) * 100 / source.GetHealthWithShield();

                    Drawing.DrawText(
                        Drawing.WorldToScreen(source.Position)[0], 
                        Drawing.WorldToScreen(source.Position)[1], 
                        currentPercentage >= 100 ? Color.DarkRed : Color.White, 
                        currentPercentage >= 100 ? "Killable With E" : "Current Damage: " + currentPercentage + "%");
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
            if (!TickLimiter.CanTick("ModulesLimiter"))
            {
                return;
            }

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

            foreach (var module in Modules.Where(x => x.ShouldRun()))
            {
                module.Run();
            }
        }

        /// <summary>
        ///     The things we always want to process
        /// </summary>
        private void UpdateFunctions()
        {
            this.AutoSentinel();
        }

        #endregion
    }
}