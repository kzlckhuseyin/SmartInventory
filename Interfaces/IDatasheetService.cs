public interface IDatasheetService
{
    Task<Guid> UploadAndQueueAsync(IFormFile file);
    Task ProcessDatasheetJobAsync(Guid datasheetId);
}