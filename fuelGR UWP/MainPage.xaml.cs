using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace fuelGR_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        CommonMethods commonMethods;
        public static bool mapCentered = false;

        private string BaseHeading = "50"; // 0 - 360
        private string BaseFov = "90"; // 0 - 120, όσο μικρότερο, τόσο πιο μεγάλο ζουμ
        private string BasePitch = "0"; // -90 - +90, θετικό πάνω, αρνητικό κάτω
        //Ρυθμός αλλαγής στροφής δεξιά αριστερά και ζουμ
        private int ChangeRateHeading = 10;
        private int ChangeRateFov = 10;
        private int ChangeRatePitch = 10;

        public  MainPage()
        {
            this.InitializeComponent();

            if (!CommonMethods.areSettingsInitialized())
            {
                //Είναι η πρώτη εκτέλεση της εφαρμογής και οι ρυθμίσεις δεν υπάρχουν
                //Άρα πρέπει να αρχικοποιηθούν απο την εφαρμογή 
                CommonMethods.InitializeAllSettings();
            }

            if (CommonMethods.IsInternetConnected())
            {
                // Register a handler for BackRequested events and set the
                // visibility of the Back button
                SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
                Window.Current.Closed += Current_Closed;
                
                MakeCall();
            }
            else
            {
                string caption = "Σφάλμα Σύνδεσης στο Internet";
                string message = "Η εφαρμογή απαιτεί σύνδεση στο internet για να λάβει τα πιο πρόσφατα δεδομένα" +
                    " τιμών καυσίμων. Παρακαλούμε ελέγξτε την σύνδεσή σας στο internet και προσπαθήστε ξανά. " +
                    "Στο μεταξύ, η εφαρμογή θα σας δείξει τα δεδομένα τιμών που λάβατε πιο πρόσφατα.";

                MessageDialog msg = new MessageDialog(message, caption);
                UICommand okBtn = new UICommand("Το κατάλαβα",okBtnPressed);
                msg.Commands.Add(okBtn);
                UICommand cancelBtn = new UICommand("Όχι ευχαριστώ",cancel2BtnPressed);
                msg.Commands.Add(cancelBtn);
                msg.ShowAsync();
                
            }
        }

       

        private void cancel2BtnPressed(IUICommand command)
        {
            return;
        }

        private async void okBtnPressed(IUICommand command)
        {
            //Διάβασμα του αρχείου από το δίσμο και προβολή τον δοδομένων!
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile sampleFile = await localFolder.GetFileAsync("gasStations");
                string responseXml = await FileIO.ReadTextAsync(sampleFile);

                ShowData(responseXml);
            }
            catch (Exception)
            {
                //Δεν βρέθηκε
                CommonMethods.ShowMessage("Σφάλμα!", "Το αρχείο δεν βρέθηκε.");
            }
        }

        public enum PriceColor
        {
            Red,
            Blue,
            Green
        }

        public enum StationAccuracy
        {
            Rooftop,
            Else
        }

        public  List<GasStation> cheapestStations = new List<GasStation>();

        //Για τον υπολογισμό του μέσου όρου τιμής
        double minPrice = 1000;
        double maxPrice = 0;
        double avgPrice = 0;

        //====================> All the magic <===================================================
        public async void MakeCall()
        {
            if (progessRing.IsActive == false)
                progessRing.IsActive = true;
            NetworkConector nc = new NetworkConector();
            CommonMethods cm = new CommonMethods();
            LocationManager lm = new LocationManager();

            try
            {
                Geoposition gp = await lm.GetPosition();
                Geopoint loc = new Geopoint(new BasicGeoposition()
                {
                    Latitude = gp.Coordinate.Point.Position.Latitude,
                    Longitude = gp.Coordinate.Point.Position.Longitude
                });

                Image userLocation = new Image();
                userLocation.Source= new BitmapImage(new Uri("ms-appx:///Assets/MapElements/locationMarker.png"));
                userLocation.Width = 12;
                userLocation.Height = 12;
                myMap.Children.Add(userLocation);
                MapControl.SetLocation(userLocation, loc);
                MapControl.SetNormalizedAnchorPoint(userLocation, new Point(0, 0));

                string latitude = gp.Coordinate.Point.Position.Latitude.ToString("0.00000");
                string longitude = gp.Coordinate.Point.Position.Longitude.ToString("0.00000");

                // myMap.TrySetViewAsync(gp.Coordinate.Point,15);

                string url = cm.GetCallUrl(latitude, longitude);

                string response = await nc.GetResponese(url);

                ShowData(response);
            }
            catch (Exception ex)
            {
                if (progessRing.IsActive == true)
                    progessRing.IsActive = false;
                CommonMethods.ShowMessage("Error:", ex.Message + Environment.NewLine+ ex.InnerException);
            }


        }

        private async void ShowData(string response)
        {
            XDocument xdoc = XDocument.Parse(response);

            List<GasStation> gss = (from item in xdoc.Descendants("gs")
                                    select new GasStation()
                                    {
                                        ID = item.Attribute("id").Value,
                                        //CNT = item.Attribute("cnt").Value,
                                        //MUNID = item.Attribute("munid").Value,
                                        //MUN = item.Attribute("mun").Value,
                                        //DDID = item.Attribute("ddid").Value,
                                        //DD = item.Attribute("dd").Value,
                                        AC = item.Element("ac").Value,
                                        LT = item.Element("lt").Value,
                                        LG = item.Element("lg").Value,
                                        DIS = item.Element("dis").Value,
                                        //FEAT = item.Element("feat").Value,
                                        //FEAT_H24 = item.Element("feat").Attribute("id").Value,
                                        BR = item.Element("br").Value,
                                        BR_ID = item.Element("br").Attribute("id").Value,
                                        FT = item.Element("fts").Element("ft").Value,
                                        FT_ID = item.Element("fts").Element("ft").Attribute("id").Value,
                                        FT_DT = item.Element("fts").Element("ft").Attribute("dt").Value,
                                        FT_PR = item.Element("fts").Element("ft").Attribute("pr").Value,
                                        AD = item.Element("ad").Value,
                                        OW = item.Element("ow").Value,
                                        MSG = item.Element("msg").Value,
                                        PH1 = item.Element("ph1").Value,
                                        PH2 = item.Element("ph2").Value,
                                        EX = item.Element("ex").Value
                                    }).ToList();

            //==================> Αποθήκευση του αντικειμένου στο δίσκο <============================= 
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile myStorage = await localFolder.CreateFileAsync("gasStations", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(myStorage, response);
            //========================================================================================

            // Έλεγχος για το αν επέστρεψε αποτελέσματα η αναζήτηση
            if (gss.Count > 0)
            {
                //Έχουμε αποτελέσματα 

                //Ανανέωση αριθμού πρατηρίων
                gsNumLabel.Text = gss.Count.ToString("0");

                //Για την εύρεση των ακριανων σημείων
                double NwLat = 0;
                double SeLat = 1000;
                double NwLong = 1000;
                double SeLong = 0;

                string owners = "";
                double sumPrice = 0;

                //Έυρεση των φθηνότερων πρατηρίων!
                cheapestStations = CommonMethods.GetCheapestGasStetions(gss);

                foreach (GasStation i in gss)
                {
                    double price = 0;
                    bool isPriceOk = double.TryParse(i.FT_PR, out price);

                    //Εύρεση μέγιστης και ελάχιστης τιμής
                    if (isPriceOk.Equals(true))
                    {
                        if (price < minPrice)
                            minPrice = price;
                        if (price > maxPrice)
                            maxPrice = price;
                        sumPrice += price;
                    }
                }

                foreach (GasStation i in gss)
                {
                    double lt = 0;
                    double lg = 0;
                    bool isLatOk = double.TryParse(i.LT, out lt);
                    bool isLongOk = double.TryParse(i.LG, out lg);

                    double price = 0;
                    bool isPriceOk = double.TryParse(i.FT_PR, out price);

                    //Σύγκριση lat και long για την εύρεση του southeast και northwest
                    //σημείου πάνω στο χάρτη για την δημιουργία του GeoboundingBox

                    //Northweast: Μεγάλο Lat, Μικρό Long
                    if (lt > NwLat)
                        NwLat = lt;
                    if (lg < NwLong)
                        NwLong = lg;

                    //Southeast: Μικρό Lat, Μεγάλο Long
                    if (lt < SeLat)
                        SeLat = lt;
                    if (lg > SeLong)
                        SeLong = lg;

                    //Εισαγωγή των pin στο χάρτη
                    if (isLatOk.Equals(true) && isLongOk.Equals(true))
                    {
                        double maxmin25precent = (double)(maxPrice - minPrice) * 25 / 100;
                        PriceColor priceColor;
                        StationAccuracy accuracy;

                        if (price >= minPrice && price <= minPrice + maxmin25precent)
                            priceColor = PriceColor.Green;
                        else if (price <= maxPrice && price >= maxPrice - maxmin25precent)
                            priceColor = PriceColor.Red;
                        else
                            priceColor = PriceColor.Blue;

                        if (i.AC == "ROOFTOP")
                            accuracy = StationAccuracy.Rooftop;
                        else
                            accuracy = StationAccuracy.Else;

                        AddPushPin(myMap, lt, lg, CommonMethods.GetFuelStationIcon(i.BR_ID), i, priceColor, accuracy);
                    }
                }

                //Έλεγχος αν έχει βρεθεί η μέγιστη και η ελάχιστη τιμή καυσίμου
                if (maxPrice != 1000 && minPrice != 0)
                {
                    //Ενημέρωση των τιμών στο ui
                    minPriceLabel.Text = minPrice.ToString("0.000") + " €";
                    maxPriceLabel.Text = maxPrice.ToString("0.000") + " €";
                }

                //Υπολογισμός μέσης τιμής
                int gsCount = gss.Count;
                avgPrice = (double)sumPrice / gsCount;

                //Ενημέρωση των avg τιμής στο ui
                avgPriceLabel.Text = avgPrice.ToString("0.000") + " €";

                BasicGeoposition northwest = new BasicGeoposition();
                northwest.Latitude = NwLat;
                northwest.Longitude = NwLong;
                BasicGeoposition southeast = new BasicGeoposition();
                southeast.Latitude = SeLat;
                southeast.Longitude = SeLong;
                //Ορισμός του GeoboundingBox
                GeoboundingBox geoboundingBox = new GeoboundingBox(northwest, southeast);

                await myMap.TrySetViewBoundsAsync(geoboundingBox, new Thickness(2, 2, 2, 2), MapAnimationKind.Bow);
            }
            else
            {
                //Δέν επέστρεψε αποτελέσματα η αναζήτηση!

                //Καθαρισμός του χάρτη και εμφάνιση μυμήματος
                if (myMap.Children.Count > 0)
                    myMap.Children.Clear();

                if (myMap.MapElements.Count > 0)
                    myMap.MapElements.Clear();
                //Μηδενισμός το τιμών καυσίμου στο ui

                minPriceLabel.Text = "0 €";
                maxPriceLabel.Text = "0 €";
                avgPriceLabel.Text = "0 €";

                CommonMethods.ShowMessage("Πληροφορία",
                    "Δεν υπάρχουν πρατήρια με τα ΄κριτήρια που έχετε ορίσει.");
            }
            if (progessRing.IsActive == true)
                progessRing.IsActive = false;
        }

        private void Current_Closed(object sender, CoreWindowEventArgs e)
        {
            e.Handled = false;
        }

        private void AddPushPin(MapControl map, double latitude, double longitude, Uri iconUri, GasStation gs, PriceColor priceColor, StationAccuracy stationAccuracy)
        {
            FuelStationPin pin = new FuelStationPin();

            BitmapImage img = new BitmapImage(iconUri);
            pin.fuelStationLogo.Source = img;
            pin.fuelStationLogo.Tag = gs;
            pin.fuelStationLogo.Tapped += FuelStationLogo_Tapped;
            pin.fuelStationPrice.Text = gs.FT_PR + " €";
            //Χρωματισμός των τιμών
            if (priceColor == PriceColor.Blue)
                pin.fuelStationPrice.Foreground = new SolidColorBrush(Colors.Blue);
            if(priceColor==PriceColor.Green)
                pin.fuelStationPrice.Foreground = new SolidColorBrush(Colors.Green);
            if (priceColor==PriceColor.Red)
                pin.fuelStationPrice.Foreground = new SolidColorBrush(Colors.Red);
            //Χρωματισμός του τριγώνου του pin
            if(stationAccuracy==StationAccuracy.Rooftop)
                pin.fuelStationLabel.Source = new BitmapImage(new Uri("ms-appx:///Assets/PinLabels/marker_green.png"));
            else
                pin.fuelStationLabel.Source = new BitmapImage(new Uri("ms-appx:///Assets/PinLabels/marker_red.png"));

            map.Children.Add(pin);

            var position = new Geopoint(new BasicGeoposition()
            {
                Latitude = latitude,
                Longitude = longitude
            });
            MapControl.SetLocation(pin, position);
            MapControl.SetNormalizedAnchorPoint(pin, new Point(0.5, 1));
        }

        private async void FuelStationLogo_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int numOfPins = int.Parse(gsNumLabel.Text.ToString());
            //+1 γιατί υπάρχει και το pin της θέσης του χρήστη!
            numOfPins += 1;

            if(myMap.Children.Count>numOfPins)
                myMap.Children.RemoveAt(myMap.Children.Count - 1);

            Image img = (Image)sender;
            GasStation gs = (GasStation)img.Tag;
            // Ελεγχος σε τι συσκευή τρέχει για την εμφάνιση του ανάλογου infoBox
            

            var deviceType = CommonMethods.GetDeviceFamily();

            if (deviceType == CommonMethods.DeviceFamily.Desktop)
            {
                //Τρέχει σε desktop
                InfoBoxDesktop infoBox = new InfoBoxDesktop();
                infoBox.Name = "MyInfoBox";
                infoBox.Margin = new Thickness(0, 0, 0, 60);

                infoBox.actions_grid.Tapped += Actions_grid_Tapped;
                infoBox.actions_grid.Tag = gs.LT + ", " + gs.LG;

                double maxmin25precent = (double)(maxPrice - minPrice) * 25 / 100;
                double price = Double.Parse(gs.FT_PR);

                if (price >= minPrice && price <= minPrice + maxmin25precent)
                    infoBox.gs_price.Foreground = new SolidColorBrush(Colors.Green);
                else if (price <= maxPrice && price >= maxPrice - maxmin25precent)
                    infoBox.gs_price.Foreground = new SolidColorBrush(Colors.Red);
                else
                    infoBox.gs_price.Foreground = new SolidColorBrush(Colors.Blue);

                if (gs.AC == "ROOFTOP")
                    infoBox.gs_location_accuracy.Text = "Άριστη";
                else
                    infoBox.gs_location_accuracy.Text = "Μέτρια";

                infoBox.gs_fuel_type.Text = gs.FT;
                infoBox.gs_price.Text = gs.FT_PR + " €";
                infoBox.gs_distance.Text = gs.DIS + " Km";
                infoBox.gs_logo.Source = new BitmapImage(CommonMethods.GetFuelStationIcon(gs.BR_ID));

                if (gs.BR == "ΑΝΕΞΑΡΤΗΤΟ ΠΡΑΤΗΡΙΟ")
                    infoBox.gs_name.Text = "ΑΠ";
                else if (gs.BR == "ΑΙΓΑΙΟ (AEGEAN)")
                    infoBox.gs_name.Text = "AEGEAN";
                else
                    infoBox.gs_name.Text = gs.BR;

                infoBox.gs_owner.Text = gs.OW;
                infoBox.gs_address.Text = gs.AD;
                infoBox.gs_date.Text = gs.FT_DT;
                infoBox.gs_CloseBtn.Click += Gs_CloseBtn_Click;
                infoBox.gs_CloseBtn.Tag = infoBox;
                string phone1 = string.Empty;
                string phone2 = string.Empty;

                if (string.IsNullOrWhiteSpace(gs.PH1))
                    phone1 = string.Empty;
                else
                    phone1 = gs.PH1;

                if (string.IsNullOrWhiteSpace(gs.PH2))
                    phone2 = string.Empty;
                else
                    phone2 = gs.PH2;


                if (string.IsNullOrWhiteSpace(phone1) && string.IsNullOrWhiteSpace(phone2))
                    infoBox.gs_phone.Text = "Μη διαθέσιμο";
                else
                {
                    if (!string.IsNullOrWhiteSpace(phone1) && !string.IsNullOrWhiteSpace(phone2))
                        infoBox.gs_phone.Text = gs.PH1 + ", " + gs.PH2;
                    if (!string.IsNullOrWhiteSpace(phone1) && string.IsNullOrWhiteSpace(phone2))
                        infoBox.gs_phone.Text = gs.PH1;
                    if (string.IsNullOrWhiteSpace(phone1) && !string.IsNullOrWhiteSpace(phone2))
                        infoBox.gs_phone.Text = gs.PH2;
                }

                double latitude = Double.Parse(gs.LT);
                double longitude = Double.Parse(gs.LG);

                myMap.Children.RemoveAt(myMap.Children.Count - 1);
                myMap.Children.Add(infoBox);

                var position = new Geopoint(new BasicGeoposition()
                {
                    Latitude = latitude,
                    Longitude = longitude
                });

                MapControl.SetLocation(infoBox, position);
                MapControl.SetNormalizedAnchorPoint(infoBox, new Point(0.5, 1));

                await myMap.TrySetViewAsync(position, 17);
            }
            else if(deviceType==CommonMethods.DeviceFamily.Mobile)
            {
                //Τρέχει σε κινητό 
                InfoBox infoBox = new InfoBox();
                infoBox.Name = "MyInfoBox";

                infoBox.actions_grid.Tapped += Actions_grid_Tapped;
                infoBox.actions_grid.Tag = gs.LT + ", " + gs.LG;

                infoBox.Margin = new Thickness(0, 0, 0, 60);

                double maxmin25precent = (double)(maxPrice - minPrice) * 25 / 100;
                double price = Double.Parse(gs.FT_PR);

                if (price >= minPrice && price <= minPrice + maxmin25precent)
                    infoBox.gs_price.Foreground = new SolidColorBrush(Colors.Green);
                else if (price <= maxPrice && price >= maxPrice - maxmin25precent)
                    infoBox.gs_price.Foreground = new SolidColorBrush(Colors.Red);
                else
                    infoBox.gs_price.Foreground = new SolidColorBrush(Colors.Blue);

                if (gs.AC == "ROOFTOP")
                    infoBox.gs_location_accuracy.Text = "Άριστη";
                else
                    infoBox.gs_location_accuracy.Text = "Μέτρια";

                infoBox.gs_fuel_type.Text = gs.FT;
                infoBox.gs_price.Text = gs.FT_PR + " €";
                infoBox.gs_distance.Text = gs.DIS + " Km";
                infoBox.gs_logo.Source = new BitmapImage(CommonMethods.GetFuelStationIcon(gs.BR_ID));

                if (gs.BR == "ΑΝΕΞΑΡΤΗΤΟ ΠΡΑΤΗΡΙΟ")
                    infoBox.gs_name.Text = "ΑΠ";
                else if (gs.BR == "ΑΙΓΑΙΟ (AEGEAN)")
                    infoBox.gs_name.Text = "AEGEAN";
                else
                    infoBox.gs_name.Text = gs.BR;

                infoBox.gs_owner.Text = gs.OW;
                infoBox.gs_address.Text = gs.AD;
                infoBox.gs_date.Text = gs.FT_DT;
                infoBox.gs_CloseBtn.Click += Gs_CloseBtn_Click;
                infoBox.gs_CloseBtn.Tag = infoBox;

                string phone1 = string.Empty;
                string phone2 = string.Empty;

                if (string.IsNullOrWhiteSpace(gs.PH1))
                    phone1 = string.Empty;
                else
                    phone1 = gs.PH1;

                if (string.IsNullOrWhiteSpace(gs.PH2))
                    phone2 = string.Empty;
                else
                    phone2 = gs.PH2;


                if (string.IsNullOrWhiteSpace(phone1) && string.IsNullOrWhiteSpace(phone2))
                    infoBox.gs_phone.Text = "Μη διαθέσιμο";
                else
                {
                    if (!string.IsNullOrWhiteSpace(phone1) && !string.IsNullOrWhiteSpace(phone2))
                        infoBox.gs_phone.Text = gs.PH1 + ", " + gs.PH2;
                    if (!string.IsNullOrWhiteSpace(phone1) && string.IsNullOrWhiteSpace(phone2))
                        infoBox.gs_phone.Text = gs.PH1;
                    if (string.IsNullOrWhiteSpace(phone1) && !string.IsNullOrWhiteSpace(phone2))
                        infoBox.gs_phone.Text = gs.PH2;
                }

                double latitude = Double.Parse(gs.LT);
                double longitude = Double.Parse(gs.LG);
                
                myMap.Children.RemoveAt(myMap.Children.Count - 1);
                myMap.Children.Add(infoBox);

                var position = new Geopoint(new BasicGeoposition()
                {
                    Latitude = latitude,
                    Longitude = longitude
                });

                MapControl.SetLocation(infoBox, position);
                MapControl.SetNormalizedAnchorPoint(infoBox, new Point(0.5, 1));

                await myMap.TrySetViewAsync(position, 17);
            }
            
        }

        private void Actions_grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Grid grid = (Grid)sender;
            //CommonMethods.ShowMessage("# πρατηρίου", grid.Tag.ToString());
            ActionsGrid.Tag = grid.Tag;

            if (ActionsGrid.Visibility == Visibility.Collapsed)
                ActionsGrid.Visibility = Visibility.Visible;
        }

        private void Gs_CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            var deviceType = CommonMethods.GetDeviceFamily();

            if (deviceType == CommonMethods.DeviceFamily.Desktop)
            {
                InfoBoxDesktop info = (InfoBoxDesktop)btn.Tag;
                myMap.Children.Remove(info);
            }
            else if (deviceType == CommonMethods.DeviceFamily.Mobile)
            {
                InfoBox info = (InfoBox)btn.Tag;
                myMap.Children.Remove(info);
            }
            else
                return;      
        }

        private void refreshBtn(object sender, RoutedEventArgs e)
        {
            if (CommonMethods.IsInternetConnected())
            {
                //Καθαρισμός του χάρτη πριν την νέα κλήση για δεδομέν
                if (myMap.Children.Count > 0)
                    myMap.Children.Clear();

                if (myMap.MapElements.Count > 0)
                    myMap.MapElements.Clear();

                //Κλήση για δεδομένα
                MakeCall();
            }
            else
            {
                CommonMethods.ShowMessage("Αδυναμία σύνδεσης!",
                    "Δεν υπάρχει ενεργή σύνδεση στο internet. Η εφαρμογή" +
                    " δεν μπορεί να συνεχίσει την εκτέλεσή της.");
                return;
            }
        }

        private void mapModeRoadBtn(object sender, RoutedEventArgs e)
        {
            if(myMap.Style!=MapStyle.Road)
                myMap.Style = MapStyle.Road;
        }

        private async void mapModeSatteliteBtn(object sender, RoutedEventArgs e)
        {
            if(NetworkInterface.GetIsNetworkAvailable())
            {
                if (myMap.Style != MapStyle.Aerial3D)
                    myMap.Style = MapStyle.Aerial3D;
            }
            else
            {
                string title = "Αποτυχία σύνδεσης!";
                string message = "Η χρήση του δορυφορικού χάρτη είναι εφικτή μόνο όταν υπάρχει ενεργή σύνδεση δικτύου.";
                MessageDialog msg = new MessageDialog(message, title);
                await msg.ShowAsync();
            }
        }

        private async void mapModeSurfaceBtn(object sender, RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (myMap.Style != MapStyle.Aerial3DWithRoads)
                    myMap.Style = MapStyle.Aerial3DWithRoads;
            }
            else
            {
                String title = "Αποτυχία σύνδεσης!";
                String message = "Η χρήση του μικτού χάρτη είναι εφικτή μόνο όταν υπάρχει ενεργή σύνδεση δικτύου.";
                MessageDialog msg = new MessageDialog(message, title);
                await msg.ShowAsync();
            }
        }

        private void settingsBtn(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings));
        }

        private void apopEuthBtn(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private void polAporBtn(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(About));
        }

        private void upomnimaBtn(object sender, RoutedEventArgs e)
        {
            StreetViewGrid.Visibility = Visibility.Visible;
        }

        public async void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame.CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
            else
            {
                e.Handled = true;
                MessageDialog msg = new MessageDialog("Η εφαρμογή θα τερματιστεί, θέλετε να συνεχίσετε;", "Έξοδος!");
                UICommand okBtn = new UICommand("Τερματισμός", oklBtnPressed);
                msg.Commands.Add(okBtn);
                UICommand cancelBtn = new UICommand("Άκυρο", cancelBtnPressed);
                msg.Commands.Add(cancelBtn);
                await msg.ShowAsync();
            }
        }

        private void oklBtnPressed(IUICommand command)
        {
            Application.Current.Exit();
        }

        private void cancelBtnPressed(IUICommand command)
        {
            return;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility ==
                AppViewBackButtonVisibility.Visible)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Collapsed;
            }

            if (!String.IsNullOrWhiteSpace(e.Parameter.ToString()))
            {
                try
                {
                    string parameter = (string)e.Parameter;

                    string[] location = parameter.Split(',');

                    double lat, lon;
                    bool isLatOk, isLonOk;

                    isLatOk = Double.TryParse(location[0], out lat);
                    isLonOk = Double.TryParse(location[1], out lon);

                    if(isLatOk && isLonOk)
                    {
                        //έχω στίγμα, μετακήνιση του χάρτη

                        var position = new Geopoint(new BasicGeoposition()
                        {
                            Latitude = lat,
                            Longitude = lon
                        });

                        await myMap.TrySetViewAsync(position, 18);
                    }
                }catch(Exception ex)
                {
                    
                }
            }

        }

        private void myMap_ZoomLevelChanged(MapControl sender, object args)
        {
            //Όταν κάνω zoom out πάνω από κάποιο όριο το infoBox να κλείνει!
        }

        private void myMap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
        }

        private void cheapestBtn_Click(object sender, RoutedEventArgs e)
        {
            if (cheapestStations != null && cheapestStations.Count > 0)
            {
                //Βρέθηκαν πρατήρια
                chepList.Items.Clear(); 

                ListItem li1 = new ListItem();
                ListItem li2 = new ListItem();
                ListItem li3 = new ListItem();

                li1.li_fuelType.Text = cheapestStations[0].FT;
                li1.li_logo.Source= new BitmapImage(CommonMethods.GetFuelStationIcon(cheapestStations[0].BR_ID));
                if (cheapestStations[0].BR == "ΑΝΕΞΑΡΤΗΤΟ ΠΡΑΤΗΡΙΟ")
                    li1.li_brand.Text = "ΑΠ";
                else if (cheapestStations[0].BR == "ΑΙΓΑΙΟ (AEGEAN)")
                    li1.li_brand.Text = "AEGEAN";
                else
                    li1.li_brand.Text = cheapestStations[0].BR;

                li1.li_price.Text = cheapestStations[0].FT_PR + " €";
                li1.li_price.Foreground = new SolidColorBrush(Colors.Black);
                li1.li_date.Text = cheapestStations[0].FT_DT;
                li1.li_owner.Text = cheapestStations[0].OW;
                li1.li_address.Text = cheapestStations[0].AD;
                li1.li_distance.Text = "Απόσταση από πρατήριο: " + cheapestStations[0].DIS + " Km";
                li1.main_li_body.Margin = new Thickness(0, 10, 0, 0);

                li2.li_fuelType.Text = cheapestStations[1].FT;
                li2.li_logo.Source = new BitmapImage(CommonMethods.GetFuelStationIcon(cheapestStations[1].BR_ID));

                if (cheapestStations[1].BR == "ΑΝΕΞΑΡΤΗΤΟ ΠΡΑΤΗΡΙΟ")
                    li2.li_brand.Text = "ΑΠ";
                else if (cheapestStations[1].BR == "ΑΙΓΑΙΟ (AEGEAN)")
                    li2.li_brand.Text = "AEGEAN";
                else
                    li2.li_brand.Text = cheapestStations[1].BR;

                li2.li_price.Text = cheapestStations[1].FT_PR + " €";
                li2.li_price.Foreground = new SolidColorBrush(Colors.Black);
                li2.li_date.Text = cheapestStations[1].FT_DT;
                li2.li_owner.Text = cheapestStations[1].OW;
                li2.li_address.Text = cheapestStations[1].AD;
                li2.li_distance.Text = "Απόσταση από πρατήριο: " + cheapestStations[1].DIS + " Km";
                li2.main_li_body.Margin = new Thickness(0, 10, 0, 0);

                li3.li_fuelType.Text = cheapestStations[2].FT;
                li3.li_logo.Source = new BitmapImage(CommonMethods.GetFuelStationIcon(cheapestStations[2].BR_ID));

                if (cheapestStations[2].BR == "ΑΝΕΞΑΡΤΗΤΟ ΠΡΑΤΗΡΙΟ")
                    li3.li_brand.Text = "ΑΠ";
                else if (cheapestStations[2].BR == "ΑΙΓΑΙΟ (AEGEAN)")
                    li3.li_brand.Text = "AEGEAN";
                else
                    li3.li_brand.Text = cheapestStations[0].BR;

                li3.li_price.Text = cheapestStations[2].FT_PR + " €";
                li3.li_price.Foreground = new SolidColorBrush(Colors.Black);
                li3.li_date.Text = cheapestStations[2].FT_DT;
                li3.li_owner.Text = cheapestStations[2].OW;
                li3.li_address.Text = cheapestStations[2].AD;
                li3.li_distance.Text = "Απόσταση από πρατήριο: " + cheapestStations[2].DIS + " Km";
                li3.main_li_body.Margin = new Thickness(0, 10, 0, 10);

                chepList.Items.Add(li1);
                chepList.Items.Add(li2);
                chepList.Items.Add(li3);

                if (cheapestStationsGrid.Visibility != Visibility.Visible)
                    cheapestStationsGrid.Visibility = Visibility.Visible;
            }
        }

        private void listViewBtn_Click(object sender, RoutedEventArgs e)
        {
            if(CommonMethods.GetDeviceFamily()==CommonMethods.DeviceFamily.Mobile)
            {
                //Τρέχει σε Mobile
                Frame.Navigate(typeof(ListViewPageMobile));
            }
            else if(CommonMethods.GetDeviceFamily() == CommonMethods.DeviceFamily.Desktop)
            {
                //Τρέχει σε Desktop
                Frame.Navigate(typeof(ListViewPageDesktop));
            }
            else
            {
                return;
            }
        }

        private void close_cheap_least_btn_Click(object sender, RoutedEventArgs e)
        {
            if (cheapestStationsGrid.Visibility == Visibility.Visible)
            {
                chepList.Items.Clear();
                cheapestStationsGrid.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Καλείτε όταν επιλέγετε ένα στοιχείο από την λίστα των φθηνότερων πρατηρίων
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void chepList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int selected = chepList.SelectedIndex;
                if (selected != -1)
                {
                    double latitude = Double.Parse(cheapestStations[selected].LT);
                    double longitude = Double.Parse(cheapestStations[selected].LG);

                    var position = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = latitude,
                        Longitude = longitude
                    });

                    cheapestStationsGrid.Visibility = Visibility.Collapsed;
                    chepList.Items.Clear();
                    await myMap.TrySetViewAsync(position, 18);
                }
            }
            catch { }
        }

        //==============================>  Μέθοδοι χειρισμού του streetView  <=================================================

        private void sv_left_btn_Click(object sender, RoutedEventArgs e)
        {
            //μείωση του heading για στροφή προς τα αριστερά (0 - 360)

            int heading = int.Parse(BaseHeading);

            string location = ActionsGrid.Tag.ToString();

            if (heading >= 0)
            {
                int newheading= heading - ChangeRateHeading;

                BaseHeading = newheading.ToString();

                var deviceType = CommonMethods.GetDeviceFamily();

                if (deviceType == CommonMethods.DeviceFamily.Desktop)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Desktop");
                else if (deviceType == CommonMethods.DeviceFamily.Mobile)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Mobile");
            }
        }

        private void sv_right_btn_Click(object sender, RoutedEventArgs e)
        {
            //αύξηση του heading για στροφή προς τα δεξιά (0 - 360)
            int heading = int.Parse(BaseHeading);

            string location = ActionsGrid.Tag.ToString();

            if (heading <= 360)
            {
                int newheading = heading + ChangeRateHeading;

                BaseHeading = newheading.ToString();

                var deviceType = CommonMethods.GetDeviceFamily();

                if (deviceType == CommonMethods.DeviceFamily.Desktop)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Desktop");
                else if (deviceType == CommonMethods.DeviceFamily.Mobile)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Mobile");
            }
        }

        private void sv_up_btn_Click(object sender, RoutedEventArgs e)
        {
            //αύξηση του pitch για κλήση προς τα επάνω (-90 - 90)
            int pitch = int.Parse(BasePitch);

            string location = ActionsGrid.Tag.ToString();

            if (pitch <= 90)
            {
                int newPitch = pitch + ChangeRatePitch;

                BasePitch = newPitch.ToString();

                var deviceType = CommonMethods.GetDeviceFamily();

                if (deviceType == CommonMethods.DeviceFamily.Desktop)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Desktop");
                else if (deviceType == CommonMethods.DeviceFamily.Mobile)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Mobile");
            }
        }

        private void sv_down_btn_Click(object sender, RoutedEventArgs e)
        {
            //μείωση του pitch για κλήση προς τα κάτω (-90 - 90)
            int pitch = int.Parse(BasePitch);

            string location = ActionsGrid.Tag.ToString();

            if (pitch >= -90)
            {
                int newPitch = pitch - ChangeRatePitch;

                BasePitch = newPitch.ToString();

                var deviceType = CommonMethods.GetDeviceFamily();

                if (deviceType == CommonMethods.DeviceFamily.Desktop)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Desktop");
                else if (deviceType == CommonMethods.DeviceFamily.Mobile)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Mobile");
            }
        }

        private void sv_zoomout_btn_Click(object sender, RoutedEventArgs e)
        {
            //Αύξηση του fov για zoom out
            int fov = int.Parse(BaseFov);

            string location = ActionsGrid.Tag.ToString();

            if (fov <= 120)
            {
                int newfov = fov +ChangeRateFov;

                BaseFov = newfov.ToString();

                var deviceType = CommonMethods.GetDeviceFamily();

                if (deviceType == CommonMethods.DeviceFamily.Desktop)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Desktop");
                else if (deviceType == CommonMethods.DeviceFamily.Mobile)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Mobile");
            }
        }

        private void sv_zoomin_btn_Click(object sender, RoutedEventArgs e)
        {
            //Μείωση του fov για zoom in
            int fov = int.Parse(BaseFov);

            string location = ActionsGrid.Tag.ToString();

            if (fov >= 0)
            {
                int newfov = fov - ChangeRateFov;

                BaseFov = newfov.ToString();

                var deviceType = CommonMethods.GetDeviceFamily();

                if (deviceType == CommonMethods.DeviceFamily.Desktop)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Desktop");
                else if (deviceType == CommonMethods.DeviceFamily.Mobile)
                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Mobile");
            }
        }


        private void sv_close_btn_Click(object sender, RoutedEventArgs e)
        {
            //Κλεισιμο του StreetViewGrid
            if (StreetViewGrid.Visibility == Visibility.Visible)
                StreetViewGrid.Visibility = Visibility.Collapsed;
        }

        private void StreetView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Άνοιγμα του StreetViewGrid
            if (StreetViewGrid.Visibility == Visibility.Collapsed)
            {
                //Κλείσιμο του ActionsGrid
                if (ActionsGrid.Visibility == Visibility.Visible)
                    ActionsGrid.Visibility = Visibility.Collapsed;
                //Άνοιγμα του StreetViewGrid

                var deviceType = CommonMethods.GetDeviceFamily();
                string location = ActionsGrid.Tag.ToString();

                if (deviceType == CommonMethods.DeviceFamily.Desktop)
                {
                    if (StreetViewGridDesktop.Visibility == Visibility.Collapsed)
                        StreetViewGridDesktop.Visibility = Visibility.Visible;

                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Desktop");
                }
                else if (deviceType == CommonMethods.DeviceFamily.Mobile)
                {
                    if (StreetViewGrid.Visibility == Visibility.Collapsed)
                        StreetViewGrid.Visibility = Visibility.Visible;

                    GetStreetViewImage(location, BaseHeading, BaseFov, BasePitch, "Mobile");
                }
                
                //GetStreetViewImage(location, "0", "30", "0", "Mobile");
            }
        }

        private void action_grid_close_btn_Click(object sender, RoutedEventArgs e)
        {
            //Κλείσιμο του grid ενεργειών
            if (ActionsGrid.Visibility == Visibility.Visible)
                ActionsGrid.Visibility = Visibility.Collapsed;
        }

        private async void GetStreetViewImage(string location,string heading, string fov, string pitch, string device)
        {
            string BASE_URL = "https://maps.googleapis.com/maps/api/streetview?";
            string KEY = "AIzaSyAE_U2HUOl79efB7J58nwv9_VaIrIjvaaA";
            //
            if (device == "Mobile")
            {
                if (sv_progress_ring.IsActive == false)
                    sv_progress_ring.IsActive = true;

                string ImageWidth = "348";
                string ImageHeight = "248";

                string call_url = BASE_URL + "size=" + ImageWidth + "x" + ImageHeight + "&location=" + location + "&heading=" + heading +
                    "&fov=" + fov + "&pitch=" + pitch + "&key=" + KEY;

                string call_url2 = BASE_URL + "size=" + ImageWidth + "x" + ImageHeight + "&location=" + location + "&key=" + KEY;

                HttpClient client = new HttpClient();
                Stream st = await client.GetStreamAsync(call_url);
                var memoryStream = new MemoryStream();
                await st.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                BitmapImage img = new BitmapImage();
                img.SetSource(memoryStream.AsRandomAccessStream());
                streetViewImage.Source = img;

                if (sv_progress_ring.IsActive == true)
                    sv_progress_ring.IsActive = false;
            }
            else if (device == "Desktop")
            {
                if (sv_progress_ring_desktop.IsActive == false)
                    sv_progress_ring_desktop.IsActive = true;

                string ImageWidth = "800";
                string ImageHeight = "400";

                string call_url = BASE_URL + "size=" + ImageWidth + "x" + ImageHeight + "&location=" + location + "&heading=" + heading +
                    "&fov=" + fov + "&pitch=" + pitch + "&key=" + KEY;

                string call_url2 = BASE_URL + "size=" + ImageWidth + "x" + ImageHeight + "&location=" + location + "&key=" + KEY;

                HttpClient client = new HttpClient();
                Stream st = await client.GetStreamAsync(call_url);
                var memoryStream = new MemoryStream();
                await st.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                BitmapImage img = new BitmapImage();
                img.SetSource(memoryStream.AsRandomAccessStream());
                streetViewImageDesktop.Source = img;

                if (sv_progress_ring_desktop.IsActive == true)
                    sv_progress_ring_desktop.IsActive = false;
            }
        }

        private void sv_dsk_close_btn_Click(object sender, RoutedEventArgs e)
        {
            //Κλείσιμο του StreetViewGridDesktop
            if (StreetViewGridDesktop.Visibility == Visibility.Visible)
                StreetViewGridDesktop.Visibility = Visibility.Collapsed;
        }

        private async void showDirectionsToFuelStation_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string location = ActionsGrid.Tag.ToString();

            // Center on New York City
            var uriNewYork = new Uri(@"bingmaps:?cp=40.726966~-74.006076");

            // Launch the Windows Maps app
            var launcherOptions = new Windows.System.LauncherOptions();
            launcherOptions.TargetApplicationPackageFamilyName = "Microsoft.WindowsMaps_8wekyb3d8bbwe";
            var success = await Windows.System.Launcher.LaunchUriAsync(uriNewYork, launcherOptions);

        }
    }
}
