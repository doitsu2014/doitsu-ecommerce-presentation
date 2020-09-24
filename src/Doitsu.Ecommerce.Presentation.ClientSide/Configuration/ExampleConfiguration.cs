﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevExpress.Blazor.DocumentMetadata;

namespace Doitsu.Ecommerce.Presentation.ClientSide.Configuration
{
    public class ExampleConfiguration
    {
        private readonly SeoConfiguration _seoConfig;
        public IEnumerable<Assembly> AdditionalRoutingAssemblies { get; }
        //public IEnumerable<DemoProductInfo> Products { get; }
        public bool IsReportingModuleLoaded => AdditionalRoutingAssemblies.Any();


        public ExampleConfiguration(IOptions<SeoConfiguration> seoOptions, IConfiguration configuration)
        {
            _seoConfig = seoOptions?.Value;
            if (_seoConfig == null) return;
            if (_seoConfig.RootExamplePages == null)
                _seoConfig = configuration.GetSection("BlazorDemo")?.Get<SeoConfiguration>();
            if (_seoConfig == null) return;

            bool IsReportsDemoModule(Assembly x) => x.GetName().Name == "BlazorDemo.Reporting";
            AdditionalRoutingAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(IsReportsDemoModule).ToArray();
            //Products = _seoConfig.Products ?? new DemoProductInfo[0];
        }

        public void ConfigureMetadata(IDocumentMetadataCollection metadataCollection)
        {
            if (_seoConfig == null) return;

            metadataCollection.AddDefault()
                .Base("~/")
                .Charset("utf-8")
                .Viewport("width=device-width, initial-scale=1.0");

            if (_seoConfig.RootExamplePages != null)
            {
                foreach (var page in _seoConfig.RootExamplePages)
                {
                    ConfigurePage(metadataCollection, page, page.Title, _seoConfig.TitleFormat ?? "{0}");
                }
            }
        }

        static void ConfigurePage(IDocumentMetadataCollection metadataCollection, ExamplePage page, string title, string titleFormat)
        {
            if (page.Url != null)
            {
                metadataCollection.AddPage(page.Url)
                    .OpenGraph("url", page.OG_Url)
                    .OpenGraph("type", page.OG_Type)
                    .OpenGraph("title", page.OG_Title)
                    .OpenGraph("description", page.OG_Description)
                    .OpenGraph("image", page.OG_Image)
                    .Title(string.Format(titleFormat, title))
                    .Meta("description", page.Description);
            }

            if (page.ExamplePages != null)
            {
                foreach (var demoPage in page.ExamplePages)
                    ConfigurePage(metadataCollection, demoPage, string.Join('-', title, demoPage.Title), titleFormat);
            }
        }
    }
}
