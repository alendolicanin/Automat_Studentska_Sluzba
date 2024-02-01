using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Automat_Studentska_Služba
{
    /// <summary>
    /// Interaction logic for UnosUplateWindow.xaml
    /// </summary>
    public partial class UnosUplateWindow : Window
    {
        // Svojstvo koje sadrži unetu uplatu i omogućava samo čitanje (nema javnog setera)
        public int UnetaUplata { get; private set; }

        // Svojstvo koje sadrži vrstu uplate i omogućava samo čitanje (nema javnog setera)
        public String VrstaUplate { get; private set; }

        public UnosUplateWindow()
        {
            InitializeComponent();
        }

        // Metoda koja se izvršava kada korisnik klikne na dugme "Potvrdi"
        private void Potvrdi_Click(object sender, RoutedEventArgs e)
        {
            // Provera da li je uneta vrednost za uplatu (da li je vrednost postavljena u integerUpDown)
            if (integerUpDown.Value.HasValue)
            {
                // Dobija vrednost unesene uplate
                int uplata = integerUpDown.Value.Value;

                // Provera da li je uplata veća ili jednaka 300
                if (uplata >= 300)
                {
                    // Dobija izabranu vrstu uplate iz ComboBox-a
                    string vrstaUplate = (cmbVrstaUplate.SelectedItem as ComboBoxItem)?.Content.ToString();

                    // Provera da li je izabrana vrsta uplate
                    if (!string.IsNullOrEmpty(vrstaUplate))
                    {
                        // Postavljanje svojstava
                        UnetaUplata = uplata;
                        VrstaUplate = vrstaUplate;
                        // Postavlja DialogResult na "true" kako bi označio potvrdu
                        DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Molimo izaberite vrstu uplate.", "Greška u unosu uplate", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Unesite uplatu koja je veća ili jednaka 300.", "Greška u unosu uplate", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Molimo unesite uplatu.", "Greška u unosu uplate", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Metoda koja se izvršava kada korisnik klikne na dugme "Odustani"
        private void Odustani_Click(object sender, RoutedEventArgs e)
        {
            // Postavlja DialogResult na "false" kako bi označio da je korisnik odustao od unosa uplate
            DialogResult = false;
        }
    }
}
