

namespace Dto;
public class DocumentGet
{
    public string Name { get; set; }
    public string LocalName { get; set; }

    public DocumentGet(Repo.Document document)
    {
        this.Name = document.Name;
        this.LocalName = document.LocalName;
    }
}