using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Epgu;

public enum SignType
{
    Unep, // УНЭП
    Ukep  // УКЭП
}

public class EpguDocumentInfo
{
    public string DocumentId { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime SignExpiration { get; set; }
}

public class EpguRequestBuilder
{
    private SignType _signType;
    private string _mnemonics;
    private string _serviceName;
    private string _orgName;
    private string _backlink;
    private string _serviceCode;
    private string _oid = string.Empty;
    private string _snils = string.Empty;
    private List<EpguDocumentInfo> _documents = new();



    public EpguRequestBuilder SetConfig(string mnemonics, string serviceName, string serviceCode, string orgName)
    {
        _mnemonics = mnemonics ?? throw new ArgumentNullException(nameof(mnemonics));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _orgName = orgName ?? throw new ArgumentNullException(nameof(orgName));
        _serviceCode = serviceCode ?? throw new ArgumentNullException(nameof(orgName));
        _backlink = "https://lk.gosuslugi.ru/notifications"; // Значение по умолчанию из спецификации
        return this;
    }

    public EpguRequestBuilder SetRecipient(string? oid, string? snils, string signType)
    {

        _snils = snils ?? string.Empty;
        _oid = oid ?? string.Empty;
        _signType = signType == "kap" ? SignType.Ukep : SignType.Unep;

        return this;
    }

    public EpguRequestBuilder AddDocument(string documentId, string mimeType, string description, DateTime signExpiration)
    {
        if (documentId.Length > 50) throw new ArgumentException("DocumentId не может превышать 50 символов.");
        if (description.Length > 250) throw new ArgumentException("Description не может превышать 250 символов.");

        _documents.Add(new EpguDocumentInfo
        {
            DocumentId = documentId,
            MimeType = mimeType,
            Description = description,
            SignExpiration = signExpiration
        });

        return this;
    }

    public EpguRequestBuilder AddDocument(EpguDocumentInfo documentInfo)
    {
        if (documentInfo.DocumentId.Length > 50) throw new ArgumentException("DocumentId не может превышать 50 символов.");
        if (documentInfo.Description.Length > 250) throw new ArgumentException("Description не может превышать 250 символов.");

        _documents.Add(documentInfo);

        return this;
    }

    public string Build()
    {
        if (_documents.Count == 0)
            throw new InvalidOperationException("В заявление должен быть добавлен хотя бы один документ.");

        string nsUri = _signType == SignType.Ukep
            ? "urn://mpkey.gosuslugi.ru/sign_document_ukep/1.0.0"
            : "urn://mpkey.gosuslugi.ru/sign_document/1.0.0";

        var ns = XNamespace.Get(nsUri);

        var docElements = new List<XElement>();
        foreach (var doc in _documents)
        {
            docElements.Add(new XElement(ns + "Document",
                new XElement(ns + "DocumentId", doc.DocumentId),
                new XElement(ns + "MimeType", doc.MimeType),
                new XElement(ns + "Description", doc.Description),
                new XElement(ns + "Backlink", _backlink),
                new XElement(ns + "SignExpiration", doc.SignExpiration.ToString("o")), // ISO 8601
                new XElement(ns + "Attribute",
                    new XElement(ns + "AttributeName", "mnemonics"),
                    new XElement(ns + "AttributeValue", _mnemonics)
                ),
                new XElement(ns + "Attribute",
                    new XElement(ns + "AttributeName", "serviceName"),
                    new XElement(ns + "AttributeValue", _serviceName)
                ),
                new XElement(ns + "Attribute",
                    new XElement(ns + "AttributeName", "orgName"),
                    new XElement(ns + "AttributeValue", _orgName)
                )
            ));
        }

        var root = new XElement(ns + "SignRequest",
            string.IsNullOrWhiteSpace(_snils) ? null : new XElement(ns + "Snils", _snils),
            docElements,
            string.IsNullOrWhiteSpace(_oid) ? null : new XElement(ns + "OID", _oid)
        );

        var xDocument = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        return xDocument.ToString(SaveOptions.DisableFormatting);
    }

    public byte[] BuildBytes() => System.Text.Encoding.UTF8.GetBytes(Build());


}

