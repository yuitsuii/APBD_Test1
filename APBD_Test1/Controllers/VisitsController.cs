using APBD_Test1.Models.DTOs;
using APBD_Test1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace APBD_Test1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisitsController : ControllerBase
    {
        private readonly IVisitService _visitsService;

        public VisitsController(IVisitService visitsService)
        {
            _visitsService = visitsService;
        }

        [HttpGet("{id}")] 
        public async Task<IActionResult> GetVisit(int id)
        {
            try
            {
                var visit = await _visitsService.GetVisit(id);
                return Ok(visit);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostVisit(VisitPostDTO visit)
        {
            try
            {
                var visitPost = await _visitsService.PostVisit(visit);
                return CreatedAtAction("GetVisit", new { id = visitPost.VisitId }, visitPost);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }

    }
}
