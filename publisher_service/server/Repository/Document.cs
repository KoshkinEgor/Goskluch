

namespace Repo;

public class Document
{
    public int Id { get; set; }
    public string Name { get; set; } // оригинальное имя файла: "Договор.pdf"
    public string LocalName { get; set; } // имя файла в файловой системе сервера: uuid
    public string ZipFileName { get; set; } // преобразованное имя файла: "Dogovor.pdf"
    public string DocumentEpguCode { get; set; } // идентификатор документа для тега <DocumentId> в req.xml: 1896644961
    public Order Order { get; set; } 

}