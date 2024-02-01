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
    /// Interaction logic for UnosOceneWindow.xaml
    /// </summary>
    public partial class UnosOceneWindow : Window
    {
        // Svojstvo koje sadrži unetu ocenu i omogućava samo čitanje (nema javnog setera)
        public int UnetaOcena { get; private set; }

        public UnosOceneWindow()
        {
            InitializeComponent();
        }

        // Metoda koja se izvršava kada korisnik klikne na dugme "Potvrdi"
        private void Potvrdi_Click(object sender, RoutedEventArgs e)
        {
            // Proverava da li je unesena ocena (da li je vrednost postavljena u integerUpDown)
            if (integerUpDown.Value.HasValue)
            {
                // Dobija vrednost unesene ocene
                int ocena = integerUpDown.Value.Value;

                if (ocena >= 5 && ocena <= 10)
                {
                    UnetaOcena = ocena; // Ako je unesena validna ocena, postavlja je u svojstvo UnetaOcena
                    DialogResult = true; // Postavlja DialogResult na "true" kako bi označio da je korisnik potvrdio unos
                }
                else
                {
                    // Ako unesena ocena nije validna, prikazuje poruku o grešci
                    MessageBox.Show("Unesite validnu ocenu između 5 i 10.", "Greška u unosu ocene", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Ako korisnik nije uneo ocenu, prikazuje poruku o grešci
                MessageBox.Show("Molimo unesite ocenu između 5 i 10.", "Greška u unosu ocene", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Metoda koja se izvršava kada korisnik klikne na dugme "Odustani"
        private void Odustani_Click(object sender, RoutedEventArgs e)
        {
            // Postavlja DialogResult na "false" kako bi označio da je korisnik odustao od unosa
            DialogResult = false;
        }
    }
}
