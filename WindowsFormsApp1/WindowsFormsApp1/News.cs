using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsFormsApp1
{
    class News
    {
        private String title;
        private String authors;
        private String text;
        private String urlImage;
        private String tag;

        public string Title { get => title; set => title = value; }
        public string Authors { get => authors; set => authors = value; }
        public string Text { get => text; set => text = value; }
        public string UrlImage { get => urlImage; set => urlImage = value; }
        public string Tag { get => tag; set => tag = value; }
    }
}
