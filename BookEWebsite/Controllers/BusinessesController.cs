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

namespace BookEWebsite.Views
{
    [Authorize(Roles = "Business")]
    public class BusinessesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GoogleMapsService _gMapService;
        private readonly ScheduleOptionsService _schedOptService;

        public BusinessesController(ApplicationDbContext context, GoogleMapsService gMapService, ScheduleOptionsService schedOptService)
        {
            _context = context;
            _gMapService = gMapService;
            _schedOptService = schedOptService;

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

            ViewData["ArtistAvailabilities"] = await _context.ArtistAvailabilities.Include(b => b.Artist).Include(b => b.Artist.Address).ToListAsync();
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
            Models.Address address = businessAddressVM.Address;
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
        public async Task<IActionResult> Availability(string error = null)
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();

            BusinessAvailability bAvailability = new BusinessAvailability
            {
                BusinessId = business.Id,
                BAvailabilitiesList = await _context.BusinessAvailabilities.Where(a => a.BusinessId.Equals(business.Id)).ToListAsync()
            };

            ViewData["Error"] = error;
            ViewData["DaysOfWeek"] = new SelectList(_schedOptService.DaysOfTheWeek);
            ViewData["Hours"] = new SelectList(_schedOptService.Hours);
            ViewData["Minutes"] = new SelectList(_schedOptService.Minutes);
            ViewData["TimeOfDay"] = new SelectList(_schedOptService.TimeOfDay);
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

                    if (bAvailability.StartTime > bAvailability.EndTime || bAvailability.StartTime == bAvailability.EndTime)
                    {
                        return RedirectToAction("Availability", new { error = "Start Time cannot be after (or equal to) EndTime" });
                    }

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

        public async Task<IActionResult> ArtistOpenings(int? id, string dayToCheck = null, string error = null)
        {
            var artist = await _context.Artists.Where(b => b.Id.Equals(id)).Include(b => b.Address).SingleOrDefaultAsync();
            List<ArtistAvailability> artistAvailabilities = new List<ArtistAvailability>();
            List<ArtistEvent> eventABookings = new List<ArtistEvent>();
            List<BusinessEvent> eventBBookings = new List<BusinessEvent>();

            if (dayToCheck == null)
            {
                dayToCheck = DateTime.Now.ToString();
            }
            DateTime dayToCheckDT = Convert.ToDateTime(dayToCheck);

            artistAvailabilities = await _context.ArtistAvailabilities.Where(b => b.ArtistId.Equals(id) && b.DayOfWeek.Equals(dayToCheckDT.DayOfWeek.ToString())).ToListAsync();

            eventABookings = await _context.ArtistEvents.Where(b => b.ArtistId.Equals(artist.Id) && b.StartTime.Day.Equals(dayToCheckDT.Day)).ToListAsync();
            eventBBookings = await _context.BusinessEvents.Where(b => b.ArtistId.Equals(artist.Id) && b.StartTime.Day.Equals(dayToCheckDT.Day)).ToListAsync();

            artist.DayToCheck = dayToCheckDT;
            ViewData["Error"] = error;
            ViewData["ArtistAvailabilities"] = artistAvailabilities;
            ViewData["ArtistEventBookings"] = eventABookings;
            ViewData["BusinessEventBookings"] = eventBBookings;
            return View(artist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ArtistOpeningsNewDate(int? id, DateTime? dayToCheck)
        {
            string dayToString = dayToCheck.ToString();
            return RedirectToAction("ArtistOpenings", new { id = id, dayToCheck = dayToString });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookTime(int? id, Artist aModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var aAvailForDayOfWeek = await _context.ArtistAvailabilities.Where(b => b.ArtistId.Equals(id) && b.DayOfWeek.Equals(aModel.DayToCheck.Value.DayOfWeek.ToString())).ToListAsync();
                    var aBookedBusEvents = await _context.BusinessEvents.Where(b => b.ArtistId.Equals(aModel.Id) && b.StartTime.Day.Equals(aModel.DayToCheck.Value.Day)).ToListAsync();
                    var aBookedArtEvents = await _context.ArtistEvents.Where(b => b.ArtistId.Equals(aModel.Id) && b.StartTime.Day.Equals(aModel.DayToCheck.Value.Day)).ToListAsync();

                    DateTime startBook = new DateTime(aModel.DayToCheck.Value.Year, aModel.DayToCheck.Value.Month, aModel.DayToCheck.Value.Day, aModel.StartTime.Value.Hour, aModel.StartTime.Value.Minute, 00);
                    DateTime endBook = new DateTime(aModel.DayToCheck.Value.Year, aModel.DayToCheck.Value.Month, aModel.DayToCheck.Value.Day, aModel.EndTime.Value.Hour, aModel.EndTime.Value.Minute, 00);

                    foreach (var avail in aAvailForDayOfWeek)
                    {
                        DateTime startAvail = new DateTime(aModel.DayToCheck.Value.Year, aModel.DayToCheck.Value.Month, aModel.DayToCheck.Value.Day, avail.StartTime.Hour, avail.StartTime.Minute, 00);
                        DateTime endAvail = new DateTime(aModel.DayToCheck.Value.Year, aModel.DayToCheck.Value.Month, aModel.DayToCheck.Value.Day, avail.EndTime.Hour, avail.EndTime.Minute, 00);

                        bool passesAvail = _schedOptService.CompareAvailabilityToBookTimes(startAvail, endAvail, startBook, endBook);
                        if (passesAvail)
                        {
                            bool passesEvents = true;
                            foreach (var busEvent in aBookedBusEvents)
                            {
                                passesEvents = _schedOptService.CompareEventTimesToBookTimes(busEvent.StartTime, busEvent.EndTime, startBook, endBook);
                                if (passesEvents == false)
                                {
                                    return RedirectToAction("ArtistOpenings", new { id = aModel.Id, dayToCheck = aModel.DayToCheck.Value, error = "Event already booked during that time" });
                                }
                            }
                            foreach (var artEvent in aBookedArtEvents)
                            {
                                passesEvents = _schedOptService.CompareEventTimesToBookTimes(artEvent.StartTime, artEvent.EndTime, startBook, endBook);
                                if (passesEvents == false)
                                {
                                    return RedirectToAction("ArtistOpenings", new { id = aModel.Id, dayToCheck = aModel.DayToCheck.Value, error = "Event already booked during that time" });
                                }
                            }
                        }
                        else
                        {
                            return RedirectToAction("ArtistOpenings", new { id = aModel.Id, dayToCheck = aModel.DayToCheck.Value, error = "Outside of Availability" });
                        }

                    }
                    return RedirectToAction("CreateEvent", new { aId = aModel.Id, bookStart = startBook, bookEnd = endBook });
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }

            return RedirectToAction("ArtistOpenings", new { id = aModel.Id, dayToCheck = aModel.DayToCheck.Value });
        }

        public async Task<IActionResult> CreateEvent(int aId, DateTime bookStart, DateTime bookEnd, string error = null)
        {
            double totalCost;
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();
            var artist = await _context.Artists.Where(b => b.Id.Equals(aId)).SingleOrDefaultAsync();

            double totalHours = (bookEnd - bookStart).TotalHours;
            if (bookStart.DayOfWeek.ToString() == "Saturday" || bookStart.DayOfWeek.ToString() == "Sunday")
            {
                totalCost = (totalHours * business.WeekendHourlyCost.Value);
            }
            else
            {
                totalCost = (totalHours * business.HourlyCost.Value);
            }

            BusinessEvent busEvent = new BusinessEvent
            {
                Artist = artist,
                Business = business,
                StartTime = bookStart,
                EndTime = bookEnd,
                Cost = totalCost
            };

            ViewBag.StripePublishKey = Secrets.STRIPES_PUBLIC_KEY;
            return View(busEvent);
        }

        [HttpPost]
        public async Task<IActionResult> PaymentConfirmation(string stripeToken, string description, string stripeEmail, int amount, BusinessEvent busEvent)
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
                _context.BusinessEvents.Add(busEvent);
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
            var business = await _context.Businesses.Where(c => c.IdentityUserId == userId).SingleOrDefaultAsync();

            var today = DateTime.Now;
            var yesterday = today.AddDays(-1);

            if (dayToCheck == null)
            {
                dayToCheck = today;
            }

            var businessEvents = await _context.BusinessEvents.Where(b => b.BusinessId.Equals(business.Id) && b.StartTime.Day > yesterday.Day).Include(b => b.Artist).ToListAsync();
            var artistEvents = await _context.ArtistEvents.Where(a => a.BusinessId.Equals(business.Id) && a.StartTime.Day > yesterday.Day).Include(b => b.Artist).ToListAsync();
            var artistEventsToday = artistEvents.Where(a => a.StartTime.Day.Equals(dayToCheck.Value.Day)).ToList();

            ViewData["DayToCheck"] = dayToCheck;
            ViewData["BusinessEvents"] = businessEvents;
            ViewData["BusinessEventsToday"] = businessEvents.Where(b => b.StartTime.Day.Equals(dayToCheck.Value.Day)).ToList();
            ViewData["ArtistEvents"] = artistEvents;
            return View(artistEventsToday);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Schedule(DateTime dayToCheck)
        {
            return RedirectToAction("Schedule", new { dayToCheck = dayToCheck });
        }


    }
}
