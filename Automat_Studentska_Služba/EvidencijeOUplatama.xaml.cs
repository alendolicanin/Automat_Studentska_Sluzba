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
    /// Interaction logic for EvidencijeOUplatama.xaml
    /// </summary>
    public partial class EvidencijeOUplatama : Window
    {
        // Kreiranje instance klase my_DB za pristup bazi podataka
        private my_DB db = new my_DB();

        public EvidencijeOUplatama()
        {
            InitializeComponent();
        }

        public class Uplate
        {
            public string VrstaUplate { get; set; }
            public int Uplata { get; set; }
            public int Isplata { get; set; }
            public int StanjeNaRacunu { get; set; }
            public DateTime TrenutakPromene { get; set; }
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

        // Metoda za prikazivanje uplata za određenog studenta iz baze podataka
        private List<Uplate> PrikaziUplate(string brojIndeksa)
        {
            try
            {
                // Establiše vezu sa bazom podataka korišćenjem konekcijskog stringa iz objekta db
                using (SqlConnection connection = new SqlConnection(db.GetConnection.ConnectionString))
                {
                    connection.Open();

                    // SQL upit za dohvat svih uplata za određenog studenta, sortiranih po opadajućem datumu
                    string ispitQuery = "SELECT * FROM uplata WHERE broj_indeksa = @broj_indeksa ORDER BY Datum DESC";

                    List<Uplate> uplate = new List<Uplate>(); // Kreira praznu listu uplata

                    using (SqlCommand cmd = new SqlCommand(ispitQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@broj_indeksa", brojIndeksa); // Postavlja parametar za broj indeksa studenta
                        
                        using (SqlDataReader reader = cmd.ExecuteReader()) // Izvršava SQL upit i dobija rezultate
                        {
                            while (reader.Read()) // Iterira kroz rezultate (uplate)
                            {
                                // Proverava da li je vrednost NULL pre nego što je konvertuje
                                int uplata = reader["Uplata"] != DBNull.Value ? Convert.ToInt32(reader["Uplata"]) : 0;
                                int isplata = reader["Isplata"] != DBNull.Value ? Convert.ToInt32(reader["Isplata"]) : 0;
                                int stanjeNaRacunu = reader["StanjeNaRacunu"] != DBNull.Value ? Convert.ToInt32(reader["StanjeNaRacunu"]) : 0;

                                // Kreira objekt tipa Uplate za svaku uplatu i popunjava ga podacima iz rezultata upita
                                Uplate Uplata = new Uplate
                                {
                                    VrstaUplate = reader["VrstaUplate"].ToString(),
                                    Uplata = uplata,
                                    Isplata = isplata,
                                    StanjeNaRacunu = stanjeNaRacunu,
                                    TrenutakPromene = Convert.ToDateTime(reader["Datum"]),
                                };
                                uplate.Add(Uplata); // Dodaje uplatu u listu uplata
                            }
                        }
                    }

                    return uplate; // Vraća listu uplata za studenta
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom pretrage ispita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new List<Uplate>(); // Ako nema uplata ili se dogodila greška, vraća praznu listu uplata
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

                // Poziva metodu PrikaziUplate da prikaže uplate studenta u DataGrid kontroli
                List<Uplate> uplate = PrikaziUplate(pronadjeniStudent.BrojIndeksa);
                uplateDataGrid.ItemsSource = uplate;

                // Postavlja vidljivost DataGrid-a i dugmeta za uplatu
                uplateDataGrid.Visibility = Visibility.Visible;
                btnUplata.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Student sa unetim brojem indeksa nije pronađen.", "Nema rezultata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Metoda za dodavanje nove uplate za određenog studenta
        private void DodajUplatu(object sender, RoutedEventArgs e)
        {
            // Kreira instancu UnosUplateWindow - prozor za unos uplate
            UnosUplateWindow unosUplateWindow = new UnosUplateWindow();

            if (unosUplateWindow.ShowDialog() == true) // Ako korisnik uspešno unese podatke za uplatu u prozoru za unos uplate
            {
                int uplata = unosUplateWindow.UnetaUplata; // Dohvata iznos uplate iz prozora za unos uplate
                string vrstaUplate = unosUplateWindow.VrstaUplate; // Dohvata vrstu uplate iz prozora za unos uplate

                string brojIndeksa = txtBrojIndeksa.Text; // Dohvata broj indeksa iz TextBox-a
                STUDENT student = PronadjiStudentaPoBrojuIndeksa(brojIndeksa); // Pronalazi studenta sa unetim brojem indeksa u bazi podataka

                if (student != null)
                {
                    using (SqlConnection connection = new SqlConnection(db.GetConnection.ConnectionString))
                    {
                        connection.Open();

                        if (uplata >= 300 && !string.IsNullOrEmpty(vrstaUplate))
                        {
                            // Pronađi sumu uplata za određenu vrstu uplate
                            string selectQuery = "SELECT TOP 1 StanjeNaRacunu FROM uplata WHERE broj_indeksa = @broj_indeksa AND " +
                                "VrstaUplate = @VrstaUplate ORDER BY Datum DESC";

                            int sumaUplata = 0; // Postavite početnu vrednost na 0
                            // Ova vrednost predstavlja trenutno stanje na računu studenta za određenu vrstu uplate

                            using (SqlCommand selectCmd = new SqlCommand(selectQuery, connection))
                            {
                                selectCmd.Parameters.Add("@broj_indeksa", SqlDbType.Int).Value = student.BrojIndeksa;
                                selectCmd.Parameters.Add("@VrstaUplate", SqlDbType.VarChar).Value = vrstaUplate;

                                object result = selectCmd.ExecuteScalar();

                                if (result != DBNull.Value && result != null)
                                {
                                    sumaUplata = (int)result; // Ako postoji vrednost, dodelite je promenljivoj sumaUplata
                                }

                                // Dodaj iznos uplate na sumu uplata za određenu vrstu uplate
                                sumaUplata += uplata;

                                // Ažuriraj kolonu "StanjeNaRacunu" sa novom sumom uplata za određenu vrstu uplate
                                string updateQuery = "INSERT INTO uplata (VrstaUplate, broj_indeksa, Datum, Uplata, StanjeNaRacunu) " +
                                    "VALUES (@VrstaUplate, @broj_indeksa, GETDATE(), @Uplata, @StanjeNaRacunu)";

                                using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                                {
                                    cmd.Parameters.Add("@VrstaUplate", SqlDbType.VarChar).Value = vrstaUplate;
                                    cmd.Parameters.Add("@Uplata", SqlDbType.Int).Value = uplata;
                                    cmd.Parameters.Add("@broj_indeksa", SqlDbType.Int).Value = student.BrojIndeksa;
                                    cmd.Parameters.Add("@StanjeNaRacunu", SqlDbType.Int).Value = sumaUplata;

                                    int rowsAffected = cmd.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        MessageBox.Show("Uspešno ste uneli uplatu.", "Uspešna uplata", MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Nije uspelo unošenje uplate.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            }
                            // Osvežava izvor podataka za DataGrid
                            string broj_Indeksa = student.BrojIndeksa;
                            List<Uplate> uplate = PrikaziUplate(broj_Indeksa);
                            uplateDataGrid.ItemsSource = uplate;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Student sa unetim brojem indeksa nije pronađen.", "Nema rezultata", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }
}
