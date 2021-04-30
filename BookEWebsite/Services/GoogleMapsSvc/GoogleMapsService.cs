using BookEWebsite.Models;
using GoogleMapsApi;
using GoogleMapsApi.Entities.Geocoding.Request;
using GoogleMapsApi.Entities.Geocoding.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookEWebsite.Services.GoogleMapsSvc
{
    public class GoogleMapsService
    {
        public GoogleMapsService()
        {

        }

        public async Task<Address> ConvertStreetToLongLat(Address address)
        {
            var geoCodingEngine = GoogleMaps.Geocode;
            GeocodingRequest geocodeRequest = new GeocodingRequest
            {
                Address = $"{address.Street1}, {address.Street2}, {address.City}, {address.State} {address.ZipCode}",
                ApiKey = Secrets.GOOGLE_API_KEY,
            };

            GeocodingResponse geocode = await geoCodingEngine.QueryAsync(geocodeRequest);
            address.Latitude = geocode.Results.First().Geometry.Location.Latitude;
            address.Longitude = geocode.Results.First().Geometry.Location.Longitude;

            return address;
        }
    }
}
