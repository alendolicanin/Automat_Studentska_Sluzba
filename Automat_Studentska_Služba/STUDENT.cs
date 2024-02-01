using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace Automat_Studentska_Služba
{
    public class STUDENT
    {
        private my_DB db = new my_DB();

        public int StudentID { get; set; }
        public string BrojIndeksa { get; set; }
        public string ImePrezime { get; set; }
        public string ImeRoditelja { get; set; }
        public string Pol { get; set; }
        public DateTime DatumRodjenja { get; set; }
        public string MestoRodjenja{ get; set; }
        public string JMBG { get; set; }
        public string Telefon { get; set; }
        public BitmapImage Slika { get; set; }
        public string GodinaStudija { get; set; }
        public string Departman { get; set; }
        public string StudijskiProgram { get; set; }
        public int StudijskiProgramID { get; set; }

        public bool insertStudent(string brojIndeksa, string imePrezime, string imeRoditelja, string pol, DateTime datumRodjenja, 
            string mestoRodjenja, string jmbg, string telefon, byte[] slika, string godinaStudija, int programID)
        {
            try
            {
                using (SqlConnection con = db.GetConnection)
                {
                    con.Open();

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand("INSERT INTO student (broj_indeksa, ime_prezime, ime_roditelja, pol," +
                                " datum_rodjenja, mesto_rodjenja, jmbg, telefon, slika, godina_studija, StudijskiProgramID) " +
                                "VALUES (@broj_indeksa, @ime_prezime, @ime_roditelja, @pol, @datum_rodjenja, @mesto_rodjenja, " +
                                "@jmbg, @telefon, @slika, @godina_studija, @StudijskiProgramID)", con, transaction))
                            {
                                cmd.Parameters.Add("@broj_indeksa", SqlDbType.VarChar).Value = brojIndeksa;
                                cmd.Parameters.Add("@ime_prezime", SqlDbType.VarChar).Value = imePrezime;
                                cmd.Parameters.Add("@ime_roditelja", SqlDbType.VarChar).Value = imeRoditelja;
                                cmd.Parameters.Add("@pol", SqlDbType.VarChar).Value = pol;
                                cmd.Parameters.Add("@datum_rodjenja", SqlDbType.DateTime).Value = datumRodjenja;
                                cmd.Parameters.Add("@mesto_rodjenja", SqlDbType.VarChar).Value = mestoRodjenja;
                                cmd.Parameters.Add("@jmbg", SqlDbType.VarChar).Value = jmbg;
                                cmd.Parameters.Add("@telefon", SqlDbType.VarChar).Value = telefon;
                                cmd.Parameters.Add("@slika", SqlDbType.VarBinary).Value = slika;
                                cmd.Parameters.Add("@godina_studija", SqlDbType.VarChar).Value = godinaStudija;
                                cmd.Parameters.Add("@StudijskiProgramID", SqlDbType.Int).Value = programID;

                                if (cmd.ExecuteNonQuery() == 1)
                                {
                                    transaction.Commit();
                                    return true;
                                }
                                else
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Greška prilikom izvršavanja upita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom povezivanja sa bazom podataka: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public List<STUDENT> GetStudents()
        {
            my_DB db = new my_DB();

            List<STUDENT> students = new List<STUDENT>();
            using (SqlConnection con = db.GetConnection)
            {
                try
                {
                    con.Open();
                    string query = "SELECT s.*, sp.NazivStudijskogPrograma, d.NazivDepartmana FROM student s " +
                                   "INNER JOIN studijskiProgrami sp ON s.StudijskiProgramID = sp.StudijskiProgramID " +
                                   "INNER JOIN departmani d ON sp.DepartmanID = d.DepartmanID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            STUDENT student = new STUDENT
                            {
                                BrojIndeksa = reader.GetString(reader.GetOrdinal("broj_indeksa")),
                                ImePrezime = reader.GetString(reader.GetOrdinal("ime_prezime")),
                                ImeRoditelja = reader.GetString(reader.GetOrdinal("ime_roditelja")),
                                Pol = reader.GetString(reader.GetOrdinal("pol")),
                                DatumRodjenja = reader.GetDateTime(reader.GetOrdinal("datum_rodjenja")),
                                MestoRodjenja = reader.GetString(reader.GetOrdinal("mesto_rodjenja")),
                                JMBG = reader.GetString(reader.GetOrdinal("jmbg")),
                                Telefon = reader.GetString(reader.GetOrdinal("telefon")),
                                Departman = reader.GetString(reader.GetOrdinal("NazivDepartmana")),
                                StudijskiProgram = reader.GetString(reader.GetOrdinal("NazivStudijskogPrograma")),
                                GodinaStudija = reader.GetString(reader.GetOrdinal("godina_studija")),
                            };

                            byte[] imageBytes = (byte[])reader["slika"];
                            student.Slika = LoadImage(imageBytes);

                            students.Add(student);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Obrada grešaka
                    MessageBox.Show("Greška prilikom dohvata studenata: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return students;
        }

        // Metoda za učitavanje slike iz bajtova
        private BitmapImage LoadImage(byte[] imageData)
        {
            // Proveri da li su podaci o slici dostupni
            if (imageData == null || imageData.Length == 0)
                return null; // Ako nema podataka, vrati null (nema slike)

            BitmapImage image = new BitmapImage();
            using (MemoryStream stream = new MemoryStream(imageData)) // Kreiraj memorisani tok za podatke o slici
            {
                image.BeginInit();  // Počni inicijalizaciju slike
                image.CacheOption = BitmapCacheOption.OnLoad;  // Postavi opciju keširanja na "OnLoad"
                image.StreamSource = stream;  // Postavi izvor slike na memorisani tok
                image.EndInit();  // Završi inicijalizaciju slike
            }

            // Vrati učitanu sliku
            return image;
        }

        public bool updateStudent(string brojIndeksa, string imePrezime, string imeRoditelja, string pol,
            DateTime datumRodjenja, string mestoRodjenja, string jmbg, string telefon, byte[] slika)
        {
            try
            {
                using (SqlConnection con = db.GetConnection)
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("UPDATE student SET ime_prezime = @ime_prezime, ime_roditelja = @ime_roditelja, " +
                        "pol = @pol, datum_rodjenja = @datum_rodjenja, mesto_rodjenja = @mesto_rodjenja, jmbg = @jmbg, " +
                        "telefon = @telefon, slika = @slika WHERE broj_indeksa = @broj_indeksa", con))
                    {
                        cmd.Parameters.Add("@broj_indeksa", SqlDbType.VarChar).Value = brojIndeksa;
                        cmd.Parameters.Add("@ime_prezime", SqlDbType.VarChar).Value = imePrezime;
                        cmd.Parameters.Add("@ime_roditelja", SqlDbType.VarChar).Value = imeRoditelja;
                        cmd.Parameters.Add("@pol", SqlDbType.VarChar).Value = pol;
                        cmd.Parameters.Add("@datum_rodjenja", SqlDbType.DateTime).Value = datumRodjenja;
                        cmd.Parameters.Add("@mesto_rodjenja", SqlDbType.VarChar).Value = mestoRodjenja;
                        cmd.Parameters.Add("@jmbg", SqlDbType.VarChar).Value = jmbg;
                        cmd.Parameters.Add("@telefon", SqlDbType.VarChar).Value = telefon;
                        cmd.Parameters.Add("@slika", SqlDbType.VarBinary).Value = slika;

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 1)
                        {
                            // Ažuriranje uspešno, zatvori vezu i vrati "true"
                            return true;
                        }
                        else
                        {
                            // Ažuriranje nije uspelo, zatvori vezu i vrati "false"
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Uhvaćen je izuzetak, obradite ga ili zabeležite
                MessageBox.Show("Greška prilikom ažuriranja u bazi podataka: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool deleteStudent(string brojIndeksa)
        {
            try
            {
                using (SqlConnection con = db.GetConnection)
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("DELETE FROM student WHERE broj_indeksa = @broj_indeksa", con))
                    {
                        cmd.Parameters.Add("@broj_indeksa", SqlDbType.VarChar).Value = brojIndeksa;

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 1)
                        {
                            // Brisanje uspešno, zatvori vezu i vrati "true"
                            return true;
                        }
                        else
                        {
                            // Brisanje nije uspelo, zatvori vezu i vrati "false"
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Uhvaćen je izuzetak, obradite ga ili zabeležite
                MessageBox.Show("Greška prilikom brisanja iz baze podataka: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

    }
}
