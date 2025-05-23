﻿using AzureNamingTool.Helpers;
using AzureNamingTool.Models;

namespace AzureNamingTool.Services
{
    /// <summary>
    /// Service for managing custom components.
    /// </summary>
    public class CustomComponentService
    {
        /// <summary>
        /// Retrieves a list of custom component items.
        /// </summary>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> GetItems()
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<CustomComponent>();
                if (GeneralHelper.IsNotNull(items))
                {
                    serviceResponse.ResponseObject = items.OrderBy(x => x.SortOrder).ToList();
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.ResponseObject = "Custom Components not found!";
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Retrieves a list of custom component items by the parent component ID.
        /// </summary>
        /// <param name="parentcomponetid">The ID of the parent component.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> GetItemsByParentComponentId(int parentcomponetid)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get the parent component details
                serviceResponse = await ResourceComponentService.GetItem(parentcomponetid);
                if(serviceResponse.Success)
                {
                    var component = (ResourceComponent)serviceResponse.ResponseObject!;

                    // Get list of items
                    var items = await ConfigurationHelper.GetList<CustomComponent>();
                    if (GeneralHelper.IsNotNull(items))
                    {
                        serviceResponse.ResponseObject = items.Where(x => x.ParentComponent == GeneralHelper.NormalizeName(component.Name, true)).OrderBy(x => x.SortOrder).ToList();
                        serviceResponse.Success = true;
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Custom Components not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Resource Component not found!";
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Retrieves a list of custom component items by the parent type.
        /// </summary>
        /// <param name="parenttype">The parent type of the custom components.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> GetItemsByParentType(string parenttype)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<CustomComponent>();
                if (GeneralHelper.IsNotNull(items))
                {
                    serviceResponse.ResponseObject = items.Where(x => x.ParentComponent == parenttype).OrderBy(x => x.SortOrder).ToList();
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.ResponseObject = "Custom Components not found!";
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Retrieves a custom component item by its ID.
        /// </summary>
        /// <param name="id">The ID of the custom component to retrieve.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> GetItem(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<CustomComponent>();
                if (GeneralHelper.IsNotNull(items))
                {
                    var item = items.Find(x => x.Id == id);
                    if (GeneralHelper.IsNotNull(item))
                    {
                        serviceResponse.ResponseObject = item;
                        serviceResponse.Success = true;
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Custom Component not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Custom Components not found!";
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Posts a custom component item.
        /// </summary>
        /// <param name="item">The custom component item to be posted.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> PostItem(CustomComponent item)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Make sure the new item short name only contains letters/numbers
                if (!ValidationHelper.CheckAlphanumeric(item.ShortName))
                {
                    serviceResponse.Success = false;
                    serviceResponse.ResponseObject = "Short name must be alphanumeric.";
                    return serviceResponse;
                }

                // Get list of items
                var items = await ConfigurationHelper.GetList<CustomComponent>();
                if (GeneralHelper.IsNotNull(items))
                {
                    // Set the new id
                    if (item.Id == 0)
                    {
                        item.Id = items.Count + 1;
                    }

                    int position = 1;
                    items = [.. items.OrderBy(x => x.SortOrder)];

                    if (item.SortOrder == 0)
                    {
                        if (items.Count > 0)
                        {
                            item.Id = items.Max(t => t.Id) + 1;
                        }
                    }

                    // Determine new item id
                    if (items.Count > 0)
                    {
                        // Check if the item already exists
                        if (items.Exists(x => x.Id == item.Id))
                        {
                            // Remove the updated item from the list
                            var existingitem = items.Find(x => x.Id == item.Id);
                            if (GeneralHelper.IsNotNull(existingitem))
                            {
                                int index = items.IndexOf(existingitem);
                                items.RemoveAt(index);
                            }
                        }

                        // Reset the sort order of the list
                        foreach (CustomComponent thisitem in items.OrderBy(x => x.SortOrder).ToList())
                        {
                            thisitem.SortOrder = position;
                            position += 1;
                        }

                        // Check for the new sort order
                        if (items.Exists(x => x.SortOrder == item.SortOrder))
                        {
                            // Remove the updated item from the list
                            items.Insert(items.IndexOf(items.FirstOrDefault(x => x.SortOrder == item.SortOrder)!), item);
                        }
                        else
                        {
                            // Put the item at the end
                            item.SortOrder = position;
                            items.Add(item);
                        }
                    }
                                    else
                    {
                        item.Id = 1;
                        item.SortOrder = 1;
                        items.Add(item);
                    }

                    position = 1;
                    foreach (CustomComponent thisitem in items.OrderBy(x => x.SortOrder).ToList())
                    {
                        thisitem.SortOrder = position;
                        position += 1;
                    }

                    // Write items to file
                    await ConfigurationHelper.WriteList<CustomComponent>(items);
                    // Get the item
                    var newitem = (await CustomComponentService.GetItem((int)item.Id)).ResponseObject;
                    serviceResponse.ResponseObject = newitem;
                    serviceResponse.Success = true;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Deletes a custom component by its ID.
        /// </summary>
        /// <param name="id">The ID of the custom component to delete.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> DeleteItem(int id)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var items = await ConfigurationHelper.GetList<CustomComponent>();
                if (GeneralHelper.IsNotNull(items))
                {
                    // Get the specified item
                    var item = items.Find(x => x.Id == id);
                    if (GeneralHelper.IsNotNull(item))
                    {
                        // Remove the item from the collection
                        items.Remove(item);

                        // Update all the sort order values to reflect the removal
                        int position = 1;
                        foreach (CustomComponent thisitem in items.OrderBy(x => x.SortOrder).ToList())
                        {
                            thisitem.SortOrder = position;
                            position += 1;
                        }

                        // Write items to file
                        await ConfigurationHelper.WriteList<CustomComponent>(items);
                        serviceResponse.Success = true;
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Custom Component not found!";
                    }
                }
                else
                {
                    serviceResponse.ResponseObject = "Custom Component not found!";
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Posts a configuration of custom components.
        /// </summary>
        /// <param name="items">The list of custom components to be configured.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> PostConfig(List<CustomComponent> items)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get list of items
                var newitems = new List<CustomComponent>();
                int i = 1;

                // Determine new item id
                foreach (CustomComponent item in items)
                {
                    // Make sure the new item short name only contains letters/numbers
                    if (!ValidationHelper.CheckAlphanumeric(item.ShortName))
                    {
                        serviceResponse.Success = false;
                        serviceResponse.ResponseObject = "Short name must be alphanumeric.";
                        return serviceResponse;
                    }

                    item.Id = i;
                    item.SortOrder = i;
                    newitems.Add(item);
                    i += 1;
                }

                // Write items to file
                await ConfigurationHelper.WriteList<CustomComponent>(newitems);
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }

        /// <summary>
        /// Deletes custom components by parent component ID.
        /// </summary>
        /// <param name="componentid">The ID of the parent component.</param>
        /// <returns>A <see cref="Task{ServiceResponse}"/> representing the asynchronous operation.</returns>
        public static async Task<ServiceResponse> DeleteByParentComponentId(int componentid)
        {
            ServiceResponse serviceResponse = new();
            try
            {
                // Get the resource component
                serviceResponse = await ResourceComponentService.GetItem(componentid);
                if (serviceResponse.Success)
                {
                    var component = serviceResponse.ResponseObject as ResourceComponent;
                    if (GeneralHelper.IsNotNull(component))
                    {
                        // Get list of items
                        var items = await ConfigurationHelper.GetList<CustomComponent>();
                        if (GeneralHelper.IsNotNull(items))
                        {
                            // Get the custom component options
                            List<CustomComponent> customcomponents = items.Where(x => GeneralHelper.NormalizeName(component.Name, true) == x.ParentComponent).ToList();
                            if (GeneralHelper.IsNotNull(customcomponents))
                            {
                                foreach (CustomComponent customcomponent in customcomponents)
                                {
                                    // Remove the item from the collection
                                    items.Remove(customcomponent);

                                    // Update all the sort order values to reflect the removal
                                    int position = 1;
                                    foreach (CustomComponent thisitem in items.OrderBy(x => x.SortOrder).ToList())
                                    {
                                        thisitem.SortOrder = position;
                                        position += 1;
                                    }
                                }

                                // Write items to file
                                await ConfigurationHelper.WriteList<CustomComponent>(items);
                                serviceResponse.Success = true;
                            }
                            else
                            {
                                serviceResponse.ResponseObject = "Custom Component not found!";
                            }
                        }
                        else
                        {
                            serviceResponse.ResponseObject = "Custom Component not found!";
                        }
                    }
                    else
                    {
                        serviceResponse.ResponseObject = "Custom Component not found!";
                    }
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.ResponseObject = ex;
                serviceResponse.Success = false;
            }
            return serviceResponse;
        }
    }
}