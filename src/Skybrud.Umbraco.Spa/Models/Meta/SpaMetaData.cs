using System.Collections.Generic;
using Newtonsoft.Json;
using Skybrud.Umbraco.Spa.Json.Converters;
using Skybrud.Umbraco.Spa.Models.Meta.OpenGraph;
using Skybrud.Umbraco.Spa.Models.Meta.Twitter;
using Umbraco.Core.Models;

// ReSharper disable UnusedParameter.Local

namespace Skybrud.Umbraco.Spa.Models.Meta {
    
    /// <summary>
    /// Class representing the meta data and other relevant information of a page. When serialized to JSON, the value is compatible with <strong>vue-meta</strong>.
    /// </summary>
    /// <see>
    ///     <cref>https://github.com/nuxt/vue-meta/tree/1.x</cref>
    /// </see>
    [JsonConverter(typeof(SpaMetaDataJsonConverter))]
    public class SpaMetaData {

        private readonly SpaMetaLink _canonical;

        private SpaOpenGraphProperties _og;
        private ITwitterCard _twitter;

        #region Properties

        /// <summary>
        /// Gets a reference to the <see cref="IPublishedContent"/> representing the current page.
        /// </summary>
        protected IPublishedContent Content { get; }

        /// <summary>
        /// Gets or sets the canonical URL of the current page.
        /// </summary>
        public string Canonical {
            get => _canonical.Href;
            set => _canonical.Href = value;
        }

        /// <summary>
        /// Gets the meta title of the current page.
        /// </summary>
        [JsonProperty("title")]
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets the meta description of the current page.
        /// </summary>
        [JsonProperty("description")]
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets the current page should be hidden in search results.
        /// </summary>
        [JsonIgnore]
        public bool HideFromSearch { get; set; }

        /// <summary>
        /// Gets the robots value for the current page.
        /// </summary>
        [JsonProperty("robots")]
        public string Robots { get; set; }

        /// <summary>
        /// Gets a collection of Open Graph properties for the current page.
        /// </summary>
        public SpaOpenGraphProperties OpenGraph => _og ?? (_og = GetOpenGraph());

        /// <summary>
        /// Gets an instance of <see cref="ITwitterCard"/> representing the current page, or <c>null</c> if the page doesn't have a Twitter card..
        /// </summary>
        public ITwitterCard TwitterCard => _twitter ?? (_twitter = GetTwitterCard());

        /// <summary>
        /// Gets whether the page has a Twitter card.
        /// </summary>
        public bool HasTwitterCard => TwitterCard != null;

        /// <summary>
        /// Gets or sets a list of <c>&lt;link /&gt;</c> elements
        /// </summary>
        public List<SpaMetaLink> Links { get; set; }

        /// <summary>
        /// Gets or sets a list of <c>&lt;script&gt;</c> elements
        /// </summary>
        public List<SpaMetaScript> Scripts { get; set; }

        /// <summary>
        /// Gets the element types where sanitizers should not be used.
        /// </summary>
        /// <see>
        ///     <cref>https://github.com/nuxt/vue-meta/tree/1.x#__dangerouslydisablesanitizers-string</cref>
        /// </see>
        public string[] DangerouslyDisableSanitizers { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance based on the specified <paramref name="site"/> and <paramref name="content"/>.
        /// </summary>
        /// <param name="site">The current site.</param>
        /// <param name="content">The current page.</param>
        public SpaMetaData(SpaSiteModel site, IPublishedContent content) {

            Content = content;

            Links = new List<SpaMetaLink>();
            Scripts = new List<SpaMetaScript>();

            AppendLink(_canonical = new SpaMetaLink { Rel = "canonical" });

            DangerouslyDisableSanitizers = Scripts == null || Scripts.Count == 0 ? new string[0] : new[] {"script"};

        }

        #endregion

        #region Member methods

        /// <summary>
        /// Gets an instance of <see cref="SpaOpenGraphProperties"/> representing the Open Graph properties of the current page.
        /// </summary>
        /// <returns>An instance of <see cref="SpaOpenGraphProperties"/>, or <c>null</c> it not available.</returns>
        protected virtual SpaOpenGraphProperties GetOpenGraph() {
            return new SpaOpenGraphProperties(Content);
        }

        /// <summary>
        /// Gets the Twitter card of the current page. This method will return <c>null</c> by default, but can be overriden by subclasses.
        /// </summary>
        /// <returns>An instance of <see cref="ITwitterCard"/>, or <c>null</c> it not available.</returns>
        protected virtual ITwitterCard GetTwitterCard() {
            return null;
        }

        /// <summary>
        /// Appends a new <c>&lt;link /&gt;</c> element based on the specified parameters.
        /// </summary>
        /// <param name="href">The URL / value of the <c>href</c> attribute.</param>
        /// <param name="rel">The value of the <c>rel</c> attribute.</param>
        /// <param name="type">The value of the <c>type</c> attribute.</param>
        /// <param name="media">The value of the <c>media</c> attribute.</param>
        /// <param name="sizes">The value of the <c>sizes</c> attribute.</param>
        /// <returns></returns>
        public SpaMetaLink AppendLink(string href, string rel = null, string type = null, string media = null, string sizes = null) {
            return AppendLink(new SpaMetaLink {
                Href = href,
                Rel = rel,
                Type = type,
                Media = media,
                Sizes = sizes
            });
        }

        /// <summary>
        /// Appends the specified <paramref name="link"/>.
        /// </summary>
        /// <param name="link">An instance of <see cref="SpaMetaLink"/> representing the <c>&lt;link /&gt;</c> element.</param>
        /// <returns>The added link.</returns>
        public SpaMetaLink AppendLink(SpaMetaLink link) {
            if (Links == null) Links = new List<SpaMetaLink>();
            Links.Add(link);
            return link;
        }

        /// <summary>
        /// Appends a new <c>script</c> element based on the specified parameters.
        /// </summary>
        /// <param name="source">The value of the <c>src</c> attribute.</param>
        /// <param name="type">The value of the <c>type</c> attribute.</param>
        /// <returns></returns>
        public SpaMetaScript AppendScript(string source, string type = null) {
            return AppendScript(new SpaMetaScript {
                Source = source,
                Type = type
            });
        }

        /// <summary>
        /// Appends the specified <paramref name="script"/>.
        /// </summary>
        /// <param name="script">An instance of <see cref="script"/> representing the <c>&lt;script&gt;</c> element.</param>
        /// <returns>The added script.</returns>
        public SpaMetaScript AppendScript(SpaMetaScript script) {
            Scripts.Add(script);
            return script;
        }

        #endregion

    }

}