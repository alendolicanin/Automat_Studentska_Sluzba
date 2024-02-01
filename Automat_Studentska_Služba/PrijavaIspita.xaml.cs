using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class PrijavaIspita : Window
    {
        // Kreiranje instance klase my_DB za pristup bazi podataka
        private my_DB db = new my_DB();

        public PrijavaIspita()
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

        // Metoda za prikazivanje neprijavljenih ispita za određenog studenta iz baze podataka
        private List<Ispit> PronadjiNeprijavljeneIspiteZaStudenta(string brojIndeksa, int studijskiProgramID)
        {
            try
            {
                // Establiše vezu sa bazom podataka korišćenjem konekcijskog stringa iz objekta db
                using (SqlConnection connection = new SqlConnection(db.GetConnection.ConnectionString))
                {
                    connection.Open();

                    string ispitQuery = "SELECT rn.Predmet, rn.Profesor, ISNULL(pi.BrojPrijava, 0) AS BrojPrijava FROM rasporedNastave rn " +
                    "LEFT JOIN prijavaIspita pi ON rn.Predmet = pi.NazivPredmeta AND pi.broj_indeksa = @broj_indeksa " +
                    "WHERE (pi.Prijavljen IS NULL OR pi.Prijavljen = 0) AND rn.StudijskiProgramID = @StudijskiProgramID";

                    List<Ispit> ispiti = new List<Ispit>(); // Kreira praznu listu ispita

                    using (SqlCommand cmd = new SqlCommand(ispitQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@broj_indeksa", brojIndeksa); // Postavlja parametar za broj indeksa studenta
                        cmd.Parameters.AddWithValue("@StudijskiProgramID", studijskiProgramID); // Postavlja parametar za ID studijskog programa

                        using (SqlDataReader reader = cmd.ExecuteReader()) // Izvršava SQL upit i dobija rezultate
                        {
                            while (reader.Read()) // Iterira kroz rezultate (neprijavljeni ispiti)
                            {
                                // Kreira objekt tipa Ispit za svaki neprijavljeni ispit i popunjava ga podacima iz rezultata upita
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
                    return ispiti; // Vraća listu neprijavljenih ispita za studenta
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom pretrage ispita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new List<Ispit>(); // Ako nema neprijavljenih ispita ili se dogodila greška, vraća praznu listu ispita
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

                // Poziva metodu PronadjiNeprijavljeneIspiteZaStudenta da prikaže neprijavljene ispite studenta u DataGrid kontroli
                List<Ispit> ispiti = PronadjiNeprijavljeneIspiteZaStudenta(pronadjeniStudent.BrojIndeksa, pronadjeniStudent.StudijskiProgramID);
                rasporedIspitaDataGrid.ItemsSource = ispiti;

                // Postavlja vidljivost DataGrid-a
                rasporedIspitaDataGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Student sa unetim brojem indeksa nije pronađen.", "Nema rezultata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Prijavi_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; // Pretvaranje sendera u tip Button i dodjeljivanje rezultata varijabli button
            var ispit = (Ispit)button.Tag; // Dohvaćanje podataka iz Tag svojstva button i pretvaranje u tip Ispit

            // Prikazuje dijalog provere pre prijave ispita
            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da želite da prijavite ispit?", "Prijavi ispit", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                string brojIndeksa = txtBrojIndeksa.Text;
                STUDENT student = PronadjiStudentaPoBrojuIndeksa(brojIndeksa);

                if (student != null)
                {
                    PrijaviIspit(ispit, student);
                }
                else
                {
                    MessageBox.Show("Student sa unetim brojem indeksa nije pronađen.", "Nema rezultata", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void PrijaviIspit(Ispit ispit, STUDENT student)
        {
            try
            {
                // Uspostavljanje veze s bazom podataka
                using (SqlConnection connection = new SqlConnection(db.GetConnection.ConnectionString))
                {
                    connection.Open();

                    // Provera postojanja prijave ispita za studenta
                    if (ProveriPostojecuPrijavu(connection, ispit, student, out int brojPrijava, out int prijavaIspitaId))
                    {
                        // Ako student već ima prijavljene ispita
                        if (brojPrijava >= 2)
                        {
                            int stanjeNaRacunu = ProveriStanjeNaRacunu(connection, student);

                            // Ako student ima dovoljno sredstava na računu
                            if (stanjeNaRacunu >= 300)
                            {
                                IzvrsiPrijavuIspita(connection, student, ispit, brojPrijava, prijavaIspitaId, stanjeNaRacunu);
                            }
                            else
                            {
                                // Obavijest o nedovoljnim sredstvima na računu
                                MessageBox.Show("Nemate dovoljno sredstava na računu za prijavu ispita.", "Nedovoljno sredstava", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            IzvrsiPrijavuIspita(connection, student, ispit, brojPrijava, prijavaIspitaId, 0);
                        }
                    }
                    else
                    {
                        // Napravi novu prijavu ispita
                        NapraviNovuPrijavu(connection, student, ispit);
                    }
                }
            }
            catch (Exception ex)
            {
                // Prikaz greške ako dođe do izuzetka
                MessageBox.Show("Greška prilikom prijave ispita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ProveriPostojecuPrijavu(SqlConnection connection, Ispit ispit, STUDENT student, out int brojPrijava, out int prijavaIspitaId)
        {
            try
            {
                // SQL upit za proveru postojanja prijave za ispit
                string selectQuery = "SELECT PrijavaIspitaID, BrojPrijava FROM prijavaIspita WHERE NazivPredmeta = @NazivPredmeta AND broj_indeksa = @broj_indeksa";
                using (SqlCommand selectCmd = new SqlCommand(selectQuery, connection))
                {
                    // Postavljanje parametara upita
                    selectCmd.Parameters.AddWithValue("@NazivPredmeta", ispit.NazivPredmeta);
                    selectCmd.Parameters.AddWithValue("@broj_indeksa", student.BrojIndeksa);
                    using (SqlDataReader reader = selectCmd.ExecuteReader())
                    {
                        // Ako postoji prijava za ispit
                        if (reader.Read())
                        {
                            // Čitanje broja prijava i ID-a prijave iz rezultata upita
                            brojPrijava = reader.GetInt32(reader.GetOrdinal("BrojPrijava"));
                            prijavaIspitaId = reader.GetInt32(reader.GetOrdinal("PrijavaIspitaID"));
                            reader.Close();
                            return true;
                        }
                    }
                    // Ako nema prijava za ispit, postavljamo vrijednosti na 0 i vraćamo false
                    brojPrijava = 0;
                    prijavaIspitaId = 0;
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom provere postojeće prijave: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                // Postavljanje vrijednosti na 0 i vraćanje false u slučaju greške
                brojPrijava = 0;
                prijavaIspitaId = 0;
                return false;
            }
        }

        private int ProveriStanjeNaRacunu(SqlConnection connection, STUDENT student)
        {
            try
            {
                // SQL upit za proveru stanja na računu studenta
                string vrstaUplate = "Prijava ispita";
                string stanjeNaRacunuQuery = "SELECT TOP 1 StanjeNaRacunu FROM uplata WHERE broj_indeksa = @broj_indeksa AND VrstaUplate = @VrstaUplate ORDER BY Datum DESC";

                // Stvaranje i izvršavanje SQL upita
                using (SqlCommand stanjeNaRacunuCmd = new SqlCommand(stanjeNaRacunuQuery, connection))
                {
                    // Postavljanje parametara upita
                    stanjeNaRacunuCmd.Parameters.AddWithValue("@broj_indeksa", student.BrojIndeksa);
                    stanjeNaRacunuCmd.Parameters.AddWithValue("@VrstaUplate", vrstaUplate);

                    // Izvršavanje upita i čitanje rezultata
                    object stanjeResult = stanjeNaRacunuCmd.ExecuteScalar();

                    // Ako postoji rezultat, vraćamo stanje na računu, inače vraćamo 0
                    if (stanjeResult != null && stanjeResult != DBNull.Value)
                    {
                        return (int)stanjeResult;
                    }
                    return 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom provere stanja na računu: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }

        private void IzvrsiPrijavuIspita(SqlConnection connection, STUDENT student, Ispit ispit, int brojPrijava, int prijavaIspitaId, int stanjeNaRacunu)
        {
            try
            {
                // Ako student već ima dve ili više prijava, smanji stanje na računu za cenu prijave ispita
                if (brojPrijava >= 2)
                {
                    stanjeNaRacunu -= 300;
                    string vrstaUplate = "Prijava ispita";

                    // SQL upit za dodavanje uplate za prijavu ispita
                    string insertPaymentQuery = "INSERT INTO uplata (VrstaUplate, broj_indeksa, Datum, Isplata, StanjeNaRacunu) " +
                        "VALUES (@VrstaUplate, @broj_indeksa, @Datum, @Isplata, @StanjeNaRacunu)";
                    using (SqlCommand insertPaymentCmd = new SqlCommand(insertPaymentQuery, connection))
                    {
                        insertPaymentCmd.Parameters.AddWithValue("@VrstaUplate", vrstaUplate);
                        insertPaymentCmd.Parameters.AddWithValue("@broj_indeksa", student.BrojIndeksa);
                        insertPaymentCmd.Parameters.AddWithValue("@Datum", DateTime.Now);
                        insertPaymentCmd.Parameters.AddWithValue("@Isplata", 300);
                        insertPaymentCmd.Parameters.AddWithValue("@StanjeNaRacunu", stanjeNaRacunu);
                        insertPaymentCmd.ExecuteNonQuery();
                    }
                }

                // SQL upit za ažuriranje prijave ispita
                string updatePrijavaQuery = "UPDATE prijavaIspita SET BrojPrijava = @BrojPrijava, Prijavljen = @Prijavljen WHERE PrijavaIspitaID = @PrijavaIspitaID";
                using (SqlCommand updatePrijavaCmd = new SqlCommand(updatePrijavaQuery, connection))
                {
                    updatePrijavaCmd.Parameters.AddWithValue("@Prijavljen", true);
                    updatePrijavaCmd.Parameters.AddWithValue("@BrojPrijava", brojPrijava + 1);
                    updatePrijavaCmd.Parameters.AddWithValue("@PrijavaIspitaID", prijavaIspitaId);
                    updatePrijavaCmd.ExecuteNonQuery();
                }

                // Obaveštenje o uspešnoj prijavi ispita
                MessageBox.Show("Uspešno ste prijavili ispit: " + ispit.NazivPredmeta, "Uspešna prijava", MessageBoxButton.OK, MessageBoxImage.Information);

                // Pronalazi neprijavljene ispite za studenta i ažurira prikaz rasporeda ispita
                string broj_Indeksa = student.BrojIndeksa;
                int studijski_ProgramID = student.StudijskiProgramID;
                List<Ispit> ispiti = PronadjiNeprijavljeneIspiteZaStudenta(broj_Indeksa, studijski_ProgramID);
                rasporedIspitaDataGrid.ItemsSource = ispiti;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom izvršavanja prijave ispita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NapraviNovuPrijavu(SqlConnection connection, STUDENT student, Ispit ispit)
        {
            try
            {
                // SQL upit za kreiranje nove prijave ispita
                string insertQuery = "INSERT INTO prijavaIspita (NazivPredmeta, Profesor, broj_indeksa, Prijavljen, Polozen, BrojPrijava) " +
                    "VALUES (@NazivPredmeta, @Profesor, @broj_indeksa, @Prijavljen, @Polozen, @BrojPrijava)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                {
                    // Postavljanje parametara upita
                    cmd.Parameters.AddWithValue("@NazivPredmeta", ispit.NazivPredmeta);
                    cmd.Parameters.AddWithValue("@Profesor", ispit.Profesor);
                    cmd.Parameters.AddWithValue("@broj_indeksa", student.BrojIndeksa);
                    cmd.Parameters.AddWithValue("@Prijavljen", true);
                    cmd.Parameters.AddWithValue("@Polozen", false);
                    cmd.Parameters.AddWithValue("@BrojPrijava", 1);

                    // Izvršavanje upita za dodavanje nove prijave ispita
                    cmd.ExecuteNonQuery();

                    // Obaveštenje o uspešnoj prijavi ispita
                    MessageBox.Show("Uspešno ste prijavili ispit: " + ispit.NazivPredmeta, "Uspešna prijava", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Ažuriranje prikaza rasporeda ispita
                    string broj_Indeksa = student.BrojIndeksa;
                    int studijski_ProgramID = student.StudijskiProgramID;
                    List<Ispit> ispiti = PronadjiNeprijavljeneIspiteZaStudenta(broj_Indeksa, studijski_ProgramID);
                    rasporedIspitaDataGrid.ItemsSource = ispiti;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom kreiranja nove prijave ispita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
