// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Damages.cs" company="LeagueSharp">
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
// --------------------------------------------------------------------------------------------------------------------

namespace iKalista.Helpers
{
    using LeagueSharp;
    using LeagueSharp.SDK.Core.Wrappers;

    /// <summary>
    /// TODO The damages.
    /// </summary>
    internal static class Damages
    {
        #region Static Fields

        /// <summary>
        /// TODO The player.
        /// </summary>
        private static readonly Obj_AI_Hero Player = ObjectManager.Player;

        /// <summary>
        /// TODO The raw rend damage.
        /// </summary>
        private static readonly float[] RawRendDamage = { 20, 30, 40, 50, 60 };

        /// <summary>
        /// TODO The raw rend damage multiplier.
        /// </summary>
        private static readonly float[] RawRendDamageMultiplier = { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f };

        /// <summary>
        /// TODO The raw rend damage per spear.
        /// </summary>
        private static readonly float[] RawRendDamagePerSpear = { 10, 14, 19, 25, 32 };

        /// <summary>
        /// TODO The raw rend damage per spear multiplier.
        /// </summary>
        private static readonly float[] RawRendDamagePerSpearMultiplier = { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f };

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// TODO The get raw rend damage.
        /// </summary>
        /// <param name="target">
        /// TODO The target.
        /// </param>
        /// <param name="customStacks">
        /// TODO The custom stacks.
        /// </param>
        /// <returns>
        /// The <see cref="float"/>.
        /// </returns>
        public static float GetRawRendDamage(Obj_AI_Base target, int customStacks = -1)
        {
            // Get buff
            var buff = target.GetRendBuff();

            if (buff != null || customStacks > -1)
            {
                return (RawRendDamage[SpellManager.Spell[SpellSlot.E].Level - 1]
                        + RawRendDamageMultiplier[SpellManager.Spell[SpellSlot.E].Level - 1]
                        * Player.TotalAttackDamage()) + // Base damage
                       ((customStacks < 0 ? buff.Count : customStacks) - 1) * // Spear count
                       (RawRendDamagePerSpear[SpellManager.Spell[SpellSlot.E].Level - 1]
                        + RawRendDamagePerSpearMultiplier[SpellManager.Spell[SpellSlot.E].Level - 1]
                        * Player.TotalAttackDamage()); // Damage per spear
            }

            return 0;
        }

        /// <summary>
        /// TODO The get rend damage.
        /// </summary>
        /// <param name="target">
        /// TODO The target.
        /// </param>
        /// <returns>
        /// The <see cref="float"/>.
        /// </returns>
        public static float GetRendDamage(Obj_AI_Hero target)
        {
            return GetRendDamage(target, -1);
        }

        /// <summary>
        /// TODO The get rend damage.
        /// </summary>
        /// <param name="target">
        /// TODO The target.
        /// </param>
        /// <param name="customStacks">
        /// TODO The custom stacks.
        /// </param>
        /// <returns>
        /// The <see cref="float"/>.
        /// </returns>
        public static float GetRendDamage(Obj_AI_Base target, int customStacks = -1)
        {
            // Calculate the damage and return
            return ((float)Player.CalculateDamage(target, DamageType.Physical, GetRawRendDamage(target, customStacks))
                    - 20) * 0.98f;
        }

        /// <summary>
        /// TODO The total attack damage.
        /// </summary>
        /// <param name="target">
        /// TODO The target.
        /// </param>
        /// <returns>
        /// The <see cref="float"/>.
        /// </returns>
        public static float TotalAttackDamage(this Obj_AI_Base target)
        {
            return target.BaseAttackDamage + target.FlatPhysicalDamageMod;
        }

        #endregion
    }
}