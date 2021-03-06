﻿//-----------------------------------------------------------------------
// <copyright file="WebApiAssemblyToSwaggerGeneratorSettings.cs" company="NSwag">
//     Copyright (c) Rico Suter. All rights reserved.
// </copyright>
// <license>https://github.com/NSwag/NSwag/blob/master/LICENSE.md</license>
// <author>Rico Suter, mail@rsuter.com</author>
//-----------------------------------------------------------------------

using NSwag.SwaggerGeneration;
using NSwag.SwaggerGeneration.WebApi;

namespace NSwag.SwaggerGeneration.WebApi
{
    /// <summary>Settings for the WebApiAssemblyToSwaggerGenerator.</summary>
    public class WebApiAssemblyToSwaggerGeneratorSettings : WebApiToSwaggerGeneratorSettings, IAssemblySettings
    {
        /// <summary>Initializes a new instance of the <see cref="WebApiAssemblyToSwaggerGeneratorSettings"/> class.</summary>
        public WebApiAssemblyToSwaggerGeneratorSettings()
        {
            AssemblySettings = new AssemblySettings();
        }

        /// <summary>Gets or sets the Web API assembly paths.</summary>
        public AssemblySettings AssemblySettings { get; }
    }
}