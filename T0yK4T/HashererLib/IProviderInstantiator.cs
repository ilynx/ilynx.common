﻿using System.Collections.Generic;
namespace Hasherer
{
    /// <summary>
    /// This interface is used to find external <see cref="AsyncHashProvider"/>s
    /// </summary>
    public interface IProviderInstantiator
    {
        /// <summary>
        /// When implemented, should return a list of <see cref="AsyncHashProvider"/>s
        /// </summary>
        /// <returns></returns>
        AsyncHashProvider Instantiate();

        /// <summary>
        /// Gets a value indicating wether or not the provider generated through <see cref="Instantiate()"/> is enabled by default
        /// </summary>
        bool DefaultEnabled { get; set; }

        /// <summary>
        /// Gets or Sets the displayname of this <see cref="IProviderInstantiator"/>
        /// </summary>
        string DisplayName { get; set; }
    }
}