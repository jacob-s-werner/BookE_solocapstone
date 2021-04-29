using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookEWebsite.Data;
using BookEWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BookEWebsite.Views
{
    [Authorize(Roles = "Business")]
    public class BusinessesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BusinessesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> RegisterAccount(Business business)
        {
            _context.Add(business);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Businesses
        public async Task<IActionResult> Index()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();
            
            if (!business.CompletedRegistration)
            {
                return RedirectToAction(nameof(Edit));
            }

            return View(business);
        }

        // GET: Businesses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await _context.Businesses
                .Include(b => b.Address)
                .Include(b => b.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // GET: Businesses/Edit/5
        public async Task<IActionResult> Edit()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();
            BusinessAddressVM businessAddressVM = new BusinessAddressVM
            {
                Business = business,

                Address = null
            };

            if (business.AddressId != null)
            {
                businessAddressVM.Address = await _context.Addresses.Where(a => a.Id.Equals(business.AddressId)).FirstOrDefaultAsync();
            }

            if (business == null)
            {
                return NotFound();
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", business.IdentityUserId);
            return View(businessAddressVM);
        }

        // POST: Businesses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BusinessAddressVM businessAddressVM)
        {
            Business business = businessAddressVM.Business;
            Address address = businessAddressVM.Address;
            //insert Google Geocoding here to get long,lat (make method/service) add validation so address is required

            if (ModelState.IsValid)
            {
                try
                {
                    if (address.Id == 0)
                    {
                        _context.Add(address);
                        await _context.SaveChangesAsync();
                        Address savedAddress = await _context.Addresses.Where(a => a.Equals(address)).FirstOrDefaultAsync();
                        business.AddressId = savedAddress.Id;
                    }
                    else
                    {
                        _context.Update(address);
                        await _context.SaveChangesAsync();
                    }

                    business.CompletedRegistration = true;
                    _context.Update(business);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessExists(business.Id))
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
            return View(businessAddressVM);
        }

        // GET: Businesses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await _context.Businesses
                .Include(b => b.Address)
                .Include(b => b.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // POST: Businesses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var business = await _context.Businesses.FindAsync(id);
            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusinessExists(int id)
        {
            return _context.Businesses.Any(e => e.Id == id);
        }
        public async Task<IActionResult> Availability()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();

            BusinessAvailability bAvailability = new BusinessAvailability
            {
                BusinessId = business.Id,
                bAvailabilitiesList = await _context.BusinessAvailabilities.Where(a => a.BusinessId.Equals(business.Id)).ToListAsync()
            };

            ViewData["DaysOfWeek"] = new SelectList(new List<string>() { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" });
            return View(bAvailability);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Availability(BusinessAvailability bAvailability)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(bAvailability);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Availability));
        }

        public async Task<IActionResult> AvailabilityDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bAvailability = await _context.BusinessAvailabilities.Where(a => a.Id.Equals(id))
                .FirstOrDefaultAsync();
            if (bAvailability == null)
            {
                return NotFound();
            }

            return View(bAvailability);
        }

        [HttpPost, ActionName("AvailabilityDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvailabilityDeleteConfirmed(int id)
        {
            var bAvailability = await _context.BusinessAvailabilities.FindAsync(id);
            _context.BusinessAvailabilities.Remove(bAvailability);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Availability));
        }



    }
}
