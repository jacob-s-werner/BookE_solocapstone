# BookE_solocapstone
An online booking website for artists/businesses using C#, HTML/CSS, Razor, ASP.Net/MVC and Entity Framework

Bootstrap is used for styling along with site.css

Hidden file Secrets.cs (ignored in gitignore file) includes authorization and secret tokens for the following:
* Geocoding API - Updates Longitude and Latitude using tokens after editing address (or creating a profile).
* GoogleMaps API - Uses secret key every time the Artist's Index is opened to generate the map with business locations.
* Stripe API - Uses tokens after hitting pay now, sends a request out and recieves success or fail.



Whether you’re in a team, group, band, or even a solo artist/comedian, it can be hard to find a place to your liking to play at. On the other hand, a business might have trouble searching for artists or groups to play at their location. This is where our online booking agency comes in handy!
