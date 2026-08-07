using System.Xml.Linq;

namespace Smev;public class SmevRequestBuilder
{
    private string _snils = string.Empty;
    private string _desc = string.Empty;
    private DateTime _signExp;
    private string _backlink = string.Empty;
    private string _routeNumber = "MNSV03";
    private string _requestId = string.Empty;
    private readonly List<SmevDocumentInfo> _documents = new();

    public string RequestId => _requestId;

    public SmevRequestBuilder SetRequest(string snils, string desc, string? backlink, DateTime? signExp)
    {
        _snils = snils;
        _desc = desc;
        _signExp = signExp ?? DateTime.Now.AddHours(24);
        _backlink = backlink ?? "https://lk.gosuslugi.ru/notifications";
        _requestId = $"Q-{Guid.NewGuid()}";
        return this;
    }

    public SmevRequestBuilder SetRouteNumber(string routeNumber)
    {
        _routeNumber = routeNumber;
        return this;
    }

    public SmevRequestBuilder AddDocument(SmevDocumentInfo documentInfo)
    {
        if (string.IsNullOrEmpty(documentInfo.DocumentId) || documentInfo.DocumentId.Length > 250)
            throw new ArgumentException("DocumentId должен быть от 1 до 250 символов.");

        if (!string.IsNullOrEmpty(documentInfo.Description) && documentInfo.Description.Length > 255)
            throw new ArgumentException("Description не может превышать 255 символов.");

        // Проверка размера вложения для MTOM (не более 5 МБ согласно п. 4.5 РП)
        if (documentInfo.Content.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException(
                $"Размер файла '{documentInfo.FileName}' превышает 5 МБ. " +
                "Для вложений более 5 МБ необходимо использовать FTP (RefAttachmentHeaderList).");

        _documents.Add(documentInfo);
        return this;
    }

    public string Build()
    {
        if (string.IsNullOrWhiteSpace(_snils))
            throw new InvalidOperationException("Необходимо инициализировать запрос через SetRequest.");

        if (_documents.Count == 0)
            throw new InvalidOperationException("Необходимо добавить хотя бы один документ.");

        if (_documents.Count > 1000)
            throw new InvalidOperationException("Максимальное количество документов в одном запросе — 1000 (п. 4.5 РП).");

        XNamespace tns = "urn://gosuslugi/sig-contract-snils-UKEP/1.0.0";
        string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
        string signExpStr = _signExp.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");

        var root = new XElement(tns + "RequestSignUkep",
            new XAttribute("Id", _requestId),
            new XAttribute("routeNumber", _routeNumber),
            new XAttribute("timestamp", timestamp),

            new XElement(tns + "SNILS", _snils),
            new XElement(tns + "signExp", signExpStr),
            new XElement(tns + "descDoc", _desc),

            new XElement(tns + "Contracts",
                _documents.Select(doc => new XElement(tns + "Contract",
                    // Документ на подпись
                    new XElement(tns + "Document",
                        new XAttribute("docId", doc.DocumentId),
                        new XAttribute("uuid", doc.DocumentUuid),
                        new XAttribute("mimeType", doc.MimeType),
                        string.IsNullOrWhiteSpace(doc.Description)
                            ? null
                            : new XAttribute("description", doc.Description)
                    ),
                    // Подпись инициатора
                    new XElement(tns + "Signature",
                        new XAttribute("docId", $"sig-{doc.DocumentId}"),
                        new XAttribute("uuid", doc.SignatureUuid),
                        new XAttribute("mimeType", "application/sig"),
                        new XAttribute("description", "Подпись отправителя")
                    )
                ))
            )
        );

        if (!string.IsNullOrWhiteSpace(_backlink))
        {
            root.Add(new XElement(tns + "Backlink", _backlink));
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root)
            .ToString(SaveOptions.DisableFormatting);
    }

    public byte[] BuildBytes() => System.Text.Encoding.UTF8.GetBytes(Build());

    public IReadOnlyList<SmevDocumentInfo> GetDocuments() => _documents.AsReadOnly();
}