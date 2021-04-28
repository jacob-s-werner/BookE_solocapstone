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

namespace BookEWebsite.Controllers
{
    [Authorize(Roles = "Artist")]
    public class ArtistsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArtistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> RegisterAccount(Artist artist)
        {
            _context.Add(artist);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Artists
        public async Task<IActionResult> Index()
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();
            
            if (!artist.CompletedRegistration)
            {
                return RedirectToAction(nameof(Edit));
            }
            var applicationDbContext = _context.Artists.Include(a => a.Address).Include(a => a.IdentityUser);

            return View(await applicationDbContext.ToListAsync());
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

        // GET: Artists/Create
        public IActionResult Create()
        {
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "Id");
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Artists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,PhoneNumber,EmailAddress,Specialization,Description,LookingForGigs,GroupName,SizeOfGroup,HourlyCost,WeekendHourlyCost,IdentityUserId,AddressId")] Artist artist)
        {
            if (ModelState.IsValid)
            {
                _context.Add(artist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "Id", artist.AddressId);
            ViewData["IdentityUserId"] = new SelectList(_context.Users, "Id", "Id", artist.IdentityUserId);
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
            //insert Google Geocoding here to get long,lat (make method/service) add validation so address is required

            if (ModelState.IsValid)
            {
                try
                {
                    if (address.Id == 0 || address.Id == null)
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
    }
}
