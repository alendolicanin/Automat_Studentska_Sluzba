using System;
using System.Collections.Generic;
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
    /// Interaction logic for PolozeniIspiti.xaml
    /// </summary>
    public partial class PolozeniIspiti : Window
    {
        // Kreiranje instance klase my_DB za pristup bazi podataka
        private my_DB db = new my_DB();

        public PolozeniIspiti()
        {
            InitializeComponent();
        }

        public class Ispit
        {
            public string NazivPredmeta { get; set; }
            public string Profesor { get; set; }
            public int Ocena { get; set; }
        }

        // Metoda za pronalaženje studenta po broju indeksa u bazi podataka
        private STUDENT PronadjiStudentaPoBrojuIndeksa(string brojIndeksa)
        {
            try
            {
                // Otvara vezu sa bazom podataka korišćenjem konekcijskog stringa iz objekta db
                using (SqlConnection connection = new SqlConnection(db.GetConnection.ConnectionString))
                {
                    connection.Open();

                    // SQL upit za pronalaženje studenta po broju indeksa
                    string query = "SELECT * FROM student WHERE broj_indeksa = @broj_indeksa";
                    
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@broj_indeksa", brojIndeksa); // Postavlja parametar za broj indeksa
                        
                        using (SqlDataReader reader = cmd.ExecuteReader()) // Izvršava SQL upit i dobija rezultate
                        {
                            if (reader.Read()) // Ako postoji rezultat (student pronađen)
                            {
                                // Kreira novi objekt tipa STUDENT i popunjava ga podacima iz rezultata upita
                                STUDENT student = new STUDENT
                                {
                                    BrojIndeksa = reader["broj_indeksa"].ToString(),
                                    ImePrezime = reader["ime_prezime"].ToString(),
                                };
                                return student;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom pretrage studenta: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null; // Ako student nije pronađen ili se dogodila greška, vraća null
        }

        // Metoda za prikazivanje položenih ispita za određenog studenta iz baze podataka
        private List<Ispit> PronadjiPolozeneIspiteZaStudenta(string brojIndeksa)
        {
            try
            {
                // Establiše vezu sa bazom podataka korišćenjem konekcijskog stringa iz objekta db
                using (SqlConnection connection = new SqlConnection(db.GetConnection.ConnectionString))
                {
                    connection.Open();

                    // SQL upit za dohvat položenih ispita za studenta koji su položeni
                    string ispitQuery = "SELECT NazivPredmeta, Profesor, Ocena FROM prijavaIspita WHERE broj_indeksa = @broj_indeksa AND " +
                        "Prijavljen = 1 AND Polozen = 1";

                    List<Ispit> ispiti = new List<Ispit>(); // Kreira praznu listu ispita

                    using (SqlCommand cmd = new SqlCommand(ispitQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@broj_indeksa", brojIndeksa); // Postavlja parametar za broj indeksa studenta

                        using (SqlDataReader reader = cmd.ExecuteReader()) // Izvršava SQL upit i dobija rezultate
                        {
                            while (reader.Read()) // Iterira kroz rezultate (položeni ispiti)
                            {
                                // Kreira objekt tipa Ispit za svaki položeni ispit i popunjava ga podacima iz rezultata upita
                                Ispit ispit = new Ispit
                                {
                                    NazivPredmeta = reader["NazivPredmeta"].ToString(),
                                    Profesor = reader["Profesor"].ToString(),
                                    Ocena = Convert.ToInt32(reader["Ocena"]),
                                };
                                ispiti.Add(ispit); // Dodaje ispit u listu ispita
                            }
                        }
                    }
                    return ispiti; // Vraća listu položenih ispita za studenta
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom pretrage ispita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new List<Ispit>(); // Ako nema položenih ispita ili se dogodila greška, vraća praznu listu ispita
        }

        // Metoda za pretragu studenta na osnovu unetog broja indeksa i prikaz podataka o studentu
        private void Pronadji_BrojIndeksa(object sender, RoutedEventArgs e)
        {
            string brojIndeksa = txtBrojIndeksa.Text; // Dobavlja unos broja indeksa iz TextBox-a

            if (string.IsNullOrWhiteSpace(brojIndeksa))
            {
                // Ako nije unet broj indeksa, prikaži poruku i prekini izvršavanje
                MessageBox.Show("Molimo unesite broj indeksa studenta za pretragu.", "Pretraga studenta", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Poziva metodu PronadjiStudentaPoBrojuIndeksa da pronađe studenta sa unetim brojem indeksa
            STUDENT pronadjeniStudent = PronadjiStudentaPoBrojuIndeksa(brojIndeksa);

            if (pronadjeniStudent != null)
            {
                // Ako je student pronađen, postavi određene vizualne efekte i prikaži studentove podatke
                txtPodaciStudenta.Background = new SolidColorBrush(Color.FromRgb(42, 92, 153));
                txtPodaciBorder.BorderBrush = new SolidColorBrush(Colors.White);

                // Postavlja tekst TextBox-a sa informacijama o studentu
                txtPodaciStudenta.Text = "Broj indeksa: " + pronadjeniStudent.BrojIndeksa + "\n" +
                    "Ime i prezime: " + pronadjeniStudent.ImePrezime;

                // Poziva metodu PronadjiPolozeneIspiteZaStudenta da prikaže položene ispite studenta u DataGrid kontroli
                List<Ispit> ispiti = PronadjiPolozeneIspiteZaStudenta(pronadjeniStudent.BrojIndeksa);
                rasporedIspitaDataGrid.ItemsSource = ispiti;

                // Postavlja vidljivost DataGrid-a
                rasporedIspitaDataGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Student sa unetim brojem indeksa nije pronađen.", "Nema rezultata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

    }
}
