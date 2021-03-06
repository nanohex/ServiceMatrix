﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuPattern;
using NuPattern.Runtime;
using AbstractEndpoint;
using NServiceBusStudio.Automation.Extensions;
using NuPattern.VisualStudio.Solution;

namespace NServiceBusStudio.Automation.Infrastructure.Authentication
{
    public class AuthenticationFeature : IInfrastructureFeature
    {
        // This feature requires:
        // 1. A menu "Add Authentication" that generates Infrastructure\Security\Authentication Code.
        // 2. Add references to the infrastructure project from all the endpoints
        // 3. Also, add a new reference when a new endpoint has been instantiated
        // 4. Add a menu option for "Customize Authentication" on each endpoint
        // This should exist only if the endpoint doesn't have custom authentication already.


        // Initialization
        public void Initialize(IApplication app, IProductElement infrastructure, IServiceProvider serviceProvider, IPatternManager patternManager)
        {
            // InitializeAuthenticationValues
            InitializeAuthenticationValues(app, serviceProvider.TryGetService<ISolution>(), serviceProvider);

            // Listen for a new endpoint initialization
            // in order to add the required artifacts (typically menu items)
            app.OnInitializingEndpoint += (s, e) =>
            {
                UpdateEndpointsForAuthentication(app, serviceProvider.TryGetService<ISolution>(), serviceProvider);
            };

            // Listen for endpoint instantiated, in order to generate
            // associated code.
            app.OnInstantiatedEndpoint += (s, e) =>
            {
                GenerateAuthenticationCodeOnEndpoints(app, serviceProvider);
            };

            app.OnApplicationLoaded += (s, e) =>
            {
                GenerateAuthenticationCodeOnEndpoints(app, serviceProvider);
            };
        }

        public static void InitializeAuthenticationValues(IApplication app, ISolution solution, IServiceProvider sp)
        {
            if (app.HasAuthentication)
            {
            //
                var authentication = app.Design.Infrastructure.Security.Authentication;
                var infrastructure = app.Design.Infrastructure;
                var project = Helpers.GenerateInfrastructureProjectIfNeeded(infrastructure, solution);

                if (project != null)
                {
                    authentication.Namespace = project.Data.RootNamespace + ".Security";
                    authentication.CodePath = project.Name + @"\GeneratedCode\Security";
                    authentication.CustomCodePath = project.Name + @"\Security";
                }
            }

            // Update endpoints artifacts for authentication
            UpdateEndpointsForAuthentication(app, solution, sp);
        }

        // Creates if needed and sets MenuItem visibility according to the authentication state
        public static void UpdateEndpointsForAuthentication(IApplication app, ISolution solution, IServiceProvider sp)
        {
            app.SetEndpointsMenuItems("View Authentication Code"
                , (e, a) => ApplyCustomAuthentication(e, a, sp)
                , (e, a) => a.HasAuthentication);
        }

        // Generates code for custom authentication and shows the file on IDE editor
        public static void ApplyCustomAuthentication(IAbstractEndpoint endpoint, IApplication application, IServiceProvider sp)
        {
            var solution = sp.TryGetService<ISolution>();

            // If the custom file doesn't exist... we will generate it
            if (!solution.Find(endpoint.Project.Name + @"\Infrastructure\Authentication.cs").Any())
            {
                application.Design.Infrastructure.Security.Authentication.LocalNamespace = endpoint.Project.Data.RootNamespace
                    + ".Infrastructure";
                application.Design.Infrastructure.Security.Authentication.As<IProductElement>()
                    // We are using Temp as we don't want the generation run on each build
                    .CreateTempGenerateCodeCommand(sp, "Authentication.cs"
                    , endpoint.Project.Name + @"\Infrastructure"
                    , @"t4://extension/a5e9f15b-ad7f-4201-851e-186dd8db3bc9/T/T4/Security/EndpointCustomAuthentication.tt")
                .Execute();
            }
            var item = solution.Find(endpoint.Project.Name + @"\Infrastructure\Authentication.cs").FirstOrDefault();
            if (item != null)
            {
                // Open the file on the IDE Editor
                item.As<EnvDTE.ProjectItem>()
                    .Open(EnvDTE.Constants.vsViewKindCode)
                    .Visible = true;
            }
        }


        public static void GenerateAuthenticationCodeOnEndpoints(IApplication app, IServiceProvider sp)
        {
            if (app.HasAuthentication)
            {
                foreach (var endpoint in app.Design.Endpoints.GetAll())
                {
                    var prefixAuthentication = "AuthenticationEndpointCode" + endpoint.As<IProductElement>().InstanceName;
                    if (!app.Design.Infrastructure.Security.Authentication.AsElement().AutomationExtensions.Any(a => a.Name.StartsWith(prefixAuthentication)))
                    {
                        app.Design.Infrastructure.Security.Authentication.LocalNamespace = endpoint.Project.Data.RootNamespace
                            + ".Infrastructure";
                        app.Design.Infrastructure.Security.Authentication.As<IProductElement>()
                            .CreateTempGenerateCodeCommand(sp, "Authentication.cs"
                            , endpoint.Project.Name + @"\Infrastructure\GeneratedCode"
                            , @"t4://extension/a5e9f15b-ad7f-4201-851e-186dd8db3bc9/T/T4/Security/EndpointAuthentication.tt"
                            , prefixAuthentication)
                        .Execute();
                    }

                    var prefixAuthorize = "AuthorizeOutgoingMessagesEndpointCode" + endpoint.As<IProductElement>().InstanceName;
                    if (!app.Design.Infrastructure.Security.Authentication.AsElement().AutomationExtensions.Any(a => a.Name.StartsWith(prefixAuthorize)))
                    {
                        app.Design.Infrastructure.Security.Authentication.LocalNamespace = endpoint.Project.Data.RootNamespace
                            + ".Infrastructure";
                        app.Design.Infrastructure.Security.Authentication.As<IProductElement>()
                            .CreateTempGenerateCodeCommand(sp, "AuthorizeOutgoingMessages.cs"
                            , endpoint.Project.Name + @"\Infrastructure\GeneratedCode"
                            , @"t4://extension/a5e9f15b-ad7f-4201-851e-186dd8db3bc9/T/T4/Security/EndpointAuthorizeOutgoingMessages.tt"
                            , prefixAuthorize)
                        .Execute();
                    }
                }
            }
        }
    }
}
