using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEBTRUYEN.Data;
using WEBTRUYEN.Models;

namespace WEBTRUYEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ChapterCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChapterCommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ChapterComments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ChapterComments.Include(c => c.Chapter).Include(c => c.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Admin/ChapterComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapterComment = await _context.ChapterComments
                .Include(c => c.Chapter)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chapterComment == null)
            {
                return NotFound();
            }

            return View(chapterComment);
        }

        // GET: Admin/ChapterComments/Create
        public IActionResult Create()
        {
            ViewData["ChapterId"] = new SelectList(_context.Chapters, "Id", "Title");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/ChapterComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,CreatedAt,UserId,ChapterId")] ChapterComment chapterComment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chapterComment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChapterId"] = new SelectList(_context.Chapters, "Id", "Title", chapterComment.ChapterId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", chapterComment.UserId);
            return View(chapterComment);
        }

        // GET: Admin/ChapterComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapterComment = await _context.ChapterComments.FindAsync(id);
            if (chapterComment == null)
            {
                return NotFound();
            }
            ViewData["ChapterId"] = new SelectList(_context.Chapters, "Id", "Title", chapterComment.ChapterId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", chapterComment.UserId);
            return View(chapterComment);
        }

        // POST: Admin/ChapterComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,CreatedAt,UserId,ChapterId")] ChapterComment chapterComment)
        {
            if (id != chapterComment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chapterComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChapterCommentExists(chapterComment.Id))
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
            ViewData["ChapterId"] = new SelectList(_context.Chapters, "Id", "Title", chapterComment.ChapterId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", chapterComment.UserId);
            return View(chapterComment);
        }

        // GET: Admin/ChapterComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chapterComment = await _context.ChapterComments
                .Include(c => c.Chapter)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chapterComment == null)
            {
                return NotFound();
            }

            return View(chapterComment);
        }

        // POST: Admin/ChapterComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chapterComment = await _context.ChapterComments.FindAsync(id);
            if (chapterComment != null)
            {
                _context.ChapterComments.Remove(chapterComment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChapterCommentExists(int id)
        {
            return _context.ChapterComments.Any(e => e.Id == id);
        }
    }
}
