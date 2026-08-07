using System.Xml.Linq;

namespace Smev;

public static class SmevSoapBuilder
{
    public static string BuildEnvelope(string requestXml, IReadOnlyList<SmevDocumentInfo> documents)
    {
        XNamespace soapenv = "http://www.w3.org/2003/05/soap-envelope";
        XNamespace smev = "urn://x-artefacts-smev-gov-ru/services/message-exchange/types/1.2";

        var requestDoc = XDocument.Parse(requestXml);

        var attachmentHeaderList = new XElement(smev + "AttachmentHeaderList");
        foreach (var doc in documents)
        {
            attachmentHeaderList.Add(
                new XElement(smev + "AttachmentHeader",
                    new XElement(smev + "contentId", $"cid:{doc.DocumentUuid}"),
                    new XElement(smev + "fileName", doc.FileName),
                    new XElement(smev + "mimeType", doc.MimeType),
                    new XElement(smev + "fileSize", doc.Content.Length.ToString())
                ),
                new XElement(smev + "AttachmentHeader",
                    new XElement(smev + "contentId", $"cid:{doc.SignatureUuid}"),
                    new XElement(smev + "fileName", $"{doc.FileName}.sig"),
                    new XElement(smev + "mimeType", "application/sig"),
                    new XElement(smev + "fileSize", doc.SignatureContent.Length.ToString())
                )
            );
        }

        var envelope = new XElement(soapenv + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
            new XAttribute(XNamespace.Xmlns + "smev", smev.NamespaceName),
            new XElement(soapenv + "Header"),
            new XElement(soapenv + "Body",
                new XElement(smev + "sendRequestRequest", 
                    new XElement(smev + "SenderProvidedRequestData",
                        new XElement(smev + "MessageID", $"urn:uuid:{Guid.NewGuid()}"),
                        new XElement(smev + "MessagePrimaryContent",
                            requestDoc.Root!
                        ),
                        attachmentHeaderList
                    )
                )
            )
        );

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), envelope)
            .ToString(SaveOptions.DisableFormatting);
    }
}