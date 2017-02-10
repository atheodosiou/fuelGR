using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace fuelGR_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListViewPageMobile : Page
    {
        public ListViewPageMobile()
        {
            this.InitializeComponent();
        }

        public List<GasStation> GasStations;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                //Διάβασμα του αρχείου από το δίσμο και προβολή τον δοδομένων!
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                StorageFile sampleFile = await localFolder.GetFileAsync("gasStations");
                string responseXml = await FileIO.ReadTextAsync(sampleFile);

                ShowDataOnList(responseXml);
            }
            catch (Exception ex)
            {
                //Δεν βρέθηκε
                CommonMethods.ShowMessage("Σφάλμα!", ex.Message);
            }
        }

        private void ShowDataOnList(string responseXml)
        {
            GasStations = GetItemList(responseXml);

            int items = GasStations.Count;
            list_title.Text = "Λίστα Πρατηρίων (" + items.ToString() + ")";

            foreach (GasStation gs in GasStations)
            {
                ListItem lid = new ListItem();

                lid.li_fuelType.Text = gs.FT;
                //Εισαγωγή του latitude και του longintude στα παιδία αυτά για χρήση αργότερα 
                lid.li_fuelType.Tag = gs.LT;
                lid.li_brand.Tag = gs.LG;

                if (gs.BR == "ΑΝΕΞΑΡΤΗΤΟ ΠΡΑΤΗΡΙΟ")
                    lid.li_brand.Text = "ΑΠ";
                else if (gs.BR == "ΑΙΓΑΙΟ (AEGEAN)")
                    lid.li_brand.Text = "AEGEAN";
                else
                    lid.li_brand.Text = gs.BR;

                lid.li_logo.Source = new BitmapImage(CommonMethods.GetFuelStationIcon(gs.BR_ID));
                lid.li_price.Text = gs.FT_PR + " €";
                lid.li_date.Text = gs.FT_DT;
                lid.li_owner.Text = gs.OW;
                lid.li_address.Text = gs.AD;
                lid.li_distance.Text = "Απόσταση από πρατήριο: " + gs.DIS + " km";
                gs_gridViewMobile.Items.Add(lid);
            }
        }

        private List<GasStation> GetItemList(string responseXml)
        {
            XDocument xdoc = XDocument.Parse(responseXml);

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

            return gss;
        }

        private void gs_gridViewMobile_ItemClick(object sender, ItemClickEventArgs e)
        {
            ListItem lid = (ListItem)e.ClickedItem;
            if (lid != null)
            {
                string location = lid.li_fuelType.Tag.ToString() + "," +
                    lid.li_brand.Tag.ToString();

                Frame.Navigate(typeof(MainPage), location);
               
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //αφαιρώ δύο entries απο το backstack  γιατί το gs_gridViewMobile_ItemClick
            //δημηουργεί και δεύτερο navigation entry και δεν λειτουργεί σωστά bakcButton
            for (int i=0; i<2; i++)
            {
                Frame.BackStack.Remove(Frame.BackStack.FirstOrDefault());
            }
        }
    }
}
