using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using nc = NewsComp;
using System.Data;
using SSNews.HtmlExpress;

/// <summary>
///NewsBiz 的摘要说明
/// </summary>
public class NewsInterFaceBiz
{
    /// <summary>
    /// 单行栏目广告读取
    /// </summary>
    /// <param name="cid">栏目号</param>
    /// <param name="index">偏移数</param>
    /// <returns></returns>
    public static NewsLinkEntity GetNewsByColumnAdSingle(int cid, int index)
    {
        var e = new NewsLinkEntity();
        var nc = new NewsComp.NewsComponent();
        var length = nc.GetNewsByColumnOrder(cid, index, 1);
        for (int i = 0; i < length; i++)
        {
            e = new NewsLinkEntity()
            {
                Title = nc.GetSubject(),
                PubCode = nc.GetNewsID(),
                NewsTime = nc.GetNewsDateTime(),
                Uri = nc.GetDigest(),
            };
        }
        nc.Dispose();
        return e;
    }

    /// <summary>
    /// 读取栏目新闻
    /// </summary>
    /// <param name="cid">栏目号</param>
    /// <param name="index">偏移数</param>
    /// <param name="count">数据</param>
    /// <returns></returns>
    public static List<NewsLinkEntity> GetNewsByColumn(int cid, int index, int count)
    {
        return GetNewsByColumn(cid, index, count, true, false);
    }


