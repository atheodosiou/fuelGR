using fuelGR_UWP.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using fuelGR_UWP.Model;
using Windows.UI.Popups;
using Windows.UI.Core;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace fuelGR_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {    
        //Μεταβλητές για την διαχείρηση των επιλεγμένων εταιριών
        private static string NewCompanySelection = "";
        private static bool IsNewCompaniesSelected = false;

        public Settings()
        {
            this.InitializeComponent();
            //εμφάνιση του πισω κουμπιού στην περίπτωση desktop ή tablet
            if (DeviceTypeHelper.GetDeviceFormFactorType()==DeviceFormFactorType.Desktop)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Visible;
            }
            else
            {
                if (SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility ==
                    AppViewBackButtonVisibility.Visible)
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                    AppViewBackButtonVisibility.Visible;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(companiesBtn.Content.ToString()))
                CommonMethods.UpdateCompaniesNumLabel(companiesBtn);
        }
                
        //χρησιμοποιήτε για να υλοποιηθεί ο έλεγχος για το ποτε πρεπει να πάει πίσω
        public bool canGoback = true;

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (canGoback)
            {
                if (companiesWindow.Visibility == Visibility.Visible)
                {
                    //stop back navigation because companies window is open!! 
                    e.Cancel = true;
                    MessageDialog msg = new MessageDialog("Πρέπει πρώτα να ολοκληρώσετε την επιλογή εταιριών ή να την ακυρώσετε για να συνεχίσεται.",
                        "Προσοχή!");
                    UICommand okBtn = new UICommand("Το κατάλαβα");
                    msg.Commands.Add(okBtn);
                    await msg.ShowAsync();
                }
                else
                {
                    canGoback = false;

                    Frame rootFrame = Window.Current.Content as Frame;

                    if (rootFrame.CanGoBack)
                    {
                        rootFrame.GoBack();
                    }
                }
            }
        }
        
        private List<FuelCompany> BindCompaniesList()
        {
            try
            {
                var localSettings = ApplicationData.Current.LocalSettings;
                var selectedFuelCompanies = localSettings.Values["selectedCompanies"];

                string[] ids = selectedFuelCompanies.ToString().Split(',');

                List<FuelCompany> stations = new List<FuelCompany>();

                stations.Add(new FuelCompany { Id = "1", CompanyName = "ΑΝΕΞΑΡΤΗΤΟ ΠΡΑΤΗΡΙΟ", Selected = ids.Contains("1"), LogoUri = "Assets/StationLogos/1b.png" });
                stations.Add(new FuelCompany { Id = "2", CompanyName = "JETOIL", Selected = ids.Contains("2"), LogoUri = "Assets/StationLogos/2.png" });
                stations.Add(new FuelCompany { Id = "3", CompanyName = "SHELL", Selected = ids.Contains("3"), LogoUri = "Assets/StationLogos/3.png" });
                stations.Add(new FuelCompany { Id = "4", CompanyName = "SILKOIL", Selected = ids.Contains("4"), LogoUri = "Assets/StationLogos/4.png" });
                stations.Add(new FuelCompany { Id = "5", CompanyName = "ΕΤΕΚΑ", Selected = ids.Contains("5"), LogoUri = "Assets/StationLogos/5.png" });
                stations.Add(new FuelCompany { Id = "6", CompanyName = "AVIN", Selected = ids.Contains("6"), LogoUri = "Assets/StationLogos/6b.png" });
                stations.Add(new FuelCompany { Id = "7", CompanyName = "AEGEAN", Selected = ids.Contains("7"), LogoUri = "Assets/StationLogos/7.png" });
                stations.Add(new FuelCompany { Id = "8", CompanyName = "EKO", Selected = ids.Contains("8"), LogoUri = "Assets/StationLogos/8.png" });
                stations.Add(new FuelCompany { Id = "9", CompanyName = "REVOIL", Selected = ids.Contains("9"), LogoUri = "Assets/StationLogos/9.png" });
                stations.Add(new FuelCompany { Id = "10", CompanyName = "BP", Selected = ids.Contains("10"), LogoUri = "Assets/StationLogos/10.png" });
                stations.Add(new FuelCompany { Id = "11", CompanyName = "ΕΛΙΝΟΙΛ", Selected = ids.Contains("11"), LogoUri = "Assets/StationLogos/11.png" });
                stations.Add(new FuelCompany { Id = "12", CompanyName = "DRACOIL", Selected = ids.Contains("12"), LogoUri = "Assets/StationLogos/12.png" });
                stations.Add(new FuelCompany { Id = "13", CompanyName = "CYCLON", Selected = ids.Contains("13"), LogoUri = "Assets/StationLogos/13.png" });
                stations.Add(new FuelCompany { Id = "14", CompanyName = "KMOIL", Selected = ids.Contains("14"), LogoUri = "Assets/StationLogos/14.png" });
                stations.Add(new FuelCompany { Id = "15", CompanyName = "EL-PETROIL", Selected = ids.Contains("15"), LogoUri = "Assets/StationLogos/15b.png" });
                stations.Add(new FuelCompany { Id = "16", CompanyName = "ΑΡΓΩ", Selected = ids.Contains("16"), LogoUri = "Assets/StationLogos/16.png" });
                stations.Add(new FuelCompany { Id = "17", CompanyName = "KAOIL", Selected = ids.Contains("17"), LogoUri = "Assets/StationLogos/17b.png" });
                stations.Add(new FuelCompany { Id = "18", CompanyName = "MEDOIL", Selected = ids.Contains("18"), LogoUri = "Assets/StationLogos/18b.png" });
                stations.Add(new FuelCompany { Id = "19", CompanyName = "ΤΡΙΑΙΝΑ", Selected = ids.Contains("19"), LogoUri = "Assets/StationLogos/19.png" });
                stations.Add(new FuelCompany { Id = "20", CompanyName = "CRETA PETROL", Selected = ids.Contains("20"), LogoUri = "Assets/StationLogos/3.png" });
                return stations;
            }
            catch (Exception e)
            {
                CommonMethods.ShowMessage("Error - BindCompanies", e.Message);
                return null;
            }
        }

        private void navigateBackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void companiesBtn_Click(object sender, RoutedEventArgs e)
        {
            CompaniesListView.ItemsSource = BindCompaniesList();

            if (companiesWindow.Visibility == Visibility.Collapsed)
                companiesWindow.Visibility = Visibility.Visible;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (companiesWindow.Visibility == Visibility.Visible)
                companiesWindow.Visibility = Visibility.Collapsed;
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
        
        private void selectedCompanieBox_checked(object sender, RoutedEventArgs e)
        {
            //Έλεγχος για το πιο checkBox έχει επιλεχθεί
            CheckBox cb = (CheckBox)sender;
            //Στο cb.Tag είναι αποθυκευμένο το Id του πρατηρίου που θέλω να επεξεργαστώ
            var localSettings = ApplicationData.Current.LocalSettings;
            var selectedFuelCompanies = localSettings.Values["selectedCompanies"];
            var temporarySelection = localSettings.Values["temporarySelection"];

            if(temporarySelection==null)
            {
                if (selectedFuelCompanies != null)
                {
                    // οι ρυθμίσεις υπάρχουν και άρα μπορώ να συνεχίσω 
                    string[] savedCompaniesArray = ((string)localSettings.Values["selectedCompanies"]).Split(',');
                    string savedCompanies = (string)localSettings.Values["selectedCompanies"];

                    //Έλεγχος αν υπάρχει η τρέχουσα εταιρία στο string των εταιριών για να την προσθέσω

                    if (!savedCompaniesArray.Contains(cb.Tag.ToString()))
                    {
                        //Η εταιρία δεν περιέχετε άρα την προσθέτω!

                        savedCompanies += "," + cb.Tag.ToString();

                        if (localSettings.Values.ContainsKey("temporarySelection"))
                        {
                            //Έχει γίνει και άλλη αλλαγή πριν την οριστικοποίηση της λίστας
                            //Αρα αποθηκεύω τις αλλαγές μου προσορινά εδω και οριστικοποιώ στο commit
                            localSettings.Values["temporarySelection"] = savedCompanies;
                        }
                        else
                        {
                            localSettings.Values.Add("temporarySelection", savedCompanies);
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    //CommonMethods.ShowMessage("selectedCompanieBox_checked",
                        //"Οι ρυθμίσεις δεν υπάρχουν!");
                }
            }
            else
            {
                if (temporarySelection != null)
                {
                    // οι ρυθμίσεις υπάρχουν και άρα μπορώ να συνεχίσω απο την προσορινή λίστα
                    string[] savedCompaniesArray = ((string)localSettings.Values["temporarySelection"]).Split(',');
                    string savedCompanies = (string)localSettings.Values["temporarySelection"];

                    //Έλεγχος αν υπάρχει η τρέχουσα εταιρία στο string των εταιριών για να την προσθέσω

                    if (!savedCompaniesArray.Contains(cb.Tag.ToString()))
                    {
                        //Η εταιρία δεν περιέχετε άρα την προσθέτω!

                        savedCompanies += "," + cb.Tag.ToString();
                        if (localSettings.Values.ContainsKey("temporarySelection"))
                        {
                            //Έχει γίνει και άλλη αλλαγή πριν την οριστικοποίηση της λίστας
                            //Αρα αποθηκεύω τις αλλαγές μου προσορινά εδω και οριστικοποιώ στο commit
                            localSettings.Values["temporarySelection"] = savedCompanies;
                            IsNewCompaniesSelected = true;
                        }
                        else
                        {
                            localSettings.Values.Add("temporarySelection", savedCompanies);
                            IsNewCompaniesSelected = true;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    CommonMethods.ShowMessage("selectedCompanieBox_checked",
                        "Οι ρυθμίσεις δεν υπάρχουν!");
                }
            }

        }

        private void selectedCompanieBox_unChecked(object sender, RoutedEventArgs e)
        {
            //Έλεγχος για το πιο checkBox έχει επιλεχθεί
            CheckBox cb = (CheckBox)sender;
            //Στο cb.Tag είναι αποθυκευμένο το Id του πρατηρίου που θέλω να επεξεργαστώ
            var localSettings = ApplicationData.Current.LocalSettings;
            var selectedFuelCompanies = localSettings.Values["selectedCompanies"];
            var temporarySelection = localSettings.Values["temporarySelection"];

            if (temporarySelection == null)
            {
                //Αν η προσορινή λίστα εταιριών είναι καινή, τότε μάλλόν είμαι στην πρώτη 
                //αλλαγή
                if (selectedFuelCompanies != null && cb.Tag != null)
                {
                    List<String> newList = new List<string>();

                    string[] selected = selectedFuelCompanies.ToString().Split(',');

                    //Αφαιρώ την εταιρία που διάλεξε ο χρήστης
                    foreach (string x in selected)
                    {
                        if (x != cb.Tag.ToString())
                            newList.Add(x);
                    }

                    //Χτίζω το νέο string 
                    string newCompanies = "";
                    for (int i = 0; i < newList.Count; i++)
                    {
                        if (i < newList.Count - 1)
                            newCompanies += newList[i] + ",";
                        else
                            newCompanies += newList[i];
                    }

                    //Αν οι νέα επιλογή δεν είναι καινή, τότε την αποθηκεύω στην static
                    //μεταβλητη για να μπορέσω να την διαχειριστώ από τα κου commit buttons

                    if (!string.IsNullOrWhiteSpace(newCompanies))
                    {
                        IsNewCompaniesSelected = true;
                        //NewCompanySelection = newCompanies;
                        if (localSettings.Values.ContainsKey("temporarySelection"))
                        {
                            //Έχει γίνει και άλλη αλλαγή πριν την οριστικοποίηση της λίστας
                            //Αρα αποθηκεύω τις αλλαγές μου προσορινά εδω και οριστικοποιώ στο commit
                            localSettings.Values["temporarySelection"] = newCompanies;
                        }
                        else
                        {
                            localSettings.Values.Add("temporarySelection", newCompanies);
                        }
                    }
                }
            }
            else
            {
                //Αν η προσορινή λίστα δεν είναι καινή, τότε κάνω τις αλλαγές μου
                // σε αυτήν την λίστα
                if (temporarySelection != null && cb.Tag != null)
                {
                    List<String> newList = new List<string>();

                    string[] selected = temporarySelection.ToString().Split(',');

                    //Αφαιρώ την εταιρία που διάλεξε ο χρήστης
                    foreach (string x in selected)
                    {
                        if (x != cb.Tag.ToString())
                            newList.Add(x);
                    }

                    //Χτίζω το νέο string 
                    string newCompanies = "";
                    for (int i = 0; i < newList.Count; i++)
                    {
                        if (i < newList.Count - 1)
                            newCompanies += newList[i] + ",";
                        else
                            newCompanies += newList[i];
                    }

                    //Αν οι νέα επιλογή δεν είναι καινή, τότε την αποθηκεύω στην static
                    //μεταβλητη για να μπορέσω να την διαχειριστώ από τα κου commit buttons

                    if (!string.IsNullOrWhiteSpace(newCompanies))
                    {
                        IsNewCompaniesSelected = true;
                        //NewCompanySelection = newCompanies;
                        if (localSettings.Values.ContainsKey("temporarySelection"))
                        {
                            //Έχει γίνει και άλλη αλλαγή πριν την οριστικοποίηση της λίστας
                            //Αρα αποθηκεύω τις αλλαγές μου προσορινά εδω και οριστικοποιώ στο commit
                            localSettings.Values["temporarySelection"] = newCompanies;
                        }
                        else
                        {
                            localSettings.Values.Add("temporarySelection", newCompanies);
                        }
                    }
                }
            }
        }

        //======= Commit & Cancel Btns for Companies Selection ===========

        private void commitBtn_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            if(localSettings!=null)
            {
                if(IsNewCompaniesSelected)
                {
                    if(localSettings.Values["temporarySelection"]!=null)
                    {
                        //έχουν γίνει αλλαγές και πρέπει να αποθηκευτούν
                        string comps= (string)localSettings.Values["temporarySelection"];
                        localSettings.Values["selectedCompanies"] = comps;
                        
                        CommonMethods.ShowMessage("Αποθύκευση αλλαγών!", "Οι αλλαγές αποθυκεύτηκαν επιτυχώς!");
                    }
                }
                CommonMethods.UpdateCompaniesNumLabel(companiesBtn);
            }
            else
            {
                CommonMethods.ShowMessage("Αποθύκευση αλλαγών!", "Κάτι πήγε στραβά! Οι αλλαγές δεν αποθυκεύτηκαν.");
            }

            if (companiesWindow.Visibility == Visibility.Visible)
                companiesWindow.Visibility = Visibility.Collapsed;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (companiesWindow.Visibility == Visibility.Visible)
                companiesWindow.Visibility = Visibility.Collapsed;
        }


        //================== Event Handlers for data colevtion ======================

        private void fuelTypeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            try
            {
                if (localSettings.Values.ContainsKey("fuelIndex"))
                {
                    int index = (int)localSettings.Values["fuelIndex"];
                    if (fuelTypeSelector.SelectedIndex != index)
                        localSettings.Values["fuelIndex"] =
                            fuelTypeSelector.SelectedIndex;
                }
            }
            catch { }

        }

        private void fuelTypeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            int selectedIndex = fuelTypeSelector.SelectedIndex;

            try
            {
                int index = (int)localSettings.Values["fuelIndex"];

                if (localSettings.Values.ContainsKey("fuelIndex") &&
                    fuelTypeSelector.SelectedIndex != index)
                    fuelTypeSelector.SelectedIndex = index;
            }
            catch { }
        }

        private void comboBoxPremiumFuels_Loaded(object sender, RoutedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            int selectedIndex = comboBoxPremiumFuels.SelectedIndex;

            try
            {
                int index = (int)localSettings.Values["premFuelIndex"];

                if (localSettings.Values.ContainsKey("premFuelIndex") &&
                    comboBoxPremiumFuels.SelectedIndex != index)
                    comboBoxPremiumFuels.SelectedIndex = index;
            }
            catch { }
        }

        private void comboBoxPremiumFuels_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            try
            {
                if (localSettings.Values.ContainsKey("premFuelIndex"))
                {
                    int index = (int)localSettings.Values["premFuelIndex"];
                    if (comboBoxPremiumFuels.SelectedIndex != index)
                        localSettings.Values["premFuelIndex"] =
                            comboBoxPremiumFuels.SelectedIndex;
                }
            }
            catch { }
        }

        private void comboBoxPriceOld_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            try
            {
                if (localSettings.Values.ContainsKey("priceOldIndex"))
                {
                    int index = (int)localSettings.Values["priceOldIndex"];
                    if (comboBoxPriceOld.SelectedIndex != index)
                        localSettings.Values["priceOldIndex"] =
                            comboBoxPriceOld.SelectedIndex;
                }
            }
            catch { }
        }

        private void comboBoxPriceOld_Loaded(object sender, RoutedEventArgs e)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            int selectedIndex = comboBoxPriceOld.SelectedIndex;

            try
            {
                int index = (int)localSettings.Values["priceOldIndex"];

                if (localSettings.Values.ContainsKey("priceOldIndex") &&
                    comboBoxPriceOld.SelectedIndex != index)
                    comboBoxPriceOld.SelectedIndex = index;
            }
            catch { }
        }
        
    }
}
