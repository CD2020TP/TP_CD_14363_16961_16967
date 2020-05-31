using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Linq.Expressions;

namespace Client
{
    public partial class RegisterWindow : Form
    {

        string connectionString;

        public RegisterWindow()
        {
            InitializeComponent();
            connectionString = @"Data Source=.\AccountDB.db;Version=3;";
        }

        private void Register_Click(object sender, EventArgs e)
        {
            //Verify if all fields are filled
            if (STBox.Text == "[Select]" || Course.Text == "" || CourseUnit.Text == "" || Username.Text == "" || Password.Text == "" || ConfirmPass.Text == "")
                MessageBox.Show("Please Submit All Fields Correctly");
            //verify if both passwords match 
            else if (Password.Text != ConfirmPass.Text)
                MessageBox.Show("The Passowrds Do Not Match");
            else
            {

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    
                    //Call to fuction on data base to add INfo about the created account
                    SQLiteCommand cmd = new SQLiteCommand();
                    cmd.CommandText = @"INSERT INTO AccountsT (ST,Course,CourseUnit,Username,Password) VALUES (@STBox,@Course,@CourseUnit,@Username,@Password)";
                    cmd.Connection = conn;
                    cmd.Parameters.Add(new SQLiteParameter("@STBox", STBox.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@Course", Course.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@CourseUnit", CourseUnit.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@Username", Username.Text));
                    cmd.Parameters.Add(new SQLiteParameter("@Password", Password.Text));

                    //Open connection to the data base
                    conn.Open();

                    int i = cmd.ExecuteNonQuery();

                    if (i == 1)
                    {
                        MessageBox.Show("Account Created Successfully");
                    }
                    else
                    {
                        MessageBox.Show("Error Creating Account");
                    }

                    //Functions to clean text boxes
                    Clear();
                }
            }
            





            void Clear()
            {
                STBox.Text = Course.Text = CourseUnit.Text = Username.Text = Password.Text = ConfirmPass.Text = "";
            }

            #region V1Method

            ///////////////////////////////////////////////////////////////////////////////
            ///     Method to create Account using Local DB on SQL Manager Studio       ///
            ///////////////////////////////////////////////////////////////////////////////

            /*
            
            //Link string to connect program to local data base
            string connectionString = @"Data Source=DESKTOP-I0BR7JC;Initial Catalog=Contas CD;Integrated Security=True";

            if (STBox.Text == "[Select]" || Course.Text == "" || CourseUnit.Text == "" || Username.Text == "" || Password.Text == "" || ConfirmPass.Text == "")
                MessageBox.Show("Please Submit All Fields Correctly");
            //verify if the passwords match
            else if (Password.Text != ConfirmPass.Text)
                MessageBox.Show("The Passowrds Do Not Match");
            else
            {
                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    //Open connection to the data base
                    sqlCon.Open();

                    //Call to fuction on data base to add INfo about the created account
                    SqlCommand sqlCmd = new SqlCommand("UserAdd", sqlCon);
                    sqlCmd.CommandType = CommandType.StoredProcedure;

                    //Add fields written on the text boxes after Clicking on the "Register" button
                    sqlCmd.Parameters.AddWithValue("@Profissao", STBox.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@Curso", Course.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@Unidade", CourseUnit.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@Utilizador", Username.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@Passe", Password.Text.Trim());
                    sqlCmd.ExecuteNonQuery();

                    //Success message
                    MessageBox.Show("Conta criada com sucesso");

                    //Functions to clean text boxes
                    Clear();
                }
            }

            */

            #endregion


        }

        private void STBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
