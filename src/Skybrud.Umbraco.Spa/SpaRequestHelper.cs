﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Skybrud.Essentials.Strings.Extensions;
using Skybrud.Umbraco.Redirects.Models;
using Skybrud.Umbraco.Spa.Exceptions;
using Skybrud.Umbraco.Spa.Json.Converters;
using Skybrud.Umbraco.Spa.Models;
using Skybrud.Umbraco.Spa.Models.Flow;
using Skybrud.Umbraco.Spa.Repositories;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;

namespace Skybrud.Umbraco.Spa  {

    /// <summary>
    /// Helper class used for handling a SPA request.
    /// </summary>
    public partial class SpaRequestHelper {

        #region Properties

        /// <summary>
        /// Gets a reference to the current Umbraco context.
        /// </summary>
        protected UmbracoContext UmbracoContext { get; }

        /// <summary>
        /// Gets a reference to Umbraco's service context.
        /// </summary>
        protected ServiceContext Services { get; }

        /// <summary>
        /// Gets a reference to Umbraco's app caches.
        /// </summary>
        protected AppCaches AppCaches { get; }

        /// <summary>
        /// Gets a reference to the redirects service.
        /// </summary>
        protected IRedirectsService RedirectsService { get; }

        /// <summary>
        /// Gets or sets the JSON converter to be used when serializing the Umbraco grid.
        /// </summary>
        public SpaGridJsonConverterBase GridJsonConverter { get; set; }

        /// <summary>
        /// Gets a reference to Umbraco's logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Gets a reference to the current <see cref="IVariationContextAccessor"/>.
        /// </summary>
        public IVariationContextAccessor VariationContextAccessor { get; }

        /// <summary>
        /// Gets a reference to the current published snapshot.
        /// </summary>
        public IPublishedSnapshot PublishedSnapshot { get; }

        /// <summary>
        /// Gets a reference to the current published snapshot.
        /// </summary>
        public SpaDomainRepository DomainRepository { get; }

