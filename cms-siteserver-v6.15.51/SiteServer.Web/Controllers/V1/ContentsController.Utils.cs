﻿using Datory;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.Model;
using SiteServer.Utils;
using SiteServer.Utils.Enumerations;
using SqlKata;

namespace SiteServer.API.Controllers.V1
{
    public partial class ContentsController
    {
        private Query GetQuery(int siteId, int? channelId, QueryRequest request)
        {
            var query = Q.Where(nameof(ContentInfo.SiteId), siteId).Where(nameof(ContentInfo.ChannelId), ">", 0);

            if (channelId.HasValue)
            {
                //query.Where(nameof(Abstractions.Content.ChannelId), channelId.Value);
                var channel = ChannelManager.GetChannelInfo(siteId, channelId.Value);
                var channelIds = ChannelManager.GetChannelIdList(channel, EScopeType.All);

                query.WhereIn(nameof(ContentInfo.ChannelId), channelIds);
            }

            if (request.Checked.HasValue)
            {
                query.Where(nameof(ContentInfo.IsChecked), request.Checked.Value.ToString());
            }
            if (request.Top.HasValue)
            {
                query.Where(nameof(ContentInfo.IsTop), request.Top.Value.ToString());
            }
            if (request.Recommend.HasValue)
            {
                query.Where(nameof(ContentInfo.IsRecommend), request.Recommend.Value.ToString());
            }
            if (request.Color.HasValue)
            {
                query.Where(nameof(ContentInfo.IsColor), request.Color.Value.ToString());
            }
            if (request.Hot.HasValue)
            {
                query.Where(nameof(ContentInfo.IsHot), request.Hot.Value.ToString());
            }

            if (request.GroupNames != null)
            {
                query.Where(q =>
                {
                    foreach (var groupName in request.GroupNames)
                    {
                        if (!string.IsNullOrEmpty(groupName))
                        {
                            q
                                .OrWhere(nameof(ContentInfo.GroupNameCollection), groupName)
                                .OrWhereLike(nameof(ContentInfo.GroupNameCollection), $"{groupName},%")
                                .OrWhereLike(nameof(ContentInfo.GroupNameCollection), $"%,{groupName},%")
                                .OrWhereLike(nameof(ContentInfo.GroupNameCollection), $"%,{groupName}");
                        }
                    }
                    return q;
                });
            }

            if (request.TagNames != null)
            {
                query.Where(q =>
                {
                    foreach (var tagName in request.TagNames)
                    {
                        if (!string.IsNullOrEmpty(tagName))
                        {
                            q
                                .OrWhere(nameof(ContentInfo.Tags), tagName)
                                .OrWhereLike(nameof(ContentInfo.Tags), $"{tagName},%")
                                .OrWhereLike(nameof(ContentInfo.Tags), $"%,{tagName},%")
                                .OrWhereLike(nameof(ContentInfo.Tags), $"%,{tagName}");
                        }
                    }
                    return q;
                });
            }

            if (request.Wheres != null)
            {
                foreach (var where in request.Wheres)
                {
                    if (string.IsNullOrEmpty(where.Operator)) where.Operator = OpEquals;
                    if (StringUtils.EqualsIgnoreCase(where.Operator, OpIn))
                    {
                        query.WhereIn(where.Column, TranslateUtils.StringCollectionToStringList(where.Value));
                    }
                    else if (StringUtils.EqualsIgnoreCase(where.Operator, OpNotIn))
                    {
                        query.WhereNotIn(where.Column, TranslateUtils.StringCollectionToStringList(where.Value));
                    }
                    else if (StringUtils.EqualsIgnoreCase(where.Operator, OpLike))
                    {
                        query.WhereLike(where.Column, $"%{where.Value}%");
                    }
                    else if (StringUtils.EqualsIgnoreCase(where.Operator, OpNotLike))
                    {
                        query.WhereNotLike(where.Column, $"%{where.Value}%");
                    }
                    else
                    {
                        query.Where(where.Column, where.Operator, where.Value);
                    }
                }
            }

            if (request.Orders != null)
            {
                foreach (var order in request.Orders)
                {
                    if (order.Desc)
                    {
                        query.OrderByDesc(order.Column);
                    }
                    else
                    {
                        query.OrderBy(order.Column);
                    }
                }
            }
            else
            {
                query.OrderByDesc(nameof(ContentInfo.IsTop),
                    nameof(ContentInfo.ChannelId),
                    nameof(ContentInfo.Taxis),
                    nameof(ContentInfo.Id));
            }

            var page = request.Page > 0 ? request.Page : 1;
            var perPage = request.PerPage > 0 ? request.PerPage : 20;

            query.ForPage(page, perPage);

            return query;
        }
    }
}
