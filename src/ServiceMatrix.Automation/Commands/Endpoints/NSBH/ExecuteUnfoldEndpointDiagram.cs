﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Composition;
using NuPattern.Runtime;
using NuPattern.Runtime.ToolkitInterface;

namespace NServiceBusStudio.Automation.Commands.Endpoints.NSBH
{
    [DisplayName("ExecuteUnfoldEndpointDiagram")]
    [Description("ExecuteUnfoldEndpointDiagram")]
    [CLSCompliant(false)]
    public class ExecuteUnfoldEndpointDiagram : NuPattern.Runtime.Command
    {
        [Required]
        [Import(AllowDefault = true)]
        public IProductElement CurrentElement { get; set; }

        public override void Execute()
        {
            var endpoint = this.CurrentElement.Parent.Parent.Parent;
            var product = endpoint.As<IToolkitInterface>().As<IProductElement>();
            var automation = product.AutomationExtensions.FirstOrDefault(x => x.Name == "UnfoldDiagramFile");
            if (automation != null)
            {
                automation.Execute();
            }
        }
    }
}
