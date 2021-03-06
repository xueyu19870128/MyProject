﻿using System.Collections.Generic;
using System.Linq;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache.Core;
using SiteServer.CMS.Model;

namespace SiteServer.CMS.DataCache
{
	public static class UserGroupManager
	{
	    private static class UserGroupManagerCache
        {
	        private static readonly object LockObject = new object();

	        private static readonly string CacheKey = DataCacheManager.GetCacheKey(nameof(UserGroupManager));

	        public static void Clear()
	        {
	            DataCacheManager.Remove(CacheKey);
	        }

	        public static List<UserGroupInfo> GetAllUserGroups()
	        {
	            var retVal = DataCacheManager.Get<List<UserGroupInfo>>(CacheKey);
	            if (retVal != null) return retVal;

	            lock (LockObject)
	            {
	                retVal = DataCacheManager.Get<List<UserGroupInfo>>(CacheKey);
	                if (retVal == null)
	                {
	                    retVal = DataProvider.UserGroupDao.GetUserGroupInfoList() ?? new List<UserGroupInfo>();

	                    DataCacheManager.Insert(CacheKey, retVal);
	                }
	            }

	            return retVal;
	        }
	    }

	    public static void ClearCache()
	    {
	        UserGroupManagerCache.Clear();
	    }

	    public static bool IsExists(string groupName)
	    {
	        var list = UserGroupManagerCache.GetAllUserGroups();
	        return list.Any(group => group.GroupName == groupName);
	    }

        public static UserGroupInfo GetUserGroupInfo(int groupId)
	    {
	        var list = UserGroupManagerCache.GetAllUserGroups();
	        return list.FirstOrDefault(group => group.Id == groupId) ?? list[0];
	    }

	    public static UserGroupInfo GetUserGroupInfo(string groupName)
	    {
	        var list = UserGroupManagerCache.GetAllUserGroups();
	        return list.FirstOrDefault(group => group.GroupName == groupName) ?? list[0];
        }

	    public static List<UserGroupInfo> GetUserGroupInfoList()
	    {
	        return UserGroupManagerCache.GetAllUserGroups();
	    }
    }
}
