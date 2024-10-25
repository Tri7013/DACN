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
    public class ChaptersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChaptersController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Chapters.Include(c => c.Product);
            return View(await applicationDbContext.ToListAsync());
        }
        // GET: Admin/Chapters/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var chapter = await _context.Chapters
                .Include(c => c.Product) // Nếu bạn cần truy xuất thông tin sản phẩm liên quan
                .FirstOrDefaultAsync(m => m.Id == id);

            if (chapter == null)
            {
                return NotFound();
            }

            // Thiết lập ViewData cho WebHostEnvironment
            ViewData["WebHostEnvironment"] = _webHostEnvironment;

            return View(chapter);
        }

        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Description");
            return View();
        }
        // POST: Admin/Chapters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,ProductId,CreatedAt,ChapterNumber,FilePath")] Chapter chapter, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "chapters");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    if (file != null && file.Length > 0)
                    {
                        var extension = Path.GetExtension(file.FileName).ToLower();
                        var allowedExtensions = new[] { ".pdf", ".docx", ".txt" };

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("", "Chỉ chấp nhận các định dạng tệp: pdf, docx, txt.");
                            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Description", chapter.ProductId);
                            return View(chapter);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        chapter.FilePath = "/chapters/" + uniqueFileName;
                    }

                    _context.Add(chapter);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.InnerException?.Message);
                }

                ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Description", chapter.ProductId);
                return View(chapter);
            }

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Description", chapter.ProductId);
            return View(chapter);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Description", chapter.ProductId);
            return View(chapter);
        }
        // POST: Admin/Chapters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ProductId,CreatedAt,ChapterNumber,FilePath")] Chapter chapter, IFormFile file)
        {
            if (id != chapter.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (file != null && file.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "chapters");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        chapter.FilePath = "/chapters/" + uniqueFileName;
                    }

                    _context.Update(chapter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChapterExists(chapter.Id))
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

            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Description", chapter.ProductId);
            return View(chapter);
        }

        // GET: Admin/Chapters/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapter = await _context.Chapters
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chapter == null)
            {
                return NotFound();
            }

            return View(chapter);
        }

        // POST: Admin/Chapters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter != null)
            {
                _context.Chapters.Remove(chapter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChapterExists(int id)
        {
            return _context.Chapters.Any(e => e.Id == id);
        }
    }
}
