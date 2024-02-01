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
    /// Interaction logic for RasporedNastave.xaml
    /// </summary>
    public partial class RasporedNastave : Window
    {
        // SqlConnection objekat koji se koristi za povezivanje s bazom podataka
        private SqlConnection con;

        // Svojstva za filtriranje
        public string Predmet { get; set; }
        public string Profesor { get; set; }
        public string Dan { get; set; }
        public string Vreme { get; set; }

        public RasporedNastave()
        {
            InitializeComponent();

            // Inicijalizacija SqlConnection objekta koristeći my_DB klasu
            my_DB db = new my_DB();
            con = db.GetConnection;

            // Popunite ComboBox sa departmanima
            FillDepartmaniComboBox();
        }

        private void FillDepartmaniComboBox()
        {
            try
            {
                // Otvaranje veze sa bazom podataka
                con.Open();

                // Kreiranje SQL upita za dohvat departmana
                string query = "SELECT NazivDepartmana FROM departmani";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nazivDepartmana = reader["NazivDepartmana"].ToString();
                            cmbDepartmani.Items.Add(nazivDepartmana);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Došlo je do greške prilikom dohvatanja departmana iz baze podataka: " + ex.Message);
            }
            finally
            {
                // Zatvaranje veze sa bazom podataka
                con.Close();
            }
        }

        private void cmbDepartmani_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Dohvatanje izabranog departmana iz ComboBox-a
            string selectedDepartman = cmbDepartmani.SelectedItem as string;

            // Provera da li je izabrana vrednost
            if (selectedDepartman != null)
            {
                // Očistite cmbProgrami prije nego što dodate nove programe
                cmbProgrami.Items.Clear();

                try
                {
                    // Otvaranje veze sa bazom podataka
                    con.Open();

                    // Kreiranje SQL upita za dohvat studijskih programa za odabrani departman
                    string query = "SELECT NazivStudijskogPrograma FROM studijskiProgrami WHERE DepartmanID = " +
                        "(SELECT DepartmanID FROM departmani WHERE NazivDepartmana = @NazivDepartmana)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Postavljanje parametra u SQL upitu na izabrani departman
                        cmd.Parameters.AddWithValue("@NazivDepartmana", selectedDepartman);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) // Iteriranje kroz rezultate upita
                            {
                                string studijskiProgram = reader["NazivStudijskogPrograma"].ToString(); // Dohvatanje studijskog programa iz rezultata
                                cmbProgrami.Items.Add(studijskiProgram); // Dodavanje studijskog programa u ComboBox cmbProgrami
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Došlo je do greške prilikom dohvatanja studijskih programa iz baze podataka: " + ex.Message);
                }
                finally
                {
                    // Zatvaranje veze sa bazom podataka
                    con.Close();
                }
            }
        }

        private void UpdateDataGrid(string filterValue, string filterType)
        {
            // Provera da li su svi potrebni kriterijumi izabrani
            if (cmbDepartmani.SelectedItem != null && cmbProgrami.SelectedItem != null && filterType != null && filterValue != null)
            {
                try
                {
                    // Otvaranje veze sa bazom podataka
                    con.Open();

                    // Kreiranje SQL upita za filtriranje podataka
                    string query = "SELECT Predmet, Profesor, Dan, Vreme FROM rasporedNastave " +
                                   "WHERE StudijskiProgramID = (SELECT StudijskiProgramID FROM studijskiProgrami " +
                                   "WHERE NazivStudijskogPrograma = @NazivStudijskogPrograma) " +
                                   "AND " + filterType + " = @FilterValue";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Postavljanje parametara u SQL upitu na osnovu izabranih kriterijuma
                        cmd.Parameters.AddWithValue("@NazivStudijskogPrograma", cmbProgrami.SelectedItem as string);
                        cmd.Parameters.AddWithValue("@FilterValue", filterValue);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Kreiranje liste za čuvanje rezultata upita
                            List<RasporedNastave> rasporedNastave = new List<RasporedNastave>();

                            while (reader.Read())
                            {
                                // Kreiranje objekta RasporedNastave sa podacima iz rezultata upita
                                RasporedNastave item = new RasporedNastave
                                {
                                    Predmet = reader["Predmet"].ToString(),
                                    Profesor = reader["Profesor"].ToString(),
                                    Dan = reader["Dan"].ToString(),
                                    Vreme = reader["Vreme"].ToString()
                                };
                                rasporedNastave.Add(item); // Dodavanje objekta u listu rezultata
                            }
                            // Postavljanje izvora podataka za DataGrid na osnovu liste rezultata
                            rasporedDataGrid.ItemsSource = rasporedNastave;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Došlo je do greške prilikom dohvatanja podataka iz baze podataka: " + ex.Message);
                }
                finally
                {
                    // Zatvaranje veze sa bazom podataka
                    con.Close();
                }
            }
        }

        private void UpdateComboBox(string opcija, ComboBox comboBox)
        {
            // Očistite ComboBox prije nego što dodate nove predmete
            comboBox.Items.Clear();

            // Dobijanje vrednosti izabranih departmana i studijskih programa programa iz ComboBox-ova
            string selectedDepartman = cmbDepartmani.SelectedItem as string;
            string selectedProgram = cmbProgrami.SelectedItem as string;

            // Provera da li su departman i studijski program izabrani
            if (selectedDepartman != null && selectedProgram != null)
            {
                try
                {
                    // Otvaranje veze sa bazom podataka
                    con.Open();

                    // Kreiranje SQL upita za dohvat predmeta ili drugih vrednosti za odabrani departman i studijski program
                    string query = $"SELECT DISTINCT {opcija} FROM rasporedNastave " +
                                   "WHERE StudijskiProgramID = (SELECT StudijskiProgramID FROM studijskiProgrami " +
                                   "WHERE NazivStudijskogPrograma = @NazivStudijskogPrograma)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Postavljanje parametara u SQL upitu na osnovu studijskih programa
                        cmd.Parameters.AddWithValue("@NazivStudijskogPrograma", selectedProgram);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Čitanje rezultata upita i dodavanje vrednosti u ComboBox
                            while (reader.Read())
                            {
                                string pretraga = reader[opcija].ToString();
                                comboBox.Items.Add(pretraga);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Došlo je do greške prilikom dohvatanja {opcija} iz baze podataka: " + ex.Message);
                }
                finally
                {
                    // Zatvaranje veze sa bazom podataka
                    con.Close();
                }
            }
        }

        private void SetComboBoxTag(ComboBox comboBox, string filterType)
        {
            comboBox.Tag = filterType; // Ova funkcija postavlja tag na ComboBox-u
        }

        private string GetComboBoxTag(ComboBox comboBox)
        {
            return comboBox.Tag as string; // Ova funkcija vraća tag sa ComboBox-a
        }

        private void ButtonPredmet_Click(object sender, RoutedEventArgs e)
        {
            // Pokreće funkciju za ažuriranje ComboBox-a za filtriranje sa opcijom "Predmet"
            UpdateComboBox("Predmet", cmbPretraga);
            // Postavlja tip izabrane opcije za kasnije korišćenje pri filtriranju
            SetComboBoxTag(cmbPretraga, "Predmet");
        }

        private void ButtonProfesor_Click(object sender, RoutedEventArgs e)
        {
            // Pokreće funkciju za ažuriranje ComboBox-a za filtriranje sa opcijom "Profesor"
            UpdateComboBox("Profesor", cmbPretraga);
            // Postavlja tip izabrane opcije za kasnije korišćenje pri filtriranju
            SetComboBoxTag(cmbPretraga, "Profesor");
        }

        private void ButtonDan_Click(object sender, RoutedEventArgs e)
        {
            // Pokreće funkciju za ažuriranje ComboBox-a za filtriranje sa opcijom "Dan"
            UpdateComboBox("Dan", cmbPretraga);
            // Postavlja tip izabrane opcije za kasnije korišćenje pri filtriranju
            SetComboBoxTag(cmbPretraga, "Dan");
        }

        private void cmbPretraga_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Dobijanje izabrane vrednosti za filtriranje iz ComboBox-a cmbPretraga
            string selectedFilterValue = cmbPretraga.SelectedItem as string; 
            // Pokreće funkciju za ažuriranje DataGrid-a sa filtriranim podacima
            string filterType = GetComboBoxTag(cmbPretraga);
            UpdateDataGrid(selectedFilterValue, filterType);
            rasporedDataGrid.Visibility = Visibility.Visible;
        }

        private void cmbProgrami_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Dobijanje izabranog studijskog programa iz ComboBox-a cmbProgrami
            string selectedProgram = cmbProgrami.SelectedItem as string; // Detektuje promene u izboru studijskog programa u ComboBox-u cmbProgrami
        }

        private void cmbPretraga_KeyUp(object sender, KeyEventArgs e)
        {
            // Ovde ćete omogućiti filtriranje rezultata kako korisnik unosi tekst
            string userInput = cmbPretraga.Text; // Omogućava filtriranje rezultata dok korisnik unosi tekst u ComboBox cmbPretraga
        }
    }
}
