// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KillstealModule.cs" company="LeagueSharp">
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
//   The Kill steal module
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista.Modules
{
    using System;

    using iKalista.Helpers;

    using LeagueSharp;
    using LeagueSharp.SDK.Core;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;

    /// <summary>
    ///     The Kill steal module
    /// </summary>
    internal class KillstealModule : IModule
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
            return "Killsteal Module";
        }

        /// <summary>
        ///     The On Module Load Function
        /// </summary>
        public void OnLoad()
        {
            Console.WriteLine("Killsteal Module loaded.");
        }

        /// <summary>
        ///     The Module Run Function
        /// </summary>
        public void Run()
        {
            var target =
                GameObjects.EnemyHeroes.Find(
                    x => x.IsValidTarget(SpellManager.Spell[SpellSlot.E].Range) && x.IsRendKillable());

            if (target != null)
            {
                Console.WriteLine("Running Module - Casting E KS");
                SpellManager.Spell[SpellSlot.E].Cast();
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
            return MenuGenerator.RootMenu["com.kalista.combo"]["useE"].GetValue<MenuBool>().Value && SpellManager.Spell[SpellSlot.E].IsReady();
        }

        #endregion
    }
}