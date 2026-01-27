using System;
using System.Collections.Generic;

namespace Emmetienne.TOMLConfigManager.Registries
{
    public class RepositoryRegistry
    {
        private readonly Dictionary<string, object> repositories = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public void Add<T>(string key, T repository) where T : class
        {
            repositories[key] = repository;
        }

        public T Get<T>(string key) where T : class
        {
            if (!repositories.TryGetValue(key, out var repo))
                throw new KeyNotFoundException($"Repository '{key}' not found.");

            if (!(repo is T typed))
                throw new InvalidCastException(
                    $"Repository '{key}' is not of type {typeof(T).Name}.");

            return typed;
        }

        public bool TryGet<T>(string key, out T repository) where T : class
        {
            if (repositories.TryGetValue(key, out var repo) && repo is T typed)
            {
                repository = typed;
                return true;
            }

            repository = null;
            return false;
        }
    }
}