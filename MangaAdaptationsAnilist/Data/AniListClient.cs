using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using MangaAdaptationsAnilist.GraphQl;

namespace MangaAdaptationsAnilist.Data
{
    public static class AniListClient
    {
        private static readonly GraphQLHttpClient GraphQlHttpClient;

        static AniListClient()
        {
            var uri = new Uri("https://graphql.anilist.co");
            var graphQlOptions = new GraphQLHttpClientOptions
            {
                EndPoint = uri
            };

            GraphQlHttpClient = new GraphQLHttpClient(graphQlOptions, new NewtonsoftJsonSerializer());
        }

        public static async Task<AniListResponse[]> ExecuteQuery(string username, MediaListStatus?[] statusFilter, Action<long, long>? progress)
        {
            var responses = new List<AniListResponse>();
            var page = 1;
            do
            {
                var query = await ExecutePartialQuery(page, username, statusFilter);
                responses.Add(query);
                progress?.Invoke(query.Page?.PageInfo?.CurrentPage ?? 1, query.Page?.PageInfo?.LastPage ?? 1);
                page++;
            } while (responses.LastOrDefault() != null && (responses.Last().Page?.PageInfo?.HasNextPage ?? false));

            return responses.ToArray();
        }
        
        public static async Task<AniListResponse> ExecutePartialQuery(int page, string username, MediaListStatus?[] statusFilter)
        {
            var builder = new QueryQueryBuilder()
                .WithPage(
                    new PageQueryBuilder()
                        .WithPageInfo(
                            new PageInfoQueryBuilder().WithAllFields())
                        .WithMediaList(
                            new MediaListQueryBuilder()
                                .WithStatus()
                                .WithMedia(
                                    new MediaQueryBuilder()
                                        .WithId()
                                        .WithTitle(
                                            new MediaTitleQueryBuilder()
                                                .WithRomaji()
                                                .WithEnglish())
                                        .WithGenres()
                                        .WithAverageScore()
                                        .WithCoverImage(
                                            new MediaCoverImageQueryBuilder()
                                                .WithLarge())
                                        .WithRelations(
                                            new MediaConnectionQueryBuilder()
                                                .WithNodes( 
                                                    new MediaQueryBuilder()
                                                        .WithId()
                                                        .WithType()
                                                        .WithTitle(
                                                            new MediaTitleQueryBuilder()
                                                                .WithRomaji()
                                                                .WithEnglish())
                                                        .WithTags(
                                                            new MediaTagQueryBuilder().WithName())
                                                        .WithGenres()
                                                        .WithAverageScore()
                                                        .WithDescription(asHtml: true)
                                                        .WithCoverImage(
                                                            new MediaCoverImageQueryBuilder()
                                                                .WithLarge())
                                                )
                                        )),
                            userName: username, type: MediaType.Manga, 
                            statusIn:  new QueryBuilderParameter<IEnumerable<MediaListStatus?>>(statusFilter)),
                    page: page, perPage: 50);
            
            var request = new GraphQLRequest
            {
                Query = builder.Build(), 
                Variables = new Dictionary<string, IEnumerable<string>> {{"status_in", statusFilter.Select(x => x.ToString())}}
            };
            
            var response = await GraphQlHttpClient.SendQueryAsync<object>(request);
            if (response.Errors is {Length: > 0})
            {
                throw new GraphQlException(response.Errors);
            }
            return AniListResponse.FromJson(response.Data.ToString());
        }
    }
}