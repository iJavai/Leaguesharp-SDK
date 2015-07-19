// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MobStealModule.cs" company="LeagueSharp">
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
//   TODO The mob steal module.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista.Modules
{
    using System;
    using System.Linq;

    using iKalista.Helpers;

    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;

    /// <summary>
    ///     TODO The mob steal module.
    /// </summary>
    internal class MobStealModule : IModule
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Gets the module name
        /// </summary>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public string GetModuleName()
        {
            return "Mob Steal Module";
        }

        /// <summary>
        ///     The On Module Load Function
        /// </summary>
        public void OnLoad()
        {
            Console.WriteLine("Jungle steal module loaded!");
        }

        /// <summary>
        ///     The Module Run Function
        /// </summary>
        public void Run()
        {
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

            switch (MenuGenerator.RootMenu["com.kalista.misc"]["stealMode"].GetValue<MenuList<string>>().Index)
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
        ///     Checks if the Module should be loaded
        /// </summary>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool ShouldBeLoaded()
        {
            return true;
        }

        /// <summary>
        ///     Checks if the module should run
        /// </summary>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public bool ShouldRun()
        {
            return SpellManager.Spell[SpellSlot.E].IsReady()
                   && MenuGenerator.RootMenu["com.kalista.misc"]["useJungleSteal"].GetValue<MenuBool>().Value;
        }

        #endregion
    }
}