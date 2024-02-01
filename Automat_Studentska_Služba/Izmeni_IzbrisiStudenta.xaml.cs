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
    /// Interaction logic for Izmeni_IzbrisiStudenta.xaml
    /// </summary>
    public partial class Izmeni_IzbrisiStudenta : Window
    {
        // Ovde ćemo sačuvati selektovanog studenta
        private STUDENT selectedStudent;
        private my_DB db = new my_DB();

        public Izmeni_IzbrisiStudenta()
        {
            InitializeComponent();

            // Postavi DatePicker na trenutni datum
            txtdatumRodjenja.SelectedDate = DateTime.Now;
        }

        public Izmeni_IzbrisiStudenta(STUDENT student)
        {
            InitializeComponent();

            // Postavljanje selektovanog studenta iz prenetog argumenta
            selectedStudent = student;

            // Postavljanje vrednosti TextBox-ova i RadioButton-ova na osnovu selektovanog studenta
            txtBrojIndeksa.Text = selectedStudent.BrojIndeksa;
            txtImePrezime.Text = selectedStudent.ImePrezime;
            txtImeRoditelja.Text = selectedStudent.ImeRoditelja;
            radioButton1.IsChecked = (selectedStudent.Pol == "Muški");
            radioButton2.IsChecked = (selectedStudent.Pol == "Ženski");
            txtdatumRodjenja.SelectedDate = selectedStudent.DatumRodjenja;
            txtMestoRodjenja.Text = selectedStudent.MestoRodjenja;
            txtJMBG.Text = selectedStudent.JMBG;
            txtTelefon.Text = selectedStudent.Telefon;

            // Postavljanje slike studenta
            Slika.Source = selectedStudent.Slika;
        }

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
        }

        // Ova metoda služi da izmeni podatke o studentu iz baze podataka 
        private void Izmeni_Studenta(object sender, RoutedEventArgs e)
        {
            STUDENT student = new STUDENT();
            string brojIndeksa = txtBrojIndeksa.Text;
            string imePrezime = txtImePrezime.Text;
            string imeRoditelja = txtImeRoditelja.Text;
            string pol = radioButton1.IsChecked == true ? "Muški" : "Ženski";
            DateTime datumRodjenja = txtdatumRodjenja.SelectedDate.Value;
            string mestoRodjenja = txtMestoRodjenja.Text;
            string jmbg = txtJMBG.Text;
            string telefon = txtTelefon.Text;

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

            if ((this_year - born_year) < 18)
            {
                MessageBox.Show("Student mora da ima više od 18 godina.", "Ne validan unos datuma.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (verifikacija())
            {
                // Ažuriraj podatke o studentu pozivajući updateStudent metodu iz instance student
                if (student.updateStudent(brojIndeksa, imePrezime, imeRoditelja, pol, datumRodjenja, mestoRodjenja, jmbg, telefon, imageBytes))
                {
                    MessageBox.Show("Podaci o studentu su uspešno ažurirani.", "Ažuriranje studenta je uspelo.", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Došlo je do greške prilikom ažuriranja podataka o studentu.", "Ažuriranje nije uspelo. Molimo pokušajte ponovo.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Molimo popunite sva obavezna polja.", "Ažuriraj studenta.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private bool verifikacija()
        {
            if (string.IsNullOrWhiteSpace(txtBrojIndeksa.Text) || string.IsNullOrWhiteSpace(txtImePrezime.Text) ||
                string.IsNullOrWhiteSpace(txtImeRoditelja.Text) || string.IsNullOrWhiteSpace(txtMestoRodjenja.Text) ||
                string.IsNullOrWhiteSpace(txtJMBG.Text) || string.IsNullOrWhiteSpace(txtTelefon.Text) ||
                (radioButton1.IsChecked != true && radioButton2.IsChecked != true) ||
                Slika.Source == null)
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

        // Ova metoda služi da obriše studenta iz baze podataka 
        private void Izbrisi_Studenta(object sender, RoutedEventArgs e)
        {
            STUDENT student = new STUDENT();
            string brojIndeksa = txtBrojIndeksa.Text;

            if (string.IsNullOrWhiteSpace(brojIndeksa))
            {
                MessageBox.Show("Molimo unesite broj indeksa studenta.", "Brisanje studenta", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Da li ste sigurni da želite da obrišete ovog studenta?", "Potvrdi brisanje", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {

                if (student.deleteStudent(brojIndeksa))
                {
                    MessageBox.Show("Student je uspešno obrisan.", "Brisanje studenta", MessageBoxButton.OK, MessageBoxImage.Information);
                    ObrisiPolja();

                }
                else
                {
                    MessageBox.Show("Došlo je do greške prilikom brisanja studenta.", "Brisanje studenta", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Ova metoda se poziva kada korisnik pritisne dugme za pretragu na osnovu broja indeksa.
        private void Pronadji_BrojIndeksa(object sender, RoutedEventArgs e)
        {
            string brojIndeksa = txtBrojIndeksa.Text;

            if (string.IsNullOrWhiteSpace(brojIndeksa))
            {
                MessageBox.Show("Molimo unesite broj indeksa studenta za pretragu.", "Pretraga studenta", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Pronađi studenta na osnovu unetog broja indeksa
            STUDENT pronadjeniStudent = PronadjiStudentaPoBrojuIndeksa(brojIndeksa);

            if (pronadjeniStudent != null)
            {
                txtImePrezime.Text = pronadjeniStudent.ImePrezime;
                txtImeRoditelja.Text = pronadjeniStudent.ImeRoditelja;
                radioButton1.IsChecked = (pronadjeniStudent.Pol == "Muški");
                radioButton2.IsChecked = (pronadjeniStudent.Pol == "Ženski");
                txtdatumRodjenja.SelectedDate = pronadjeniStudent.DatumRodjenja;
                txtMestoRodjenja.Text = pronadjeniStudent.MestoRodjenja;
                txtJMBG.Text = pronadjeniStudent.JMBG;
                txtTelefon.Text = pronadjeniStudent.Telefon;
                Slika.Source = (pronadjeniStudent.Slika != null) ? pronadjeniStudent.Slika : null;
            }
            else
            {
                MessageBox.Show("Student sa unetim brojem indeksa nije pronađen.", "Nema rezultata", MessageBoxButton.OK, MessageBoxImage.Information);
                ObrisiPolja();
            }
        }

        // Ova metoda se koristi za pretragu baze podataka kako bi se pronašao student na osnovu broja indeksa
        private STUDENT PronadjiStudentaPoBrojuIndeksa(string brojIndeksa)
        {
            try
            {
                db.openConnection();

                SqlCommand cmd = new SqlCommand("SELECT * FROM student WHERE broj_indeksa = @broj_indeksa", db.GetConnection);
                cmd.Parameters.AddWithValue("@broj_indeksa", brojIndeksa);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    STUDENT student = new STUDENT
                    {
                        ImePrezime = reader["ime_prezime"].ToString(),
                        ImeRoditelja = reader["ime_roditelja"].ToString(),
                        Pol = reader["pol"].ToString(),
                        DatumRodjenja = Convert.ToDateTime(reader["datum_rodjenja"]),
                        MestoRodjenja = reader["mesto_rodjenja"].ToString(),
                        JMBG = reader["jmbg"].ToString(),
                        Telefon = reader["telefon"].ToString(),
                    };

                    // Učitajte sliku iz baze podataka i postavite je na svojstvo Slika
                    byte[] imageBytes = (byte[])reader["slika"];
                    student.Slika = LoadImage(imageBytes);

                    return student;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom pretrage studenta: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                db.closeConnection();
            }

            return null; // Student nije pronađen
        }

        // Metoda za učitavanje slike iz bajtova
        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            BitmapImage image = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(imageData))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            return image;
        }
    }
}
