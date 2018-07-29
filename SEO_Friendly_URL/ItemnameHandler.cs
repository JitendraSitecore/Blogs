#region Namespace Used
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
#endregion

namespace XXXX.Events
{

    /// <summary>
    /// Sitecore Event handler : Item Save & Rename override.
    /// Handle SEO friendly URL
    /// </summary>
    public class ItemnameHandler
    {
        #region Private member variables
        private string PageTemplateIds = string.Empty;
        private string PageTemplateIdsDelimeter = string.Empty;
        private string ItemHandlerDatabase = string.Empty;
        private string ItemHandlerParentItemPath = string.Empty;
        private string SEOFriendlyCharacter = string.Empty;
        private List<string> PageTemplates;
        #endregion

        #region Private methods
        /// <summary>
        /// Initialize setting values
        /// </summary>
        private void InitializeConfiguration()
        {
            PageTemplateIds = Settings.GetSetting(Constants.PageTemplateIds);
            PageTemplateIdsDelimeter = Settings.GetSetting(Constants.PageTemplateIdsDelimeter);
            ItemHandlerDatabase = Settings.GetSetting(Constants.ItemHandlerDatabase);
            ItemHandlerParentItemPath = Settings.GetSetting(Constants.ItemHandlerParentItemPath);
            SEOFriendlyCharacter = Settings.GetSetting(Constants.SEOFriendlyCharacter);

            PageTemplates = new List<string>();
            PageTemplates.AddRange(PageTemplateIds.Split(PageTemplateIdsDelimeter.ToCharArray()[0]));
        }

        #endregion

        /// <summary>
        /// Event handler for item:save & item:rename
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void HandleItemName(object sender, EventArgs args)
        {
            try
            {
                InitializeConfiguration();

                Item item = Event.ExtractParameter(args, 0) as Item;

                if (PageTemplates.Contains(item.TemplateID.ToString()) &&
                    item.Database.Name == this.ItemHandlerDatabase &&
                    item.Paths.Path.StartsWith(this.ItemHandlerParentItemPath) &&
                    !item.Name.Equals(item.Name.ToLower().Replace(" ", SEOFriendlyCharacter))
                    )
                {

                    string displalyName = item.Appearance.DisplayName;
                    if (string.IsNullOrEmpty(displalyName))
                    {
                        displalyName = item.Name;
                    }
                    using (new SecurityDisabler())
                    {
                        item.Editing.BeginEdit();
                        
                        item.Appearance.DisplayName = displalyName;

                        item.Name = displalyName.ToLower().Replace(" ", SEOFriendlyCharacter);

                        item.Editing.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("ItemnameHandler -> HandleItemName :", ex, this);
            }
        }
    }
}