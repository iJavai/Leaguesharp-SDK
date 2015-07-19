// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FleeModule.cs" company="LeagueSharp">
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
//   The Flee Module
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
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.UI.IMenu.Values;

    using SharpDX;

    /// <summary>
    ///     The Flee Module
    /// </summary>
    internal class FleeModule : IModule
    {
        #region Static Fields

        /// <summary>
        ///     The Flee target position
        /// </summary>
        public static Vector3? fleeTargetPosition;

        /// <summary>
        ///     The Wall Jump initiation time
        /// </summary>
        public static int? wallJumpInitTime;

        /// <summary>
        ///     Check if wall jump possible
        /// </summary>
        public static bool wallJumpPossible = false;

        /// <summary>
        ///     The Wall Jump target position
        /// </summary>
        public static Vector3? wallJumpTarget;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     Gets the module name
        /// </summary>
        /// <returns>
        ///     The <see cref="string" />.
        /// </returns>
        public string GetModuleName()
        {
            return "Flee Module";
        }

        /// <summary>
        ///     The On Module Load Function
        /// </summary>
        public void OnLoad()
        {
            Console.WriteLine("Flee Module Loaded");
        }

        /// <summary>
        ///     The Module Run Function
        /// </summary>
        public void Run()
        {
            var useWalljump = MenuGenerator.RootMenu["com.kalista.flee"]["wallJump"].GetValue<MenuBool>().Value;
            var useAutoAttack = MenuGenerator.RootMenu["com.kalista.flee"]["aaFlee"].GetValue<MenuBool>().Value;

            // A jump has been triggered, move into the set direction and
            // return the function to stop further calculations in the flee code
            if (wallJumpTarget.HasValue)
            {
                // Move to the target
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, wallJumpTarget.Value);

                // This is only to validate when the jump get aborted by, for example, stuns
                if (Environment.TickCount - wallJumpInitTime > 500)
                {
                    wallJumpTarget = null;
                    wallJumpInitTime = null;
                }
                else
                {
                    return;
                }
            }

            // Quick AAing without jumping over walls
            if (useAutoAttack && !useWalljump)
            {
                var dashObjects = VectorHelper.GetDashObjects();
                Orbwalker.Orbwalk(dashObjects.Count > 0 ? dashObjects[0] : null, Game.CursorPos);
            }

            // Wall jumping with possible AAing aswell
            if (useWalljump)
            {
                // We need to define a new move position since jumping over walls
                // requires you to be close to the specified wall. Therefore we set the move
                // point to be that specific piont. People will need to get used to it,
                // but this is how it works.
                var wallCheck = VectorHelper.GetFirstWallPoint(ObjectManager.Player.Position, Game.CursorPos);

                // Be more precise
                if (wallCheck != null)
                {
                    wallCheck = VectorHelper.GetFirstWallPoint((Vector3)wallCheck, Game.CursorPos, 5);
                }

                // Define more position point
                var movePosition = wallCheck != null ? (Vector3)wallCheck : Game.CursorPos;

                // Update fleeTargetPosition
                var tempGrid = NavMesh.WorldToGrid(movePosition.X, movePosition.Y);
                fleeTargetPosition = NavMesh.GridToWorld((short)tempGrid.X, (short)tempGrid.Y);

                // Also check if we want to AA aswell
                Obj_AI_Base target = null;
                if (useAutoAttack)
                {
                    var dashObjects = VectorHelper.GetDashObjects();
                    if (dashObjects.Count > 0)
                    {
                        target = dashObjects[0];
                    }
                }

                // Reset walljump indicators
                wallJumpPossible = false;

                // Only calculate stuff when our Q is up and there is a wall inbetween
                if (SpellManager.Spell[SpellSlot.Q].IsReady() && wallCheck != null)
                {
                    // Get our wall position to calculate from
                    var wallPosition = movePosition;

                    // Check 300 units to the cursor position in a 160 degree cone for a valid non-wall spot
                    var direction = (Game.CursorPos.ToVector2() - wallPosition.ToVector2()).Normalized();
                    const float MaxAngle = 80;
                    const float Step = MaxAngle / 20;
                    float currentAngle = 0;
                    float currentStep = 0;
                    var jumpTriggered = false;
                    while (true)
                    {
                        // Validate the counter, break if no valid spot was found in previous loops
                        if (currentStep > MaxAngle && currentAngle < 0)
                        {
                            break;
                        }

                        // Check next angle
                        if ((currentAngle == 0 || currentAngle < 0) && currentStep != 0)
                        {
                            currentAngle = currentStep * (float)Math.PI / 180;
                            currentStep += Step;
                        }
                        else if (currentAngle > 0)
                        {
                            currentAngle = -currentAngle;
                        }

                        Vector3 checkPoint;

                        // One time only check for direct line of sight without rotating
                        if (currentStep == 0)
                        {
                            currentStep = Step;
                            checkPoint = wallPosition + 300 * direction.ToVector3();
                        }

                        // Rotated check
                        else
                        {
                            checkPoint = wallPosition + 300 * direction.Rotated(currentAngle).ToVector3();
                        }

                        // Check if the point is not a wall
                        if (!checkPoint.IsWall())
                        {
                            // Check if there is a wall between the checkPoint and wallPosition
                            wallCheck = VectorHelper.GetFirstWallPoint(checkPoint, wallPosition);
                            if (wallCheck != null)
                            {
                                // There is a wall inbetween, get the closes point to the wall, as precise as possible
                                var wallPositionOpposite =
                                    (Vector3)VectorHelper.GetFirstWallPoint((Vector3)wallCheck, wallPosition, 5);

                                // Check if it's worth to jump considering the path length
                                if (ObjectManager.Player.GetPath(wallPositionOpposite).ToList().ToVector2().PathLength()
                                    - ObjectManager.Player.Distance(wallPositionOpposite) > 200)
                                {
                                    // Check the distance to the opposite side of the wall
                                    if (ObjectManager.Player.Distance(wallPositionOpposite)
                                        < Math.Pow(300 - ObjectManager.Player.BoundingRadius / 2, 2))
                                    {
                                        // Make the jump happen
                                        wallJumpInitTime = Environment.TickCount;
                                        wallJumpTarget = wallPositionOpposite;
                                        SpellManager.Spell[SpellSlot.Q].Cast(wallPositionOpposite);

                                        // Update jumpTriggered value to not orbwalk now since we want to jump
                                        jumpTriggered = true;

                                        // Break the loop
                                        break;
                                    }

                                    // If we are not able to jump due to the distance, draw the spot to
                                    // make the user notice the possibliy
                                    else
                                    {
                                        // Update indicator values
                                        wallJumpPossible = true;
                                    }
                                }
                            }
                        }
                    }

                    // Check if the loop triggered the jump, if not just orbwalk
                    if (!jumpTriggered)
                    {
                        Orbwalker.Orbwalk(target, movePosition);
                    }
                }
                else
                {
                    // Either no wall or Q on cooldown, just move towards to wall then
                    Orbwalker.Orbwalk(target, movePosition);
                }
            }
        }

        /// <summary>
        ///     Checks if the Module should be loaded
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool ShouldBeLoaded()
        {
            return true;
        }

        /// <summary>
        ///     Checks if the module should run
        /// </summary>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool ShouldRun()
        {
            return MenuGenerator.RootMenu["com.kalista.flee"]["fleeActive"].GetValue<MenuKeyBind>().Active;
        }

        #endregion
    }
}