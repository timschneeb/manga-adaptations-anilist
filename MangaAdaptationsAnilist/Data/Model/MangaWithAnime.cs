using System;

namespace MangaAdaptationsAnilist.Data.Model
{
    [Serializable]
    public class MangaWithAnime
    {
        public MediaItem OriginManga { set; get; }
        public MediaItem Anime { set; get; }

        public string GetDescription()
        {
            return $"Source: {OriginManga.Title}";
        }
    }
}