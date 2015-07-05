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
        ///     Auto Harass Function
        /// </summary>
        private void AutoHarass()
        {
            // TODO
        }

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

                if (rendTarget.IsRendKillable())
                {
                    Console.WriteLine("Killable: " + rendTarget.IsRendKillable());
                    SpellManager.Spell[SpellSlot.E].Cast();
                }
            }
        }

        /// <summary>
        ///     The Harass Function
        /// </summary>
        private void OnHarass()
        {
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
            }

            if (this.Menu["com.kalista.flee"]["fleeActive"].GetValue<MenuKeyBind>().Active)
            {
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
            // this.AutoHarass();
            this.JungleSteal();
            this.AutoSentinel();

            this.ProcessKillsteal();
        }

        #endregion
    }
}