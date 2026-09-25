public interface IDatasheetProcessingService
{
    Task ProcessPdfAndExtractDataAsync(Guid datasheetId);
}