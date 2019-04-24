﻿using System;
using Newtonsoft.Json;

namespace Skybrud.Umbraco.Spa.Models {

    /// <summary>
    /// Class representing a SPA data model - the main object returned by a SPA <c>GetData</c> response.
    /// </summary>
    public class SpaDataModel {

        #region Properties

        /// <summary>
        /// Gets the ID of the current page.
        /// </summary>
        [JsonProperty("pageId", Order = -99)]
        public int PageId { get; set; }

        /// <summary>
        /// Gets the ID of the current site.
        /// </summary>
        [JsonProperty("siteId", Order = -98)]
        public int SiteId { get; set; }

        /// <summary>
        /// Gets the current content GUID. When the frontend detects that this value has changed, it should do a page refresh.
        /// </summary>
        [JsonProperty("contentGuid", Order = -97)]
        public Guid ContentGuid { get; set; }

        /// <summary>
        /// Gets or sets the execution time - that is the time used for processing the SPA request and initializing the
        /// data model. The time is specified in milliseconds.
        /// </summary>
        [JsonProperty("executeTimeMs", Order = -96)]
        public long ExecuteTimeMs { get; set; }

        /// <summary>
        /// Gets or sets whether the current data model was loaded from the cache.
        /// </summary>
        [JsonProperty("cached", Order = -95, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsCached { get; set; }

        /// <summary>
        /// Gets a reference to the <see cref="SpaSiteModel"/> representing the site. This property will only be
        /// initialized if <see cref="SpaApiPart.Site"/> was specified in the request arguments.
        /// </summary>
        [JsonProperty("site", NullValueHandling = NullValueHandling.Ignore)]
        public SpaSiteModel Site { get; set; }

        /// <summary>
        /// Gets a reference to the <see cref="SpaNavigationModel"/> representing the navigation. This property will
        /// only be initialized if <see cref="SpaApiPart.Navigation"/> was specified in the request arguments.
        /// </summary>
        [JsonProperty("navigation", NullValueHandling = NullValueHandling.Ignore)]
        public SpaNavigationModel Navigation { get; set; }

        /// <summary>
        /// Gets a reference to the <see cref="SpaContentModel"/> representing the current page. This property will
        /// only be initialized if <see cref="SpaApiPart.Content"/> was specified in the request arguments.
        /// </summary>
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public SpaContentModel Content { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance with default options.
        /// </summary>
        public SpaDataModel() {
            ContentGuid = SpaEnvironment.ContentGuid;
            ExecuteTimeMs = -1;
        }

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="request"/>.
        /// </summary>
        /// <param name="request">An instance of <see cref="SpaRequest"/>.</param>
        public SpaDataModel(SpaRequest request) {

            PageId = request.Content?.Id ?? -1;
            SiteId = request.Site?.Id ?? -1;
            ContentGuid = SpaEnvironment.ContentGuid;

            ExecuteTimeMs = -1;

            if (request.Arguments.Parts.Contains(SpaApiPart.Site)) {
                Site = request.SiteModel;
            }

            if (request.Arguments.Parts.Contains(SpaApiPart.Content)) {
                Content = request.ContentModel;
            }

        }

        #endregion

    }

}