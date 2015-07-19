// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IModule.cs" company="">
//   
// </copyright>
// <summary>
//   The Module Interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace iKalista.Modules
{
    /// <summary>
    ///     The Module Interface
    /// </summary>
    public interface IModule
    {
        #region Public Methods and Operators

        /// <summary>
        ///     Gets the module name
        /// </summary>
        /// <returns>
        ///     <see cref="string" />
        /// </returns>
        string GetModuleName();

        /// <summary>
        ///     The On Module Load Function
        /// </summary>
        void OnLoad();

        /// <summary>
        ///     The Module Run Function
        /// </summary>
        void Run();

        /// <summary>
        ///     Checks if the Module should be loaded
        /// </summary>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        bool ShouldBeLoaded();

        /// <summary>
        ///     Checks if the module should run
        /// </summary>
        /// <returns>
        ///     <see cref="bool" />
        /// </returns>
        bool ShouldRun();

        #endregion
    }
}