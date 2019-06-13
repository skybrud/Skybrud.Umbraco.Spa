namespace Skybrud.Umbraco.Spa.Models.Meta  {

    public class SpaMetaContent {

        public string Name { get; set; }

        public string Content { get; set; }

        public SpaMetaContent()
        {

        }

        public SpaMetaContent(string name, string content)
        {
            Name = name;
            Content = content;
        }

    }

}