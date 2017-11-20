namespace Oxide.Ext.Discord.DiscordObjects
{
    using System.Collections.Generic;

    public class Embed
    {
        public class Thumbnail
        {
            public string url { get; set; }

            public string proxy_url { get; set; }

            public int? height { get; set; }

            public int? width { get; set; }
        }

        public class Video
        {
            public string url { get; set; }

            public int? height { get; set; }

            public int? width { get; set; }
        }

        public class Image
        {
            public string url { get; set; }

            public string proxy_url { get; set; }

            public int? height { get; set; }

            public int? width { get; set; }
        }

        public class Provider
        {
            public string name { get; set; }

            public string url { get; set; }
        }

        public class Author
        {
            public string name { get; set; }

            public string url { get; set; }

            public string icon_url { get; set; }

            public string proxy_icon_url { get; set; }
        }

        public class Footer
        {
            public string text { get; set; }

            public string icon_url { get; set; }

            public string proxy_icon_url { get; set; }
        }

        public class Field
        {
            public string name { get; set; }

            public string value { get; set; }

            public bool inline { get; set; }
        }

        public string title { get; set; }

        public string type { get; set; }

        public string description { get; set; }

        public string url { get; set; }

        public string timestamp { get; set; }

        public int? color { get; set; }

        public Footer footer { get; set; }

        public Image image { get; set; }

        public Thumbnail thumbnail { get; set; }

        public Video video { get; set; }

        public Provider provider { get; set; }

        public Author author { get; set; }

        public List<Field> fields { get; set; }
    }
}
