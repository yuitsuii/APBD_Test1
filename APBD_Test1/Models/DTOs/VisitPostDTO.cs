using System.Text.Json.Serialization;

namespace APBD_Test1.Models.DTOs;

public class VisitPostDTO
{
    public int VisitId { get; set; }
    public int ClientId { get; set; }
    
    [JsonPropertyName("mechanicLicenceNumber")]
    public string LicenceNumber { get; set; }
    
    [JsonPropertyName("Services")]
    public List<ServiceRequestDTO> VisitServices { get; set; }
}

public class ServiceRequestDTO
{
    [JsonPropertyName("serviceName")]  
    public string Name { get; set; }   
    
    [JsonPropertyName("serviceFee")]
    public decimal Fee { get; set; }
}