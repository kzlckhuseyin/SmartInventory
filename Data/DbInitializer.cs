using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Apply automatic migrations (creates the database if it does not exist)
        await context.Database.MigrateAsync();

        // If data already exists, do not reseed.
        if (await context.Manufacturers.AnyAsync())
            return;

        // 1. Manufacturers
        var stMicro = new Manufacturer
        {
            Id = Guid.NewGuid(),
            Name = "STMicroelectronics",
            Country = "Switzerland",
            IsRestricted = false,
            CreatedBy = "SEED"
        };

        var ti = new Manufacturer
        {
            Id = Guid.NewGuid(),
            Name = "Texas Instruments",
            Country = "USA",
            IsRestricted = false,
            CreatedBy = "SEED"
        };

        var microchip = new Manufacturer
        {
            Id = Guid.NewGuid(),
            Name = "Microchip Technology",
            Country = "USA",
            IsRestricted = false,
            CreatedBy = "SEED"
        };

        await context.Manufacturers.AddRangeAsync(stMicro, ti, microchip);

        // 2. Parts
        var stm32Standard = new Part
        {
            Id = Guid.NewGuid(),
            ManufacturerId = stMicro.Id,
            PartNumber = "STM32F407VGT6",
            Category = "Microcontroller",
            PackageType = "LQFP-100",
            PinCount = 100,
            MinOperatingTemp = -40,
            MaxOperatingTemp = 85,
            MinVoltage = 1.8m,
            MaxVoltage = 3.6m,
            IsMilSpec = false,
            IsApproved = true, // Onaylı parça
            AdditionalFeatures = "{\"Core\":\"Cortex-M4\",\"FlashKB\":1024,\"ClockMHz\":168}",
            CreatedBy = "SEED"
        };

        // Military Standard (MIL-STD) Equivalent of STM32
        var stm32MilSpec = new Part
        {
            Id = Guid.NewGuid(),
            ManufacturerId = stMicro.Id,
            PartNumber = "STM32F407VGT6-MIL",
            Category = "Microcontroller",
            PackageType = "LQFP-100",
            PinCount = 100,
            MinOperatingTemp = -55,
            MaxOperatingTemp = 125, // Askeri sıcaklık dayanımı
            MinVoltage = 1.8m,
            MaxVoltage = 3.6m,
            IsMilSpec = true,
            IsApproved = true,
            AdditionalFeatures = "{\"Core\":\"Cortex-M4\",\"FlashKB\":1024,\"ClockMHz\":168}",
            CreatedBy = "SEED"
        };

        // Voltage Regulator from a Different Manufacturer
        var lm7805 = new Part
        {
            Id = Guid.NewGuid(),
            ManufacturerId = ti.Id,
            PartNumber = "LM7805CT",
            Category = "Voltage Regulator",
            PackageType = "TO-220",
            PinCount = 3,
            MinOperatingTemp = 0,
            MaxOperatingTemp = 125,
            MinVoltage = 7.0m,
            MaxVoltage = 25.0m,
            IsMilSpec = false,
            IsApproved = true,
            AdditionalFeatures = "{\"OutputVoltage\":5.0,\"OutputCurrentA\":1.5}",
            CreatedBy = "SEED"
        };

        await context.Parts.AddRangeAsync(stm32Standard, stm32MilSpec, lm7805);
        await context.SaveChangesAsync();
    }
}