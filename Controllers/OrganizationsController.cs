using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradingApp.Models;

namespace TradingApp.Controllers
{
    [Authorize]
    public class OrganizationsController : Controller
    {
        private readonly TradingAppContext _context;

        public OrganizationsController(TradingAppContext context)
        {
            _context = context;
        }

        // GET: Organizations
        public async Task<IActionResult> Index()
        {
              return _context.Organizations != null ? 
                          View(await _context.Organizations.ToListAsync()) :
                          Problem("Entity set 'TradingAppContext.Organizations'  is null.");
        }

        // GET: Organizations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Organizations == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // GET: Organizations/Create
        public IActionResult Create(bool IsOnboarding)
        {
            ViewBag.IsOnboarding = IsOnboarding;

            return View();
        }

        // POST: Organizations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,DefaultCurrency,ImageFile")] Organization organization, bool IsOnboarding)
        {
            if (ModelState.IsValid)
            {
                string FolderName  = RemoveInvalidCharacters(organization.Name);
                organization.FolderName= FolderName;
                // Save the uploaded image to the server
                string imagePath = await SaveImageAsync(organization.ImageFile, FolderName);
                organization.LogoUrl= imagePath;
                _context.Add(organization);
                await _context.SaveChangesAsync();
                int newOrganizationId = organization.Id;
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
              

                if (int.TryParse(userId, out int userIdInt))
                {
                    var user = await _context.AppUsers.FindAsync(userIdInt);
                    user.DefaultOrganization = newOrganizationId;
                    await _context.SaveChangesAsync();
                }

                if (IsOnboarding)
                {
                    // If it's part of onboarding flow, redirect to create customer action
                    return RedirectToAction("Create", "Customer", new { IsOnboarding = true});
                }
                else
                {
                    // If it's not part of onboarding flow, redirect to index action
                    return RedirectToAction(nameof(Index));
                }

                //  return RedirectToAction(nameof(Index));
            }
            ViewBag.IsOnboarding = IsOnboarding;

            return View(organization);
        }

        // GET: Organizations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Organizations == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }
            return View(organization);
        }

        // POST: Organizations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,LogoUrl,IsActive,CreatedBy,CraetedOn,EditedBy,EditedOn")] Organization organization)
        {
            if (id != organization.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(organization);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationExists(organization.Id))
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
            return View(organization);
        }

        // GET: Organizations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Organizations == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        // POST: Organizations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Organizations == null)
            {
                return Problem("Entity set 'TradingAppContext.Organizations'  is null.");
            }
            var organization = await _context.Organizations.FindAsync(id);
            if (organization != null)
            {
                _context.Organizations.Remove(organization);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrganizationExists(int id)
        {
          return (_context.Organizations?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private async Task<string> SaveImageAsync(IFormFile imageFile,string FolderName)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                // Handle case where no image is uploaded
                return null;
            }

            // Save the uploaded image to the server
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Media", FolderName);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            string imagePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(imagePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/Media/{FolderName}/{uniqueFileName}"; // Return the relative path to the saved image
        }

        public static string RemoveInvalidCharacters(string input)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("", input.Where(c => !invalidChars.Contains(c))).Trim();
        }
    }
}
