namespace Repo;

public class EpguOrder
{
    public int Id { get; set; }
    public string? EpguOrderCode { get; set; }
    public string? OrderStatusId { get; set; } = "";
    public string SignatureType { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; }
}