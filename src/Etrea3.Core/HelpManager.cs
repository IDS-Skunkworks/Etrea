using System;
using Etrea3.Objects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etrea3.Core
{
    public class HelpManager
    {
        private static HelpManager instance = null;
        private ConcurrentDictionary<string, HelpArticle> Articles { get; set; }
        public int Count => Articles.Count;

        public HelpManager()
        {
            Articles = new ConcurrentDictionary<string, HelpArticle>();
        }

        public static HelpManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HelpManager();
                }
                return instance;
            }
        }

        public void LoadAllArticles(out bool hasErr)
        {
            var result = DatabaseManager.LoadAllArticles(out hasErr);
            if (!hasErr && result != null)
            {
                foreach(var article in result)
                {
                    Instance.Articles.AddOrUpdate(article.Key.ToLower(), article.Value, (k, v) => v = article.Value);
                }
            }
        }

        public HelpArticle GetArticle(string name)
        {
            if (Instance.Articles.TryGetValue(name, out var article))
            {
                return article;
            }
            return Instance.Articles.Values.FirstOrDefault(x => x.Title == name);
        }

        public List<HelpArticle> GetArticle()
        {
            return Instance.Articles.Values.ToList();
        }

        public bool AddOrUpdateArticle(HelpArticle article, bool isNew)
        {
            try
            {
                if (!DatabaseManager.SaveArticleToWorldDatabase(article, isNew))
                {
                    Game.LogMessage($"ERROR: Failed to save Article {article.Title} to World Database", LogLevel.Error);
                    return false;
                }
                if (isNew)
                {
                    if (!Instance.Articles.TryAdd(article.Title.ToLower(), article))
                    {
                        Game.LogMessage($"ERROR: Failed to add new Article {article.Title} to Help Manager", LogLevel.Error);
                        return false;
                    }
                }
                else
                {
                    if (!Instance.Articles.TryGetValue(article.Title.ToLower(), out var existingArticle))
                    {
                        Game.LogMessage($"ERROR: Article {article.Title} not found in Help Manager for update", LogLevel.Error);
                        return false;
                    }
                    if (!Instance.Articles.TryUpdate(article.Title.ToLower(), article, existingArticle))
                    {
                        Game.LogMessage($"ERROR: Failed to update Article {article.Title} in Help Manager due to a value mismatch", LogLevel.Error);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Game.LogMessage($"ERROR: Error in HelpManager.AddOrUpdateArticle(): {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        public bool RemoveArticle(string title)
        {
            if (Instance.Articles.ContainsKey(title.ToLower()))
            {
                return Instance.Articles.TryRemove(title.ToLower(), out _) && DatabaseManager.RemoveArticle(title);
            }
            Game.LogMessage($"ERROR: Error removig Article '{title}', no such Article in Help Manager", LogLevel.Error);
            return false;
        }
    }
}
