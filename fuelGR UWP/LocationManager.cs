using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace fuelGR_UWP
{
    class LocationManager
    {
        /// <summary>
        /// Returns the user's current position. Throws exception if access denied!
        /// </summary>
        /// <returns>Task<Geoposition></returns>
        public async Task<Geoposition> GetPosition()
        {
            var accessStatus= await Geolocator.RequestAccessAsync();

            if (accessStatus != GeolocationAccessStatus.Allowed) throw new Exception("Access location denied!");

            var geolocator = new Geolocator { DesiredAccuracyInMeters = 50 };

            var position = await geolocator.GetGeopositionAsync();

            return position;
        }

        public async Task<bool> RequestLocationAccess()
        {
            //Επιστέφει true η false ανάλογα με την άδεια του χρήστη
            bool access = false;

            return access;
        }

       
    }
}
