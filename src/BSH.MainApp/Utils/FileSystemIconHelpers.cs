// Copyright (c) Alexander Seeliger. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Brightbits.BSH.Engine;
using Microsoft.UI.Xaml.Media.Imaging;
using Serilog;

namespace BSH.MainApp.Utils;

/// <summary>
/// Shell file icons for the backup browser.
/// Lookup uses file attributes only, so it never creates or deletes a file.
/// Call <see cref="GetFileIconAsync"/> on the UI thread; the shell calls run there.
/// </summary>
public static class FileSystemIconHelpers
{
    private const int ShilExtraLarge = 2;
    private const int ShilJumbo = 4;
    private const int IldTransparent = 1;
    private const uint DibRgbColors = 0;
    private const uint BiRgb = 0;
    private const int MaxIconPixels = 1024;

    private static readonly Guid ImageListId = new("46EB5926-582E-4017-9FDF-E8998DAA0950");
    private static readonly ConcurrentDictionary<string, BitmapSource> cachedIcons = new();

    /// <summary>
    /// Icon for the extension of <paramref name="fileName"/> at <paramref name="requestedSize"/>.
    /// Files without an extension return null. The shell calls run synchronously on the caller.
    /// </summary>
    public static Task<BitmapSource?> GetFileIconAsync(string fileName, uint requestedSize = 16)
    {
        try
        {
            return Task.FromResult(LoadFileIcon(fileName, requestedSize));
        }
        catch (Exception ex)
        {
            // Icon lookup is best-effort. A failure must not stop the browser listing.
            Log.Debug(ex, "Could not load a file icon for {FileName}", fileName);
            return Task.FromResult<BitmapSource?>(null);
        }
    }

    /// <summary>
    /// Clear the icon cache.
    /// </summary>
    public static void ClearIconCache()
    {
        cachedIcons.Clear();
    }

