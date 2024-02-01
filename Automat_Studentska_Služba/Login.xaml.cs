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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;

namespace Automat_Studentska_Služba
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // try i catch blokovi - služe da prilikom izvršavanja koda ako se pojavi greška hvata se catch
            try
            {
                // 'using' blok služi kako bi se oslobodili resursi nakon završetka rada s bazom podataka
                using (my_DB db = new my_DB())
                {
                    string username = txtUsername.Text;
                    string password = txtPassword.Password;

                    // Validacija korisničkog unosa
                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        MessageBox.Show("Unesite korisničko ime i lozinku.");
                        return;
                    }

                    // Kreiranje SqlDataAdapter i DataTable za prikupljanje rezultata upita
                    // SqlDataAdapter za izvršavanje SQL upita, a DataTable za skladištenje rezultata
                    SqlDataAdapter sda = new SqlDataAdapter();
                    DataTable dt = new DataTable();

                    // Kreiranje SqlCommand za izvršenje upita za prijavu korisnika
                    SqlCommand cmd = new SqlCommand("SELECT * FROM login WHERE username=@username AND password=@password", db.GetConnection);

                    // Postavljanje parametara upita na vrednosti iz korisničkog unosa
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    // Povezivanje SqlCommand sa SqlDataAdapter-om
                    sda.SelectCommand = cmd;
                    // Ispunjavanje DataTable sa rezultatima upita
                    sda.Fill(dt);

                    //string cmbItemValue = comboBox.Text;
                    if (dt.Rows.Count > 0)
                    {
                        // Ako postoji barem jedan red u rezultatu upita (ispravno korisničko ime i lozinka)
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            Admin admin = new Admin();
                            admin.Show();
                            this.Close();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Pogrešno korisničko ime ili lozinka.");
                    }
                }
            }
            // U slučaju bilo kakve greške pri pristupu bazi podataka, prikazuje se
            // poruka o grešci sa detaljima o izuzetku
            catch (Exception ex)
            {
                MessageBox.Show("Došlo je do greške prilikom pristupa bazi podataka: " + ex.Message);
            }
        }
    }
}
