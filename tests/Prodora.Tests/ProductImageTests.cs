using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Prodora.Business.Abstract;
using Prodora.Entitys;
using Prodora.WebUI.Audit;
using Prodora.WebUI.Controllers;
using Prodora.WebUI.Images;
using Prodora.WebUI.Models;

namespace Prodora.Tests;

public sealed class ProductImageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "Prodora_ImageChecks_" + Guid.NewGuid().ToString("N"));
    private readonly ProductImageStorage storage;
    private readonly IProductServices products = Substitute.For<IProductServices>();
    private readonly ICategoryServices categories = Substitute.For<ICategoryServices>();
    private readonly AdminController controller;

    public ProductImageTests()
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.WebRootPath.Returns(root);
        storage = new(environment, NullLogger<ProductImageStorage>.Instance);
        categories.GetAll().Returns([new Category { Id = 1, Name = "Test" }]);
        categories.GetById(1).Returns(new Category { Id = 1, Name = "Test" });
        controller = new(products, categories, storage, Substitute.For<IAuditLog>(), NullLogger<AdminController>.Instance);
    }

    private static IFormFile File() => new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "files", "photo.png");
    private static ProductModel Form() => new()
    { Id = 7, Name = "Test product", Description = "Description", Brand = "Brand", Price = 10, CategoryId = "1", Stock = true };

    [Fact]
    public async Task A_partial_upload_failure_cleans_both_the_completed_and_partial_files()
    {
        var failed = Substitute.For<IFormFile>();
        failed.FileName.Returns("second.png");
        failed.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>()).Returns(async call =>
        {
            await call.Arg<Stream>().WriteAsync(new byte[] { 9 });
            throw new IOException("Synthetic upload failure");
        });

        await Assert.ThrowsAsync<IOException>(() => storage.SaveAsync([File(), failed]));

        Assert.Empty(Directory.GetFiles(Path.Combine(root, "img")));
    }

    [Fact]
    public async Task Failed_product_creation_leaves_no_uploaded_files()
    {
        products.When(p => p.Create(Arg.Any<Product>())).Do(_ => throw new InvalidOperationException("Synthetic database failure"));

        var result = await controller.CreateProduct(Form(), [File(), File(), File(), File()]);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Empty(Directory.GetFiles(Path.Combine(root, "img")));
    }

    [Fact]
    public async Task Failed_product_update_cleans_new_files_and_keeps_the_existing_image()
    {
        var existing = (await storage.SaveAsync([File()])).Single();
        existing.Id = 2;
        products.GetProductDetail(7).Returns(new Product { Id = 7, Images = [existing] });
        products.Update(Arg.Any<Product>(), Arg.Any<int[]>(), Arg.Any<int[]>()).Throws(new InvalidOperationException("Synthetic database failure"));
        var model = Form();
        model.RemovedImageIds = [2];

        var result = await controller.EditProduct(model, [File()], [1]);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(existing.ImageUrl, Path.GetFileName(Assert.Single(Directory.GetFiles(Path.Combine(root, "img")))));
        Assert.Same(existing, Assert.Single(model.Images));
        Assert.Equal(new[] { 2 }, model.RemovedImageIds);
    }

    [Fact]
    public async Task Removing_another_products_image_is_rejected_before_uploading()
    {
        products.GetProductDetail(7).Returns(new Product { Id = 7, Images = [new Image { Id = 2, ImageUrl = "old.png" }] });
        var model = Form();
        model.RemovedImageIds = [999];

        Assert.IsType<ViewResult>(await controller.EditProduct(model, [File()], [1]));

        Assert.False(Directory.Exists(Path.Combine(root, "img")));
        products.DidNotReceive().Update(Arg.Any<Product>(), Arg.Any<int[]>(), Arg.Any<int[]>());
    }

    [Fact]
    public async Task Cleanup_is_limited_to_generated_uploads_inside_the_image_folder()
    {
        var uploaded = (await storage.SaveAsync([File()])).Single();
        var outsideName = Guid.NewGuid().ToString("N") + ".png";
        var outsidePath = Path.Combine(root, outsideName);
        await System.IO.File.WriteAllTextAsync(outsidePath, "keep");
        var bundled = Path.Combine(root, "img", "product-placeholder.svg");
        await System.IO.File.WriteAllTextAsync(bundled, "keep");

        storage.Delete(["../" + outsideName, "product-placeholder.svg", uploaded.ImageUrl]);

        Assert.True(System.IO.File.Exists(outsidePath));
        Assert.True(System.IO.File.Exists(bundled));
        Assert.False(System.IO.File.Exists(Path.Combine(root, "img", uploaded.ImageUrl!)));
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
