using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DelhiHighCourt;
[Route("api/[controller]")]
[ApiController]
public class ScrapController : ControllerBase
{
    private readonly ScrapingService _scrapingService;
    public ScrapController(ScrapingService scrapingService)
    {

        _scrapingService = scrapingService;

    }

    [HttpPost("scrape")]
    public async Task<IActionResult> Scrape()
    {
        await _scrapingService.ScrapeDataAsync();
        return Ok();
    }
 }
