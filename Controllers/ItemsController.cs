using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradingApp.Models;

namespace TradingApp.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly TradingAppContext _context;

        public ItemsController(TradingAppContext context)
        {
            _context = context;
        }

        // GET: Items
        public async Task<IActionResult> Index()
        {
            var tradingAppContext = _context.Items.Include(i => i.SaleUnitNavigation);
            return View(await tradingAppContext.ToListAsync());
        }

        //laoding without logo
        //public async Task<IActionResult> Index()
        //{
        //    var tradingAppContext = _context.Items
        //        .Include(i => i.SaleUnitNavigation) // You can still include the SaleUnitNavigation if needed
        //        .Select(item => new
        //        {
        //            item.Id,
        //            item.Name,
        //            item.Description,
        //            item.Price,
        //            item.SaleUnit
        //        });

        //    var items = await tradingAppContext.ToListAsync();

        //    // Convert the anonymous type back to the model if needed
        //    var itemList = items.Select(i => new TradingApp.Models.Item
        //    {
        //        Id = i.Id,
        //        Name = i.Name,
        //        Description = i.Description,
        //        Price = i.Price,
        //        SaleUnit = i.SaleUnit
        //    }).ToList();

        //    return View(itemList);
        //}


        // GET: Items/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.SaleUnitNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        // GET: Items/Create
        public IActionResult Create(bool IsOnboarding)
        {
            ViewBag.IsOnboarding = IsOnboarding;

            ViewData["SaleUnits"] = new SelectList(_context.MeasureUnits, "Id", "Name");
            ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name");
            ViewData["ProductBrand"] = new SelectList(_context.ProductBrands, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     [Bind("Id, Name, Description, Price, SaleUnit, Code, Barcode, QRCode, Color, Material, Size, Length, Width, Height, Weight, ProductBrandId, ProductCategoryId")]
    Item item,
     bool IsOnboarding,
     IFormFile? thumbnailFile,
     IEnumerable<IFormFile> productMediaAssets) // Now it's called productMediaAssets
        {
            if (ModelState.IsValid)
            {
                // Handle Thumbnail (saving it in the database)
                if (thumbnailFile != null && thumbnailFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await thumbnailFile.CopyToAsync(memoryStream);
                        item.Thumbnail = Convert.ToBase64String(memoryStream.ToArray()); // Store as Base64 string
                    }
                }

                // Handle Product Media Assets (store them on file server and save the path in DB)
                if (productMediaAssets != null && productMediaAssets.Any())
                {
                    var mediaFilePaths = await SaveMediaFilesAsync(productMediaAssets);
                    item.ProductMediaAssets = SaveMediaPathsToDatabase(mediaFilePaths); // Save as comma-separated paths
                }

                // Add the item to the database
                _context.Add(item);
                await _context.SaveChangesAsync();

                if (IsOnboarding)
                {
                    return RedirectToAction("Create", "Invoices", new { id = 0 });
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }
            }

            // If model is not valid, return to the view with existing data
            ViewBag.IsOnboarding = IsOnboarding;
            ViewData["SaleUnits"] = new SelectList(_context.MeasureUnits, "Id", "Name");
            ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name");
            ViewData["ProductBrand"] = new SelectList(_context.ProductBrands, "Id", "Name");
            return View(item);
        }

        private async Task<List<string>> SaveMediaFilesAsync(IEnumerable<IFormFile> mediaFiles)
        {
            var mediaFilePaths = new List<string>();

            foreach (var mediaFile in mediaFiles)
            {
                var fileName = Path.GetFileName(mediaFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                // Save the file to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await mediaFile.CopyToAsync(stream);
                }

                // Add the relative path to the list
                mediaFilePaths.Add("/uploads/" + fileName); // Store the relative path
            }

            return mediaFilePaths;
        }

        private string SaveMediaPathsToDatabase(List<string> mediaFilePaths)
        {
            // Store media file paths (image or video paths) in the item
            // The paths are stored as a comma-separated string
            return string.Join(",", mediaFilePaths);
        }


        public async Task<IActionResult> Edit(int? id, bool IsOnboarding = false)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Set ViewData for dropdowns
            ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name", item.ProductCategoryId);
            ViewData["ProductBrand"] = new SelectList(_context.ProductBrands, "Id", "Name", item.ProductBrandId);
            ViewData["SaleUnits"] = new SelectList(_context.MeasureUnits, "Id", "Name", item.SaleUnit);



            // Pass IsOnboarding flag
            ViewBag.IsOnboarding = IsOnboarding;

            return View("Create", item); // Reuse the Create view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
      int id,
      [Bind("Id, Name, Description, Price, SaleUnit, Code, Barcode, QRCode, Color, Material, Size, Length, Width, Height, Weight, ProductBrandId, ProductCategoryId, Thumbnail")]
    Item item,
      bool IsOnboarding,
      IFormFile? thumbnailFile,
      IEnumerable<IFormFile> productMediaAssets)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the existing item from the database
                    var existingItem = await _context.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
                    if (existingItem == null)
                    {
                        return NotFound();
                    }

                    // Handle Thumbnail
                    if (thumbnailFile != null && thumbnailFile.Length > 0)
                    {
                        // Update thumbnail only if a new file is uploaded
                        using (var memoryStream = new MemoryStream())
                        {
                            await thumbnailFile.CopyToAsync(memoryStream);
                            item.Thumbnail = Convert.ToBase64String(memoryStream.ToArray()); // Store as Base64 string
                        }
                    }
                    else
                    {
                        // Retain the existing thumbnail if no new file is uploaded
                        item.Thumbnail = existingItem.Thumbnail;
                    }

                    // Handle Product Media Assets
                    if (productMediaAssets != null && productMediaAssets.Any())
                    {
                        // If new files are uploaded, save them and update the database
                        var mediaFilePaths = await SaveMediaFilesAsync(productMediaAssets);
                        item.ProductMediaAssets = SaveMediaPathsToDatabase(mediaFilePaths); // Save new paths
                    }
                    else
                    {
                        // Retain existing media assets if no new files are uploaded
                        item.ProductMediaAssets = existingItem.ProductMediaAssets;
                    }

                    // Update the item in the database
                    _context.Update(item);
                    await _context.SaveChangesAsync();

                    if (IsOnboarding)
                    {
                        return RedirectToAction("Create", "Invoices", new { id = 0 });
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ItemExists(item.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If model is not valid, return to the Create view with existing data
            ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name", item.ProductCategoryId);
            ViewData["ProductBrand"] = new SelectList(_context.ProductBrands, "Id", "Name", item.ProductBrandId);
            ViewData["SaleUnits"] = new SelectList(_context.MeasureUnits, "Id", "Name");
            ViewBag.IsOnboarding = IsOnboarding;

            return View("Create", item); // Reuse the Create view
        }


        // GET: Items/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Items == null)
            {
                return NotFound();
            }

            var item = await _context.Items
                .Include(i => i.SaleUnitNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (item == null)
            {

                return NotFound();
            }

            ViewData["SaleUnits"] = new SelectList(_context.MeasureUnits, "Id", "Name");
            ViewData["ProductCategory"] = new SelectList(_context.ProductCategories, "Id", "Name");
            ViewData["ProductBrand"] = new SelectList(_context.ProductBrands, "Id", "Name");
            return View(item);
        }

        // POST: Items/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Items == null)
            {
                return Problem("Entity set 'TradingAppContext.Items'  is null.");
            }
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("[controller]/{itemId}/Media")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMedia(int itemId, IFormFile productMediaAssets)
        {
            if (productMediaAssets == null)
            {
                return BadRequest(new { message = "No media file provided" });
            }

            var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null)
            {
                return NotFound(new { message = "Item not found" });
            }

            try
            {
                var fileName = Path.GetFileName(productMediaAssets.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productMediaAssets.CopyToAsync(stream);
                }

                var mediaPath = "/uploads/" + fileName;

                // Append new media path to existing ones
                var existingMediaPaths = item.ProductMediaAssets?.Split(',') ?? Array.Empty<string>();
                item.ProductMediaAssets = string.Join(",", existingMediaPaths.Append(mediaPath));

                _context.Update(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Media uploaded successfully", newMediaPaths = mediaPath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading media", error = ex.Message });
            }
        }



        [HttpDelete("[controller]/{itemId}/Media")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int itemId, [FromQuery] string mediaPath)
        {
            if (string.IsNullOrEmpty(mediaPath))
            {
                return NotFound(); // Ensure media path is valid
            }

            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                return NotFound(); // Ensure item exists
            }

            
            var decodedMediaPath = Uri.UnescapeDataString(mediaPath);
            mediaPath = decodedMediaPath;

            // Remove the media path from the item's media assets
            var mediaPaths = item.ProductMediaAssets?.Split(',').ToList();
            if (mediaPaths != null && mediaPaths.Contains(mediaPath))
            {
                mediaPaths.Remove(mediaPath); // Remove the specific image path
                item.ProductMediaAssets = string.Join(",", mediaPaths); // Update the media paths

                _context.Update(item);
                await _context.SaveChangesAsync();

                // Optionally delete the image from the server
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", mediaPath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); // Delete file from server
                }

                return Json(new { success = true, message = "Media remove successfully!" });
            }

            return NotFound(); // Return NotFound if media path is invalid or not found
        }
        [HttpDelete("[controller]/{id}/logo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLogo(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound(); // Ensure item exists
            }

            // Check if the item has a thumbnail
            if (!string.IsNullOrEmpty(item.Thumbnail))
            {
                // Delete the thumbnail image from the server (if it exists)
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", item.Thumbnail); // Assuming the thumbnail is stored in the "uploads" folder
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath); // Delete file from server
                }

                // Remove the thumbnail from the database
                item.Thumbnail = null;
                _context.Update(item);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Logo remove successfully!" });
        }
        [HttpPut("[controller]/{id}/logo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditThumbnail(
    int id,
    IFormFile? thumbnailFile)
        {
            if (thumbnailFile == null || thumbnailFile.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound(); // Ensure item exists
            }

            // Handle Thumbnail (update it if a new file is uploaded)
            using (var memoryStream = new MemoryStream())
            {
                await thumbnailFile.CopyToAsync(memoryStream);
                item.Thumbnail = Convert.ToBase64String(memoryStream.ToArray()); // Store the thumbnail as a Base64 string
            }

            // Update the item in the database
            _context.Update(item);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Logo edit successfully!" });
        }


        private bool ItemExists(int id)
        {
            return (_context.Items?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