    /// <summary>
    /// 读取栏目新闻
    /// </summary>
    /// <param name="cid">栏目编号</param>
    /// <param name="index">偏移值</param>
    /// <param name="count">数量</param>
    /// <param name="isOrder">是否使用手工排序</param>
    /// <param name="showThumbnail">是否需要缩略图</param>
    /// <returns></returns>
    public static List<NewsLinkEntity> GetNewsByColumn(int cid, int index, int count, bool isOrder, bool showThumbnail)
    {
        var lists = new List<NewsLinkEntity>();
        var nc = new nc.NewsComponent();
        var length = isOrder ? nc.GetNewsByColumnOrder(cid, index, count) : nc.GetNewsByColumn(cid, index, count);
        for (int i = 0; i < length; i++)
        {
            var e = new NewsLinkEntity()
            {   
                Title = nc.GetSubject(),
                PubCode = nc.GetNewsID(),
                NewsTime = nc.GetNewsDateTime(),
                Uri = ChannelBiz.GetArticleUri(nc.GetNewsID(), cid, ChannelBiz.ConvertChannel(nc.GetNewsChannels())),
                Thumbnail = showThumbnail ? nc.GetNewsPicture() : string.Empty
            };
            try
            {
                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(e.Title, @"^<a(?: [^>]*)+href=[\'\""]?([^\""\'>]*)(?:[^>]*)*>([\s\S]*?)<\/a>$");
                if (match.Success)
                {
                    e.Uri = (match.Groups[1].Value ?? "").Trim();
                    e.Title = (match.Groups[2].Value ?? "").Trim();
                }
            }
            catch
            {
            }
            lists.Add(e);
            nc.MoveNext();
        }
        return lists;
    }

    public static List<NewsLinkEntity> GetNewsByColumnWithStock(int cid, int count)
    {
        var lists = new List<NewsLinkEntity>();
        var nc = new nc.NewsComponent();
        var length = nc.GetNewsByColumnOrder(cid, 1, (count + ((int)count / 5)));
        var total = 0;
        for (int i = 0; i < length; i++)
        {
            var e = new NewsLinkEntity()
            {
                Title = nc.GetSubject(),
                PubCode = nc.GetNewsID(),
                NewsTime = nc.GetNewsDateTime(),
                Uri = ChannelBiz.GetArticleUri(nc.GetNewsID(), cid, ChannelBiz.ConvertChannel(nc.GetNewsChannels())),
                Channels = nc.GetNewsChannels()
            };
            if (string.IsNullOrEmpty(e.Title))
            {
                continue;
            }

            //读取相关个股
            HashSet<string> stocklists = new HashSet<string>();
            var related = nc.GetRelatedStockCount();
            for (int j = 0; j < related; j++)
            {
                var code = nc.GetRelatedStockCode();
                if (!string.IsNullOrEmpty(code) && System.Text.RegularExpressions.Regex.IsMatch(code, @"[\d]{6}"))
                {
                    stocklists.Add(code);
                }
                nc.NextRelatedStock();
            }
            e.StockCodes = stocklists.ToArray();
            lists.Add(e);
            total += 1;
            if (total == count)
            {
                break;
            }
            nc.MoveNext();
        }
        return lists;
    }


    /// <summary>
    /// 读取单个栏目新闻
    /// </summary>
    /// <param name="cid">栏目号</param>
    /// <param name="index">偏移数</param>
    /// <returns></returns>
    public static NewsLinkEntity GetNewsByColumnSingle(int cid, int index)
    {
        var nc = new nc.NewsComponent();
        var length = nc.GetNewsByColumnOrder(cid, index, 1);
        var e = new NewsLinkEntity();
        for (int i = 0; i < length; i++)
        {
            e = new NewsLinkEntity()
            {
                Title = nc.GetSubject(),
                PubCode = nc.GetNewsID(),
                NewsTime = nc.GetNewsDateTime(),
                Uri = ChannelBiz.GetArticleUri(nc.GetNewsID(), cid, ChannelBiz.ConvertChannel(nc.GetNewsChannels()))
            };
        }
        return e;
    }

    /// <summary>
    /// 读取多个栏目广告
    /// </summary>
    /// <param name="cid">栏目号</param>
    /// <param name="index">偏移数</param>
    /// <param name="count">数据</param>
    /// <returns></returns>
    public static List<NewsLinkEntity> GetNewsByColumnAd(int cid, int index, int count)
    {
        var lists = new List<NewsLinkEntity>();
        var nc = new nc.NewsComponent();
        var length = nc.GetNewsByColumnOrder(cid, index, count);
        for (int i = 0; i < length; i++)
        {
            var e = new NewsLinkEntity()
            {
                Title = nc.GetSubject(),
                PubCode = nc.GetNewsID(),
                NewsTime = nc.GetNewsDateTime(),
                Uri = nc.GetDigest(),
            };
            if (!string.IsNullOrEmpty(e.Title))
                lists.Add(e);
            nc.MoveNext();
        }
        return lists;
    }

    /// <summary>
    /// 读取指定的栏目下的访问统计
    /// </summary>
    /// <param name="columnIds"></param>
    /// <param name="num"></param>
    /// <param name="hours"></param>
    /// <returns></returns>
    public static List<NewsLinkEntity> GetNewsStatisticsByColumn(int[] columnIds, int num, int hours)
    {
        var lists = new List<NewsLinkEntity>();
        using (var nc = new nc.NewsComponent())
        {
            var collections = APIBiz.GetByMainColumnsStatistics(columnIds, num * 3, hours);
            foreach (var item in collections)
            {
                var entity = new NewsLinkEntity();
                entity.PubCode = item;
                var x = nc.GetNewsByID(entity.PubCode);
                if (x <= 0) { continue; }
                entity.Title = nc.GetSubject();
                if (string.IsNullOrEmpty(entity.Title)) continue;
                entity.Uri = ChannelBiz.GetArticleUri(nc.GetNewsID(), ChannelBiz.ConvertChannel(nc.GetNewsChannels()));
                lists.Add(entity);
                if (lists.Count == num) break;
            }
        }
        return lists;
    }
    public static List<NewsLinkEntity> GetNewsStatisticsByColumn(int[] columnIds, int num, int hours, Func<NewsLinkEntity, bool> predicate)
    {
        var lists = new List<NewsLinkEntity>();
        using (var nc = new nc.NewsComponent())
        {
            var collections = APIBiz.GetByMainColumnsStatistics(columnIds, num * 5, hours);
            foreach (var item in collections)
            {
                var entity = new NewsLinkEntity();
                entity.PubCode = item;
                var x = nc.GetNewsByID(entity.PubCode);
                if (x <= 0) { continue; }
                entity.Title = nc.GetSubject();
                if (string.IsNullOrEmpty(entity.Title)) continue;
                entity.Uri = ChannelBiz.GetArticleUri(nc.GetNewsID(), ChannelBiz.ConvertChannel(nc.GetNewsChannels()));
                if (predicate == null || predicate(entity))
                {
                    lists.Add(entity);
                }
                if (lists.Count == num) break;
            }
        }
        return lists;
    }

    /// <summary>
    /// 根据新闻编号读新闻
    /// </summary>
    /// <param name="nid">标准新闻编号</param>
    /// <returns></returns>
    public static NewsLinkEntity GetNewsById(string nid)
    {
        using (var nc = new nc.NewsComponent())
        {
            var entity = new NewsLinkEntity();
            entity.PubCode = nid;
            var x = nc.GetNewsByID(entity.PubCode);
            entity.Title = nc.GetSubject();
            //if (string.IsNullOrEmpty(entity.Title)) continue;
            entity.Uri = ChannelBiz.GetArticleUri(nc.GetNewsID(), ChannelBiz.ConvertChannel(nc.GetNewsChannels()));
            entity.NewsTime = nc.GetNewsDateTime();
            return entity;
        }
    }


    #region 使用扩展代码
    /// <summary>
    /// 读取栏目新闻
    /// </summary>
    /// <param name="cid">栏目编号</param>
    /// <param name="limit">实际要取的数量</param>
    /// <param name="count">数量</param>
    /// <param name="isOrder">是否使用手工排序</param>
    /// <param name="showThumbnail">是否需要缩略图</param>
    /// <returns></returns>
    public static List<NewsLinkEntity> GetNewsByColumnExt(int cid, int limit, int count, bool isOrder, bool showThumbnail)
    {
        var lists = new List<NewsLinkEntity>();
        var nc = new nc.NewsComponent();
        var length = isOrder ? nc.GetNewsByColumnOrder(cid, 1, count) : nc.GetNewsByColumn(cid, 1, count);
        for (int i = 0; i < length; i++)
        {
            var e = new NewsLinkEntity()
            {
                Title = nc.GetSubject(),
                PubCode = nc.GetNewsID(),
                NewsTime = nc.GetNewsDateTime(),
                Author = nc.GetAuthor(),
                Source = nc.GetSource(),
                Uri = ChannelBiz.GetArticleUri(nc.GetNewsID(), cid, ChannelBiz.ConvertChannel(nc.GetNewsChannels())),
                Thumbnail = showThumbnail ? nc.GetNewsPicture() : string.Empty
            };

            try
            {
                System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(e.Title, @"^<a(?: [^>]*)+href=[\'\""]?([^\""\'>]*)(?:[^>]*)*>([\s\S]*?)<\/a>$");
                if (match.Success)
                {
                    e.Uri = (match.Groups[1].Value ?? "").Trim();
                    e.Title = (match.Groups[2].Value ?? "").Trim();
                }
            }
            catch
            {
            }
            if (!string.IsNullOrEmpty(e.Title))
            {
                lists.Add(e);
            }

            if (lists.Count == limit)
            {
                break;
            }
            nc.MoveNext();
        }
        return lists;
    }

    /// <summary>
    /// 读取多个栏目广告
    /// </summary>
    /// <param name="cid">栏目号</param>
    /// <param name="count">实际要取的数量</param>
    /// <param name="traverse">默认取的条数</param>
    /// <returns></returns>
    public static List<NewsLinkEntity> GetNewsByColumnAdExt(int cid, int limit, int count)
    {
        var lists = new List<NewsLinkEntity>();
        var nc = new nc.NewsComponent();
        var length = nc.GetNewsByColumnOrder(cid, 1, count);
        for (int i = 0; i < length; i++)
        {
            var e = new NewsLinkEntity()
            {
                Title = nc.GetSubject(),
                PubCode = nc.GetNewsID(),
                NewsTime = nc.GetNewsDateTime(),
                Uri = nc.GetDigest(),
            };
            if (!string.IsNullOrEmpty(e.Title))
            {
                lists.Add(e);
            }
            if (lists.Count == limit)
            {
                break;
            }
            nc.MoveNext();
        }
        return lists;
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cid"></param>
    /// <param name="limit"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static List<NewsLinkEntity> GetNews(int[] cids, int limit, int count)
    {
        List<NewsLinkEntity> lists = new List<NewsLinkEntity>();
        nc.NewsComponent nc = new nc.NewsComponent();
        HashSet<string> m = new HashSet<string>();
        foreach (var cid in cids)
        {
            int length = nc.GetNewsByColumn(cid, 1, count);
            int total = 0;
            for (int i = 0; i < length; i++)
            {
                var e = new NewsLinkEntity()
                {
                    Title = nc.GetSubject(),
                    PubCode = nc.GetNewsID(),
                    NewsTime = nc.GetNewsDateTime(),
                    Author = nc.GetAuthor(),
                    Source = nc.GetSource(),
                    Uri = ChannelBiz.GetArticleUri(nc.GetNewsID(), cid, ChannelBiz.ConvertChannel(nc.GetNewsChannels())),
                    Thumbnail = nc.GetNewsPicture(),
                    Digest = nc.GetDigest()
                };

                try
                {
                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(e.Title, @"^<a(?: [^>]*)+href=[\'\""]?([^\""\'>]*)(?:[^>]*)*>([\s\S]*?)<\/a>$");
                    if (match.Success)
                    {
                        e.Uri = (match.Groups[1].Value ?? "").Trim();
                        e.Title = (match.Groups[2].Value ?? "").Trim();
                    }
                }
                catch
                {
                }
                if (!string.IsNullOrEmpty(e.Title) && !m.Contains(e.PubCode))
                {
                    lists.Add(e);
                    //total++;
                }
                nc.MoveNext();
            }
        }

        return lists.OrderByDescending(p => p.NewsTime).Take(limit).ToList();
    }



    public static List<NewsLinkEntity> GetNews(int columnId, int begin, int count, int removeTop = 0)
    {
        nc.NewsComponent _newsCom = new nc.NewsComponent();
        var m = new HashSet<string>();
        int newsNum = -1;
        if (removeTop > 0)
        {
            newsNum = _newsCom.GetNewsByColumn(columnId + 1, 1, 100);
            for (int i = 0; i < newsNum; i++)
            {
                NewsLinkEntity news = new NewsLinkEntity();
                news.Title = _newsCom.GetSubject();
                news.PubCode = _newsCom.GetNewsID();
                if (!string.IsNullOrWhiteSpace(news.Title))
                {
                    m.Add(news.PubCode);
                    if (m.Count == removeTop)
                    {
                        break;
                    }
                }
                _newsCom.MoveNext();
            }
        }
        newsNum = _newsCom.GetNewsByColumn(columnId, begin, begin <= 1 ? Math.Max(count, 100) : count);
        List<NewsLinkEntity> list = new List<NewsLinkEntity>();
        for (int i = 0; i < newsNum; i++)
        {
            NewsLinkEntity news = new NewsLinkEntity();
            news.Title = _newsCom.GetSubject();
            news.PubCode = _newsCom.GetNewsID();
            news.NewsTime = _newsCom.GetNewsDateTime();
            news.Uri = SSLib.Base.Channel.ChannelBiz.GetArticleUri(news.PubCode, columnId, new int[] { columnId });
            if (!string.IsNullOrWhiteSpace(news.Title) && !m.Contains(news.PubCode))
            {
                try
                {
                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(news.Title, @"^<a(?: [^>]*)+href=[\'\""]?([^\""\'>]*)(?:[^>]*)*>([\s\S]*?)<\/a>$");
                    if (match.Success)
                    {
                        news.Uri = (match.Groups[1].Value ?? "").Trim();
                        news.Title = (match.Groups[2].Value ?? "").Trim();
                    }
                }
                catch
                {
                }
                list.Add(news);
                if (list.Count == count)
                {
                    break;
                }
            }
            _newsCom.MoveNext();
        }
        return list;
    }


    public static NewsLinkEntity 栏目重要新闻(int columnId)
    {
        var lists = new List<NewsLinkEntity>();
        using (var nc = new NewsComp.NewsComponent())
        {
            var length = nc.GetNewsByColumnOrder(columnId + 1, 1, 100);
            for (int i = 0; i < length; i++)
            {
                var e = new NewsLinkEntity();
                e.PubCode = nc.GetNewsID();//当前编号
                e.Title = nc.GetSubject();

                e.NewsTime = nc.GetNewsDateTime();
                e.Uri = SSLib.Base.Channel.ChannelBiz.GetArticleUri(e.PubCode, columnId, new int[] { columnId });// "https://finance.stockstar.com/" + e.PubCode.Replace(",", "").Replace(".", "") + ".shtml";
                if (!string.IsNullOrEmpty(e.Title))
                {
                    e.Digest = nc.GetDigest();
                    try
                    {
                        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(e.Title, @"^<a(?: [^>]*)+href=[\'\""]?([^\""\'>]*)(?:[^>]*)*>([\s\S]*?)<\/a>$");
                        if (match.Success)
                        {
                            e.Uri = (match.Groups[1].Value ?? "").Trim();
                            e.Title = (match.Groups[2].Value ?? "").Trim();
                        }
                    }
                    catch
                    {
                    }
                    return e;
                }
                nc.MoveNext();
            }
        }
        return null;
    }


    public static List<NewsLinkEntity> 栏目重要新闻(int columnId, int count)
    {
        var lists = new List<NewsLinkEntity>();
        using (var nc = new NewsComp.NewsComponent())
        {
            //var length = nc.GetNewsByColumnOrder(columnId + 1, 1, 100);
            var length = nc.GetNewsByColumnOrder(columnId, 1, 100);//Update 20200421  columnid不+1  不取中要
            for (int i = 0; i < length; i++)
            {
                var e = new NewsLinkEntity();
                e.PubCode = nc.GetNewsID();//当前编号
                e.Title = nc.GetSubject();

                e.NewsTime = nc.GetNewsDateTime();
                e.Uri = SSLib.Base.Channel.ChannelBiz.GetArticleUri(e.PubCode, columnId, new int[] { columnId });// "https://finance.stockstar.com/" + e.PubCode.Replace(",", "").Replace(".", "") + ".shtml";
                if (!string.IsNullOrEmpty(e.Title))
                {
                    e.Digest = nc.GetDigest();
                    if (string.IsNullOrEmpty(e.Digest))
                    {
                        e.Content = nc.GetContent();
                    }
                    try
                    {
                        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(e.Title, @"^<a(?: [^>]*)+href=[\'\""]?([^\""\'>]*)(?:[^>]*)*>([\s\S]*?)<\/a>$");
                        if (match.Success)
                        {
                            e.Uri = (match.Groups[1].Value ?? "").Trim();
                            e.Title = (match.Groups[2].Value ?? "").Trim();
                        }
                    }
                    catch
                    {
                    }

                    lists.Add(e);
                    if (lists.Count == count)
                    {
                        return lists;
                    }
                }
                nc.MoveNext();
            }
        }
        return lists;
    }
}