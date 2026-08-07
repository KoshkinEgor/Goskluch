using System.Xml.Linq;

namespace Smev;

public class SmevDocumentInfo
{
    /// <summary>Логический идентификатор документа (атрибут docId, до 250 символов).</summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>MIME-тип содержимого (например, application/pdf).</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Описание документа (до 255 символов).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Имя файла (для AttachmentHeaderList).</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Бинарное содержимое документа.</summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>UUID документа — используется как Content-ID в MTOM (атрибут uuid в XML).</summary>
    public string DocumentUuid { get; set; } = Guid.NewGuid().ToString();

    /// <summary>UUID подписи — используется как Content-ID в MTOM (атрибут uuid для Signature).</summary>
    public string SignatureUuid { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Бинарное содержимое отсоединённой подписи отправителя.</summary>
    public byte[] SignatureContent { get; set; } = Array.Empty<byte>();
}