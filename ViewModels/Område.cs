using System.Collections.Generic;

namespace GlobalPopup.AnmeldFejl
{
    public class Område
    {
        public string OmrådeNavn
        {
            get;
            set;
        }

        public ICollection<Kategori> Kategorier
        {
            get;
            set;
        }
    }

    public class Kategori
    {
        public string KategoriNavn
        {
            get;
            set;
        }

        public Område Område
        {
            get;
            set;
        }
    }
}