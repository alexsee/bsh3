// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using BSH.Main;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;

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
            if (string.IsNullOrEmpty(fileExtension))
            {
                return null;
            }

            // cache?
            if (cachedIcons.TryGetValue($"{fileExtension}_{requestedSize}", out var cachedIcon))
            {
                return cachedIcon;
            }

            var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var localFolder = await StorageFolder.GetFolderFromPathAsync(applicationData);

            var emptyFile = await localFolder.CreateFileAsync(string.Join(Constants.Filesystem.CachedEmptyItemName, fileExtension), CreationCollisionOption.OpenIfExists);
            var icon = await emptyFile.GetThumbnailAsync(ThumbnailMode.SingleItem, requestedSize, ThumbnailOptions.UseCurrentScale);
            await emptyFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(icon);

            // cache!
            cachedIcons.Add($"{fileExtension}_{requestedSize}", bitmap);

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
