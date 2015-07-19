// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoHarassModule.cs" company="LeagueSharp">
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
//   TODO The auto harass module.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace iKalista.Modules
{
    using System;

    /// <summary>
    /// TODO The auto harass module.
    /// </summary>
    internal class AutoHarassModule : IModule
    {
        /// <summary>
        ///     Gets the module name
        /// </summary>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        public string GetModuleName()
        {
            return "Auto Harass Module";
        }

        /// <summary>
        ///     The On Module Load Function
        /// </summary>
        public void OnLoad()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     The Module Run Function
        /// </summary>
        public void Run()
        {
            Console.WriteLine("Auto Harass Module Loaded!");
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
            return false;
        }
    }
}