using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Autofac;
using FunctionsInjection.Services;
using Microsoft.Extensions.Configuration;

namespace FunctionsInjection.Infrastructure
{
    /// <summary>
    ///   Provides a means to create or access a dependency resolver suitable for use with Azure Functions.
    /// </summary>
    /// 
    public static class AzureFunctionDependencyResolver
    {
        /// <summary>
        ///   Creates a container to be used for resolving dependencies.
        /// </summary>
        /// 
        /// <param name="configurationBasePath">The base path to use as the root of file-based configuration paths; if <c>null</c> or <c>String.Empty</c>, no base path will be used.</param>
        /// <param name="jsonSettingsFilePath">The JSON file containing the function configuration settings; if <c>null</c> or <c>String.Empty</c>, no base path will be used.</param>
        /// 
        /// <returns>A dependency injection container, suitable for caching or singleton access, that can be used to resolve dependencies.</returns>
        /// 
        public static IContainer CreateContainer(string configurationBasePath = null,
                                                 string jsonSettingsFilePath  = null)
        {           
            var builder = new ContainerBuilder();            

             builder                
                 .RegisterInstance(AzureFunctionDependencyResolver.CreateHttpClient())
                 .AsSelf()
                 .SingleInstance();

             builder
                 .Register<ApplicationConfiguration>(context => AzureFunctionDependencyResolver.BuildConfiguration(configurationBasePath, jsonSettingsFilePath))
                 .AsSelf()
                 .InstancePerLifetimeScope();

             builder
                 .RegisterType<ProductService>()
                 .AsImplementedInterfaces()
                 .InstancePerLifetimeScope();

             builder
                 .RegisterType<UserService>()
                 .AsImplementedInterfaces()
                 .InstancePerLifetimeScope();

             builder
                 .RegisterType<ReviewService>()
                 .AsImplementedInterfaces()
                 .InstancePerLifetimeScope();
                 
             return builder.Build();
        }

        /// <summary>
        ///   Creates an HTTP client instance to use as a dependency.
        /// </summary>
        /// 
        /// <returns>An <see cref="System.Net.Http.HttpClient" /> instance suitable for caching or use as a singleton.</returns>
        /// 
        private static HttpClient CreateHttpClient()
        {
             var assemblyName = typeof(AzureFunctionDependencyResolver).Assembly.GetName(); 

             var httpClient = new HttpClient
             {
                DefaultRequestHeaders = { UserAgent = { new ProductInfoHeaderValue(assemblyName.Name, assemblyName.Version.ToString()) }}
              
             };

             httpClient.DefaultRequestHeaders.Accept.Clear();
             httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaTypeFormatter.DefaultMediaType.MediaType));

             return httpClient;
        }

        /// <summary>
        ///   Builds the application configuration, using the provided values to control inclusion of file-based
        ///   settings.
        /// </summary>
        /// 
        /// <param name="basePath">The base path to use as the root of file-based configuration paths; if <c>null</c> or <c>String.Empty</c>, no base path will be used.</param>
        /// <param name="jsonSettingsFilePath">The JSON file containing the function configuration settings; if <c>null</c> or <c>String.Empty</c>, no base path will be used.</param>
        /// 
        /// <returns>The application configuration based on the discovered configuration settings.</returns>
        /// 
        private static ApplicationConfiguration BuildConfiguration(string basePath,
                                                                   string jsonSettingsFilePath)
        {
            var builder = new ConfigurationBuilder()                
                .AddEnvironmentVariables();

            if (!String.IsNullOrEmpty(basePath))
            {
                builder.SetBasePath(basePath);
            }

            if (!String.IsNullOrEmpty(jsonSettingsFilePath))
            {
                builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
            }

            return new ApplicationConfiguration(builder.Build());
        }
    }
}
