using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DatasheetsController : ControllerBase
{
    private readonly IDatasheetService _datasheetService;
    public DatasheetsController(IDatasheetService datasheetService)
    {
        _datasheetService = datasheetService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] DatasheetUploadDto dto)
    {
        var datasheetId = await _datasheetService.UploadAndQueueAsync(dto.File);
        return Accepted(new
        {
            Message = "PDF başarıyla yüklendi ve işleme kuyruğuna alındı.",
            DatasheetId = datasheetId
        });
    }

}