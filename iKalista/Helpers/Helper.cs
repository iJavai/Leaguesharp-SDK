﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.cs" company="LeagueSharp">
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
//   The Helper class
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Math.Prediction;
    using LeagueSharp.SDK.Core.Wrappers;

    using SharpDX;

    using Collision = LeagueSharp.SDK.Core.Math.Collision;

    /// <summary>
    ///     The Helper class
    /// </summary>
    internal static class Helper
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Gets the list of minions currently between the source and target
        /// </summary>
        /// <param name="source">
        ///     The Source
        /// </param>
        /// <param name="targetPosition">
        ///     The Target Position
        /// </param>
        /// <returns>
        ///     The <see cref="List" />.
        /// </returns>
        public static List<Obj_AI_Base> GetCollisionMinions(Obj_AI_Hero source, Vector3 targetPosition)
        {
            var input = new PredictionInput
                            {
                                Unit = source, Radius = SpellManager.Spell[SpellSlot.Q].Width, 
                                Delay = SpellManager.Spell[SpellSlot.Q].Delay, 
                                Speed = SpellManager.Spell[SpellSlot.Q].Speed, 
                                CollisionObjects = CollisionableObjects.Minions
                            };

            return
                Collision.GetCollision(new List<Vector3> { targetPosition }, input)
                    .OrderBy(x => x.Distance(source))
                    .ToList();
        }

        /// <summary>
        ///     Gets the targets current health including shield damage
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float GetHealthWithShield(this Obj_AI_Base target)
        {
            /*var result = target.Health;

            if (target.AttackShield > 0)
            {
                result += target.AttackShield;
            }*/
            return target.Health;
        }

        /// <summary>
        ///     Gets the rend buff
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="BuffInstance" />.
        /// </returns>
        public static BuffInstance GetRendBuff(this Obj_AI_Base target)
        {
            return
                target.Buffs.Find(
                    b => b.Caster.IsMe && b.IsValid && b.DisplayName.ToLowerInvariant() == "kalistaexpungemarker");
        }

        /// <summary>
        ///     Gets the current <see cref="BuffInstance" /> Count of Expunge
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="int" />.
        /// </returns>
        public static int GetRendBuffCount(this Obj_AI_Base target)
        {
            return target.Buffs.Count(x => x.Name == "kalistaexpungemarker");
        }

        /// <summary>
        ///     Gets the Rend Damage for each target
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        public static float GetRendDamage(Obj_AI_Base target)
        {
            // If that target doesn't have a rend stack then calculating this is pointless
            if (!target.HasRendBuff())
            {
                return 0f;
            }

            // The base damage of E
            var baseDamage = Damages.GetRendDamage(target); // TODO replace with common damage :S

            // With exhaust players damage is reduced by 40%
            if (ObjectManager.Player.HasBuff("summonerexhaust"))
            {
                return baseDamage * 0.4f;
            }

            // Alistars ultimate reduces damage dealt by 70%
            if (target.HasBuff("FerociousHowl"))
            {
                return baseDamage * 0.7f;
            }

            // Damage to dragon is reduced by 7% * (stacks)
            if (target.Name.Contains("Dragon") && ObjectManager.Player.HasBuff("s5test_dragonslayerbuff"))
            {
                return baseDamage * (1 - (0.7f * ObjectManager.Player.GetBuffCount("s5test_dragonslayerbuff")));
            }

            // Damage to baron is reduced by 50% if the player has the 'barontarget'
            if (target.Name.Contains("Baron") && ObjectManager.Player.HasBuff("barontarget"))
            {
                return (float)(baseDamage * 0.5);
            }

            return baseDamage;
        }

        /// <summary>
        ///     Checks if a target has the Expunge <see cref="BuffInstance" />
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool HasRendBuff(this Obj_AI_Base target)
        {
            return target.GetRendBuff() != null;
        }

        /// <summary>
        ///     Checks if the given target has an invulnerable buff
        /// </summary>
        /// <param name="target1">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool HasUndyingBuff(this Obj_AI_Base target1)
        {
            var target = target1 as Obj_AI_Hero;

            if (target != null)
            {
                // Tryndamere R
                if (target.ChampionName == "Tryndamere"
                    && target.Buffs.Any(
                        b => b.Caster.NetworkId == target.NetworkId && b.IsValid && b.DisplayName == "Undying Rage"))
                {
                    return true;
                }

                // Zilean R
                if (target.Buffs.Any(b => b.IsValid && b.DisplayName == "Chrono Shift"))
                {
                    return true;
                }

                // Kayle R
                if (target.Buffs.Any(b => b.IsValid && b.DisplayName == "JudicatorIntervention"))
                {
                    return true;
                }

                // Poppy R
                if (target.ChampionName == "Poppy")
                {
                    if (
                        GameObjects.AllyHeroes.Any(
                            o =>
                            !o.IsMe
                            && o.Buffs.Any(
                                b =>
                                b.Caster.NetworkId == target.NetworkId && b.IsValid && b.DisplayName == "PoppyDITarget")))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///     TODO The is mob killable.
        /// </summary>
        /// <param name="target">
        ///     TODO The target.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsMobKillable(this Obj_AI_Minion target)
        {
            return GetRendDamage(target) >= target.Health;
        }

        /// <summary>
        ///     Checks if the given target is killable
        /// </summary>
        /// <param name="target">
        ///     The Target
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public static bool IsRendKillable(this Obj_AI_Hero target)
        {
            return GetRendDamage(target) >= GetHealthWithShield(target) && !HasUndyingBuff(target);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the Base Damage
        /// </summary>
        /// <param name="target">
        ///     The target
        /// </param>
        /// <returns>
        ///     The <see cref="float" />.
        /// </returns>
        private static float GetBaseDamage(Obj_AI_Base target)
        {
            var rawRendDamage = new float[] { 20, 30, 40, 50, 60 };
            var rawRendDamageMultiplier = new[] { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f };
            var rawRendDamagePerSpear = new float[] { 10, 14, 19, 25, 32 };
            var rawRendDamagePerSpearMultiplier = new[] { 0.2, 0.225, 0.25, 0.275, 0.3 };
            var buffCount = target.GetBuffCount("kalistaexpungemarker");

            var baseDamage = (rawRendDamage[SpellManager.Spell[SpellSlot.E].Level - 1]
                              + rawRendDamageMultiplier[SpellManager.Spell[SpellSlot.E].Level - 1]
                              * ObjectManager.Player.TotalAttackDamage)
                             + (buffCount - 1)
                             * (rawRendDamagePerSpear[SpellManager.Spell[SpellSlot.E].Level - 1]
                                + rawRendDamagePerSpearMultiplier[SpellManager.Spell[SpellSlot.E].Level - 1]
                                * ObjectManager.Player.TotalAttackDamage);

            return (float)ObjectManager.Player.CalculateDamage(target, DamageType.Physical, baseDamage);
        }

        #endregion
    }
}