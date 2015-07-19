// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TickLimiter.cs" company="">
//   
// </copyright>
// <summary>
//   The Tick Limiter
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp.SDK.Core;

    /// <summary>
    ///     The Tick Limiter
    /// </summary>
    internal class TickLimiter
    {
        #region Static Fields

        /// <summary>
        ///     The tick dictionary
        /// </summary>
        public static Dictionary<TickLimiterKey, float> TickDictionary = new Dictionary<TickLimiterKey, float>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The Addition Method
        /// </summary>
        /// <param name="name">
        ///     The name
        /// </param>
        /// <param name="interval">
        ///     The interval.
        /// </param>
        public static void Add(string name, float interval)
        {
            TickDictionary.Add(new TickLimiterKey { Interval = interval, Name = name }, 0f);
        }

        /// <summary>
        ///     Checks if a module can tick
        /// </summary>
        /// <param name="name">
        ///     The Name
        /// </param>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        public static bool CanTick(string name)
        {
            var currentKey = TickDictionary.Select(k => k.Key).FirstOrDefault(h => h.Name == name);
            if (currentKey == null)
            {
                return false;
            }

            if (!(Variables.TickCount - TickDictionary[currentKey] >= currentKey.Interval))
            {
                return false;
            }

            TickDictionary[currentKey] = Variables.TickCount;
            return true;
        }

        #endregion
    }

    /// <summary>
    ///     The tick limiter key.
    /// </summary>
    internal class TickLimiterKey
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets the interval.
        /// </summary>
        public float Interval { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        #endregion
    }
}