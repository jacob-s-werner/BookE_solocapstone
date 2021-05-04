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
using BookEWebsite.Services.GoogleMapsSvc;
using BookEWebsite.Services.ScheduleOptionsSvc;

namespace BookEWebsite.Controllers
{
    [Authorize(Roles = "Artist")]
    public class ArtistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleMapsService _gMapService;
        private readonly ScheduleOptionsService _schedOptService;

        public ArtistsController(ApplicationDbContext context, GoogleMapsService gMapService, ScheduleOptionsService schedOptService)
        {
            _context = context;
            _gMapService = gMapService;
            _schedOptService = schedOptService;
        }

        public async Task<IActionResult> RegisterAccount(Artist artist)
        {
            _context.Add(artist);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Artists
        public async Task<IActionResult> Index(double? searchLat = null, double? searchLong = null)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists.Where(c => c.IdentityUserId == userId).Include(a => a.Address).SingleOrDefaultAsync();

            if (!artist.CompletedRegistration)
            {
                return RedirectToAction(nameof(Edit));
            }

            if (searchLat == null || searchLong == null)
            {
                searchLat = artist.Address.Latitude;
                searchLong = artist.Address.Longitude;
            }

            artist.CenterLatitude = searchLat;
            artist.CenterLongitude = searchLong;

            ViewData["BusinessMarkers"] = await _context.Businesses.Include(a => a.Address).ToListAsync();
            ViewData["BusinessAvailabilities"] = await _context.BusinessAvailabilities.Include(b => b.Business).ToListAsync();
            return View(artist);
        }

        // GET: Artists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists
                .Include(a => a.Address)
                .Include(a => a.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        // GET: Artists/Edit/5
        public async Task<IActionResult> Edit()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();
            ArtistAddressVM artistAddressVM = new ArtistAddressVM
            {
                Artist = artist,

                Address = null
            };

            if (artist.AddressId != null)
            {
                artistAddressVM.Address = await _context.Addresses.Where(a => a.Id.Equals(artist.AddressId)).FirstOrDefaultAsync();
            }

            if (artist == null)
            {
                return NotFound();
            }
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", artist.IdentityUserId);
            return View(artistAddressVM);
        }

        // POST: Artists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ArtistAddressVM artistAddressVM)
        {
            Artist artist = artistAddressVM.Artist;
            Address address = artistAddressVM.Address;
            address = await _gMapService.ConvertStreetToLongLat(address);

            if (ModelState.IsValid)
            {
                try
                {
                    if (address.Id == 0)
                    {
                        _context.Add(address);
                        await _context.SaveChangesAsync();
                        Address savedAddress = await _context.Addresses.Where(a => a.Equals(address)).FirstOrDefaultAsync();
                        artist.AddressId = savedAddress.Id;
                    }
                    else
                    {
                        _context.Update(address);
                        await _context.SaveChangesAsync();
                    }

                    artist.CompletedRegistration = true;
                    _context.Update(artist);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtistExists(artist.Id))
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
            return View(artistAddressVM);
        }

        // GET: Artists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists
                .Include(a => a.Address)
                .Include(a => a.IdentityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        // POST: Artists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Availability()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();
            
            ArtistAvailability aAvailability = new ArtistAvailability { ArtistId = artist.Id, 
                aAvailabilitiesList = await _context.ArtistAvailabilities.Where(a => a.ArtistId.Equals(artist.Id)).ToListAsync() };

            ViewData["DaysOfWeek"] = new SelectList(new List<string>(){ "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" });
            return View(aAvailability);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Availability(ArtistAvailability aAvailability)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(aAvailability);
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

            var aAvailability = await _context.ArtistAvailabilities.Where(a => a.Id.Equals(id))
                .FirstOrDefaultAsync();
            if (aAvailability == null)
            {
                return NotFound();
            }

            return View(aAvailability);
        }
       
        [HttpPost, ActionName("AvailabilityDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvailabilityDeleteConfirmed(int id)
        {
            var aAvailability = await _context.ArtistAvailabilities.FindAsync(id);
            _context.ArtistAvailabilities.Remove(aAvailability);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Availability));
        }

        public async Task<IActionResult> BusinessOpenings(int? id, string dayToCheck = null)
        {
            var business = await _context.Businesses.Where(b => b.Id.Equals(id)).Include(b => b.Address).SingleOrDefaultAsync();
            List<BusinessAvailability> businessAvailabilities;
            if (dayToCheck == null)
            {
                dayToCheck = DateTime.Now.ToString();
            }
            DateTime dayToCheckDT = Convert.ToDateTime(dayToCheck);
            
            businessAvailabilities = await _context.BusinessAvailabilities.Where(b => b.BusinessId.Equals(id) && b.DayOfWeek.Equals(dayToCheckDT.DayOfWeek.ToString())).ToListAsync();

            business.DayToCheck = dayToCheckDT;
            ViewData["BusinessAvailabilities"] = businessAvailabilities;
            return View(business);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BusinessOpeningsNewDate(int? id, DateTime? dayToCheck)
        {
            string dayToString = dayToCheck.ToString();
            return RedirectToAction("BusinessOpenings", new { id = id, dayToCheck = dayToString });
        }



    }
}
