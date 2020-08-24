﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;

    using Microsoft.Extensions.DependencyInjection;

    using INewHostController = DotNetNuke.Abstractions.Entities.Controllers.IHostController;

    /// <inheritdoc />
    public partial class HostController : ComponentBase<IHostController, HostController>, IHostController
    {
        // None of the APIs are deprecated, but the IHostController
        // is deprecated and moved to the abstractions project. When
        // it is time to remove APIs we should remove the parent
        // classes listed here

        private static IHostController currentInstance;

        [Obsolete("Deprecated in 9.7.1. Scheduled for removal in v11.0.0, use DotNetNuke.Abstractions.Controllers.IHostController instead.")]
        public static new IHostController Instance
        {
            get
            {
                if (currentInstance != null)
                {
                    return currentInstance;
                }

                var newHostController = Globals.DependencyProvider.GetRequiredService<INewHostController>();
                return newHostController is IHostController castedController ? currentInstance = castedController : new HostController();
            }
        }

        /// <inheritdoc/>
        [Obsolete("Deprecated in 9.7.1. Scheduled for removal in v11.0.0, use DotNetNuke.Abstractions.Controllers.IHostController instead.")]
        public Dictionary<string, ConfigurationSetting> GetSettings() =>
            (this as INewHostController).GetSettings()
                .Where(setting => setting.Value is ConfigurationSetting)
                .Select(setting => new KeyValuePair<string, ConfigurationSetting>(setting.Key, (ConfigurationSetting)setting.Value))
                .ToDictionary(setting => setting.Key, setting => setting.Value);

        /// <inheritdoc/>
        [Obsolete("Deprecated in 9.7.1. Scheduled for removal in v11.0.0, use DotNetNuke.Abstractions.Controllers.IHostController instead.")]
        public void Update(ConfigurationSetting config) =>
            (this as INewHostController).Update(config);

        /// <inheritdoc/>
        [Obsolete("Deprecated in 9.7.1. Scheduled for removal in v11.0.0, use DotNetNuke.Abstractions.Controllers.IHostController instead.")]
        public void Update(ConfigurationSetting config, bool clearCache) =>
            (this as INewHostController).Update(config, clearCache);
    }
}
