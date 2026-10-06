using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateMarketplace.Data;
using RealEstateMarketplace.Models;

namespace RealEstateMarketplace.Controllers
{
    public class PropertiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PropertiesController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Properties
        public async Task<IActionResult> Index()
        {
            var properties = await _context.Properties.ToListAsync();

            return View(properties);
        }

        // GET: Properties/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Properties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Property property,
            IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "properties");

                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName =
                        Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                    string filePath = Path.Combine(
                        uploadsFolder,
                        uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    property.ImagePath = "/uploads/properties/" + uniqueFileName;
                }

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(property);
        }

        // GET: Properties/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties.FindAsync(id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // POST: Properties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Property property,
            IFormFile? imageFile)
        {
            if (id != property.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "properties");

                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName =
                        Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                    string filePath = Path.Combine(
                        uploadsFolder,
                        uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    property.ImagePath = "/uploads/properties/" + uniqueFileName;
                }

                _context.Properties.Update(property);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(property);
        }

        // GET: Properties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // GET: Properties/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        // POST: Properties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties.FindAsync(id);

            if (property != null)
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}