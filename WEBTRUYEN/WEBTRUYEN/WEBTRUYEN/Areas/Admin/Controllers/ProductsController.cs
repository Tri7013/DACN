using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBTRUYEN.Data;
using WEBTRUYEN.Models;

namespace WEBTRUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            return View(await _context.Products.ToListAsync());
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync(); // Lấy danh sách thể loại từ bảng Categories
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel viewModel, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    IsPremium = viewModel.IsPremium,
                };

                // Xử lý file hình ảnh
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var imageFileName = Path.GetFileName(imageFile.FileName);
                    var imageFilePath = Path.Combine(uploadsFolder, imageFileName);

                    using (var stream = new FileStream(imageFilePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn file vào thuộc tính ImageUrl
                    product.ImageUrl = "/Uploads/" + imageFileName; // Đường dẫn tương đối để hiển thị ảnh
                }
                else
                {
                    // Nếu không có hình ảnh, có thể đặt giá trị mặc định hoặc xử lý theo cách của bạn
                    ModelState.AddModelError("ImageFile", "Hình ảnh không được để trống.");
                    return View(viewModel); // Quay lại view với thông báo lỗi
                }

                _context.Add(product);
                await _context.SaveChangesAsync(); // Lưu sản phẩm trước

                // Thêm các thể loại vào sản phẩm
                if (viewModel.SelectedCategoryIds != null && viewModel.SelectedCategoryIds.Count > 0)
                {
                    foreach (var categoryId in viewModel.SelectedCategoryIds)
                    {
                        var productCategory = new ProductCategory
                        {
                            ProductId = product.Id,
                            CategoryId = categoryId
                        };
                        _context.ProductCategories.Add(productCategory);
                    }

                    await _context.SaveChangesAsync(); // Lưu các thể loại liên kết
                }
                else
                {
                    // Nếu không có thể loại nào được chọn, có thể thêm thông báo lỗi
                    ModelState.AddModelError("SelectedCategoryIds", "Bạn phải chọn ít nhất một thể loại.");
                    return View(viewModel); // Quay lại view với thông báo lỗi
                }

                TempData["SuccessMessage"] = "Sản phẩm đã được tạo thành công."; // Thêm thông báo thành công
                return RedirectToAction(nameof(Index));
            }

            // Nếu ModelState không hợp lệ, lấy lại danh sách thể loại
            var categories = await _context.Categories.ToListAsync(); // Lấy danh sách thể loại từ bảng Categories
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(viewModel);
        }


        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsPremium,CreatedAt,ImageUrl,IsActive,Likes,ViewCount")] Product product, IFormFile imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý file hình ảnh
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var imageFileName = Path.GetFileName(imageFile.FileName);
                        var imageFilePath = Path.Combine(uploadsFolder, imageFileName);

                        using (var stream = new FileStream(imageFilePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // Lưu đường dẫn file vào thuộc tính ImageUrl
                        product.ImageUrl = "/Uploads/" + imageFileName; // Đường dẫn tương đối để hiển thị ảnh
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sản phẩm đã được cập nhật thành công."; // Thêm thông báo thành công
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sản phẩm đã bị xóa thành công."; // Thêm thông báo thành công
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
