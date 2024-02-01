using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Interaction logic for DodajStudenta.xaml
    /// </summary>
    public partial class DodajStudenta : Window
    {
        // SqlConnection objekat koji se koristi za povezivanje s bazom podataka
        private SqlConnection con;

        // Kreiranje instance klase my_DB za pristup bazi podataka
        private my_DB db = new my_DB();

        public DodajStudenta()
        {
            InitializeComponent();

            // Inicijalizacija SqlConnection objekta koristeći my_DB klasu
            con = db.GetConnection;

            // Postavi DatePicker na trenutni datum
            txtdatumRodjenja.SelectedDate = DateTime.Now;

            // Popunite ComboBox sa departmanima
            FillDepartmaniComboBox();
        }

        // Metoda za popunjavanje ComboBox-a sa departmanima iz baze podataka
        private void FillDepartmaniComboBox()
        {
            try
            {
                // Otvaranje veze prema bazi podataka
                con.Open();

                // SQL upit za dohvaćanje naziva departmana iz tablice "departmani"
                string query = "SELECT NazivDepartmana FROM departmani";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Iteracija kroz rezultate upita
                        while (reader.Read())
                        {
                            // Dohvati naziv departmana iz rezultata i pretvori ga u string
                            string nazivDepartmana = reader["NazivDepartmana"].ToString();

                            // Dodaj naziv departmana u ComboBox kontrolu
                            cmbDepartmani.Items.Add(nazivDepartmana);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Prikaz poruke o grešci ako se dogodi iznimka pri izvršavanju koda
                MessageBox.Show("Došlo je do greške prilikom dohvatanja departmana iz baze podataka: " + ex.Message);
            }
            finally
            {
                // Zatvaranje veze prema bazi podataka u svakom slučaju (čak i ako se dogodi greška)
                con.Close();
            }
        }

        // Metoda se poziva kad se promijeni odabrani element u ComboBoxu cmbDepartmani
        private void cmbDepartmani_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Dohvaćanje odabranog departmana kao string
            string selectedDepartman = cmbDepartmani.SelectedItem as string;

            if (selectedDepartman != null)
            {
                // Obriši sve postojeće elemente u ComboBoxu cmbProgrami
                cmbProgrami.Items.Clear();

                try
                {
                    // Otvaranje veze prema bazi podataka
                    con.Open();

                    // SQL upit za dohvaćanje naziva studijskih programa povezanih s odabranim departmanom
                    string query = "SELECT NazivStudijskogPrograma FROM studijskiProgrami WHERE DepartmanID = " +
                        "(SELECT DepartmanID FROM departmani WHERE NazivDepartmana = @NazivDepartmana)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Postavljanje parametra za naziv odabranog departmana
                        cmd.Parameters.AddWithValue("@NazivDepartmana", selectedDepartman);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Dohvati naziv studijskog programa iz rezultata i dodaj ga u ComboBox cmbProgrami
                                string studijskiProgram = reader["NazivStudijskogPrograma"].ToString();
                                cmbProgrami.Items.Add(studijskiProgram);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Prikazi poruku o grešci ako dođe do iznimke pri dohvaćanju podataka iz baze
                    MessageBox.Show("Došlo je do greške prilikom dohvatanja studijskih programa iz baze podataka: " + ex.Message);
                }
                finally
                {
                    // Zatvori vezu prema bazi podataka
                    con.Close();
                }
            }
        }

        // Metoda se poziva kada korisnik želi odabrati sliku putem dijaloga za otvaranje datoteka
        private void Unesi_Sliku(object sender, RoutedEventArgs e)
        {
            // Inicijalizacija i konfigurisanje dijaloga za otvaranje fajla
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Slike (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|Svi fajlovi (*.*)|*.*";

            // Prikazivanje dijaloga za odabir slike
            if (openFileDialog.ShowDialog() == true)
            {
                // Dobijanje putanje do odabrane slike
                string imagePath = openFileDialog.FileName;

                // Kreiranje i prikazivanje slike u Image kontrolu
                BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
                Slika.Source = bitmapImage;
            }
        }

        private void ObrisiPolja()
        {
            txtBrojIndeksa.Text = "";
            txtImePrezime.Text = "";
            txtImeRoditelja.Text = "";
            radioButton1.IsChecked = false;
            radioButton2.IsChecked = false;
            txtdatumRodjenja.SelectedDate = DateTime.Now;
            txtMestoRodjenja.Text = "";
            txtJMBG.Text = "";
            txtTelefon.Text = "";
            Slika.Source = null;

            cmbDepartmani.SelectedItem = null;
            cmbProgrami.SelectedItem = null;
            cmbGodinaStudija.SelectedItem = null;
        }

        // Metoda se poziva kada korisnik želi dodati novog studenta
        private void Dugme_Dodaj(object sender, RoutedEventArgs e)
        {
            // Stvaranje instance klase STUDENT koja predstavlja model studenta.
            STUDENT student = new STUDENT();

            // Dohvaćanje podataka o studentu iz odgovarajućih grafičkih elemenata
            string brojIndeksa = txtBrojIndeksa.Text;
            string imePrezime = txtImePrezime.Text;
            string imeRoditelja = txtImeRoditelja.Text;
            string pol = radioButton1.IsChecked == true ? "Muški" : "Ženski";
            DateTime datumRodjenja = txtdatumRodjenja.SelectedDate.Value;
            string mestoRodjenja = txtMestoRodjenja.Text;
            string jmbg = txtJMBG.Text;
            string telefon = txtTelefon.Text;
            string program = cmbProgrami.SelectedItem as string;
            string godinaStudija = (cmbGodinaStudija.SelectedItem as ComboBoxItem)?.Content.ToString();

            byte[] imageBytes = null;
            if (Slika.Source != null && Slika.Source is BitmapImage bitmapImage)
            {
                // Pretvaranje slike u bajtove radi spremanja u bazu podataka
                using (MemoryStream stream = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder(); // Enkoder koji će pretvoriti sliku u bajtove
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage)); // Nakon dodavanja slike u enkoder, stvara se okvir slike
                    encoder.Save(stream);
                    imageBytes = stream.ToArray(); // Spremamo u varijabli imageBytes
                }
            }

            int born_year = datumRodjenja.Year;
            int this_year = DateTime.Now.Year;

            // Provjera dobi studenta (mora biti stariji od 18 godina)
            if ((this_year - born_year) < 18)
            {
                MessageBox.Show("Student mora da ima više od 18 godina.", "Nevalidan unos datuma.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (verifikacija())
            {
                // Provjera jedinstvenosti broja indeksa u bazi podataka
                string queryCheckIndex = "SELECT COUNT(*) FROM student WHERE broj_indeksa = @broj_indeksa";

                using (SqlConnection con = new SqlConnection(db.GetConnection.ConnectionString))
                {
                    con.Open();

                    using (SqlCommand cmdCheckIndex = new SqlCommand(queryCheckIndex, con))
                    {
                        cmdCheckIndex.Parameters.AddWithValue("@broj_indeksa", brojIndeksa);
                        int studentCount = (int)cmdCheckIndex.ExecuteScalar();

                        if (studentCount > 0)
                        {
                            MessageBox.Show("Student sa istim brojem indeksa već postoji.", "Greška pri unosu.", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            // Dohvaćanje ID-a studijskog programa na temelju odabira iz ComboBoxa
                            string program_id = "SELECT StudijskiProgramID FROM studijskiProgrami WHERE NazivStudijskogPrograma = @NazivStudijskogPrograma";

                            using (SqlCommand cmdProgramId = new SqlCommand(program_id, con))
                            {
                                cmdProgramId.Parameters.AddWithValue("@NazivStudijskogPrograma", program);
                                int programId = (int)cmdProgramId.ExecuteScalar();

                                // Unos novog studenta u bazu podataka pomoću metode insertStudent
                                if (student.insertStudent(brojIndeksa, imePrezime, imeRoditelja, pol, datumRodjenja, mestoRodjenja, jmbg, telefon, imageBytes, godinaStudija, programId))
                                {
                                    MessageBox.Show("Novi student je dodat.", "Dodaj studenta.", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ObrisiPolja();
                                }
                                else
                                {
                                    MessageBox.Show("Greška prilikom dodavanja studenta.", "Dodaj studenta.", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Molimo popunite sva obavezna polja.", "Dodaj studenta.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private bool verifikacija()
        {
            if (string.IsNullOrWhiteSpace(txtBrojIndeksa.Text) || string.IsNullOrWhiteSpace(txtImePrezime.Text) ||
                string.IsNullOrWhiteSpace(txtImeRoditelja.Text) || string.IsNullOrWhiteSpace(txtMestoRodjenja.Text) ||
                string.IsNullOrWhiteSpace(txtJMBG.Text) || string.IsNullOrWhiteSpace(txtTelefon.Text) ||
                (radioButton1.IsChecked != true && radioButton2.IsChecked != true) ||
                Slika.Source == null ||
                cmbDepartmani.SelectedItem == null ||
                cmbProgrami.SelectedItem == null ||
                cmbGodinaStudija.SelectedItem == null)
            {
                return false;
            }

            if (txtJMBG.Text.Length != 13 || !txtJMBG.Text.All(char.IsDigit))
            {
                MessageBox.Show("JMBG mora imati najmanje 13 karaktera.", "Ne validan unos JMBG.", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void Dugme_Otkazi(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

