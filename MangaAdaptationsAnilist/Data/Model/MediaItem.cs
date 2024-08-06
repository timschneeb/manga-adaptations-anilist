using System;
using System.Linq;
using System.Text.Json.Serialization;
using MangaAdaptationsAnilist.GraphQl;

namespace MangaAdaptationsAnilist.Data.Model
{
    [Serializable]
    [method: JsonConstructor]
    public class MediaItem(
        long id,
        string title,
        MediaType type,
        string[] genres,
        long averageScore,
        string thumbnailUrl)
    {
        public long Id { set; get; } = id;
        public string Title { set; get; } = title;
        public MediaType Type { set; get; } = type;
        public string[] Genres { set; get; } = genres;
        public long AverageScore { set; get; } = averageScore;
        public string ThumbnailUrl { set; get; } = thumbnailUrl;

        public string Url(MediaType? type = null)
        {
            return $"https://anilist.co/{(type ?? Type).ToString().ToLowerInvariant()}/{Id.ToString()}";
        }

        public MediaItem(Media media) :
            this(media.Id, media.Title.English ?? media.Title.Romaji, media.Type, media.Genres.ToArray(), 
                media.AverageScore ?? -1, media.CoverImage.Large) {}
    }
}