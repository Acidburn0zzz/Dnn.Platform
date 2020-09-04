﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals
{
    using DotNetNuke.Abstractions.Urls;

    /// <summary>
    /// Portal Alias Info interface.
    /// </summary>
    public interface IPortalAliasInfo
    {
        /// <summary>
        /// Gets or sets the http alias.
        /// </summary>
        string HTTPAlias { get; set; }

        /// <summary>
        /// Gets or sets the Portal Alias ID.
        /// </summary>
        int PortalAliasID { get; set; }

        /// <summary>
        /// Gets or sets the Portal ID.
        /// </summary>
        int PortalID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if the portal is the primary portal.
        /// </summary>
        bool IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the poral is a redirect portal.
        /// </summary>
        bool Redirect { get; set; }

        /// <summary>
        /// Gets or sets the Browser Type.
        /// </summary>
        BrowserTypes BrowserType { get; set; }

        /// <summary>
        /// Gets or sets the culture code.
        /// </summary>
        string CultureCode { get; set; }

        /// <summary>
        /// Gets or sets the skin.
        /// </summary>
        string Skin { get; set; }

        /// <summary>
        /// Gets or sets the key ID.
        /// </summary>
        int KeyID { get; set; }
    }
}
