﻿using System;
using System.Web.Http;
using NSwag.Annotations;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.DataCache.Content;
using SiteServer.CMS.Model;
using SiteServer.CMS.StlParser.Model;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Pages.Cms
{
    [OpenApiIgnore]
    [RoutePrefix("pages/cms/contentsLayerGroup")]
    public class PagesContentsLayerGroupController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public IHttpActionResult GetConfig()
        {
            try
            {
                var request = new AuthenticatedRequest();

                var siteId = request.GetQueryInt("siteId");
                var channelId = request.GetQueryInt("channelId");

                if (!request.IsAdminLoggin ||
                    !request.AdminPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentDelete))
                {
                    return Unauthorized();
                }

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                var contentGroupNameList = ContentGroupManager.GetGroupNameList(siteId);

                return Ok(new
                {
                    Value = contentGroupNameList
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Submit()
        {
            try
            {
                var request = new AuthenticatedRequest();

                var siteId = request.GetPostInt("siteId");
                //var channelId = request.GetPostInt("channelId");
                var channelContentIds =
                    MinContentInfo.ParseMinContentInfoList(request.GetPostString("channelContentIds"));
                var pageType = request.GetPostString("pageType");
                var groupNames = TranslateUtils.StringCollectionToStringList(request.GetPostString("groupNames"));
                var groupName = request.GetPostString("groupName");
                var description = request.GetPostString("description");

                if (!request.IsAdminLoggin)
                {
                    return Unauthorized();
                }

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                if (pageType == "setGroup")
                {
                    foreach (var channelContentId in channelContentIds)
                    {
                        var channelInfo = ChannelManager.GetChannelInfo(siteId, channelContentId.ChannelId);
                        var contentInfo = ContentManager.GetContentInfo(siteInfo, channelInfo, channelContentId.Id);
                        if (contentInfo == null) continue;

                        var list = TranslateUtils.StringCollectionToStringList(contentInfo.GroupNameCollection);
                        foreach (var name in groupNames)
                        {
                            if (!list.Contains(name)) list.Add(name);
                        }
                        contentInfo.GroupNameCollection = TranslateUtils.ObjectCollectionToString(list);

                        DataProvider.ContentDao.Update(siteInfo, channelInfo, contentInfo);
                    }

                    request.AddSiteLog(siteId, "批量设置内容组", $"内容组:{TranslateUtils.ObjectCollectionToString(groupNames)}");
                }
                else if(pageType == "cancelGroup")
                {
                    foreach (var channelContentId in channelContentIds)
                    {
                        var channelInfo = ChannelManager.GetChannelInfo(siteId, channelContentId.ChannelId);
                        var contentInfo = ContentManager.GetContentInfo(siteInfo, channelInfo, channelContentId.Id);
                        if (contentInfo == null) continue;

                        var list = TranslateUtils.StringCollectionToStringList(contentInfo.GroupNameCollection);
                        foreach (var name in groupNames)
                        {
                            if (list.Contains(name)) list.Remove(name);
                        }
                        contentInfo.GroupNameCollection = TranslateUtils.ObjectCollectionToString(list);

                        DataProvider.ContentDao.Update(siteInfo, channelInfo, contentInfo);
                    }

                    request.AddSiteLog(siteId, "批量取消内容组", $"内容组:{TranslateUtils.ObjectCollectionToString(groupNames)}");
                }
                else if (pageType == "addGroup")
                {
                    var groupInfo = new ContentGroupInfo
                    {
                        GroupName = AttackUtils.FilterXss(groupName),
                        SiteId = siteId,
                        Description = AttackUtils.FilterXss(description)
                    };

                    if (ContentGroupManager.IsExists(siteId, groupInfo.GroupName))
                    {
                        DataProvider.ContentGroupDao.Update(groupInfo);
                        request.AddSiteLog(siteId, "修改内容组", $"内容组:{groupInfo.GroupName}");
                    }
                    else
                    {
                        DataProvider.ContentGroupDao.Insert(groupInfo);
                        request.AddSiteLog(siteId, "添加内容组", $"内容组:{groupInfo.GroupName}");
                    }

                    foreach (var channelContentId in channelContentIds)
                    {
                        var channelInfo = ChannelManager.GetChannelInfo(siteId, channelContentId.ChannelId);
                        var contentInfo = ContentManager.GetContentInfo(siteInfo, channelInfo, channelContentId.Id);
                        if (contentInfo == null) continue;

                        var list = TranslateUtils.StringCollectionToStringList(contentInfo.GroupNameCollection);
                        if (!list.Contains(groupInfo.GroupName)) list.Add(groupInfo.GroupName);
                        contentInfo.GroupNameCollection = TranslateUtils.ObjectCollectionToString(list);

                        DataProvider.ContentDao.Update(siteInfo, channelInfo, contentInfo);
                    }

                    request.AddSiteLog(siteId, "批量设置内容组", $"内容组:{groupInfo.GroupName}");
                }

                return Ok(new
                {
                    Value = true
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }
    }
}
