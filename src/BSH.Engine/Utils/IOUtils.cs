namespace Brightbits.BSH.Engine.Utils;
public static class IOUtils
{
    public static string GetRelativeFolder(string path, string rootPath)
    {
        var result = path;
        result = result.Replace(rootPath, "");

        if (result.StartsWith("\\") && result.Length > 1)
        {
            result = result[1..];
        }
        else if (result == "\\")
        {
            return "";
        }

        return result;
    }
}
