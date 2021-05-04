using BookEWebsite.Data;
using BookEWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookEWebsite.Services.ScheduleOptionsSvc
{
    public class ScheduleOptionsService
    {
        public int[] Hours { get;}
        public int[] Minutes { get; }
        public string[] TimeOfDay { get; }
        public string[] DaysOfTheWeek { get; }

        public ScheduleOptionsService()
        {
            Hours = new int[] { 12, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
            Minutes = new int[] { 00, 15, 30, 45 };
            TimeOfDay = new string[] { "AM", "PM" };
            DaysOfTheWeek = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        }

        //public void CheckTimeSlotsAvailable(List<ArtistAvailability> availToCheck, List<BusinessEvent> bEvents, List<ArtistEvent> aEvents, DateTime dayToCheck)
        //{
        //    foreach (var avail in availToCheck)
        //    {
        //        avail.StartTime;
        //        avail.EndTime;

        //        foreach (var b in bEvents)
        //        {

        //        }
        //        foreach (var a in aEvents)
        //        {

        //        }

        //    }

        //    dayToCheck.


        //    if (true){ return true; }
        //    return false;
        //}

        public bool CompareAvailabilityToBookTimes(DateTime availStart, DateTime availEnd, DateTime bookStart, DateTime bookEnd)
        {
            if ((availStart < bookStart || availStart == bookStart) && (availEnd > bookEnd || availEnd == bookEnd))
            {
                return true;
            }
            return false;
        }

        public bool CompareEventTimesToBookTimes(DateTime eventStart, DateTime eventEnd, DateTime bookStart, DateTime bookEnd)
        {
            if (((eventStart < bookStart || eventStart == bookStart) && (eventEnd < bookStart || eventEnd == bookStart)) ||
               ((eventStart > bookEnd || eventStart == bookEnd) && (eventEnd > bookEnd || eventEnd == bookEnd)))
            {
                return true;
            }
            else if ((eventStart > bookStart && (eventEnd > bookStart || eventEnd == bookStart)) && 
                ((eventStart > bookEnd || eventStart == bookEnd) && eventEnd > bookEnd))
            {
                return true;
            }
            return false;
        }

        //public async Task<string> MakeStringOfAvailabilities(int id, bool isArtist, ApplicationDbContext _appDBContext, DateTime dayToCheck)
        //{
        //    List<BusinessEvent> userBEvents;
        //    List<ArtistEvent> userAEvents;
        //    List<IAvailability> userAvailList = new List<IAvailability>();

        //    if (isArtist)
        //    {
        //        var availList = await _appDBContext.ArtistAvailabilities.Where(a => a.ArtistId.Equals(id)).ToListAsync();
        //        userBEvents = await _appDBContext.BusinessEvents.Where(a => a.ArtistId.Equals(id)).ToListAsync();
        //        userAEvents = await _appDBContext.ArtistEvents.Where(a => a.ArtistId.Equals(id)).ToListAsync();
        //        foreach (var avail in availList)
        //        {
        //            userAvailList.Add(avail);
        //        }
        //    }
        //    else
        //    {
        //        var availList = await _appDBContext.BusinessAvailabilities.Where(a => a.BusinessId.Equals(id)).ToListAsync();
        //        userBEvents = await _appDBContext.BusinessEvents.Where(a => a.BusinessId.Equals(id)).ToListAsync();
        //        userAEvents = await _appDBContext.ArtistEvents.Where(a => a.BusinessId.Equals(id)).ToListAsync();
        //        foreach (var avail in availList)
        //        {
        //            userAvailList.Add(avail);
        //        }
        //    }

        //    CheckTimeSlotsAvailable(userAvailList, userBEvents, userAEvents, dayToCheck);

        //    return availHTMLString;
        //}

    }
}
