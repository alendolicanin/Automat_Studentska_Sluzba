using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing.Printing;
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
    /// Interaction logic for PotvrdaOUpisu.xaml
    /// </summary>
    public partial class PotvrdaOUpisu : Window
    {
        private my_DB db = new my_DB();

        public PotvrdaOUpisu()
        {
            InitializeComponent();
        }

        private void btnGenerisiPotvrdu_Click(object sender, RoutedEventArgs e)
        {
            string brojIndeksa = txtBrojIndeksa.Text;

            if (string.IsNullOrWhiteSpace(brojIndeksa))
            {
                MessageBox.Show("Molimo unesite broj indeksa studenta za generisanje potvrde.", "Generisanje potvrde", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Pronalaženje studenta na osnovu unetog broja indeksa
            STUDENT pronadjeniStudent = PronadjiStudentaPoBrojuIndeksa(brojIndeksa);

            if (pronadjeniStudent != null)
            {
                // Generišimo PDF potvrdu
                try
                {
                    // Generiranje imena datoteke za potvrdu o upisu
                    string filePath = $"PotvrdaOUpisu_{pronadjeniStudent.BrojIndeksa}.pdf";

                    GeneratePdfPotvrda(pronadjeniStudent.ImePrezime, pronadjeniStudent.ImeRoditelja, pronadjeniStudent.JMBG, 
                        pronadjeniStudent.DatumRodjenja, pronadjeniStudent.MestoRodjenja, pronadjeniStudent.Departman,
                        pronadjeniStudent.StudijskiProgram, pronadjeniStudent.GodinaStudija, filePath);

                    // Prikazivanje poruke sa informacijama o generisanoj potvrdi
                    string absolutePath = System.IO.Path.Combine(Environment.CurrentDirectory, filePath);
                    MessageBox.Show($"Potvrda o upisu je uspešno generisana i smeštena na sledećoj putanji: {absolutePath}", "Generisano", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Došlo je do greške prilikom generisanja potvrde: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Student sa unetim brojem indeksa nije pronađen.", "Nema rezultata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void GeneratePdfPotvrda(string imePrezime, string imeRoditelja, string jmbg, DateTime datumRodjenja, 
            string mestoRodjenja, string departman, string program, string godinaStudija, string filePath)
        {
            // Kreirajte novi PDF dokument
            iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 50, 50, 25, 25);
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, new FileStream(filePath, FileMode.Create));

            pdfDoc.Open();

            // Postavite fontove
            iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont("Calibri", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 16);
            iTextSharp.text.Font contentFont = iTextSharp.text.FontFactory.GetFont("Calibri", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 14, iTextSharp.text.Font.ITALIC | iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font textFont = iTextSharp.text.FontFactory.GetFont("Calibri", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 12);

            // Naslov
            iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("Republika Srbija\n Državni univerzitet u Novom Pazaru\n", titleFont);
            title.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            pdfDoc.Add(title);

            // Dodajte sliku
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Slike/dunp_logo1.png"));
            image.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
            image.ScaleAbsolute(120f, 100f);
            pdfDoc.Add(image);

            // Naslov - Uverenje o statusu studenta
            iTextSharp.text.Paragraph statusParagraph = new iTextSharp.text.Paragraph("Uverenje o statusu studenta\n\n", contentFont);
            statusParagraph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            pdfDoc.Add(statusParagraph);

            // Tekst sa podacima iz baze
            iTextSharp.text.Paragraph dataParagraph = new iTextSharp.text.Paragraph();
            dataParagraph.Font = textFont;

            iTextSharp.text.Font boldFont = new iTextSharp.text.Font(textFont.BaseFont, textFont.Size, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Chunk boldChunkImePrezime = new iTextSharp.text.Chunk(imePrezime, boldFont);
            iTextSharp.text.Chunk boldChunkImeRoditelja = new iTextSharp.text.Chunk(imeRoditelja, boldFont);

            DateTime now = DateTime.Now;
            int year_time = now.Year;
            int lastYear_time = year_time - 1;

            // Dodajte podebljane delove teksta u Paragraph
            dataParagraph.Add(boldChunkImePrezime);
            dataParagraph.Add(new iTextSharp.text.Chunk($" (", textFont)); // Ovde dodajte običan tekst
            dataParagraph.Add(boldChunkImeRoditelja);
            dataParagraph.Add(new iTextSharp.text.Chunk($"), JMBG {jmbg}, rodjen/a {datumRodjenja} godine u {mestoRodjenja}, student je " +
                $"Državnog univerziteta u Novom Pazaru na departmanu {departman}, studijski program {program}, godina i tip studija " +
                $"{godinaStudija}, školske {lastYear_time.ToString()}/{year_time.ToString()}. godine. Prema Statutu Univerziteta " +
                $"OAS studije traju cetiri godine (osam semestara), a MAS studije traju jednu godinu (dva semestra).\n\n", textFont));
            pdfDoc.Add(dataParagraph);

            // Footer - Studentska služba
            string formattedDate = now.ToString("dd.MM.yyyy");
            string footerText = "Studentska služba Državnog univerziteta u Novom Pazaru\nDatum: " + formattedDate + ". godine";
            iTextSharp.text.Paragraph footer = new iTextSharp.text.Paragraph(footerText, textFont);
            footer.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
            pdfDoc.Add(footer);

            pdfDoc.Close();
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
                txtPodaciStudenta.Background = new SolidColorBrush(Color.FromRgb(36, 46, 56));
                txtPodaciStudenta.Text = "Broj indeksa: " + pronadjeniStudent.BrojIndeksa + "\n" + 
                    "Ime i prezime: " + pronadjeniStudent.ImePrezime + "\n" + "Departman: " + pronadjeniStudent.Departman +
                    "\n" + "Studijski program: " + pronadjeniStudent.StudijskiProgram + "\n" + "Godina studija: " + 
                    pronadjeniStudent.GodinaStudija;

                btnGenerisiPotvrdu.Visibility = Visibility.Visible;
            }
            else
            {
                MessageBox.Show("Student sa unetim brojem indeksa nije pronađen.", "Nema rezultata", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Ova metoda se koristi za pretragu baze podataka kako bi se pronašao student na osnovu broja indeksa.
        private STUDENT PronadjiStudentaPoBrojuIndeksa(string brojIndeksa)
        {
            try
            {
                db.openConnection();

                SqlCommand cmd = new SqlCommand("SELECT s.*, sp.NazivStudijskogPrograma, d.NazivDepartmana FROM student s " +
                                "INNER JOIN studijskiProgrami sp ON s.StudijskiProgramID = sp.StudijskiProgramID " +
                                "INNER JOIN departmani d ON sp.DepartmanID = d.DepartmanID WHERE s.broj_indeksa = @broj_indeksa", db.GetConnection);
                cmd.Parameters.AddWithValue("@broj_indeksa", brojIndeksa);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    STUDENT student = new STUDENT
                    {
                        BrojIndeksa = reader["broj_indeksa"].ToString(),
                        ImePrezime = reader["ime_prezime"].ToString(),
                        ImeRoditelja = reader["ime_roditelja"].ToString(),
                        DatumRodjenja = Convert.ToDateTime(reader["datum_rodjenja"]),
                        MestoRodjenja = reader["mesto_rodjenja"].ToString(),
                        JMBG = reader["jmbg"].ToString(),
                        Departman = reader["NazivDepartmana"].ToString(),
                        StudijskiProgram = reader["NazivStudijskogPrograma"].ToString(),
                        GodinaStudija = reader["godina_studija"].ToString(),
                    };

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

    }
}
