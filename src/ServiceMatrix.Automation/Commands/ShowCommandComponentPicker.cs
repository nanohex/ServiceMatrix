﻿using AbstractEndpoint;
using NuPattern.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBusStudio.Automation.Dialog;
using System.Windows.Input;
using NuPattern.Presentation;
using NuPattern;

namespace NServiceBusStudio.Automation.Commands
{
    public class ShowCommandComponentPicker : NuPattern.Runtime.Command
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

        /// <summary>
        /// Gets or sets the Window Factory, used to create a Window Dialog.
        /// </summary>
        [Required]
        [Import(AllowDefault = true)]
        private IDialogWindowFactory WindowFactory
        {
            get;
            set;
        }

        public override void Execute()
        {
            var endpoint = this.CurrentElement.As<IAbstractEndpoint>();

            // Verify all [Required] and [Import]ed properties have valid values.
            this.ValidateObject();

            var app = this.CurrentElement.Root.As<IApplication>();
            
            // Get available commands
            var elements = new Dictionary<string, ICollection<string>>();
            foreach (var service in app.Design.Services.Service)
            {
                elements.Add(service.InstanceName, service.Contract.Commands.Command.Select(x => x.InstanceName).ToList());
            }

            var picker = WindowFactory.CreateDialog<ElementHierarchyPicker>() as IElementHierarchyPicker;
            
            picker.SlaveName = "Command Name:";
            picker.Elements = elements;
            picker.Title = "Send Command";

            using (new MouseCursor(Cursors.Arrow))
            {
                if (picker.ShowDialog().Value)
                {
                    var selectedService = picker.SelectedMasterItem;
                    var selectedCommand = picker.SelectedSlaveItem;
                   
                    var service = app.Design.Services.Service.FirstOrDefault(x => x.InstanceName == selectedService);
                    if (service == null)
                    {
                        service = app.Design.Services.CreateService(selectedService);
                    }

                    var command = service.Contract.Commands.Command.FirstOrDefault(x => x.InstanceName == selectedCommand);
                    if (command == null)
                    {
                        command = service.Contract.Commands.CreateCommand(selectedCommand);
                    }

                    var component = service.Components.Component.FirstOrDefault(x => x.Publishes.CommandLinks.Any(y => y.CommandReference.Value == command));
                    if (component == null)
                    {
                        var deployToEndpoint = default(EventHandler);

                        deployToEndpoint = new EventHandler((s, e) =>
                        {
                            var c = s as IComponent;
                            if (c.InstanceName == selectedCommand + "Sender")
                            {
                                c.DeployTo(endpoint);
                                app.OnInstantiatedComponent -= deployToEndpoint;
                            }
                        });

                        app.OnInstantiatedComponent += deployToEndpoint;
                    }
                    else
                    {
                        component.DeployTo(endpoint);
                    }
                }
            }
        }
    }
}
