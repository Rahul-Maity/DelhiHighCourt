using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DelhiHighCourt;
[Route("api/[controller]")]
[ApiController]
public class ScrapController : ControllerBase
{
    private readonly ScrapingService _scrapingService;
    private readonly AppDbContext _context;
    public ScrapController(ScrapingService scrapingService, AppDbContext context)
    {

        _scrapingService = scrapingService;
        _context = context;
    }

    [HttpPost("scrape")]
    public async Task<IActionResult> Scrape()
    {
        await _scrapingService.ScrapeDataAsync();
        return Ok();
    }


    [HttpDelete("deleteall")]
    public async Task<IActionResult> DeleteAll()
    {
        // Fetch all data from the database
        var allCaseDetails = _context.caseDetails.ToList();

        if (!allCaseDetails.Any())
        {
            return NotFound("No data found to delete.");
        }

        // Remove all entities
        _context.caseDetails.RemoveRange(allCaseDetails);

        // Save changes to the database
        await _context.SaveChangesAsync();

        return Ok("All data has been deleted successfully.");
    }
}
 
