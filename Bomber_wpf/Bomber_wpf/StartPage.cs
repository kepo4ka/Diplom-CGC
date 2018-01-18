using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_CGC;
using User_class;
using System.IO;
using System.Diagnostics;


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
            Compiler compiler = new Compiler();
            compiler.ComplineAndStart();


            compiler.UserClientStart();


            Form1 realGameForm = new Form1(this);
           

            this.Hide();
            realGameForm.Show();

        }

        private void savedGameButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Serialized file | *.dat";


            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<GameBoard> gbstates = GetGameBoardStatesFromFile(ofd.FileName);
                Form1 saveGameForm = new Form1(this, gbstates);

                this.Hide();
                saveGameForm.Show();
            }
        }


        /// <summary>
        /// Десериализовать сохранённую ранее игру
        /// </summary>
        /// <param name="psource">Путь до файла сохранения</param>
        /// <returns>Список, содержащий состояния игры в каждый момент времени</returns>
        private List<GameBoard> GetGameBoardStatesFromFile(string psource)
        {
            List<GameBoard> gameBoardStates = new List<GameBoard>();
            StreamReader sr = new StreamReader(psource);
            IFormatter formatter = new BinaryFormatter();
            gameBoardStates = (List<GameBoard>)formatter.Deserialize(sr.BaseStream);

            return gameBoardStates;
        }
    }
}

