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
using Stripe;

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
            Models.Address address = artistAddressVM.Address;
            address = await _gMapService.ConvertStreetToLongLat(address);

            if (ModelState.IsValid)
            {
                try
                {
                    if (address.Id == 0)
                    {
                        _context.Add(address);
                        await _context.SaveChangesAsync();
                        Models.Address savedAddress = await _context.Addresses.Where(a => a.Equals(address)).FirstOrDefaultAsync();
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

        public async Task<IActionResult> Availability(string error = null)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();

            ArtistAvailability aAvailability = new ArtistAvailability
            {
                ArtistId = artist.Id,
                AAvailabilitiesList = await _context.ArtistAvailabilities.Where(a => a.ArtistId.Equals(artist.Id)).ToListAsync()
            };

            ViewData["Error"] = error;
            ViewData["DaysOfWeek"] = new SelectList(_schedOptService.DaysOfTheWeek);
            ViewData["Hours"] = new SelectList(_schedOptService.Hours);
            ViewData["Minutes"] = new SelectList(_schedOptService.Minutes);
            ViewData["TimeOfDay"] = new SelectList(_schedOptService.TimeOfDay);
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

                    if (aAvailability.StartTime > aAvailability.EndTime || aAvailability.StartTime == aAvailability.EndTime)
                    {
                        return RedirectToAction("Availability", new { error = "Start Time cannot be after (or equal to) EndTime" });
                    }

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

        public async Task<IActionResult> BusinessOpenings(int? id, string dayToCheck = null, string error = null)
        {
            var business = await _context.Businesses.Where(b => b.Id.Equals(id)).Include(b => b.Address).SingleOrDefaultAsync();
            List<BusinessAvailability> businessAvailabilities = new List<BusinessAvailability>();
            List<ArtistEvent> eventABookings = new List<ArtistEvent>();
            List<BusinessEvent> eventBBookings = new List<BusinessEvent>();

            if (dayToCheck == null)
            {
                dayToCheck = DateTime.Now.ToString();
            }
            DateTime dayToCheckDT = Convert.ToDateTime(dayToCheck);

            businessAvailabilities = await _context.BusinessAvailabilities.Where(b => b.BusinessId.Equals(id) && b.DayOfWeek.Equals(dayToCheckDT.DayOfWeek.ToString())).ToListAsync();

            eventABookings = await _context.ArtistEvents.Where(b => b.BusinessId.Equals(business.Id) && b.StartTime.Day.Equals(dayToCheckDT.Day)).ToListAsync();
            eventBBookings = await _context.BusinessEvents.Where(b => b.BusinessId.Equals(business.Id) && b.StartTime.Day.Equals(dayToCheckDT.Day)).ToListAsync();

            business.DayToCheck = dayToCheckDT;
            ViewData["Error"] = error;
            ViewData["BusinessAvailabilities"] = businessAvailabilities;
            ViewData["ArtistEventBookings"] = eventABookings;
            ViewData["BusinessEventBookings"] = eventBBookings;
            return View(business);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BusinessOpeningsNewDate(int? id, DateTime? dayToCheck)
        {
            string dayToString = dayToCheck.ToString();
            return RedirectToAction("BusinessOpenings", new { id = id, dayToCheck = dayToString });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookTime(int? id, Business bModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var bAvailForDayOfWeek = await _context.BusinessAvailabilities.Where(b => b.BusinessId.Equals(id) && b.DayOfWeek.Equals(bModel.DayToCheck.Value.DayOfWeek.ToString())).ToListAsync();
                    var bBookedBusEvents = await _context.BusinessEvents.Where(b => b.BusinessId.Equals(bModel.Id) && b.StartTime.Day.Equals(bModel.DayToCheck.Value.Day)).ToListAsync();
                    var bBookedArtEvents = await _context.ArtistEvents.Where(b => b.BusinessId.Equals(bModel.Id) && b.StartTime.Day.Equals(bModel.DayToCheck.Value.Day)).ToListAsync();

                    DateTime startBook = new DateTime(bModel.DayToCheck.Value.Year, bModel.DayToCheck.Value.Month, bModel.DayToCheck.Value.Day, bModel.StartTime.Value.Hour, bModel.StartTime.Value.Minute, 00);
                    DateTime endBook = new DateTime(bModel.DayToCheck.Value.Year, bModel.DayToCheck.Value.Month, bModel.DayToCheck.Value.Day, bModel.EndTime.Value.Hour, bModel.EndTime.Value.Minute, 00);

                    foreach (var avail in bAvailForDayOfWeek)
                    {
                        DateTime startAvail = new DateTime(bModel.DayToCheck.Value.Year, bModel.DayToCheck.Value.Month, bModel.DayToCheck.Value.Day, avail.StartTime.Hour, avail.StartTime.Minute, 00);
                        DateTime endAvail = new DateTime(bModel.DayToCheck.Value.Year, bModel.DayToCheck.Value.Month, bModel.DayToCheck.Value.Day, avail.EndTime.Hour, avail.EndTime.Minute, 00);

                        bool passesAvail = _schedOptService.CompareAvailabilityToBookTimes(startAvail, endAvail, startBook, endBook);
                        if (passesAvail)
                        {
                            bool passesEvents = true;
                            foreach (var busEvent in bBookedBusEvents)
                            {
                                passesEvents = _schedOptService.CompareEventTimesToBookTimes(busEvent.StartTime, busEvent.EndTime, startBook, endBook);
                                if (passesEvents == false)
                                {
                                    return RedirectToAction("BusinessOpenings", new { id = bModel.Id, dayToCheck = bModel.DayToCheck.Value, error = "Event already booked during that time" });
                                }
                            }
                            foreach (var artEvent in bBookedArtEvents)
                            {
                                passesEvents = _schedOptService.CompareEventTimesToBookTimes(artEvent.StartTime, artEvent.EndTime, startBook, endBook);
                                if (passesEvents == false)
                                {
                                    return RedirectToAction("BusinessOpenings", new { id = bModel.Id, dayToCheck = bModel.DayToCheck.Value, error = "Event already booked during that time" });
                                }
                            }
                        }
                        else
                        {
                            return RedirectToAction("BusinessOpenings", new { id = bModel.Id, dayToCheck = bModel.DayToCheck.Value, error = "Outside of Availability" });
                        }

                    }
                    return RedirectToAction("CreateEvent", new { bId = bModel.Id, bookStart = startBook, bookEnd = endBook });
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }

            return RedirectToAction("BusinessOpenings", new { id = bModel.Id, dayToCheck = bModel.DayToCheck.Value });
        }

        public async Task<IActionResult> CreateEvent(int bId, DateTime bookStart, DateTime bookEnd, string error = null)
        {
            double totalCost;
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();
            var business = await _context.Businesses.Where(b => b.Id.Equals(bId)).SingleOrDefaultAsync();

            double totalHours = (bookEnd - bookStart).TotalHours;
            if (bookStart.DayOfWeek.ToString() == "Saturday" || bookStart.DayOfWeek.ToString() == "Sunday")
            {
                totalCost = (totalHours * business.WeekendHourlyCost.Value);
            }
            else
            {
                totalCost = (totalHours * business.HourlyCost.Value);
            }

            ArtistEvent artEvent = new ArtistEvent
            {
                Artist = artist,
                Business = business,
                StartTime = bookStart,
                EndTime = bookEnd,
                Cost = totalCost
            };

            ViewBag.StripePublishKey = Secrets.STRIPES_PUBLIC_KEY;
            return View(artEvent);
        }

        [HttpPost]
        public async Task<IActionResult> PaymentConfirmation(string stripeToken, string description, string stripeEmail, int amount, ArtistEvent artistEvent)
        {
            StripeConfiguration.ApiKey = Secrets.STRIPES_API_KEY;

            var myCharge = new ChargeCreateOptions
            {
                Amount = amount,
                Currency = "USD",
                ReceiptEmail = stripeEmail,
                Description = description,
                Source = stripeToken,
                Capture = true
            };
            var chargeService = new ChargeService();

            try
            {
                Charge stripeCharge = chargeService.Create(myCharge);
                _context.ArtistEvents.Add(artistEvent);
                await _context.SaveChangesAsync();

                double amountPaid = Convert.ToDouble(myCharge.Amount);
                ViewBag.PaymentInfo = myCharge;
                ViewBag.PaymentTotal = Math.Round(amountPaid / 100, 2);
                return View(true);
            }
            catch (Exception exceptionThrown)
            {
                ViewBag.Exception = exceptionThrown;
                return View(false);
            }
        }

        public async Task<IActionResult> Schedule(DateTime? dayToCheck = null)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var artist = await _context.Artists.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();
            if (dayToCheck == null)
            {
                dayToCheck = DateTime.Now;
            }
            var businessEvents = await _context.BusinessEvents.Where(b => b.ArtistId.Equals(artist.Id)).ToListAsync();
            var artistEvents = await _context.ArtistEvents.Where(a => a.ArtistId.Equals(artist.Id)).ToListAsync();
            var artistEventsToday = artistEvents.Where(a => a.StartTime.Day.Equals(dayToCheck.Value.Date)).ToList();

            ViewData["BusinessEvents"] = businessEvents;
            ViewData["BusinessEventsToday"] = businessEvents.Where(b => b.StartTime.Day.Equals(dayToCheck.Value.Date)).ToList();
            ViewData["ArtistEvents"] = artistEvents;
            return View(artistEventsToday);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(DateTime dayToCheck)
        {


            return RedirectToAction("Schedule", new { dayToCheck = dayToCheck });
        }



    }
}
