using APBD_Test1.Models.DTOs;

namespace APBD_Test1.Services;

public interface IVisitService
{
    public Task<VisitRequestDTO> GetVisit(int visitId);
    public Task<VisitPostDTO> PostVisit(VisitPostDTO visit);
}