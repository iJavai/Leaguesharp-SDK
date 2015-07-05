// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellManager.cs" company="LeagueSharp">
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
//   The Spell Manager
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista.Helpers
{
    using System.Collections.Generic;

    using LeagueSharp;
    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.Wrappers;

    /// <summary>
    ///     The Spell Manager
    /// </summary>
    internal class SpellManager
    {
        /// <summary>
        ///     The dictionary to call the Spell slot and the Spell Class
        /// </summary>
        public static readonly Dictionary<SpellSlot, Spell> Spell = new Dictionary<SpellSlot, Spell>
                                                                   {
                                                                       { SpellSlot.Q, new Spell(SpellSlot.Q, 1130) }, 
                                                                       { SpellSlot.W, new Spell(SpellSlot.W, 5200) }, 
                                                                       { SpellSlot.E, new Spell(SpellSlot.E, 950) }, 
                                                                       { SpellSlot.R, new Spell(SpellSlot.R, 1200) }
                                                                   };

        /// <summary>
        ///     Initializes static members of the <see cref="SpellManager"/> class.
        /// </summary>
        static SpellManager()
        {
            // Spell initialization
            Spell[SpellSlot.Q].SetSkillshot(0.25f, 30f, 1700f, true, SkillshotType.SkillshotLine);
            Spell[SpellSlot.R].SetSkillshot(0.50f, 1500f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }
    }
}