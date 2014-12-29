﻿using System;
using System.Collections.Generic;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Sitecore.Sites;
using Sitecore.StringExtensions;

namespace Sitecore.SharedSource.DynamicSites.Utilities
{
    internal static class DynamicSiteManager
    {

        public static void AddBaseTemplate([NotNull] TemplateItem item)
        {

            var template = DynamicSiteSettings.BaseSiteDefinitionTemplateItem;
            
            //Get List of Existing Base Templates
            var existingTemplates = GetBaseTemplates(item);

            //Double check Base Template doesn't already exist.
            if (HasBaseTemplate(item)) return;

            //Add Template
            existingTemplates.Add(template.ID.ToString());

            //Save Item
            SaveItemBaseTemplates(item, existingTemplates);
        }

        public static void RemoveBaseTemplate([NotNull] TemplateItem item)
        {

            var template = DynamicSiteSettings.BaseSiteDefinitionTemplateItem;

            //Get List of Existing Base Templates
            var existingTemplates = GetBaseTemplates(item);

            //Double check Base Template exists.
            if (!HasBaseTemplate(item)) return;
            
            //Add Template
            existingTemplates.Remove(template.ID.ToString());
                
            //Save Item
            SaveItemBaseTemplates(item,existingTemplates);
        }

        public static bool HasBaseTemplate([NotNull] Item item)
        {

            return HasBaseTemplate(item.Template);
        }

        private static bool HasBaseTemplate([NotNull] TemplateItem item)
        {
            var template = DynamicSiteSettings.BaseSiteDefinitionTemplateItem;
            
            //Get List of Existing Base Templates
            var existingTemplates = new List<string>(item.InnerItem[FieldIDs.BaseTemplate].Split('|'));
            return existingTemplates.Contains(template.ID.ToString());
        }

        private static List<string> GetBaseTemplates([NotNull] BaseItem item)
        {
            return new List<string>(item[FieldIDs.BaseTemplate].Split('|'));
        }

        private static void SaveItemBaseTemplates([NotNull] Item item, [NotNull] IEnumerable<string> baseTemplates)
        {
            using (new SecurityDisabler())
            {
                item.Editing.BeginEdit();
                item[FieldIDs.BaseTemplate] = string.Join("|", baseTemplates);
                item.Editing.EndEdit();
            }
        }

        public static bool SettingsInitialized()
        {
            var settingsItem = DynamicSiteSettings.GetSettingsItem;
            if (settingsItem == null) return false;

            if (settingsItem.SiteDefinitionTemplate.Item == null) return false;
            if (settingsItem.SitesFolder.Item == null) return false;

            return true;
        }

        public static bool InitializeSettings()
        {
            if (SettingsInitialized())
                return true;

            var settingsItem = DynamicSiteSettings.GetSettingsItem;
            if (settingsItem == null) return false;

            var websiteItem = SiteManager.GetSite("website") ?? SiteManager.GetSite(DynamicSiteSettings.SiteName);

            var rootPath = websiteItem.Properties["rootPath"];
            var defaultPath = rootPath + websiteItem.Properties["startItem"];
            if (defaultPath.IsNullOrEmpty() || rootPath.IsNullOrEmpty()) return false;

            var siteFolderItem = DynamicSiteSettings.GetCurrentDatabase.GetItem(rootPath);
            var siteItem = DynamicSiteSettings.GetCurrentDatabase.GetItem(defaultPath);
            if (siteFolderItem == null || siteItem == null) return false;

            try
            {
                using (new SecurityDisabler())
                {
                    settingsItem.InnerItem.Editing.BeginEdit();
                    settingsItem.InnerItem[settingsItem.SitesFolder.Field.InnerField.ID] = siteFolderItem.ID.ToString();
                    settingsItem.InnerItem[settingsItem.SiteDefinitionTemplate.Field.InnerField.ID] = siteItem.TemplateID.ToString();
                    settingsItem.InnerItem.Editing.EndEdit();
                }

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
