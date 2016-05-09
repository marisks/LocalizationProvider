using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Framework.Localization;

namespace DbLocalizationProvider
{
    public class CachedLocalizationResourceRepository : ILocalizationResourceRepository
    {
        private const string CacheKeyPrefix = "DbLocalizationProviderCache";
        private readonly LocalizationResourceRepository _repository;

        public CachedLocalizationResourceRepository(LocalizationResourceRepository repository)
        {
            if(repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            _repository = repository;
        }

        public string GetTranslation(string key, CultureInfo language)
        {
            var cacheKey = BuildCacheKey(key);
            var localizationResource = GetFromCache(cacheKey);
            if(localizationResource != null)
            {
                // if value for the cache key is null - this is non-existing resource (no hit)
                return localizationResource.Translations?.FirstOrDefault(t => t.Language == language.Name)?.Value;
            }

            var resource = _repository.GetResource(key);
            LocalizationResourceTranslation localization = null;
            if(resource == null)
            {
                // create empty null resource 0 to indicate non-existing resource
                resource = LocalizationResource.CreateNonExisting(key);
            }
            else
            {
                localization = resource.Translations.FirstOrDefault(t => t.Language == language.Name);
            }

            CacheManager.Insert(cacheKey, resource);
            return localization?.Value;
        }

        public IEnumerable<CultureInfo> GetAvailableLanguages()
        {
            var cacheKey = BuildCacheKey("AvailableLanguages");
            var cachedLanguages = CacheManager.Get(cacheKey) as IEnumerable<CultureInfo>;
            if(cachedLanguages != null)
            {
                return cachedLanguages;
            }

            var languages = _repository.GetAvailableLanguages();
            CacheManager.Insert(cacheKey, languages);

            return languages;
        }

        public IEnumerable<LocalizationResource> GetAllResources()
        {
            return _repository.GetAllResources();
        }

        public IEnumerable<ResourceItem> GetAllTranslations(string key, CultureInfo language)
        {
            return _repository.GetAllTranslations(key, language);
        }

        public void CreateResource(string key, string username)
        {
            _repository.CreateResource(key, username);
        }

        public void DeleteResource(string key)
        {
            _repository.DeleteResource(key);
            CacheManager.Remove(BuildCacheKey(key));
        }

        public void CreateOrUpdateTranslation(string key, CultureInfo language, string newValue)
        {
            _repository.CreateOrUpdateTranslation(key, language, newValue);
            CacheManager.Remove(BuildCacheKey(key));
        }

        public void ClearCache()
        {
            if(HttpContext.Current == null)
            {
                return;
            }

            if(HttpContext.Current.Cache == null)
            {
                return;
            }

            var itemsToRemove = new List<string>();
            var enumerator = HttpContext.Current.Cache.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if(enumerator.Key.ToString().ToLower().StartsWith(CacheKeyPrefix.ToLower()))
                {
                    itemsToRemove.Add(enumerator.Key.ToString());
                }
            }

            foreach (var itemToRemove in itemsToRemove)
            {
                CacheManager.Remove(itemToRemove);
            }
        }

        internal void PopulateCache()
        {
            var allResources = GetAllResources();

            ClearCache();

            foreach (var resource in allResources)
            {
                var key = BuildCacheKey(resource.ResourceKey);
                CacheManager.Insert(key, resource);
            }
        }

        internal LanguageEntities GetDatabaseContext()
        {
            return _repository.GetDatabaseContext();
        }

        private static string BuildCacheKey(string key)
        {
            return $"{CacheKeyPrefix}_{key}";
        }

        private static LocalizationResource GetFromCache(string cacheKey)
        {
            var cachedResource = CacheManager.Get(cacheKey);
            return cachedResource as LocalizationResource;
        }
    }
}