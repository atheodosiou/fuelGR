using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

namespace fuelGR_UWP
{
    class CommonMethods
    {
        /// <summary>
        /// Center a map to the user's current location and adding a location marker!
        /// </summary>
        /// <param name="map"></param>
        /// <param name="zoomLevel"></param>
        public async void CenterMapToCurrentLocation(MapControl map, double zoomLevel)
        {
            try
            {
                LocationManager lm = new LocationManager();
                Geoposition pos = await lm.GetPosition();
                await map.TrySetViewAsync(pos.Coordinate.Point, zoomLevel);
                MapIcon mapIcon = new MapIcon();
                mapIcon.Location = pos.Coordinate.Point;
                //BasicGeoposition cityPosition = new BasicGeoposition() { Latitude = 40.856013, Longitude = 25.909239 };
                //Geopoint cityCenter = new Geopoint(cityPosition);
                mapIcon.NormalizedAnchorPoint = new Point(0.5, 1);
                mapIcon.Image = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Pins/pin50x50.png"));
                mapIcon.ZIndex = 0;
                map.MapElements.Add(mapIcon);

                MainPage.mapCentered = true;
            }
            catch (Exception e)
            {
                MessageDialog msg = new MessageDialog(e.Message, "Error!");
                await msg.ShowAsync();
            }
        }

        public static Uri GetFuelStationIcon(string brandId)
        {
            //RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/Pins/pin50x50.png"));
            string baseUri = "ms-appx:///Assets/StationLogos/";
            string logoCode = "";

            switch (brandId)
            {
                case "1": { logoCode = "1.png"; } break;
                case "2": { logoCode = "2.png"; } break;
                case "3": { logoCode = "3.png"; } break;
                case "4": { logoCode = "4.png"; } break;
                case "5": { logoCode = "5.png"; } break;
                case "6": { logoCode = "6.png"; } break;
                case "7": { logoCode = "7.png"; } break;
                case "8": { logoCode = "8.png"; } break;
                case "9": { logoCode = "9.png"; } break;
                case "10": { logoCode = "10.png"; } break;
                case "11": { logoCode = "11.png"; } break;
                case "12": { logoCode = "12.png"; } break;
                case "13": { logoCode = "13.png"; } break;
                case "14": { logoCode = "14.png"; } break;
                case "15": { logoCode = "15.png"; } break;
                case "16": { logoCode = "16.png"; } break;
                case "17": { logoCode = "17.png"; } break;
                case "18": { logoCode = "18.png"; } break;
                case "19": { logoCode = "19.png"; } break;
                case "20": { logoCode = "20.png"; } break;
                default: { logoCode = "1.png"; } break;
            }
            Uri uri = new Uri(baseUri + logoCode);
            return uri;
        }

        public static void InitializeCompaniesSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            var selectedFuelCompanies = localSettings.Values["selectedCompanies"];

            if (selectedFuelCompanies == null)
            {
                //"1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20"
                //Δεν υπάρχουν δεδομένα και θα δηλωθούν τώρα
                localSettings.Values.Add("selectedCompanies",
                    "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20");
            }
        }

