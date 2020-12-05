using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem =new Models.MenuItem()
            };
        }

        public async Task<IActionResult> Index()
        {
            var menuItem = await _db.MenuItems
                .Include(m=>m.Category)
                .Include(m=>m.SubCategory)
                .ToListAsync();
            return View(menuItem);
        }

        //Get - Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        //Create - Post
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _db.MenuItems.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            //work on image saving
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemfromDb = await _db.MenuItems.FindAsync(MenuItemVM.MenuItem.Id);
            if (files.Count > 0)
            {
                //file has been uploaded.
                var uploads = Path.Combine(webRootPath, "images");
                var extention = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extention ), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }

                menuItemfromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extention;
            }
            else
            {
                //no file was uploaded. so use default.
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                menuItemfromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Get - Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItems
                .Include(m => m.Category)
                .Include(m=> m.SubCategory)
                .SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = await _db.SubCategory
                .Where(s => s.CategoryId == MenuItemVM.MenuItem.SubCategoryId).ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //Edit - Post
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategory =await _db.SubCategory
                    .Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId)
                    .ToListAsync();
                return View(MenuItemVM);
            }
            //work on image updatin
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menuItemfromDb = await _db.MenuItems.FindAsync(MenuItemVM.MenuItem.Id);
            if (files.Count > 0)
            {
                //file has been uploaded.
                var uploads = Path.Combine(webRootPath, "images");
                var extention_new = Path.GetExtension(files[0].FileName);

                //Delete the original File
                var imgPath = Path.Combine(webRootPath, menuItemfromDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imgPath))
                {
                    System.IO.File.Delete(imgPath);
                }

                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extention_new), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }

                menuItemfromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extention_new;
            }

            menuItemfromDb.Name = MenuItemVM.MenuItem.Name;
            menuItemfromDb.Price = MenuItemVM.MenuItem.Price;
            menuItemfromDb.Description = MenuItemVM.MenuItem.Description;
            menuItemfromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemfromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;
            menuItemfromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Get - Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItems
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .SingleOrDefaultAsync(m => m.Id == id);
            //MenuItemVM.SubCategory = await _db.SubCategory
            //    .Where(s => s.CategoryId == MenuItemVM.MenuItem.SubCategoryId).ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //Delete - Get
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItems
                .Include(m => m.Category)
                .Include(m => m.SubCategory)
                .SingleOrDefaultAsync(m => m.Id == id);
            //MenuItemVM.SubCategory = await _db.SubCategory
            //    .Where(s => s.CategoryId == MenuItemVM.MenuItem.SubCategoryId).ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }
        //Delete - Post
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
           
            //MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            //MenuItemVM.MenuItem = await _db.MenuItems
            //    .Include(s=>s.Category)
            //    .Include(s=>s.SubCategory)
            //    .SingleOrDefaultAsync(s => s.Id == id);
            //work on image delete
            string webRootPath = _webHostEnvironment.WebRootPath;
            MenuItem menuItem = await _db.MenuItems.FindAsync(id);
            //var menuItemfromDb = await _db.MenuItems.FindAsync(MenuItemVM.MenuItem.Id);
            if (menuItem != null)
            {

                //Delete the original File
                var imgPath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imgPath))
                {
                    System.IO.File.Delete(imgPath);
                }

            }


            _db.MenuItems.Remove(menuItem);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
