using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bomber_wpf
{
    public partial class StartPage : Form
    {
        public StartPage()
        {
            InitializeComponent();
        }

        private void realGameButton_Click(object sender, EventArgs e)
        {
            Form1 realGameForm = new Form1(this);
            this.Hide();
            realGameForm.Show();
        }
    }
}
