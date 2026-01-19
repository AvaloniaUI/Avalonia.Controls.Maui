using Microsoft.Maui.Controls;
using System;

namespace ControlGallery.Pages
{
    public partial class ToolbarItemPage : ContentPage
    {
        private ToolbarItem? _dynamicPrimaryItem;
        private ToolbarItem? _dynamicSecondaryItem;

        public ToolbarItemPage()
        {
            InitializeComponent();
        }

        private void LogPrimary(string message)
        {
            PrimaryLogLabel.Text = $"{DateTime.Now:HH:mm:ss}: {message}";
        }

        private void LogSecondary(string message)
        {
            SecondaryLogLabel.Text = $"{DateTime.Now:HH:mm:ss}: {message}";
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            LogPrimary("Edit Clicked");
            DisplayAlert("Primary", "Edit Item Clicked", "OK");
        }

        private void OnDeleteClicked(object sender, EventArgs e)
        {
            LogPrimary("Delete Clicked");
            DisplayAlert("Primary", "Delete Item Clicked", "OK");
        }

        private void OnSecondaryClicked(object sender, EventArgs e)
        {
            LogSecondary("Secondary Clicked");
            DisplayAlert("Secondary", "Secondary Item Clicked", "OK");
        }



        private void OnExecuteEditClicked(object sender, EventArgs e)
        {
            if (ToolbarItems.Count > 0)
            {
                var editItem = ToolbarItems[0]; 
                editItem.Command?.Execute(editItem.CommandParameter);
                // Or manually trigger handler if no command
                OnEditClicked(editItem, EventArgs.Empty);
                LogPrimary("Executed 'Edit' via Button");
            }
        }

        private void OnExecuteSecondaryClicked(object sender, EventArgs e)
        {
            foreach(var item in ToolbarItems)
            {
                if(item.Order == ToolbarItemOrder.Secondary && item.Text == "Secondary")
                {
                    OnSecondaryClicked(item, EventArgs.Empty);
                    LogSecondary("Executed 'Secondary' via Button");
                    return;
                }
            }
        }

        private void OnAddPrimaryItemClicked(object sender, EventArgs e)
        {
            if (_dynamicPrimaryItem == null)
            {
                _dynamicPrimaryItem = new ToolbarItem
                {
                    Text = "Dyn. Pri.",
                    Order = ToolbarItemOrder.Primary,
                    Command = new Command(() => 
                    {
                        LogPrimary("Dyn. Primary Clicked");
                        DisplayAlert("Primary", "Dynamic Primary Item Clicked", "OK");
                    })
                };
            }

            if (!ToolbarItems.Contains(_dynamicPrimaryItem))
            {
                ToolbarItems.Add(_dynamicPrimaryItem);
                LogPrimary("Added Dynamic Item");
            }
        }

        private void OnRemovePrimaryItemClicked(object sender, EventArgs e)
        {
            if (_dynamicPrimaryItem != null && ToolbarItems.Contains(_dynamicPrimaryItem))
            {
                ToolbarItems.Remove(_dynamicPrimaryItem);
                LogPrimary("Removed Dynamic Item");
            }
        }

        private void OnAddSecondaryItemClicked(object sender, EventArgs e)
        {
             if (_dynamicSecondaryItem == null)
            {
                _dynamicSecondaryItem = new ToolbarItem
                {
                    Text = "Dyn. Sec.",
                    Order = ToolbarItemOrder.Secondary,
                    Command = new Command(() => 
                    {
                        LogSecondary("Dyn. Secondary Clicked");
                        DisplayAlert("Secondary", "Dynamic Secondary Item Clicked", "OK");
                    })
                };
            }

            if (!ToolbarItems.Contains(_dynamicSecondaryItem))
            {
                ToolbarItems.Add(_dynamicSecondaryItem);
                LogSecondary("Added Dynamic Item");
            }
        }

        private void OnRemoveSecondaryItemClicked(object sender, EventArgs e)
        {
            if (_dynamicSecondaryItem != null && ToolbarItems.Contains(_dynamicSecondaryItem))
            {
                ToolbarItems.Remove(_dynamicSecondaryItem);
                LogSecondary("Removed Dynamic Item");
            }
        }

