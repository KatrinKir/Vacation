using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.IO;

namespace Vacation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Get a reference to the trips collection.
            Trips _trips = (Trips)this.Resources["trips"];
            Trips _hotels = (Trips)this.Resources["hotels"];

            using (var reader = new StreamReader(@"C:\code\reisid.txt"))
            {
                Decimal abix;
                Random rnd = new Random();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    Decimal.TryParse(values[2], out abix);
                    _trips.Add(new Trip()
                    {
                        ProjectName = values[0],
                        TripName = values[1],
                        TripPrice = abix,
                        //Complete = (rnd.Next(1, 20) % 2 == 0)
                    });

                }
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(';');
                    Decimal.TryParse(values[2], out abix);
                    _hotels.Add(new Trip()
                    {
                        ProjectName = values[0],
                        HotelPrice = abix,
                        //Complete = (rnd.Next(1, 20) % 2 == 0)
                    });
                }
            }

            //// Generate some trip data and add it to the trip list.
            //for (int i = 1; i <= 14; i++)
            //{
            //    _trips.Add(new Trip()
            //    {
            //        ProjectName = "Kontinent " + ((i % 3) + 1).ToString(),
            //        TripName = "Riik " + i.ToString(),
            //        DueDate = DateTime.Now.AddDays(i),
            //        Complete = (i % 2 == 0)
            //    });
            //}

        }

        private void UngroupButton_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView cvTrips = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
            if (cvTrips != null)
            {
                cvTrips.GroupDescriptions.Clear();
            }

        }


        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView cvTrips = CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource);
            if (cvTrips != null && cvTrips.CanGroup == true)
            {
                cvTrips.GroupDescriptions.Clear();
                cvTrips.GroupDescriptions.Add(new PropertyGroupDescription("ProjectName"));
                cvTrips.GroupDescriptions.Add(new PropertyGroupDescription("Complete"));
            }
        }

        private void CompleteFilter_Changed(object sender, RoutedEventArgs e)
        {
            // Refresh the view to apply filters.
            CollectionViewSource.GetDefaultView(dataGrid1.ItemsSource).Refresh();
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            Trip t = e.Item as Trip;
            if (t != null)
            // If filter is turned on, filter completed items.
            {
                if (this.cbCompleteFilter.IsChecked == true && t.Complete == true)
                    e.Accepted = false;
                else
                    e.Accepted = true;
            }
        }
    }

    [ValueConversion(typeof(Boolean), typeof(String))]
    public class CompleteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool complete = (bool)value;
            if (complete)
                return "Complete";
            else
                return "Active";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string strComplete = (string)value;
            if (strComplete == "Complete")
                return true;
            else
                return false;
        }
    }

    // Trip Class
    // Requires using System.ComponentModel;
    public class Trip : INotifyPropertyChanged, IEditableObject
    {
        // The Trip class implements INotifyPropertyChanged and IEditableObject 
        // so that the datagrid can properly respond to changes to the 
        // data collection and edits made in the DataGrid.

        // Private trip data.
        private string m_Continent = string.Empty;
        private string m_TripName = string.Empty;
        private Decimal m_TripPrice = 0;
        private Decimal m_HotelPrice = 0;
        private string m_HotelName = string.Empty;
        private bool m_Complete = false;
        private bool m_Hotel = false;

        // Data for undoing canceled edits.
        private Trip temp_Trip = null;
        private Trip temp_Hotel = null;
        private bool m_Editing = false;

        // Public properties. 
        public string ProjectName
        {
            get { return this.m_Continent; }
            set
            {
                if (value != this.m_Continent)
                {
                    this.m_Continent = value;
                    NotifyPropertyChanged("ProjectName");
                }
            }
        }

        public string TripName
        {
            get { return this.m_TripName; }
            set
            {
                if (value != this.m_TripName)
                {
                    this.m_TripName = value;
                    NotifyPropertyChanged("TripName");
                }
            }
        }

        public Decimal TripPrice
        {
            get { return this.m_TripPrice; }
            set
            {
                if (value != this.m_TripPrice)
                {
                    this.m_TripPrice = value;
                    NotifyPropertyChanged("Trip Price");
                }
            }
        }

        

        public Decimal HotelPrice
        {
            get { return this.m_HotelPrice; }
            set
            {
                if (value != this.m_HotelPrice)
                {
                    this.m_HotelPrice = value;
                    NotifyPropertyChanged("Hotel Price");
                }
            }
        }

        public bool Complete
        {
            get { return this.m_Complete; }
            set
            {
                if (value != this.m_Complete)
                {
                    if (value)
                    {
                        MessageBox.Show( this.m_TripName + " price " + this.m_TripPrice.ToString() + " €");
                    }
                   
                    this.m_Complete = value;
                    NotifyPropertyChanged("Complete");
                }
            }
        }

        public bool Hotel
        {
            get { return this.m_Complete; }
            set
            {
                if (value != this.m_Complete)
                {
                    if (value)
                    {
                        MessageBox.Show("Hotel price " + this.m_HotelPrice.ToString() + "€");
                    }

                    this.m_Complete = value;
                    NotifyPropertyChanged("Complete");
                }
            }
        }

        // Implement INotifyPropertyChanged interface.
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Implement IEditableObject interface.
        public void BeginEdit()
        {
            if (m_Editing == false)
            {
                temp_Trip = this.MemberwiseClone() as Trip;
                temp_Hotel = this.MemberwiseClone() as Trip;
                m_Editing = true;
            }
        }

        public void CancelEdit()
        {
            if (m_Editing == true)
            {
                this.ProjectName = temp_Trip.ProjectName;
                this.TripName = temp_Trip.TripName;
                this.TripPrice = temp_Trip.TripPrice;
                this.Complete = temp_Trip.Complete;
                this.HotelPrice = temp_Hotel.HotelPrice;
                this.Complete = temp_Hotel.Complete;
                m_Editing = false;
            }
        }

        public void EndEdit()
        {
            if (m_Editing == true)
            {
                temp_Trip = null;
                temp_Hotel = null;
                m_Editing = false;
            }
        }
    }
    // Requires using System.Collections.ObjectModel;
    public class Trips : ObservableCollection<Trip>
    {
        // Creating the Trips collection in this way enables data binding from XAML.
    }
}
