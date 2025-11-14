using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Models;

namespace StudentPlanner.Controllers
{
    public class DayEntriesController : Controller
    {
        private readonly AppDbContext _context;

        public DayEntriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: DayEntries
        public async Task<IActionResult> Index()
        {
            return View(await _context.DayEntries.ToListAsync());
        }

        // GET: DayEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dayEntry = await _context.DayEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dayEntry == null)
            {
                return NotFound();
            }

            return View(dayEntry);
        }

        // GET: DayEntries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DayEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Course,TaskDescription,Hours,Status,Notes")] DayEntry dayEntry)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dayEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dayEntry);
        }

        // GET: DayEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dayEntry = await _context.DayEntries.FindAsync(id);
            if (dayEntry == null)
            {
                return NotFound();
            }
            return View(dayEntry);
        }

        // POST: DayEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Course,TaskDescription,Hours,Status,Notes")] DayEntry dayEntry)
        {
            if (id != dayEntry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dayEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DayEntryExists(dayEntry.Id))
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
            return View(dayEntry);
        }

        // GET: DayEntries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dayEntry = await _context.DayEntries
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dayEntry == null)
            {
                return NotFound();
            }

            return View(dayEntry);
        }

        // POST: DayEntries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dayEntry = await _context.DayEntries.FindAsync(id);
            if (dayEntry != null)
            {
                _context.DayEntries.Remove(dayEntry);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DayEntryExists(int id)
        {
            return _context.DayEntries.Any(e => e.Id == id);
        }
    }
}
