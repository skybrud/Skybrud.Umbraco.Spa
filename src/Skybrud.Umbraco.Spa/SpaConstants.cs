using System.Net;

namespace Skybrud.Umbraco.Spa {

    /// <summary>
    /// Static class with various constants related to the SPA.
    /// </summary>
    public static class SpaConstants {

        /// <summary>
        /// Gets a reference to the <strong>418 I'm a teapot</strong> status code as it isn't available in <see cref="HttpStatusCode"/>.
        /// </summary>
        public const HttpStatusCode Teapot = (HttpStatusCode) 418;

    }

}