        private ToolbarItem? _cachedEditItem;

        private void OnToggleEditClicked(object sender, EventArgs e)
        {
            var item = ToolbarItems.FirstOrDefault(i => i.Text == "Edit" || i.Text == "Updated") ?? _cachedEditItem;
            
            if (item != null)
            {
                item.IsEnabled = !item.IsEnabled;
                LogPrimary($"Edit IsEnabled: {item.IsEnabled}");
            }
            else
            {
                LogPrimary("Edit Item not found (hidden?)");
            }
        }

        private void OnToggleEditVisibilityClicked(object sender, EventArgs e)
        {
            var item = ToolbarItems.FirstOrDefault(i => i.Text == "Edit" || i.Text == "Updated");

            if (item != null)
            {
                _cachedEditItem = item;
                ToolbarItems.Remove(item);
                LogPrimary("Edit Item Hidden (Removed)");
            }
            else if (_cachedEditItem != null)
            {
                ToolbarItems.Insert(0, _cachedEditItem);
                LogPrimary("Edit Item Visible (Added)");
            }
            else
            {
                LogPrimary("Edit item not found!");
            }
        }

        private void OnTextOnlyClicked(object sender, EventArgs e)
        {
            LogPrimary("TextOnly Clicked");
            DisplayAlert("Primary", "TextOnly Item Clicked", "OK");
        }

        private void OnToggleIconClicked(object sender, EventArgs e)
        {
            var item = ToolbarItems.FirstOrDefault(i => i.Text == "Edit" || i.Text == "Updated");
            
            if (item != null)
            {
                if (item.IconImageSource == null || ((FileImageSource)item.IconImageSource).File == "redbug.png")
                {
                    item.IconImageSource = "slider_thumb.png";
                    LogPrimary("Icon changed to slider_thumb.png");
                }
                else
                {
                    item.IconImageSource = "redbug.png";
                    LogPrimary("Icon changed to redbug.png");
                }
            }
             else
            {
                LogPrimary("Edit Item not found");
            }
        }

        private void OnUpdateTextOnlyClicked(object sender, EventArgs e)
        {
            var item = ToolbarItems.FirstOrDefault(i => i.Text == "TextOnly" || i.Text == "Text Updated");

            if (item != null)
            {
                if (item.Text == "TextOnly")
                {
                    item.Text = "Text Updated";
                    LogPrimary("Renamed to 'Text Updated'");
                }
                else
                {
                    item.Text = "TextOnly";
                    LogPrimary("Renamed to 'TextOnly'");
                }
            }
            else
            {
                LogPrimary("TextOnly Item not found");
            }
        }

        private void OnSwapPrioritiesClicked(object sender, EventArgs e)
        {
            var item1 = ToolbarItems.FirstOrDefault(i => i.Text == "Edit" || i.Text == "Updated");
            var item2 = ToolbarItems.FirstOrDefault(i => i.Text == "Delete");

            if (item1 != null && item2 != null)
            {
                int p1 = item1.Priority;
                item1.Priority = item2.Priority;
                item2.Priority = p1;
                LogPrimary($"Swapped Edit({item1.Priority}) <-> Delete({item2.Priority})");
            }
            else
            {
                 LogPrimary("Items not found for swap");
            }
        }

        private void OnToggleEditOrderClicked(object sender, EventArgs e)
        {
            var item = ToolbarItems.FirstOrDefault(i => i.Text == "Edit" || i.Text == "Updated");
            if (item != null)
            {
                if (item.Order == ToolbarItemOrder.Primary)
                {
                    item.Order = ToolbarItemOrder.Secondary;
                    LogPrimary("Edit moved to Secondary");
                }
                else
                {
                    item.Order = ToolbarItemOrder.Primary;
                    LogPrimary("Edit moved to Primary");
                }
            }
            else
            {
                LogPrimary("Edit item not found");
            }
        }

    }
}