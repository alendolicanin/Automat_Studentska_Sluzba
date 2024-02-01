using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    /// Interaction logic for Admin.xaml
    /// </summary>
    public partial class Admin : Window
    {
        public Admin()
        {
            InitializeComponent();
        }

        // Generička metoda za otvaranje prozora za različite operacije nad studentima
        private void OpenStudentWindow<T>() where T : Window, new()
        {
            // Stvaranje nove instance prozora tipa T
            T studentWindow = new T();
            studentWindow.Show();
        }

        // Metoda za otvaranje prozora za dodavanje novog studenta
        private void Dodaj_Studenta(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<DodajStudenta>();
        }

        // Metoda za otvaranje prozora sa listom studenata
        private void Lista_Studenata(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<ListaStudenata>();
        }

        // Metoda za otvaranje prozora za izmenu ili brisanje studenta
        private void Izmeni_Izbrisi_Studenta(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<Izmeni_IzbrisiStudenta>();
        }

        // Metoda za otvaranje prozora za potvrdu o upisu
        private void PotvrdaOUpisu(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<PotvrdaOUpisu>();
        }

        // Metoda za otvaranje prozora za potvrdu o položenim ispitima
        private void PotvrdaOIspitima(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<PotvrdaOIspitima>();
        }

        // Metoda za otvaranje prozora za raspored nastave
        private void RasporedNastave(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<RasporedNastave>();
        }

        // Metoda za otvaranje prozora za prijavu ispita
        private void PrijavaIspita(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<PrijavaIspita>();
        }

        // Metoda za otvaranje prozora za prikazivanje liste prijavljenih ispita
        private void PrijavljeniIspiti(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<PrijavljeniIspiti>();
        }

        // Metoda za otvaranje prozora za prikazivanje liste položenih ispita
        private void PolozeniIspit(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<PolozeniIspiti>();
        }

        // Metoda za otvaranje prozora za evidenciju o uplatama
        private void EvidencijaOUplati_Click(object sender, RoutedEventArgs e)
        {
            OpenStudentWindow<EvidencijeOUplatama>();
        }

        private void OpenPdfDocument(string documentPath)
        {
            try
            {
                if (File.Exists(documentPath))
                {
                    Process.Start(documentPath);
                }
                else
                {
                    MessageBox.Show("PDF dokument nije pronađen.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Došlo je do greške prilikom otvaranja PDF dokumenta: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Metoda za otvaranje PDF dokumenta kada se klikne na sliku sa uputstvom
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Putanja do PDF dokumenta koji želite otvoriti
            string pdfPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uputstvo.pdf"); // Putanja do direktorijuma aplikacije 
            OpenPdfDocument(pdfPath);
        }
    }
}
