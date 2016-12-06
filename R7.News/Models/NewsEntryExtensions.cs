﻿//
//  NewsEntityExtensions.cs
//
//  Author:
//       Roman M. Yagodin <roman.yagodin@gmail.com>
//
//  Copyright (c) 2016 Roman M. Yagodin
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DotNetNuke.Entities.Content;
using R7.News.Components;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using R7.News.Data;

namespace R7.News.Models
{
    public static class NewsEntryExtensions
    {
        public static INewsEntry WithContentItem (this INewsEntry newsEntry)
        {
            newsEntry.ContentItem = NewsDataProvider.Instance.ContentController.GetContentItem (newsEntry.ContentItemId);
            return newsEntry;
        }

        public static IEnumerable<INewsEntry> WithContentItems (this IEnumerable<INewsEntry> newsEntries)
        {
            var contentController = new ContentController ();
            var contentItems = contentController.GetContentItemsByContentType (NewsDataProvider.Instance.NewsContentType);

            return newsEntries.Join (contentItems.DefaultIfEmpty (), 
                ne => ne.ContentItemId,
                ci => ci.ContentItemId, 
                (ne, ci) => { 
                    ne.ContentItem = ci; 
                    return ne; 
                }
            );
        }

        public static IEnumerable<INewsEntry> WithContentItemsOneByOne (this IEnumerable<INewsEntry> newsEntries)
        {
            foreach (var newsEntry in newsEntries) {
                yield return newsEntry.WithContentItem ();
            }
        }

        public static INewsEntry WithAgentModule (this INewsEntry newsEntry, ModuleController moduleController)
        {
            if (newsEntry.AgentModuleId != null) {
                newsEntry.AgentModule = GetAgentModule (moduleController, newsEntry.AgentModuleId.Value);
            }

            return newsEntry;
        }

        public static INewsEntry WithAgentModuleOnce (this INewsEntry newsEntry, ModuleController moduleController)
        {
            if (newsEntry.AgentModuleId != null && newsEntry.AgentModule == null) {
                newsEntry.AgentModule = GetAgentModule (moduleController, newsEntry.AgentModuleId.Value);
            }

            return newsEntry;
        }

        private static ModuleInfo GetAgentModule (ModuleController moduleController, int agentModuleId)
        {
            return moduleController.GetTabModulesByModule (agentModuleId)
                .FirstOrDefault (am => !am.IsDeleted);
        }

        public static IEnumerable<INewsEntry> WithAgentModules (this IEnumerable<INewsEntry> newsEntries,
                                                                ModuleController moduleController)
        {
            foreach (var newsEntry in newsEntries) {
                yield return newsEntry.WithAgentModule (moduleController);
            }
        }

        public static IFileInfo GetImage (this INewsEntry newsEntry)
        {
            return newsEntry.ContentItem.Images.FirstOrDefault ();
        }

        public static string GetImageUrl (this INewsEntry newsEntry, int width)
        {
            var image = newsEntry.GetImage ();
            if (image != null) {
                return Globals.AddHTTP (PortalSettings.Current.PortalAlias.HTTPAlias)
                + "/imagehandler.ashx?fileticket=" + UrlUtils.EncryptParameter (image.FileId.ToString ())
                + "&width=" + width + "&ext=." + image.Extension;
            }

            return string.Empty;
        }

        public static IEnumerable<INewsEntry> GroupByAgentModule (this IEnumerable<INewsEntry> newsEntries,
                                                                  bool enableGrouping)
        {
            if (enableGrouping) {
                var newsList = new List<INewsEntry> ();
                foreach (var newsEntry in newsEntries) {

                    // find group entry
                    var groupEntry = newsList
                        .SingleOrDefault (ne => ne.AgentModuleId != null && ne.AgentModuleId == newsEntry.AgentModuleId);

                    // add current entry to the group
                    if (groupEntry != null) {
                        if (groupEntry.Group == null) {
                            groupEntry.Group = new Collection<INewsEntry> ();
                        }
                        groupEntry.Group.Add (newsEntry);
                        continue;
                    }

                    // add current entry as group entry
                    newsEntry.Group = null;
                    newsList.Add (newsEntry);
                }

                return newsList;
            }
            else {
                // clear group references
                foreach (var newsEntry in newsEntries) {
                    newsEntry.Group = null;
                }

                return newsEntries;
            }
        }

        public static bool IsPublished (this INewsEntry newsEntry, DateTime now)
        {
            return ModelHelper.IsPublished (now, newsEntry.StartDate, newsEntry.EndDate);
        }

        public static bool HasBeenExpired (this INewsEntry newsEntry, DateTime now)
        {
            return ModelHelper.HasBeenExpired (now, newsEntry.StartDate, newsEntry.EndDate);
        }

        public static DateTime PublishedOnDate (this INewsEntry newsEntry)
        {
            return ModelHelper.PublishedOnDate (newsEntry.StartDate, newsEntry.ContentItem.CreatedOnDate);
        }

        public static bool IsVisible (this INewsEntry newsEntry, int minThematicWeight, int maxThematicWeight,
                                      int minStructuralWeight, int maxStructuralWeight)
        {
            return ModelHelper.IsVisible (newsEntry.ThematicWeight, newsEntry.StructuralWeight, 
                minThematicWeight, maxThematicWeight, minStructuralWeight, maxStructuralWeight);
        }

        public static string GetPermalinkFriendly (this INewsEntry newsEntry,
                                                   ModuleController moduleController,
                                                   int moduleId,
                                                   int tabId)
        {
            if (newsEntry.AgentModuleId != null) {
                newsEntry.WithAgentModuleOnce (moduleController);
                if (newsEntry.AgentModule != null) {
                    return Globals.NavigateURL (newsEntry.AgentModule.TabID);
                }
            }

            return Globals.NavigateURL (
                tabId,
                "entry",
                "mid",
                moduleId.ToString (),
                "entryid",
                newsEntry.EntryId.ToString ()); 
        }

        public static string GetPermalinkRaw (this INewsEntry newsEntry,
                                              ModuleController moduleController,
                                              PortalAliasInfo portalAlias,
                                              int moduleId,
                                              int tabId)
        {
            if (newsEntry.AgentModuleId != null) {
                newsEntry.WithAgentModuleOnce (moduleController);
                if (newsEntry.AgentModule != null) {
                    return Globals.AddHTTP (portalAlias.HTTPAlias + "/default.aspx?tabid=" + newsEntry.AgentModule.TabID);
                }
            }

            return Globals.AddHTTP (portalAlias.HTTPAlias + "/default.aspx?tabid=" + tabId
            + "&mid=" + moduleId + "&ctl=entry&entryid=" + newsEntry.EntryId);
        }
    }
}

