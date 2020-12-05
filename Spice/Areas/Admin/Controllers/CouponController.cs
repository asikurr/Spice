using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }
        //get - Create
        public IActionResult Create()
        {
            return View();
        }
        //Post - create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnCreate(Coupon coupons)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }

                    coupons.Picture = p1;
                }

                _db.Coupon.Add(coupons);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(coupons);
        }
        //edit - get
        public async Task<IActionResult> Edit(int? id)
        {
            if (id==null)
            {
                return NotFound();
            }

            var coupons =await _db.Coupon.SingleOrDefaultAsync(c => c.Id == id);
            if (coupons == null)
            {
                return NotFound();
            }
            return View(coupons);
        }

        //post - edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupons)
        {
            if (coupons.Id == 0)
            {
                return NotFound();
            }
            var couponFromDB = await _db.Coupon.Where(c => c.Id == coupons.Id).FirstOrDefaultAsync();
            //var couponFromDB = await _db.Coupon
            //    .SingleOrDefaultAsync(c => c.Id == coupon.Id);
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    couponFromDB.Picture = p1;
                }

                couponFromDB.Name = coupons.Name;
                couponFromDB.CouponType = coupons.CouponType;
                couponFromDB.Discount = coupons.Discount;
                couponFromDB.MinimumAmount = coupons.MinimumAmount;
                couponFromDB.isActive = coupons.isActive;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(coupons);
        }

        //get - Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _db.Coupon.SingleOrDefaultAsync(c => c.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        //get - Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _db.Coupon.SingleOrDefaultAsync(c => c.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        //post - delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            var coupon = await _db.Coupon.FirstOrDefaultAsync(c => c.Id == id);
            _db.Coupon.Remove(coupon);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
