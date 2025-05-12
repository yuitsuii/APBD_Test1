namespace APBD_Test1.Models.DTOs;

public class VisitGetDTO
{
    public DateTime Date {get; set;}
    public ClientDTO Client {get; set;}
    public MechanicDTO Mechanic {get; set;}
    public List<VisitServiceDTO> Visits {get; set;}
}

public class ClientDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public class MechanicDTO
{
    public int MechanicId { get; set; }
    public string LicenceNumber  { get; set; }
}

public class VisitServiceDTO
{
    public string Name { get; set; }
    public decimal ServiceFee { get; set; }
}