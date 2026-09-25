using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;

public class DatasheetProcessingService : IDatasheetProcessingService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DatasheetProcessingService> _logger;

    public DatasheetProcessingService(AppDbContext context, HttpClient httpClient, ILogger<DatasheetProcessingService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task ProcessPdfAndExtractDataAsync(Guid datasheetId)
    {
        //log
        _logger.LogInformation("==> [AI Process] İşlem başladı. DatasheetId: {DatasheetId}", datasheetId);

        var datasheet = await _context.Datasheets.FirstOrDefaultAsync(x => x.Id == datasheetId);
        if (datasheet == null || !File.Exists(datasheet.FilePath))
        {
            //log
            _logger.LogError("==> [AI Process] Datasheet veya dosya bulunamadı! Path: {Path}", datasheet?.FilePath);
            throw new FileNotFoundException("Datasheet veya PDF dosyası bulunamadı.");
        }
        // 1. PdfPig ile Kritik Sayfaları Oku (İlk 2 sayfa ve Son sayfa)

        //log
        _logger.LogInformation("==> [AI Process] PDF metni çıkarılıyor: {FilePath}", datasheet.FilePath);
        string extractedText = ExtractCriticalPagesText(datasheet.FilePath);

        //log
        _logger.LogInformation("==> [AI Process] PDF okundu. Çıkarılan metin uzunluğu: {Length} karakter.", extractedText.Length);


        // 2. Ollama API Çağrısı Yap
        _logger.LogInformation("==> [AI Process] Ollama API çağrısı başlatılıyor (Model: qwen2.5:7b)...");
        var extractedData = await CallOllamaApiAsync(extractedText);

        if (extractedData == null)
        {
            _logger.LogError("==> [AI Process] Ollama'dan veri dönmedi veya JSON parse edilemedi.");
            throw new Exception("LLM datasheettan anlamlı bir veri çıkaramadı.");
        }
        // 3. Üretici (Manufacturer) Kontrolü / Kaydı
        var manufacturer = await _context.Manufacturers
            .FirstOrDefaultAsync(m => m.Name.ToLower() == extractedData.ManufacturerName.ToLower());

        _logger.LogInformation("==> [AI Process] LLM Veriyi Başarıyla Çıkardı: PartNumber={PartNumber}, Manufacturer={Manufacturer}",
            extractedData.PartNumber, extractedData.ManufacturerName);

        if (manufacturer == null)
        {
            manufacturer = new Manufacturer
            {
                Id = Guid.NewGuid(),
                Name = string.IsNullOrWhiteSpace(extractedData.ManufacturerName) ? "Unknown" : extractedData.ManufacturerName,
                CreatedBy = "AI_EXTRACTOR"
            };
            await _context.Manufacturers.AddAsync(manufacturer);
            await _context.SaveChangesAsync();
            _logger.LogInformation("==> [AI Process] Yeni üretici oluşturuldu: {Name}", manufacturer.Name);
        }

        // 4. Onaysız Parça (Part) Olarak Veritabanına Yaz
        var part = new Part
        {
            Id = Guid.NewGuid(),
            ManufacturerId = manufacturer.Id,
            PartNumber = extractedData.PartNumber,
            Category = extractedData.Category,
            PackageType = extractedData.PackageType,
            PinCount = extractedData.PinCount,
            MinOperatingTemp = extractedData.MinOperatingTemp,
            MaxOperatingTemp = extractedData.MaxOperatingTemp,
            MinVoltage = extractedData.MinVoltage,
            MaxVoltage = extractedData.MaxVoltage,
            IsMilSpec = extractedData.IsMilSpec,
            IsApproved = false, // Mühendis panelinde onay bekleyecek
            AdditionalFeatures = JsonSerializer.Serialize(extractedData.AdditionalFeatures),
            CreatedBy = "AI_EXTRACTOR"
        };

        await _context.Parts.AddAsync(part);
        datasheet.PartId = part.Id;
        // Datasheet Statüsünü Tamamlandı Yap
        datasheet.Status = ProcessingStatus.Completed;
        await _context.SaveChangesAsync();

        _logger.LogInformation("==> [AI Process] İşlem BAŞARIYLA tamamlandı! PartId: {PartId}", part.Id);

    }

    private string ExtractCriticalPagesText(string filePath)
    {
        var textBuilder = new StringBuilder();
        using (var pdf = PdfDocument.Open(filePath))
        {
            int totalPages = pdf.NumberOfPages;
            var targetPages = new HashSet<int>();

            // İlk 2 sayfa özet bilgileri barındırır
            if (totalPages >= 1) targetPages.Add(1);
            if (totalPages >= 2) targetPages.Add(2);
            // Son sayfa kılıf/paket boyutlarını barındırır
            if (totalPages >= 3) targetPages.Add(totalPages);

            foreach (var pageNum in targetPages)
            {
                var page = pdf.GetPage(pageNum);
                textBuilder.AppendLine($"--- PAGE {pageNum} ---");
                textBuilder.AppendLine(page.Text);
            }
        }

        return textBuilder.ToString();
    }

    private async Task<PartExtractedDto?> CallOllamaApiAsync(string pdfContent)
    {
        var prompt = $@"
Extract the electronic component specifications from the provided datasheet text.
RULES:
1. Return ONLY a valid, raw JSON object.
2. No markdown formatting, no code blocks (```json), no explanations.
3. If you cannot find a string value, write ""Unknown"".
4. If you cannot find a number value, write 0.

JSON SCHEMA MUST EXACTLY MATCH THIS:

{{
  ""PartNumber"": ""string"",
  ""ManufacturerName"": ""string"",
  ""Category"": ""string"",
  ""PackageType"": ""string"",
  ""PinCount"": 0,
  ""MinOperatingTemp"": 0.0,
  ""MaxOperatingTemp"": 0.0,
  ""MinVoltage"": 0.0,
  ""MaxVoltage"": 0.0,
  ""IsMilSpec"": false,
  ""AdditionalFeatures"": {{}}
}}

Datasheet Content:
{pdfContent}
";

        var requestBody = new
        {
            model = "qwen2.5:3b",
            prompt = prompt,
            stream = false,
            format = "json",
            options = new
            {
                num_thread = 6, // Bilgisayarındaki fiziksel çekirdek sayısı (Örn: 4, 8, 12)
                temperature = 0.1
            }
        };

        // BaseAddress Program.cs'te tanımlandığı için bağıl adres yeterli
        var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", requestBody);
        response.EnsureSuccessStatusCode();

        var resultJson = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        if (resultJson == null || string.IsNullOrEmpty(resultJson.Response))
            return null;
        // Konsolda LLM'in tam olarak ne ürettiğini görmek için raw veriyi basıyoruz
        _logger.LogInformation("==> [AI Process] Ollama Raw Response: {Response}", resultJson.Response);

        return JsonSerializer.Deserialize<PartExtractedDto>(resultJson.Response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        });

    }

    private class OllamaResponse
    {
        public string Response { get; set; } = string.Empty;
    }
}