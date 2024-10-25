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
    public class PremiumPackagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PremiumPackagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/PremiumPackages
        public async Task<IActionResult> Index()
        {
            return View(await _context.PremiumPackages.ToListAsync());
        }

        // GET: Admin/PremiumPackages/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var premiumPackage = await _context.PremiumPackages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (premiumPackage == null)
            {
                return NotFound();
            }

            return View(premiumPackage);
        }

        // GET: Admin/PremiumPackages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/PremiumPackages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,DurationInMonths")] PremiumPackage premiumPackage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(premiumPackage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(premiumPackage);
        }

        // GET: Admin/PremiumPackages/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var premiumPackage = await _context.PremiumPackages.FindAsync(id);
            if (premiumPackage == null)
            {
                return NotFound();
            }
            return View(premiumPackage);
        }

        // POST: Admin/PremiumPackages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,DurationInMonths")] PremiumPackage premiumPackage)
        {
            if (id != premiumPackage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(premiumPackage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PremiumPackageExists(premiumPackage.Id))
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
            return View(premiumPackage);
        }

        // GET: Admin/PremiumPackages/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var premiumPackage = await _context.PremiumPackages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (premiumPackage == null)
            {
                return NotFound();
            }

            return View(premiumPackage);
        }

        // POST: Admin/PremiumPackages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var premiumPackage = await _context.PremiumPackages.FindAsync(id);
            if (premiumPackage != null)
            {
                _context.PremiumPackages.Remove(premiumPackage);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PremiumPackageExists(int id)
        {
            return _context.PremiumPackages.Any(e => e.Id == id);
        }
    }
}
