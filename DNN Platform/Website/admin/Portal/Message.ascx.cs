﻿
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Common.Controls
{
    public partial class Message : PortalModuleBase
    {
        private void InitializeComponent()
        {
            this.ID = "Message";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.InitializeComponent();
        }
    }
}
