using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automat_Studentska_Služba
{
    // IDisposable interfejs služi kako bi se resursi pravilno oslobodili kada se više ne koristi
    class my_DB : IDisposable
    {
        // SqlConnection objekat koji se koristi za povezivanje s bazom podataka
        private SqlConnection con = new SqlConnection(@"Data Source=DESKTOP-07RMHPR\SQLEXPRESS;Initial Catalog=Projekat_OOP2_DB;Integrated Security=True;");

        // Svojstvo koje omogućava pristup SqlConnection objektu izvan klase
        public SqlConnection GetConnection
        {
            get
            {
                return con;
            }

        }

        // Metoda za otvaranje veze s bazom podataka
        public void openConnection()
        {
            if(con.State == ConnectionState.Closed)
            {
                con.Open();
            }
        }

        // Metoda za zatvaranje veze s bazom podataka
        public void closeConnection()
        {
            if (con.State == ConnectionState.Open)
            {
                con.Close();
            }
        }

        // Implementacija IDisposable interfejsa za oslobađanje resursa, u ovom slučaju SqlConnection objekat,
        // kada više nije potreban
        public void Dispose()
        {
            con.Dispose();
        }
    }
}