        /// <summary>
        /// Gets whether the helper should overwrite the status code of responses returned by the helper. Default is <c>true</c>.
        ///
        /// When enabled, status codes like <see cref="HttpStatusCode.MovedPermanently"/>,
        /// <see cref="HttpStatusCode.TemporaryRedirect"/> and <see cref="HttpStatusCode.NotFound"/> will be set to
        /// <see cref="HttpStatusCode.OK"/>.
        /// </summary>
        public bool OverwriteStatusCodes { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new helper instance.
        /// </summary>
        protected SpaRequestHelper() {
            UmbracoContext = Current.UmbracoContext;
            RedirectsService = Current.Factory.GetInstance<IRedirectsService>();
            Services = Current.Services;
            AppCaches = Current.AppCaches;
            Logger = Current.Logger;
            VariationContextAccessor = Current.VariationContextAccessor;
            PublishedSnapshot = Current.PublishedSnapshot;
            DomainRepository = Current.Factory.GetInstance<SpaDomainRepository>();
            OverwriteStatusCodes = true;
        }


        #endregion

        #region Member methods

        /// <summary>
        /// Virtual method defining the actions groups required for a successful SPA request. 
        /// </summary>
        /// <param name="request">The current SPA request.</param>
        /// <returns>An array of <see cref="SpaActionGroup"/>.</returns>
        protected virtual SpaActionGroup[] GetActionGroups(SpaRequest request) {

            // Declare an array of action groups to be executed. Each group features a lambda
            // expression to determine whether that particular group should be executed
            return new [] {

                // First group - should always be executed
                new SpaActionGroup(
                    r => true,
                    
                    InitArguments,
                    FindDomainAndCulture,
                    UpdateArguments,

                    ReadFromCache

                ),

                // Second group - not executed if we already have a data model (eg. from the cache)
                new SpaActionGroup(

                    // Continue if we don't have a model yet
                    r => r.DataModel == null,

                    InitSite,
                    PostInitSite,

                    PreContentLookup,
                    ContentLookup,
                    PostContentLookup,

                    PreSetupCulture,
                    SetupCulture,
                    PostSetupCulture,

                    InitSiteModel,

                    HandleNotFound,
                    HandleOutboundRedirects,

                    PreInitModels,
                    InitContentModel,
                    InitDataModel,
                    InitNavigationModel,
                    InitCustomModels,
                    PostInitModels,

                    PrePushToCache,
                    PushToCache

                ),

                // Third group - always executed
                new SpaActionGroup(r => true, RaisinInTheSausageEnd),

            };

        }

        /// <summary>
        /// Virtual method responsible for building the SPA response.
        /// </summary>
        /// <param name="request">The current SPA request.</param>
        /// <returns>The response.</returns>
        public virtual HttpResponseMessage GetResponse(SpaRequest request) {

            try {
                
                // Iterate through the different methods in the page flow
                foreach (SpaActionGroup group in GetActionGroups(request)) {

                    // Should the group be executed?
                    if (group.Run(request) == false) continue;

                    // Iterate over and execute the actions of the group
                    foreach (Action<SpaRequest> method in group.Actions) {

                        // Call the current flow method
                        try {
                            method(request);
                        } catch (Exception ex) {
                            throw new SpaActionException(request, group, method.Method, ex);
                        }

                        // Break the loop if we already have a response
                        if (request.Response != null) break;

                    }

                    // Break the loop if we already have a response
                    if (request.Response != null) break;

                }

                // Generate a successful response
                if (request.Response == null && request.DataModel != null) {
                    request.Response = CreateSpaResponse(request.ResponseStatusCode, request.DataModel);
                }

                // Generate a fallback error response
                return request.Response ?? ReturnError("Næh");

            } catch (Exception ex) {

                // A bit of error handling
                request.Response = HandleGetResponseException(request, ex);

                // Return the response if present
                if (request.Response != null) return request.Response;

                throw;

            }

        }

        /// <summary>
        /// Method called when the <see cref="GetResponse"/> method results in an exception. The method can be used for
        /// custom error logging, as well as sending a different error message to the client.
        ///
        /// If the method returns a <see cref="HttpResponseMessage"/>, that response will be returned directly to the
        /// client. If the method returns <c>null</c>, the exception will bubble up through the request pipeline.
        ///
        /// By default, this method will write the exception to the Umbraco log, and if the solution is running i debug
        /// mode, and the Accept header of the request contains <c>text/html</c>, a user friendly HTML error message
        /// will be returned to the user.
        /// </summary>
        /// <param name="request">The current SPA request.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>The response.</returns>
        protected virtual HttpResponseMessage HandleGetResponseException(SpaRequest request, Exception exception) {

            if (exception is SpaActionException ex) {

                Logger.Error<SpaRequestHelper>(
                    exception, "SPA request for scheme {Scheme}, domain {Domain} and URL {Url} failed. Action was {Action}.",
                    request.HttpContext.Request.Url?.Scheme,
                    request.HttpContext.Request.Url?.Host,
                    request.HttpContext.Request.RawUrl,
                    ex.MethodName
                );

            } else {

                Logger.Error<SpaRequestHelper>(
                    exception, "SPA request for scheme {Scheme}, domain {Domain} and URL {Url} failed.",
                    request.HttpContext.Request.Url?.Scheme,
                    request.HttpContext.Request.Url?.Host,
                    request.HttpContext.Request.RawUrl
                );

            }

            if (request.HttpContext.IsDebuggingEnabled && (request.HttpContext.Request.AcceptTypes?.Contains("text/html") ?? false)) {
                return ReturnHtmlError(request, exception);
            }

            return null;

        }

        /// <summary>
        /// Gets the preview ID from the specified <paramref name="url"/>, or <c>0</c> if <paramref name="url"/> doesn't match an Umbraco preview URL.
        /// </summary>
        /// <param name="url">The url.</param>
        /// <param name="result">When this method returns, contains the preview ID of <paramref name="url"/>.</param>
        /// <returns><c>true</c> if <paramref name="url"/> matches a preview URL; otherwise <c>false</c>.</returns>
        public virtual bool TryGetPreviewId(string url, out int result) {

            // Shouldn't be null
            if (url == null) {
                result = 0;
                return false;
            }

            // Not sure the SPA API is hit for this URL
            if (url.Contains("/umbraco/dialogs") && int.TryParse(url.Split('=')[1], out result)) return true;

            // Find the ID using REGEX
            Match match = Regex.Match(url.Split('?')[0].TrimEnd('/'), "^/([0-9]+)\\.aspx$");
            result = match.Success ? match.Groups[1].Value.ToInt32() : 0;

            return result > 0;

        }

        /// <summary>
        /// Protected method which can be used to enforcing a trailing slash at the end of <see cref="SpaRequestOptions.Url"/>.
        ///
        /// If the requested URL doesn't already end with a trailing slash, the user will then be redirected to the correct URL.
        /// </summary>
        /// <param name="request">The SPA request.</param>
        protected virtual void AddTrailingSlash(SpaRequest request) {
            
            if (request.Url == null || request.IsPreview) return;

            // Slit the URL so we don't look at the query string
            string[] url = request.Url.Split('?');

            // Return as the URL already ends with a trailing slash
            if (url[0].EndsWith("/")) return;

            // Append the trailing slash
            url[0] += "/";
            
            // Redirect the user to the correct URL
            request.Response = ReturnRedirect(request, string.Join("?", url));

        }

        /// <summary>
        /// Protected method which can be used to remove a trailing slash at the end of <see cref="SpaRequestOptions.Url"/>.
        ///
        /// If the requested URL ends with trailing slash, the user will then be redirected to the correct URL.
        /// </summary>
        /// <param name="request">The SPA request.</param>
        protected virtual void RemoveTrailingSlash(SpaRequest request) {
            
            if (request.IsPreview) return;

            // Slit the URL so we don't look at the query string
            string[] url = request.Url.Split('?');

            // Return as the URL doesn't end with a trailing slash
            if (!url[0].EndsWith("/")) return;

            // Remove the trailing slash
            url[0] = url[0].Substring(0, url[0].Length - 1);
            
            // Redirect the user to the correct URL
            request.Response = ReturnRedirect(request, string.Join("?", url));

        }

        #endregion

    }

}