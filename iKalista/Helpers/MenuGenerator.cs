// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MenuGenerator.cs" company="LeagueSharp">
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
//   The menu generator class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista.Helpers
{
    using System.Windows.Forms;

    using LeagueSharp.SDK.Core.Enumerations;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;

    using Menu = LeagueSharp.SDK.Core.UI.IMenu.Menu;

    /// <summary>
    ///     The menu generator class.
    /// </summary>
    internal class MenuGenerator
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the root menu.
        /// </summary>
        public static Menu RootMenu { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The generation function.
        /// </summary>
        public static void Generate()
        {
            RootMenu = new Menu("com.ikalista", "iKalista 2.0", true);

            var comboMenu = new Menu("com.kalista.combo", "Combo Options");
            {
                comboMenu.Add(new MenuSeparator("sep111", "Combo"));
                comboMenu.Add(new MenuBool("useQ", "Use Q", true));
                comboMenu.Add(new MenuBool("useE", "Use E", true));

                comboMenu.Add(new MenuSeparator("sep11", "Auto E"));
                comboMenu.Add(new MenuBool("autoE", "Auto E"));
                comboMenu.Add(new MenuSlider("minStacks", "Min Percent for Auto E", 100, 10));

                comboMenu.Add(new MenuSeparator("sep123", "E Leaving"));
                comboMenu.Add(new MenuBool("useELeaving", "E Leaving target"));
                comboMenu.Add(new MenuSlider("eLeavePercent", "Min Percent for Leaving", 40, 10));

                comboMenu.Add(new MenuSeparator("sep231", "E Before Death"));
                comboMenu.Add(new MenuBool("eDeath", "E Before Death"));
                comboMenu.Add(new MenuSlider("eDeathPercent", "Damage Percent Before Death", 20, 10));
                comboMenu.Add(new MenuSlider("myHealthPercent", "My Health Percent Before Death", 15, 5, 50));

                comboMenu.Add(new MenuSeparator("sep321", "Ultimate Options"));
                comboMenu.Add(new MenuBool("saveAlly", "Save Ally With Ultimate"));
                comboMenu.Add(new MenuSlider("allyHealth", "Min Ally Health to save", 15, 10));
                RootMenu.Add(comboMenu);
            }

            var harassMenu = new Menu("com.kalista.harass", "Harass Options");
            {
                harassMenu.Add(new MenuBool("useQ", "Use Q", true));
                harassMenu.Add(new MenuBool("useE", "Use E", true));
                harassMenu.Add(new MenuSlider("minStacks", "Min Percent for Auto E", 50, 10));
                harassMenu.Add(new MenuBool("autoHarass", "Minion -> Champion Harass", true));
                RootMenu.Add(harassMenu);
            }

            var laneclearMenu = new Menu("com.kalista.laneclear", "Laneclear Options");
            {
                laneclearMenu.Add(new MenuBool("useQ", "Use Q", true));
                laneclearMenu.Add(new MenuSlider("qClear", "Min Minions for Q", 3, 1, 10));
                laneclearMenu.Add(new MenuSlider("qMana", "Min Mana for Q %", 50, 10));

                laneclearMenu.Add(new MenuSeparator("sep3321", "-"));

                laneclearMenu.Add(new MenuBool("useE", "Use E", true));
                laneclearMenu.Add(new MenuSlider("eClear", "Min Minions for E", 3, 1, 10));
                laneclearMenu.Add(new MenuSlider("eMana", "Min Mana for E %", 50, 10));
                RootMenu.Add(laneclearMenu);
            }

            var miscMenu = new Menu("com.kalista.misc", "Misc Options");
            {
                miscMenu.Add(new MenuKeyBind("sentinelDrake", "Send Sentinel to Dragon", Keys.T, KeyBindType.Press));
                miscMenu.Add(new MenuKeyBind("sentinelBaron", "Send Sentinel to Baron", Keys.Y, KeyBindType.Press));
                miscMenu.Add(
                    new MenuList<string>(
                        "stealMode", 
                        "Jungle Steal Mode", 
                        new[] { "Both", "Jungle Minions", "Super / Siege Minions" }));
                RootMenu.Add(miscMenu);
            }

            var fleeMenu = new Menu("com.kalista.flee", "Flee Options");
            {
                fleeMenu.Add(new MenuKeyBind("fleeActive", "Flee Key", Keys.V, KeyBindType.Press));
                fleeMenu.Add(new MenuSeparator("sep", "-"));
                fleeMenu.Add(new MenuBool("wallJump", "Try Wall Jump", true));
                fleeMenu.Add(new MenuBool("aaFlee", "Use Auto Attacks", true));
                RootMenu.Add(fleeMenu);
            }

            var drawingMenu = new Menu("com.kalista.drawing", "Drawing Options");
            {
                drawingMenu.Add(new MenuBool("drawQ", "Draw Q Range"));
                drawingMenu.Add(new MenuBool("drawE", "Draw E Range"));
                RootMenu.Add(drawingMenu);
            }

            RootMenu.Attach();
        }

        #endregion
    }
}