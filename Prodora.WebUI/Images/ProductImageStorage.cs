using Prodora.Entitys;

namespace Prodora.WebUI.Images;

/// <summary>Owns only generated upload filenames in wwwroot/img; bundled and historic filenames are never deleted.</summary>
public sealed class ProductImageStorage(IWebHostEnvironment environment, ILogger<ProductImageStorage> logger)
{
    private static readonly string[] Extensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<List<Image>> SaveAsync(IEnumerable<IFormFile> files)
    {
        var images = new List<Image>();
        var folder = Path.Combine(environment.WebRootPath, "img");
        Directory.CreateDirectory(folder);
        try
        {
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!Extensions.Contains(extension)) throw new InvalidOperationException("Unsupported image extension.");
                var name = Guid.NewGuid().ToString("N") + extension;
                await using var stream = new FileStream(Path.Combine(folder, name), FileMode.CreateNew);
                // Track only files opened by this call, including a partially written upload.
                images.Add(new Image { ImageUrl = name });
                await file.CopyToAsync(stream);
            }
            return images;
        }
        catch
        {
            Delete(images.Select(i => i.ImageUrl));
            throw;
        }
    }

    public void Delete(IEnumerable<string?> filenames)
    {
        foreach (var name in filenames.Where(n => n != null).Distinct())
        {
            // Do not interpret legacy values, path components, or static assets as upload paths.
            if (name != Path.GetFileName(name) || !Extensions.Contains(Path.GetExtension(name).ToLowerInvariant())
                || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out _)) continue;
            try { File.Delete(Path.Combine(environment.WebRootPath, "img", name!)); }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            { logger.LogWarning("An unused product upload could not be removed; error type {ErrorType}", exception.GetType().Name); }
        }
    }
}
