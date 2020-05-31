using Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            //Open Login window
            LoginWindow l1 = new LoginWindow();
            l1.ShowDialog();
            Close();
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            //Open Register window
            RegisterWindow r1 = new RegisterWindow();
            r1.ShowDialog();

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
