
using System.Text.RegularExpressions;


public static class FileNameHelper
{
    public static string GetDocumentIdForEpgu(string originalFileName)
    {
        var safeName = SanitizeForEpgu(originalFileName);
        return safeName.Length > 50 ? safeName.Substring(0, 50) : safeName;
    }

    public static string GetUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        return $"{Guid.NewGuid()}{extension}";
    }


    private static string SanitizeForEpgu(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName);

        var safeName = Regex.Replace(nameWithoutExt, @"[^a-zA-Z0-9_ ]", "_"); // Заменяем все недопустимые символы (не латиница, не цифры, не '_', не пробел) на '_'
        safeName = safeName.Trim('_', ' '); // Убираем множественные подчеркивания и пробелы по краям
        int maxNameLength = 50 - extension.Length; // Ограничиваем длину. Макс. длина имени файла с расширением = 50 символов.
        if (safeName.Length > maxNameLength)
        {
            safeName = safeName.Substring(0, maxNameLength);
        }

        return $"{safeName}{extension}";
    }

    public static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".xml" => "application/xml",
            ".txt" => "text/plain",
            ".tif" or ".tiff" => "image/tiff",
            _ => throw new Exception("Недопустимый формат фалйа")
        };
    }

}