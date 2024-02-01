using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
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
    /// Interaction logic for PotvrdaOIspitima.xaml
    /// </summary>
    public partial class PotvrdaOIspitima : Window
    {
        private my_DB db = new my_DB();

        public PotvrdaOIspitima()
        {
            InitializeComponent();
        }

        public class PolozenIspit
        {
            public string NazivPredmeta { get; set; }
            public string Profesor { get; set; }
            public int Ocena { get; set; }
        }

        private void btnGenerisiPotvrdu_Click(object sender, RoutedEventArgs e)
        {
            string brojIndeksa = txtBrojIndeksa.Text;

            if (string.IsNullOrWhiteSpace(brojIndeksa))
            {
                MessageBox.Show("Molimo unesite broj indeksa studenta za generisanje potvrde.", "Generisanje potvrde", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            // Pronađi studenta na osnovu unetog broja indeksa
            STUDENT pronadjeniStudent = PronadjiStudentaPoBrojuIndeksa(brojIndeksa);

            if (pronadjeniStudent != null)
            {
                double ProsecnaOcena = IzracunajProsecnuOcenu(pronadjeniStudent.BrojIndeksa);
                List<PolozenIspit> polozeniIspiti = DohvatiPolozeneIspite(pronadjeniStudent.BrojIndeksa);

                // Generišite PDF potvrdu
                try
                {
                    // Generiranje imena datoteke za potvrdu o položenim ispitima
                    string filePath = $"PotvrdaOPoloženimIspitima_{pronadjeniStudent.BrojIndeksa}.pdf";

                    GeneratePdfPotvrda(pronadjeniStudent.BrojIndeksa, pronadjeniStudent.ImePrezime, pronadjeniStudent.ImeRoditelja,
                        pronadjeniStudent.DatumRodjenja, pronadjeniStudent.MestoRodjenja, pronadjeniStudent.Departman,
                        pronadjeniStudent.StudijskiProgram, pronadjeniStudent.GodinaStudija, ProsecnaOcena, polozeniIspiti, filePath);

                    // Prikazivanje poruke sa informacijama o generisanoj potvrdi
                    string absolutePath = System.IO.Path.Combine(Environment.CurrentDirectory, filePath);
                    MessageBox.Show($"Potvrda o položenim ispitima je uspešno generisana i smeštena na sledećoj putanji: {absolutePath}", "Generisano", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void GeneratePdfPotvrda(string brojIndeksa, string imePrezime, string imeRoditelja, DateTime datumRodjenja,
            string mestoRodjenja, string departman, string program, string godinaStudija, double prosecnaOcena, 
            List<PolozenIspit> polozeniIspiti, string filePath)
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

            // Naslov - Uverenje o položenim ispitima
            iTextSharp.text.Paragraph statusParagraph = new iTextSharp.text.Paragraph("Uverenje o položenim ispitima\n\n", contentFont);
            statusParagraph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            pdfDoc.Add(statusParagraph);

            // Ime i prezime studenta, ime roditelja
            iTextSharp.text.Font imePrezimeFont = iTextSharp.text.FontFactory.GetFont("Calibri", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 15, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Paragraph imePrezimeParagraph = new iTextSharp.text.Paragraph($"{imePrezime} ({imeRoditelja})\n\n", imePrezimeFont);
            imePrezimeParagraph.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
            pdfDoc.Add(imePrezimeParagraph);

            // Tekst sa podacima iz baze
            iTextSharp.text.Paragraph dataParagraph = new iTextSharp.text.Paragraph();
            dataParagraph.Font = textFont;

            DateTime now = DateTime.Now;
            int year_time = now.Year;
            int nextYear_time = year_time + 1;

            iTextSharp.text.Chunk boldChunkProsecnaOcena = new iTextSharp.text.Chunk(prosecnaOcena.ToString(), new iTextSharp.text.Font(textFont.BaseFont, textFont.Size, iTextSharp.text.Font.BOLD));

            // Dodajte informacije o prosečnoj oceni
            dataParagraph.Add(new iTextSharp.text.Chunk($"Rodjen/a {datumRodjenja} godine u {mestoRodjenja}.\nBroj indeksa: " +
                            $"{brojIndeksa}\n\nStudijski program: {program}\nDepartman: {departman}\nGodina i tip studija: " +
                            $"{godinaStudija}\nŠkolska {year_time.ToString()}/{nextYear_time.ToString()}. godina\n\nOpšti uspeh: ", textFont));
            dataParagraph.Add(boldChunkProsecnaOcena); // Dodajte podebljanu prosečnu ocenu
            dataParagraph.Add(new iTextSharp.text.Chunk("\n\nPoloženi ispiti:\n\n", textFont));
            pdfDoc.Add(dataParagraph);

            // Dodajte uslov za ispis položenih ispita ako postoje
            if (polozeniIspiti.Count > 0)
            {
                // Kreirajte tabelu sa 3 kolone
                PdfPTable table = new PdfPTable(3);
                table.DefaultCell.Padding = 5;
                table.WidthPercentage = 100;
                table.HorizontalAlignment = Element.ALIGN_LEFT;

                // Postavite zaglavlje tabele
                PdfPCell cell = new PdfPCell(new Phrase("Predmet", textFont));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Profesor", textFont));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Ocena", textFont));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                // Dodajte redove sa informacijama o položenim ispitima
                foreach (var ispit in polozeniIspiti)
                {
                    table.AddCell(new Phrase(ispit.NazivPredmeta, textFont));
                    table.AddCell(new Phrase(ispit.Profesor, textFont));
                    table.AddCell(new Phrase(ispit.Ocena.ToString(), textFont));
                }

                // Dodajte tabelu u PDF dokument
                pdfDoc.Add(table);
            }

            // Footer - Studentska služba
            string formattedDate = now.ToString("dd.MM.yyyy");
            string footerText = "\n\nStudentska služba Državnog univerziteta u Novom Pazaru\nDatum: " + formattedDate + ". godine";
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

        private double IzracunajProsecnuOcenu(string brojIndeksa)
        {
            double prosecnaOcena = 0;
            int brojOcena = 0;

            try
            {
                db.openConnection();

                // SQL upit za dohvatanje ocena za određenog studenta
                SqlCommand cmd = new SqlCommand("SELECT Ocena FROM prijavaIspita WHERE broj_indeksa = @broj_indeksa", db.GetConnection);
                cmd.Parameters.AddWithValue("@broj_indeksa", brojIndeksa);

                SqlDataReader reader = cmd.ExecuteReader(); // Izvrši upit
                while (reader.Read())
                {
                    int ocena;
                    if (int.TryParse(reader["Ocena"].ToString(), out ocena))
                    {
                        prosecnaOcena += ocena;
                        brojOcena++;
                    }
                }
            }
            catch (Exception ex)
            {
                // Obrada greške
                MessageBox.Show("Greška prilikom izračunavanja prosečne ocene: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                db.closeConnection();
            }

            if (brojOcena > 0)
            {
                prosecnaOcena /= brojOcena; // Izračunaj prosečnu ocenu ako postoje ocene
            }
            return prosecnaOcena; // Vrati prosečnu ocenu
        }

        private List<PolozenIspit> DohvatiPolozeneIspite(string brojIndeksa)
        {
            List<PolozenIspit> polozeniIspiti = new List<PolozenIspit>(); // Lista za čuvanje informacija o položenim ispitima

            try
            {
                db.openConnection();

                // SQL upit za dohvatanje informacija o položenim ispitima za određenog studenta
                SqlCommand cmd = new SqlCommand("SELECT NazivPredmeta, Profesor, Ocena FROM prijavaIspita WHERE broj_indeksa = @broj_indeksa AND " +
                    "Polozen = 1", db.GetConnection);
                cmd.Parameters.AddWithValue("@broj_indeksa", brojIndeksa);

                SqlDataReader reader = cmd.ExecuteReader(); // Izvrši upit
                while (reader.Read())
                {
                    PolozenIspit polozenIspit = new PolozenIspit
                    {
                        NazivPredmeta = reader["NazivPredmeta"].ToString(),
                        Profesor = reader["Profesor"].ToString(),
                        Ocena = Convert.ToInt32(reader["Ocena"])
                    };
                    polozeniIspiti.Add(polozenIspit); // Dodaj informacije o položenom ispitu u listu
                }
            }
            catch (Exception ex)
            {
                // Obrada greške
                MessageBox.Show("Greška prilikom dohvatanja položenih ispita: " + ex.Message, "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                db.closeConnection();
            }
            return polozeniIspiti;
        }

    }
}
