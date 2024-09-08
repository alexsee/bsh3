using System;
using System.IO;
using System.Text.RegularExpressions;
using Brightbits.BSH.Engine.Contracts;
using Brightbits.BSH.Engine.Models;
using Serilog;

namespace Brightbits.BSH.Engine.Services.FileCollector;

public interface IFileExclusion
{
    bool IsFileExcluded(FileTableRow file);
}

public class DatabaseFileExclusion : IFileExclusion
{
    public bool IsFileExcluded(FileTableRow file)
    {
        var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Alexosoft\Backup Service Home 3\";
        return file.FileNamePath().StartsWith(appDataFolder, StringComparison.OrdinalIgnoreCase);
    }
}

public class PathFileExclusion(IConfigurationManager configurationManager) : IFileExclusion
{
    private static readonly ILogger _logger = Log.ForContext<PathFileExclusion>();

    public bool IsFileExcluded(FileTableRow file)
    {
        if (string.IsNullOrEmpty(configurationManager.ExcludeFolder))
        {
            return false;
        }

        var excludeFolders = configurationManager.ExcludeFolder.Split('|');
        if (Array.Exists(excludeFolders, entry => ("\\" + Path.Combine(Path.GetFileName(file.FileRoot), file.FilePath) + "\\").StartsWith(entry + "\\", StringComparison.OrdinalIgnoreCase)))
        {
            _logger.Debug("{fileName} was ignored due to file path filter.", file.FileNamePath());
            return true;
        }

        return false;
    }
}

public class TypeFileExclusion(IConfigurationManager configurationManager) : IFileExclusion
{
    private static readonly ILogger _logger = Log.ForContext<TypeFileExclusion>();

    public bool IsFileExcluded(FileTableRow file)
    {
        if (string.IsNullOrEmpty(configurationManager.ExcludeFileTypes))
        {
            return false;
        }

        var excludeFileTypes = configurationManager.ExcludeFileTypes.Split('|');

        foreach (var entry in excludeFileTypes)
        {
            var fileExt = Path.GetExtension(file.FileNamePath());

            if (string.Equals("." + entry, fileExt, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Debug("{fileName} was ignored due to file extension filter.", file.FileNamePath());
                return true;
            }
        }

        return false;
    }
}

public class SizeFileExclusion(IConfigurationManager configurationManager) : IFileExclusion
{
    private static readonly ILogger _logger = Log.ForContext<SizeFileExclusion>();

    public bool IsFileExcluded(FileTableRow file)
    {
        // file size filter
        if (!string.IsNullOrEmpty(configurationManager.ExcludeFileBigger) && file.FileSize > long.Parse(configurationManager.ExcludeFileBigger))
        {
            _logger.Debug("{fileName} was ignored due to file size filter.", file.FileNamePath());
            return true;
        }

        return false;
    }
}

public class MaskFileExclusion(IConfigurationManager configurationManager) : IFileExclusion
{
    private static readonly ILogger _logger = Log.ForContext<MaskFileExclusion>();
    private Regex _regexExcludeCache;

    public bool IsFileExcluded(FileTableRow file)
    {
        if (string.IsNullOrEmpty(configurationManager.ExcludeMask))
        {
            return false;
        }

        try
        {
            _regexExcludeCache ??= new Regex(configurationManager.ExcludeMask, RegexOptions.Compiled & RegexOptions.Singleline, TimeSpan.FromSeconds(10));

            if (_regexExcludeCache.IsMatch(file.FileNamePath()))
            {
                _logger.Debug("{fileName} was ignored due to regex filter.", file.FileNamePath());
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "{fileName} was not ignored due to error with regular expressions {expr}",
                file.FileNamePath(),
                configurationManager.ExcludeMask);
            return false;
        }

        return false;
    }
}

public class NameFileExclusion(IConfigurationManager configurationManager) : IFileExclusion
{
    private static readonly ILogger _logger = Log.ForContext<NameFileExclusion>();

    public bool IsFileExcluded(FileTableRow file)
    {
        if (string.IsNullOrEmpty(configurationManager.ExcludeFile))
        {
            return false;
        }

        var excludeFile = configurationManager.ExcludeFile.Split('|');
        if (Array.Exists(excludeFile, entry => file.FileNamePath().EndsWith(entry, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.Debug("{fileName} was ignored due to file name filter.", file.FileNamePath());
            return true;
        }

        return false;
    }
}
