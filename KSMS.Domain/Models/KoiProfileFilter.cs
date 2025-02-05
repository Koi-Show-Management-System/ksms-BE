
namespace KSMS.Domain.Models;

public class KoiProfileFilter
{
    public List<Guid> VarietyIds { get; set; } = [];
    
    public decimal StartSize { get; set; }
    
    public decimal EndSize { get; set; }
    
    public string Name { get; set; } = string.Empty;
}