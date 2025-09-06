using Jellyfin.Plugin.HomeScreenSections.Configuration;
using Jellyfin.Plugin.HomeScreenSections.Helpers;
using Jellyfin.Plugin.HomeScreenSections.Library;
using Jellyfin.Plugin.HomeScreenSections.Model;
using Jellyfin.Plugin.HomeScreenSections.Model.Dto;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Library;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Http;

namespace Jellyfin.Plugin.HomeScreenSections.HomeScreen.Sections
{
    /// <summary>
    /// My Media Small Section.
    /// </summary>
    public class MyMediaSmallSection : IHomeScreenSection
    {
        /// <inheritdoc/>
        public string Section => "MyMediaSmall";

        /// <inheritdoc/>
        public string? DisplayText { get; set; } = "My Media (Small)";

        /// <inheritdoc/>
        public int? Limit => 1;

        /// <inheritdoc/>
        public string? Route => null;

        /// <inheritdoc/>
        public string? AdditionalData { get; set; } = null;

        public object? OriginalPayload => null;
        
        private readonly IUserViewManager m_userViewManager;
        private readonly IUserManager m_userManager;
        private readonly IDtoService m_dtoService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userViewManager">Instance of <see href="IUserViewManager" /> interface.</param>
        /// <param name="userManager">Instance of <see href="IUserManager" /> interface.</param>
        /// <param name="dtoService">Instance of <see href="IDtoService" /> interface.</param>
        public MyMediaSmallSection(IUserViewManager userViewManager,
            IUserManager userManager,
            IDtoService dtoService)
        {
            m_userViewManager = userViewManager;
            m_userManager = userManager;
            m_dtoService = dtoService;
        }

        /// <inheritdoc/>
        public QueryResult<BaseItemDto> GetResults(HomeScreenSectionPayload payload, IQueryCollection queryCollection)
        {
            User? user = m_userManager.GetUserById(payload.UserId);

            if (user == null)
            {
                return new QueryResult<BaseItemDto>();
            }
            
            UserViewQuery query = new UserViewQuery
            {
                User = user,
                IncludeHidden = false
            };

            Folder[]? folders = m_userViewManager.GetUserViews(query);

            DtoOptions dtoOptions = new DtoOptions();
            List<ItemFields> f = new List<ItemFields>
            {
                ItemFields.PrimaryImageAspectRatio,
                ItemFields.DisplayPreferencesId
            };

            dtoOptions.Fields = f.ToArray();

            BaseItemDto[] dtos = folders.Select(i => {
                var dto = m_dtoService.GetBaseItemDto(i, dtoOptions, user);
                AddLibraryInfo(dto);
                return dto;
            }).ToArray();

            return new QueryResult<BaseItemDto>(dtos);
        }

        /// <summary>
        /// Adds library icon and URL to DTO using ProviderIds.
        /// </summary>
        /// <param name="dto">The DTO to add library info to</param>
        private void AddLibraryInfo(BaseItemDto dto)
        {
            var collectionType = dto.CollectionType?.ToString()?.ToLowerInvariant();
            var serverId = dto.ServerId ?? string.Empty;
            
            dto.ProviderIds ??= new Dictionary<string, string>();
            
            var (icon, url) = GetLibraryIconAndUrl(collectionType, dto.Id, serverId);
            dto.ProviderIds["LibraryIcon"] = icon;
            dto.ProviderIds["LibraryUrl"] = url;
        }

        private (string icon, string url) GetLibraryIconAndUrl(string? collectionType, Guid? itemId, string serverId)
        {
            var id = itemId?.ToString("N") ?? string.Empty;
            
            return collectionType switch
            {
                "movies" => ("movie", $"#/movies.html?topParentId={id}&collectionType={collectionType}"),
                "tvshows" => ("tv", $"#/tv.html?topParentId={id}&collectionType={collectionType}"),
                "music" => ("music_note", $"#/music.html?topParentId={id}&collectionType={collectionType}"),
                "books" => ("book", $"#/list.html?parentId={id}&serverId={serverId}"),
                "photos" => ("photo", $"#/list.html?parentId={id}&serverId={serverId}"),
                "homevideos" => ("photo", $"#/homevideos.html?topParentId={id}"),
                "livetv" => ("live_tv", $"#/livetv.html?serverId={serverId}"),
                "trailers" => ("theaters", $"#/list.html?parentId={id}&serverId={serverId}"),
                "musicvideos" => ("music_video", $"#/list.html?parentId={id}&serverId={serverId}"),
                "boxsets" => ("video_library", $"#/list.html?parentId={id}&serverId={serverId}"),
                "playlists" => ("queue", $"#/list.html?parentId={id}&serverId={serverId}"),
                "channels" => ("videocam", $"#/list.html?parentId={id}&serverId={serverId}"),
                null or "" => ("quiz", $"#/list.html?parentId={id}&serverId={serverId}"),
                _ => ("folder", $"#/list.html?parentId={id}&serverId={serverId}")
            };
        }

        /// <inheritdoc/>
        public IHomeScreenSection CreateInstance(Guid? userId, IEnumerable<IHomeScreenSection>? otherInstances = null)
        {
            return this;
        }
        
        public HomeScreenSectionInfo GetInfo()
        {
            return new HomeScreenSectionInfo
            {
                Section = Section,
                DisplayText = DisplayText,
				Info = SectionInfoHelper.CreateOfficialSectionInfo(
					description: "Small media"
				),
                AdditionalData = AdditionalData,
                Route = Route,
                Limit = Limit ?? 1,
                OriginalPayload = OriginalPayload,
                ViewMode = SectionViewMode.Landscape,
                AllowViewModeChange = false
            };
        }

		public virtual IEnumerable<PluginConfigurationOption> GetConfigurationOptions() => Enumerable.Empty<PluginConfigurationOption>();
    }
}
