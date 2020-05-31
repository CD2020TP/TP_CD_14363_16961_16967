using Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class LoginWindow : Form
    {
        string connectionString;
        public LoginWindow()
        {
            InitializeComponent();

            connectionString = @"Data Source=.\AccountDB.db;Version=3;";
        }

        private void Login_Click(object sender, EventArgs e)
        {
            // verify if the fields are empty
            if (LoginUT.Text == "" && LoginPass.Text == "")
            {
                MessageBox.Show("Empty fields");
            }
            else
            {
                //Query created to access the required data
                string query = "SELECT * FROM AccountsT where Username = @LoginUT AND Password =@LoginPass";
                //Connection link to data base
                SQLiteConnection conn = new SQLiteConnection(connectionString);
                //open connection
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(query, conn);
                //Read Values to compare to DB
                cmd.Parameters.AddWithValue("@LoginUt", LoginUT.Text);
                cmd.Parameters.AddWithValue("@LoginPass", LoginPass.Text);
                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    //if the parameters are correct opens the chat window
                    ClientChat c1 = new ClientChat();
                    c1.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Username or Password are Incorrect");
                }

                //Function to clean text boxes
                Clear();
            }

            void Clear()
            {
                LoginUT.Text = LoginPass.Text = "";
            }

            #region V1method
            ///////////////////////////////////////////////////////////////////////////
            ///     Method used to Login using SQL Local DB on Managment Studio     ///
            ///////////////////////////////////////////////////////////////////////////

            /*
            //Link to connect program to local data base
            string conString = @"Data Source=DESKTOP-I0BR7JC;Initial Catalog=Contas CD;Integrated Security=True";
            
            //Create connexion
            SqlConnection con = new SqlConnection(conString);
            //abrir base de dados
            con.Open();
            //command to compare thge writen data to the username and passwords collums on data base
            SqlCommand cmd = new SqlCommand("select * from tblContas where Utilizador = '" + LoginUT.Text + "' and Passe = '" + LoginPass.Text + "'", con);
            SqlDataReader dr;
            dr = cmd.ExecuteReader();
            int count = 0;
            while (dr.Read())
            {
                count += 1;
            }
            if (count == 1)
            {
                //if the parameters are correct opens the chat window
                ClientChat c1 = new ClientChat();
                c1.ShowDialog();
            }
            else
            {
                // error message in case of wrng pass or username
                MessageBox.Show("Username or Passowrd are Incorrect ");
            }

            //Function to clean text boxes
            Clear();

             */
            #endregion

        }
    }
}