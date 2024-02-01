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
    /// Interaction logic for PrijavljeniIspiti.xaml
    /// </summary>
    public partial class PrijavljeniIspiti : Window
    {
        // Kreiranje instance klase my_DB za pristup bazi podataka
        private my_DB db = new my_DB();

        public PrijavljeniIspiti()
        {
            InitializeComponent();
        }

        public class Ispit
        {
            public string NazivPredmeta { get; set; }
            public string Profesor { get; set; }
            public int BrojPrijava { get; set; }
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
                                    StudijskiProgramID = Convert.ToInt32(reader["StudijskiProgramID"]),
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

        // Metoda za prikazivanje prijavljenih ispita za određenog studenta iz baze podataka
        private List<Ispit> PronadjiPrijavljeneIspiteZaStudenta(string brojIndeksa, int studijskiProgramID)
        {
            try
            {
                // Establiše vezu sa bazom podataka korišćenjem konekcijskog stringa iz objekta db
                using (SqlConnection connection = new SqlConnection(db.GetConnection.ConnectionString))
                {
                    connection.Open();

                    string ispitQuery = "SELECT rn.Predmet, rn.Profesor, ISNULL(pi.BrojPrijava, 0) AS BrojPrijava FROM rasporedNastave rn " +
                    "LEFT JOIN prijavaIspita pi ON rn.Predmet = pi.NazivPredmeta AND pi.broj_indeksa = @broj_indeksa " +
                    "WHERE pi.Prijavljen = 1 AND (pi.Polozen IS NULL OR pi.Polozen = 0) AND rn.StudijskiProgramID = @StudijskiProgramID";

                    List<Ispit> ispiti = new List<Ispit>(); // Kreira praznu listu ispita

                    using (SqlCommand cmd = new SqlCommand(ispitQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@broj_indeksa", brojIndeksa); // Postavlja parametar za broj indeksa studenta
                        cmd.Parameters.AddWithValue("@StudijskiProgramID", studijskiProgramID); // Postavlja parametar za ID studijskog programa

                        using (SqlDataReader reader = cmd.ExecuteReader()) // Izvršava SQL upit i dobija rezultate
                        {
                            while (reader.Read()) // Iterira kroz rezultate (prijavljeni, ali nepoloženi ispiti)
                            {
                                // Kreira objekt tipa Ispit za svaki prijavljeni, ali nepoloženi ispit i popunjava ga podacima iz rezultata upita
                                Ispit ispit = new Ispit
                                {
                                    NazivPredmeta = reader["Predmet"].ToString(),
                                    Profesor = reader["Profesor"].ToString(),
                                    BrojPrijava = (int)reader["BrojPrijava"]
                                };
                                ispiti.Add(ispit); // Dodaje ispit u listu ispita
                            }
                        }
                    }
                    return ispiti; // Vraća listu prijavljenih, ali nepoloženih ispita za studenta
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom pretrage ispita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new List<Ispit>(); // Ako nema prijavljenih ispita ili se dogodila greška, vraća praznu listu ispita
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

                // Poziva metodu PronadjiPrijavljeneIspiteZaStudenta da prikaže prijavljene ispite studenta u DataGrid kontroli
                List<Ispit> ispiti = PronadjiPrijavljeneIspiteZaStudenta(pronadjeniStudent.BrojIndeksa, pronadjeniStudent.StudijskiProgramID);
                rasporedIspitaDataGrid.ItemsSource = ispiti;

                // Postavlja vidljivost DataGrid-a
                rasporedIspitaDataGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Student sa unetim brojem indeksa nije pronađen.", "Nema rezultata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Metoda za ocenu ispita za određenog studenta
        private void Oceni_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; // Prikuplja referencu na dugme koje je pokrenulo događaj
            var ispit = (Ispit)button.Tag; // Dohvata objekat ispita koji je povezan s tim dugmetom

            UnosOceneWindow unosOceneWindow = new UnosOceneWindow(); // Kreira prozor za unos ocene

            if (unosOceneWindow.ShowDialog() == true) // Ako korisnik unese ocenu i potvrdi unos
            {
                int ocena = unosOceneWindow.UnetaOcena; // Dohvata unetu ocenu iz prozora za unos

                string brojIndeksa = txtBrojIndeksa.Text;
                STUDENT student = PronadjiStudentaPoBrojuIndeksa(brojIndeksa);

                if (student != null)
                {
                    using (SqlConnection connection = new SqlConnection(db.GetConnection.ConnectionString))
                    {
                        connection.Open();

                        if (ocena != 5)
                        {
                            // Ako je ocena od 6 do 10, označi ispit kao položen i postavi ocenu
                            string updateQuery = "UPDATE prijavaIspita SET Polozen = @Polozen, Ocena = @Ocena " +
                                                 "WHERE broj_indeksa = @broj_indeksa AND NazivPredmeta = @NazivPredmeta";
                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@Polozen", true);
                                updateCmd.Parameters.AddWithValue("@Ocena", ocena);
                                updateCmd.Parameters.AddWithValue("@broj_indeksa", student.BrojIndeksa);
                                updateCmd.Parameters.AddWithValue("@NazivPredmeta", ispit.NazivPredmeta);

                                try
                                {
                                    updateCmd.ExecuteNonQuery(); // Izvršava SQL upit za ažuriranje ocene u bazi podataka
                                    MessageBox.Show($"Uspešno ste ocenili ispit sa ocenom {ocena}.", "Uspešna ocena", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Greška prilikom ažuriranja ocene: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        else
                        {
                            // Ako je odabrana ocena 5, označi ispit kao nepoložen i postavi ocenu
                            string updateQuery = "UPDATE prijavaIspita SET Prijavljen = @Prijavljen " +
                                                 "WHERE broj_indeksa = @broj_indeksa AND NazivPredmeta = @NazivPredmeta";
                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@Prijavljen", false);
                                updateCmd.Parameters.AddWithValue("@broj_indeksa", student.BrojIndeksa);
                                updateCmd.Parameters.AddWithValue("@NazivPredmeta", ispit.NazivPredmeta);

                                try
                                {
                                    updateCmd.ExecuteNonQuery(); // Izvršava SQL upit za ažuriranje ocene u bazi podataka
                                    MessageBox.Show("Student nije položio ispit.", "Neuspešna ocena", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Greška prilikom ažuriranja ocene: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }

                        // Osvežite izvor podataka za DataGrid
                        string broj_Indeksa = student.BrojIndeksa;
                        int studijski_ProgramID = student.StudijskiProgramID;
                        List<Ispit> ispiti = PronadjiPrijavljeneIspiteZaStudenta(broj_Indeksa, studijski_ProgramID);
                        rasporedIspitaDataGrid.ItemsSource = ispiti;
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