    /// <summary>
    /// Cache key for an extension and size. The extension is lower-cased so ".TXT" and ".txt" match.
    /// Returns null when <paramref name="fileName"/> has no extension.
    /// </summary>
    internal static string? GetIconCacheKey(string? fileName, uint size)
    {
        var extension = ExtensionOf(fileName);
        if (extension == null)
        {
            return null;
        }

        return string.Concat(extension.ToLowerInvariant(), "_", size.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Device pixels for a logical icon size. 96 DPI and unknown DPI leave the size unchanged.
    /// </summary>
    internal static uint ToDevicePixels(uint requestedSize, uint dpi)
    {
        if (requestedSize == 0 || dpi <= 96)
        {
            return requestedSize;
        }

        var scaled = Math.Ceiling(requestedSize * (dpi / 96d));
        if (scaled >= uint.MaxValue)
        {
            return uint.MaxValue;
        }

        return (uint)scaled;
    }

    /// <summary>
    /// Smallest shell image that covers the device size. Small, large and extra-large
    /// lists scale with system DPI; jumbo remains 256 pixels.
    /// </summary>
    internal static ShellIconTier SelectIconTier(uint devicePixels, uint dpi = 96)
    {
        if (devicePixels <= ToDevicePixels(16, dpi))
        {
            return ShellIconTier.Small;
        }

        if (devicePixels <= ToDevicePixels(32, dpi))
        {
            return ShellIconTier.Large;
        }

        if (devicePixels <= ToDevicePixels(48, dpi))
        {
            return ShellIconTier.ExtraLarge;
        }

        return ShellIconTier.Jumbo;
    }

    /// <summary>
    /// Straight BGRA pixels become premultiplied. A fully transparent color bitmap uses the mask.
    /// </summary>
    internal static void ApplyStraightAlpha(byte[] pixels, byte[]? maskPixels)
    {
        var hasAlpha = false;
        for (var i = 3; i < pixels.Length; i += 4)
        {
            if (pixels[i] != 0)
            {
                hasAlpha = true;
                break;
            }
        }

        if (!hasAlpha)
        {
            for (var i = 0; i < pixels.Length; i += 4)
            {
                pixels[i + 3] = IsMaskTransparent(maskPixels, i) ? (byte)0 : (byte)255;
            }
        }

        for (var i = 0; i < pixels.Length; i += 4)
        {
            var alpha = pixels[i + 3];
            if (alpha == 255)
            {
                continue;
            }

            pixels[i] = (byte)(pixels[i] * alpha / 255);
            pixels[i + 1] = (byte)(pixels[i + 1] * alpha / 255);
            pixels[i + 2] = (byte)(pixels[i + 2] * alpha / 255);
        }
    }

    internal enum ShellIconTier
    {
        Small,
        Large,
        ExtraLarge,
        Jumbo
    }

    private static BitmapSource? LoadFileIcon(string? fileName, uint requestedSize)
    {
        var key = GetIconCacheKey(fileName, requestedSize);
        if (key == null)
        {
            return null;
        }

        if (cachedIcons.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var extension = ExtensionOf(fileName)!.ToLowerInvariant();
        var dpi = GetDpiForSystem();
        var devicePixels = ToDevicePixels(requestedSize, dpi);
        var created = TryCreateIcon(extension, devicePixels, dpi);
        if (created == null)
        {
            return null;
        }

        return cachedIcons.GetOrAdd(key, created);
    }

    private static string? ExtensionOf(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var extension = Path.GetExtension(fileName);
        return string.IsNullOrEmpty(extension) ? null : extension;
    }

    private static BitmapSource? TryCreateIcon(string extension, uint devicePixels, uint dpi)
    {
        var tier = SelectIconTier(devicePixels, dpi);
        var hIcon = tier is ShellIconTier.Small or ShellIconTier.Large
            ? GetSizedIcon(extension, tier == ShellIconTier.Small)
            : GetImageListIcon(extension, tier == ShellIconTier.ExtraLarge ? ShilExtraLarge : ShilJumbo);
        if (hIcon == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            return HiconToBitmap(hIcon);
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private static IntPtr GetSizedIcon(string extension, bool small)
    {
        var flags = (uint)(Win32Stuff.SHGFI.SHGFI_ICON | Win32Stuff.SHGFI.SHGFI_USEFILEATTRIBUTES);
        if (small)
        {
            flags |= (uint)Win32Stuff.SHGFI.SHGFI_SMALLICON;
        }

        var result = QueryShell(extension, flags, out var info);
        if (result == 0 || info.hIcon == IntPtr.Zero)
        {
            DestroyIconIfSet(info.hIcon);
            return IntPtr.Zero;
        }

        return info.hIcon;
    }

    private static IntPtr GetImageListIcon(string extension, int imageList)
    {
        var flags = (uint)(Win32Stuff.SHGFI.SHGFI_SYSICONINDEX | Win32Stuff.SHGFI.SHGFI_USEFILEATTRIBUTES);
        if (QueryShell(extension, flags, out var info) == 0)
        {
            DestroyIconIfSet(info.hIcon);
            return IntPtr.Zero;
        }

        DestroyIconIfSet(info.hIcon);
        var iid = ImageListId;
        var hr = SHGetImageList(imageList, ref iid, out var list);
        if (hr < 0 || list == null)
        {
            return IntPtr.Zero;
        }

        try
        {
            var iconHr = list.GetIcon(info.iIcon, IldTransparent, out var icon);
            if (iconHr < 0 || icon == IntPtr.Zero)
            {
                DestroyIconIfSet(icon);
                return IntPtr.Zero;
            }

            return icon;
        }
        finally
        {
            Marshal.ReleaseComObject(list);
        }
    }

    private static int QueryShell(string extension, uint flags, out Win32Stuff.SHFILEINFO info)
    {
        return Win32Stuff.SHGetFileInfo(
            extension,
            Win32Stuff.FILE_ATTRIBUTE_NORMAL,
            out info,
            (uint)Marshal.SizeOf<Win32Stuff.SHFILEINFO>(),
            flags);
    }

    private static BitmapSource? HiconToBitmap(IntPtr hIcon)
    {
        if (!GetIconInfo(hIcon, out var iconInfo))
        {
            return null;
        }

        try
        {
            if (iconInfo.hbmColor == IntPtr.Zero)
            {
                return null;
            }

            var hdc = GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                if (GetObject(iconInfo.hbmColor, Marshal.SizeOf<BitmapRecord>(), out var bitmap) == 0)
                {
                    return null;
                }

                var pixels = ReadBgra32(hdc, iconInfo.hbmColor, bitmap.bmWidth, bitmap.bmHeight);
                if (pixels == null)
                {
                    return null;
                }

                byte[]? mask = null;
                if (iconInfo.hbmMask != IntPtr.Zero)
                {
                    mask = ReadBgra32(hdc, iconInfo.hbmMask, bitmap.bmWidth, bitmap.bmHeight);
                }

                ApplyStraightAlpha(pixels, mask);
                return CreateBitmap(bitmap.bmWidth, bitmap.bmHeight, pixels);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }
        finally
        {
            DeleteObjectIfSet(iconInfo.hbmColor);
            DeleteObjectIfSet(iconInfo.hbmMask);
        }
    }

    private static WriteableBitmap? CreateBitmap(int width, int height, byte[] pixels)
    {
        var bitmap = new WriteableBitmap(width, height);
        using var stream = bitmap.PixelBuffer.AsStream();
        if (stream.Length < pixels.Length)
        {
            return null;
        }

        stream.Write(pixels, 0, pixels.Length);
        return bitmap;
    }

    private static byte[]? ReadBgra32(IntPtr hdc, IntPtr bitmap, int width, int height)
    {
        if (width <= 0 || height <= 0 || width > MaxIconPixels || height > MaxIconPixels)
        {
            return null;
        }

        var byteCount = width * height * 4;
        var pixels = new byte[byteCount];
        if (CopyPixels(hdc, bitmap, width, height, pixels, topDown: true))
        {
            return pixels;
        }

        if (!CopyPixels(hdc, bitmap, width, height, pixels, topDown: false))
        {
            return null;
        }

        var stride = width * 4;
        var swap = new byte[stride];
        for (var row = 0; row < height / 2; row++)
        {
            var top = row * stride;
            var bottom = (height - 1 - row) * stride;
            Buffer.BlockCopy(pixels, top, swap, 0, stride);
            Buffer.BlockCopy(pixels, bottom, pixels, top, stride);
            Buffer.BlockCopy(swap, 0, pixels, bottom, stride);
        }

        return pixels;
    }

    private static bool CopyPixels(IntPtr hdc, IntPtr bitmap, int width, int height, byte[] pixels, bool topDown)
    {
        var info = new BitmapInfo
        {
            bmiHeader = new BitmapInfoHeader
            {
                biSize = (uint)Marshal.SizeOf<BitmapInfoHeader>(),
                biWidth = width,
                biHeight = topDown ? -height : height,
                biPlanes = 1,
                biBitCount = 32,
                biCompression = BiRgb
            }
        };

        return GetDIBits(hdc, bitmap, 0, (uint)height, pixels, ref info, DibRgbColors) == height;
    }

    private static bool IsMaskTransparent(byte[]? maskPixels, int pixelIndex)
    {
        if (maskPixels == null || pixelIndex + 2 >= maskPixels.Length)
        {
            return false;
        }

        return maskPixels[pixelIndex] >= 128
            && maskPixels[pixelIndex + 1] >= 128
            && maskPixels[pixelIndex + 2] >= 128;
    }

    private static void DestroyIconIfSet(IntPtr hIcon)
    {
        if (hIcon != IntPtr.Zero)
        {
            DestroyIcon(hIcon);
        }
    }

    private static void DeleteObjectIfSet(IntPtr handle)
    {
        if (handle != IntPtr.Zero)
        {
            DeleteObject(handle);
        }
    }

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = true)]
    private static extern int SHGetImageList(int iImageList, ref Guid riid, out IShellImageList? imageList);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern uint GetDpiForSystem();

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern bool GetIconInfo(IntPtr hIcon, out IconInfo iconInfo);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("gdi32.dll", ExactSpelling = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("gdi32.dll", EntryPoint = "GetObjectW", ExactSpelling = true, SetLastError = true)]
    private static extern int GetObject(IntPtr h, int c, out BitmapRecord bitmap);

    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    private static extern int GetDIBits(
        IntPtr hdc,
        IntPtr hbm,
        uint start,
        uint cLines,
        byte[] pixels,
        ref BitmapInfo bitmapInfo,
        uint usage);

    [StructLayout(LayoutKind.Sequential)]
    private struct IconInfo
    {
        public int fIcon;
        public uint xHotspot;
        public uint yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapRecord
    {
        public int bmType;
        public int bmWidth;
        public int bmHeight;
        public int bmWidthBytes;
        public ushort bmPlanes;
        public ushort bmBitsPixel;
        public IntPtr bmBits;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfo
    {
        public BitmapInfoHeader bmiHeader;
        public uint bmiColors;
    }
}

/// <summary>
/// Native IImageList up to GetIcon. Method order is the COM vtable.
/// </summary>
[ComImport]
[Guid("46EB5926-582E-4017-9FDF-E8998DAA0950")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IShellImageList
{
    [PreserveSig]
    int Add(IntPtr hbmImage, IntPtr hbmMask, out int pi);

    [PreserveSig]
    int ReplaceIcon(int i, IntPtr hicon, out int pi);

    [PreserveSig]
    int SetOverlayImage(int iImage, int iOverlay);

    [PreserveSig]
    int Replace(int i, IntPtr hbmImage, IntPtr hbmMask);

    [PreserveSig]
    int AddMasked(IntPtr hbmImage, int crMask, out int pi);

    [PreserveSig]
    int Draw(IntPtr pimldp);

    [PreserveSig]
    int Remove(int i);

    [PreserveSig]
    int GetIcon(int i, int flags, out IntPtr picon);
}
