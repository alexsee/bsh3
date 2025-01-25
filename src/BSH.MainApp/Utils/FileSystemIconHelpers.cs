// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;
using Windows.Storage;
using BSH.Main;

namespace BSH.MainApp.Utils;
public static class FileSystemIconHelpers
{
    private static readonly Dictionary<string, BitmapImage> cachedIcons = [];

    /// <summary>
    /// Get the icon of a file
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="requestedSize"></param>
    /// <returns></returns>
    public static async Task<BitmapSource?> GetFileIconAsync(string fileName, uint requestedSize = 16)
    {
        try
        {
            var fileExtension = Path.GetExtension(fileName);

            // cache?
            if (!string.IsNullOrEmpty(fileExtension) && cachedIcons.TryGetValue($"{fileExtension}_{requestedSize}", out var cachedIcon))
            {
                return cachedIcon;
            }

            var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var localFolder = await StorageFolder.GetFolderFromPathAsync(applicationData);

            StorageItemThumbnail icon;

            if (string.IsNullOrEmpty(fileExtension))
            {
                icon = await localFolder.GetThumbnailAsync(ThumbnailMode.ListView, requestedSize, ThumbnailOptions.UseCurrentScale);
            }
            else
            {
                var emptyFile = await localFolder.CreateFileAsync(string.Join(Constants.Filesystem.CachedEmptyItemName, fileExtension), CreationCollisionOption.OpenIfExists);
                icon = await emptyFile.GetThumbnailAsync(ThumbnailMode.SingleItem, requestedSize, ThumbnailOptions.UseCurrentScale);
                await emptyFile.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }

            if (icon == null)
            {
                return null;
            }

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(icon);

            // cache!
            if (!string.IsNullOrEmpty(fileExtension))
            {
                cachedIcons.Add($"{fileExtension}_{requestedSize}", bitmap);
            }

            return bitmap;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Clear the icon cache
    /// </summary>
    public static void ClearIconCache()
    {
        cachedIcons.Clear();
    }
}
