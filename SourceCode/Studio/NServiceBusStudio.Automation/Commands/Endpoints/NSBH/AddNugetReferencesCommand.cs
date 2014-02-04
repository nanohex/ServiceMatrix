﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Composition;
using NServiceBusStudio.Automation.Extensions;
using System.ComponentModel.DataAnnotations;
using NuPattern.Runtime;
using AbstractEndpoint;
using NServiceBusStudio;
using NuGet.VisualStudio;
using NuPattern.VisualStudio.Solution;
using NuPattern.VisualStudio;

namespace NServiceBusStudio.Automation.Commands.Endpoints.NSBH
{
    [DisplayName("Add Nuget Project References")]
    [Description("Add references in the Endpoint Project to the required nuget projects")]
    [CLSCompliant(false)]
    public class AddNugetReferencesCommand : NuPattern.Runtime.Command
    {
        /// <summary>
        /// Gets or sets the current element.
        /// </summary>
        [Required]
        [Import(AllowDefault = true)]
        public IProductElement CurrentElement
        {
            get;
            set;
        }

        [Import]
        public IVsPackageInstaller VsPackageInstaller { get; set; }

        [Import]
        public IStatusBar StatusBar { get; set; }

        public bool IgnoreHost { get; set; }

        public override void Execute()
        {
            var project = this.CurrentElement.GetProject();
            if (project == null)
            {
                return;
            }

            //<Reference Include="NServiceBus" />
            //<Reference Include="NServiceBus.Core" />
            //<Reference Include="NServiceBus.Host" />

            if (!project.HasReference("NServiceBus"))
            {
                project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "NServiceBus.Interfaces", "4.3.3");
                project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "NServiceBus", "4.3.3");

                if (!this.IgnoreHost)
                {
                    project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "NServiceBus.Host", "4.3.3");
                }
            }

            var app = this.CurrentElement.Root.As<IApplication>();

            //<Reference Include="NServiceBus.ActiveMQ" />
            if (app.Transport == TransportType.ActiveMQ.ToString()) 
            {
                if (!project.HasReference("NServiceBus.ActiveMQ"))
                {
                    project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "NServiceBus.ActiveMQ", "1.0.5");
                }
            }
            else 
            {

                project.RemoveReference ("Apache.NMS");
                project.RemoveReference ("Apache.NMS.ActiveMQ");
                project.RemoveReference ("NServiceBus.ActiveMQ");
            }

            //<Reference Include="NServiceBus.Transports.RabbitMQ" />
            if (app.Transport == TransportType.RabbitMQ.ToString())
            {
                if (!project.HasReference("NServiceBus.RabbitMQ"))
                {
                    project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "NServiceBus.RabbitMQ", "1.1.0");
                }
            }
            else
            {
                project.RemoveReference("RabbitMQ.Client");
                project.RemoveReference("NServiceBus.Transports.RabbitMQ");
            }

            //<Reference Include="NServiceBus.SqlServer" />
            if (app.Transport == TransportType.SqlServer.ToString())
            {
                if (!project.HasReference("NServiceBus.SqlServer"))
                {
                    project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "NServiceBus.SqlServer", "1.1.0");
                }
            }
            else
            {
                project.RemoveReference("NServiceBus.Transports.SQLServer");
            }

            //<Reference Include="NServiceBus.Azure.Transports.WindowsAzureStorageQueues" />
            if (app.Transport == TransportType.AzureQueues.ToString())
            {
                if (!project.HasReference("NServiceBus.Azure.Transports.WindowsAzureStorageQueues"))
                {
                    project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "NServiceBus.Azure.Transports.WindowsAzureStorageQueues", "5.0.0");
                }
            }
            else
            {
                project.RemoveReference("NServiceBus.Azure.Transports.WindowsAzureStorageQueues");
            }

            //<Reference Include="NServiceBus.Azure.Transports.WindowsAzureServiceBus" />
            if (app.Transport == TransportType.AzureServiceBus.ToString())
            {
                if (!project.HasReference("NServiceBus.Azure.Transports.WindowsAzureServiceBus"))
                {
                    project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "NServiceBus.Azure.Transports.WindowsAzureServiceBus", "5.0.0");
                }
            }
            else
            {
                project.RemoveReference("NServiceBus.Azure.Transports.WindowsAzureServiceBus");
            }

            //<Reference Include="ServiceControl.Plugin.DebugSession" />
            //<Reference Include="ServiceControl.Plugin.Heartbeat" />
            //<Reference Include="ServiceControl.Plugin.CustomChecks" />
            if (!String.IsNullOrEmpty(app.ServiceControlInstanceURI))
            {
                if (!project.HasReference("ServiceControl.Plugin.DebugSession"))
                {
                    project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "ServiceControl.Plugin.DebugSession", "1.0.0");
                }

                if (!project.HasReference("ServiceControl.Plugin.Heartbeat"))
                {
                    project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "ServiceControl.Plugin.Heartbeat", "1.0.0");
                }

                if (!project.HasReference("ServiceControl.Plugin.CustomChecks"))
                {
                    project.InstallNuGetPackage(VsPackageInstaller, StatusBar, "ServiceControl.Plugin.CustomChecks", "1.0.0");
                }
            }
            else
            {
                project.RemoveReference("ServiceControl.Plugin.DebugSession");
                project.RemoveReference("ServiceControl.Plugin.Heartbeat");
                project.RemoveReference("ServiceControl.Plugin.CustomChecks");
            }
        }
    }
}
