

public static class FileTypeHelper
{
    private static List<string> ValidExtentions = new List<string>()
    {
        ".pdf", ".tif", ".tiff", ".xml", ".txt"
    };
    public static bool IsValid(string fileName)
    {
        return ValidExtentions.Contains(Path.GetExtension(fileName));
    }

    public static bool IsValid(IFormFile file)
    {
        return ValidExtentions.Contains(Path.GetExtension(file.FileName));
    }


}