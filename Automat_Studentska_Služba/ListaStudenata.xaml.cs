using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    /// Interaction logic for ListaStudenata.xaml
    /// </summary>
    public partial class ListaStudenata : Window
    {
        // Kreirajte instancu klase STUDENT koja se koristi za pristup podacima o studentima
        private STUDENT student = new STUDENT();

        public ListaStudenata()
        {
            InitializeComponent();
            PopuniStudente();
        }

        // Ova metoda se koristi za popunjavanje DataGrid-a (studentDataGrid) sa podacima o studentima
        private void PopuniStudente()
        {
            List<STUDENT> students = student.GetStudents();
            studentDataGrid.ItemsSource = students;
        }

        // Ova metoda se poziva kada korisnik dvostruko klikne na red u DataGrid-u (studentDataGrid)
        private void studentDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (studentDataGrid.SelectedItem != null)
            {
                STUDENT selectedStudent = (STUDENT)studentDataGrid.SelectedItem;

                // Otvaranje prozora za uređivanje ili brisanje selektovanog studenta
                Izmeni_IzbrisiStudenta izmeni_IzbrisiStudenta = new Izmeni_IzbrisiStudenta(selectedStudent);
                izmeni_IzbrisiStudenta.ShowDialog();
            }
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            PopuniStudente();
        }

        // Ova metoda se poziva prilikom klika na dugme za pretragu
        private void Search(object sender, RoutedEventArgs e)
        {
            string searchText = searchTextBox.Text.Trim();

            // Provera da li je unet tekst za pretragu
            if (!string.IsNullOrEmpty(searchText))
            {
                List<STUDENT> students = student.GetStudents();

                // Filtriranje liste studenata na osnovu broja indeksa
                List<STUDENT> filteredStudents = students.Where(s => s.BrojIndeksa.ToLower().Contains(searchText.ToLower())).ToList();

                // Postavljanje filtrirane liste studenata kao izvor podataka za DataGrid
                studentDataGrid.ItemsSource = filteredStudents;
            }
            else
            {
                // Ako nema unetog teksta, prikaži sve studente
                PopuniStudente();
            }
        }
    }
}
