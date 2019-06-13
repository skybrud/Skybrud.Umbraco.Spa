namespace Skybrud.Umbraco.Spa.Models.Meta.Twitter  {

    public interface ITwitterCard {

        /// <summary>
        /// Gets the type of the card - eg. <c>summary_large_image</c>.
        /// </summary>
        string Card { get; }

        string Site { get; }

        string Creator { get; }

        SpaMetaContent[] ToMeta();

    }

}