// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VectorHelper.cs" company="LeagueSharp">
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
//   The Vector Helper, Credits Hell sing
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.SDK.Core.Extensions;
    using LeagueSharp.SDK.Core.Extensions.SharpDX;
    using LeagueSharp.SDK.Core.Utils;

    using SharpDX;

    /// <summary>
    ///     The Vector Helper, Credits Hell sing
    /// </summary>
    internal class VectorHelper
    {
        #region Static Fields

        /// <summary>
        ///     TODO The player.
        /// </summary>
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     TODO The get dash objects.
        /// </summary>
        /// <param name="predefinedObjectList">
        ///     TODO The predefined object list.
        /// </param>
        /// <returns>
        ///     a list of objects attackable
        /// </returns>
        public static List<Obj_AI_Base> GetDashObjects(IEnumerable<Obj_AI_Base> predefinedObjectList = null)
        {
            var objects = predefinedObjectList != null
                              ? predefinedObjectList.ToList()
                              : ObjectManager.Get<Obj_AI_Base>()
                                    .Where(o => o.IsValidTarget(o.GetRealAutoAttackRange()))
                                    .ToList();

            var apexPoint = player.ServerPosition.ToVector2()
                            + (player.ServerPosition.ToVector2() - Game.CursorPos.ToVector2()).Normalized()
                            * player.GetRealAutoAttackRange();

            return
                objects.Where(
                    o =>
                    IsLyingInCone(o.ServerPosition.ToVector2(), apexPoint, player.ServerPosition.ToVector2(), Math.PI))
                    .OrderBy(o => o.Distance(apexPoint))
                    .ToList();
        }

        /// <summary>
        ///     TODO The get first wall point.
        /// </summary>
        /// <param name="from">
        ///     TODO The from.
        /// </param>
        /// <param name="to">
        ///     TODO The to.
        /// </param>
        /// <param name="step">
        ///     TODO The step.
        /// </param>
        /// <returns>
        /// </returns>
        public static Vector2? GetFirstWallPoint(Vector3 from, Vector3 to, float step = 25)
        {
            return GetFirstWallPoint(from.ToVector2(), to.ToVector2(), step);
        }

        /// <summary>
        ///     TODO The get first wall point.
        /// </summary>
        /// <param name="from">
        ///     TODO The from.
        /// </param>
        /// <param name="to">
        ///     TODO The to.
        /// </param>
        /// <param name="step">
        ///     TODO The step.
        /// </param>
        /// <returns>
        /// </returns>
        public static Vector2? GetFirstWallPoint(Vector2 from, Vector2 to, float step = 25)
        {
            var direction = (to - from).Normalized();

            for (float d = 0; d < from.Distance(to); d = d + step)
            {
                var testPoint = from + d * direction;
                var flags = NavMesh.GetCollisionFlags(testPoint.X, testPoint.Y);
                if (flags.HasFlag(CollisionFlags.Wall) || flags.HasFlag(CollisionFlags.Building))
                {
                    return from + (d - step) * direction;
                }
            }

            return null;
        }

        /// <summary>
        ///     TODO The is lying in cone.
        /// </summary>
        /// <param name="position">
        ///     TODO The position.
        /// </param>
        /// <param name="apexPoint">
        ///     TODO The apex point.
        /// </param>
        /// <param name="circleCenter">
        ///     TODO The circle center.
        /// </param>
        /// <param name="aperture">
        ///     TODO The aperture.
        /// </param>
        /// <returns>
        /// </returns>
        public static bool IsLyingInCone(Vector2 position, Vector2 apexPoint, Vector2 circleCenter, double aperture)
        {
            // This is for our convenience
            var halfAperture = aperture / 2;

            // Vector pointing to X point from apex
            var apexToXVect = apexPoint - position;

            // Vector pointing from apex to circle-center point.
            var axisVect = apexPoint - circleCenter;

            // X is lying in cone only if it's lying in 
            // infinite version of its cone -- that is, 
            // not limited by "round basement".
            // We'll use dotProd() to 
            // determine angle between apexToXVect and axis.
            var isInInfiniteCone = DotProd(apexToXVect, axisVect) / Magn(apexToXVect) / Magn(axisVect) >

                                   // We can safely compare cos() of angles 
                                   // between vectors instead of bare angles.
                                   Math.Cos(halfAperture);

            if (!isInInfiniteCone)
            {
                return false;
            }

            // X is contained in cone only if projection of apexToXVect to axis
            // is shorter than axis. 
            // We'll use dotProd() to figure projection length.
            var isUnderRoundCap = DotProd(apexToXVect, axisVect) / Magn(axisVect) < Magn(axisVect);

            return isUnderRoundCap;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     TODO The dot prod.
        /// </summary>
        /// <param name="a">
        ///     TODO The a.
        /// </param>
        /// <param name="b">
        ///     TODO The b.
        /// </param>
        /// <returns>
        /// </returns>
        private static float DotProd(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        /// <summary>
        ///     TODO The magn.
        /// </summary>
        /// <param name="a">
        ///     TODO The a.
        /// </param>
        /// <returns>
        /// </returns>
        private static float Magn(Vector2 a)
        {
            return (float)Math.Sqrt(a.X * a.X + a.Y * a.Y);
        }

        #endregion
    }
}