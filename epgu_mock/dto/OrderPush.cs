

using Microsoft.AspNetCore.SignalR;

namespace Dto;

public class OrderCreate
{
    required public string region { get; set; }
    required public string serviceCode { get; set; }
    required public string targetCode { get; set; }
    required public IFormFile file { get; set; }
}