        public static void InitializeFuelTypeSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (!localSettings.Values.ContainsKey("fuelIndex"))
                localSettings.Values.Add("fuelIndex", 0);
        }

        public static void InitializePremiumFuelSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (!localSettings.Values.ContainsKey("premFuelIndex"))
                localSettings.Values.Add("premFuelIndex", 0);
        }

        public static void InitializePriceOldSettings()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (!localSettings.Values.ContainsKey("priceOldIndex"))
                localSettings.Values.Add("priceOldIndex", 7);
        }

        public static bool areSettingsInitialized()
        {
            bool settings = false;

            var localSettings = ApplicationData.Current.LocalSettings;

            var selectedFuelCompanies = localSettings.Values["selectedCompanies"];
            var selectedFuelTypeIndex = localSettings.Values["fuelIndex"];
            var selectedPremiumFuelIndex = localSettings.Values["premFuelIndex"];
            var selectedPriceOldIndex = localSettings.Values["priceOldIndex"];

            if (selectedFuelCompanies == null || selectedFuelTypeIndex == null ||
                selectedPremiumFuelIndex == null || selectedPriceOldIndex == null)
                settings = false;
            else
                settings = true;

            return settings;
        }

        public static async void ShowMessage(string Title, string Message)
        {
            MessageDialog msg = new MessageDialog(Message, Title);
            await msg.ShowAsync();
        }

        public static bool IsInternetConnected()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }

        public static void UpdateCompaniesNumLabel(Button btn)
        {
            try
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                string comps = (string)localSettings.Values["selectedCompanies"];

                string[] selectedCompanies = comps.Split(',');

                int numOfCompanies = selectedCompanies.Length;

                if (numOfCompanies < 20)
                    btn.Content = numOfCompanies.ToString() + " επιλεγμένες εταιρίες";
                else
                    btn.Content = " Όλες οι εταιρίες";
            }
            catch { }
        }

        /// <summary>
        /// Αρχικοποίηση όλων των ρυθμίσεων για την εφαρμογή
        /// </summary>
        public static void InitializeAllSettings()
        {
            InitializeCompaniesSettings();
            InitializePremiumFuelSettings();
            InitializePriceOldSettings();
            InitializeFuelTypeSettings();
        }


        /// <summary>
        /// Επιστρέφει το link για την κλήση των δεδομένων
        /// </summary>
        /// <param name="latitude">Γεωγραφικό πλάτος</param>
        /// <param name="longitude">Γεωγραφικό μήκος</param>
        /// <returns></returns>
        public string GetCallUrl(string latitude, string longitude)
        {
            string url = "";
            string baseUrl = "http://deixto.gr/fuel/get_data_v4.php?";
            string dev = "winphone.test";
            string f = GetFuelTypeValue;
            string b = GetCompaniesValue;
            string d = GetPriceOldValue;
            string p = GetPremiumFuelValue;

            //Σύνθεση url
            url = baseUrl + "dev=" + dev + "&lat=" + latitude + "&long=" + longitude + "&f=" + f + "&b=" + b + "&d=" + d + "&p=" + p;

            if (string.IsNullOrWhiteSpace(f) || string.IsNullOrWhiteSpace(b) ||
                string.IsNullOrWhiteSpace(d) || string.IsNullOrWhiteSpace(p))
            {
                return "error";
            }
            else
                return url;
        }

        /// <summary>
        /// Εύρεση φθηνότερων πρατηρίων 
        /// </summary>
        /// <param name="stations"></param>
        /// <returns></returns>
        public static List<GasStation> GetCheapestGasStetions(List<GasStation> stations)
        {
            List<GasStation> cheapest = null;

            double cheap1 = 1000;
            double cheap2 = 1000;
            double cheap3 = 1000;

            GasStation gs_1 = null;
            GasStation gs_2 = null;
            GasStation gs_3 = null;

            int i = -1;
            foreach (GasStation gs in stations)
            {
                i++;
                double price = double.Parse(gs.FT_PR);

                if (price < cheap1)
                {
                    gs_1 = stations[i];
                    cheap1 = price;
                }
            }

            if(cheap1!=1000)
            {
                //Εχουμε βρεί το φθηνότερο πάμε για το επόμενο
                int j = -1;
                foreach (GasStation gs in stations)
                {
                    j++;
                    double price = double.Parse(gs.FT_PR);

                    if (price < cheap2 && price != cheap1)
                    {
                        gs_2 = stations[j];
                        cheap2 = price;
                    }
                }
            }

            if (cheap1 != 1000 && cheap2!=1000)
            {
                //Εχουμε βρεί τα δυο φθηνότερα πάμε για το τρίτο
                int x = -1;
                foreach (GasStation gs in stations)
                {
                    x++;
                    double price = double.Parse(gs.FT_PR);

                    if (price < cheap3 && price != cheap1 && price != cheap2)
                    {
                        gs_3 = stations[x];
                        cheap3 = price;
                    }
                }
            }

            if(gs_1!=null && gs_2!=null && gs_3!=null)
            {
                //Εχουν βρεθεί τα φθηνότερα αρα το προσθέτο στην λίστα και τα επιστέφω στο χρήστη
                cheapest = new List<GasStation>();

                cheapest.Add(gs_1);
                cheapest.Add(gs_2);
                cheapest.Add(gs_3);

                return cheapest;
            }
            else
            {
                return null;
            }
        }

        public enum DeviceFamily
        {
            Desktop,
            Mobile,
            Else
        }

        /// <summary>
        /// Επιστέφει το τύπο της συσκευής που τρέχει η εφαρμογής. Π.χ. Mobile, Desktop
        /// </summary>
        /// <returns></returns>
        public static DeviceFamily GetDeviceFamily()
        {
            var str = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;

            if (str == "Windows.Desktop")
                return DeviceFamily.Desktop;
            else if (str == "Windows.Mobile")
                return DeviceFamily.Mobile;
            else
                return DeviceFamily.Else;
        }
        //====================== Ιδιότητες κλάσης ==================================

        /// <summary>
        /// Επιστρέφη την τιμή που έχει επιλέξει ο χρήστης για τον τύπο καυσίμου
        /// </summary>
        public string GetFuelTypeValue
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                int value = (int)localSettings.Values["fuelIndex"];
                value += 1;
                return value.ToString();
            }
        }

        /// <summary>
        /// Επιστρέφει τις επιλεγμένες εταιρίες καυσίμου
        /// </summary>
        public string GetCompaniesValue
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                string value = (string)localSettings.Values["selectedCompanies"];
                //Μηδέν για όλες τις εταιρίες
                if (value == "1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20")
                    return "0";
                else
                    return value;
            }
        }

        /// <summary>
        /// Επιστρέφει την επιλογή του χρήστη για τα premium καύσιμα
        /// </summary>
        public string GetPremiumFuelValue
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                int value = (int)localSettings.Values["premFuelIndex"];
                return value.ToString();
            }
        }

        /// <summary>
        /// Επιστρέφει την παλαιότητα τιμής καυσίμου βάση επιλογής χρήστη
        /// </summary>
        public string GetPriceOldValue
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                int value = (int)localSettings.Values["priceOldIndex"];

                string priceOld = "30";

                switch (value)
                {
                    case 0: { priceOld = "1"; } break;
                    case 1: { priceOld = "2"; } break;
                    case 2: { priceOld = "3"; } break;
                    case 3: { priceOld = "4"; } break;
                    case 4: { priceOld = "5"; } break;
                    case 5: { priceOld = "7"; } break;
                    case 6: { priceOld = "15"; } break;
                    case 7: { priceOld = "30"; } break;
                    default: { priceOld = "30"; } break;
                }

                return priceOld;
            }
        }
        

    }
}
