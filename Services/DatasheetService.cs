using Hangfire;
using Microsoft.EntityFrameworkCore;

public class DatasheetService : IDatasheetService
{
    private readonly AppDbContext _context;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IDatasheetProcessingService _processingService;

    public DatasheetService(AppDbContext context, IBackgroundJobClient backgroundJobClient, IDatasheetProcessingService datasheetProcessingService)
    {
        _context = context;
        _backgroundJobClient = backgroundJobClient;
        _processingService = datasheetProcessingService;
    }

    // Hangfire Arka Plan İşçisinin Çağıracağı Metod
    public async Task ProcessDatasheetJobAsync(Guid datasheetId)
    {
        var datasheet = await _context.Datasheets.FirstOrDefaultAsync(x => x.Id == datasheetId);

        if (datasheet == null)
        {
            return;
        }

        try
        {
            datasheet.Status = ProcessingStatus.Processing;
            await _context.SaveChangesAsync();

            // TODO (Bir sonraki adım)
            // 1. PdfPig ile metin ayıklama
            // 2. Ollama (Qwen/Llama) API çağrısı
            // AI İşleme Servisini Çağır
            await _processingService.ProcessPdfAndExtractDataAsync(datasheetId);
            // 3. JSON verisini Part olarak IsApproved = false kaydetme

            // Geçici olarak tamamlandı işaretleyelim
            datasheet.Status = ProcessingStatus.Completed;
            await _context.SaveChangesAsync();
        }
        catch (Exception Ex)
        {
            datasheet.Status = ProcessingStatus.Failed;
            await _context.SaveChangesAsync();
            throw; // Hangfire hatayı yakalayıp dashboard'da gösterir
        }
    }

    public async Task<Guid> UploadAndQueueAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Geçersiz veya boş dosya.");
        }

        if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
        {
            throw new ArgumentException("Yalnızca PDF dosyaları yüklenebilir.");
        }

        // 1. Sunucuda PDF depolama klasörünü hazırla
        var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "datasheets");

        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadFolder, uniqueFileName);

        // 2. Dosyayı diske kaydet
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 3. Veritabanına Pending statüsüyle kaydet
        var datasheet = new Datasheet
        {
            Id = Guid.NewGuid(),
            FilePath = filePath,
            OriginalFileName = file.FileName,
            Status = ProcessingStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM_UPLOAD"
        };

        await _context.Datasheets.AddAsync(datasheet);
        await _context.SaveChangesAsync();

        // 4. Hangfire Kuyruğuna Asenkron İş (Job) Bırak
        _backgroundJobClient.Enqueue<IDatasheetService>(service => service.ProcessDatasheetJobAsync(datasheet.Id));

        return datasheet.Id;
    }
}