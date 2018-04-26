using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_CGC;
using System.IO;

using Newtonsoft.Json;


namespace Bomber_wpf
{
    public partial class StartPage : Form
    {
        public string[] paths;
        public string[] labels;
        int[,] gbpseudo = null;


        public StartPage()
        {
            InitializeComponent();
            paths = new string[4];
            labels = new string[4];
            labels[0] = path1_lab.Text;
            labels[1] = path2_lab.Text;
            labels[2] = path3_lab.Text;
            labels[3] = path4_lab.Text;
            for (int i = 1; i < paths.Length; i++)
            {
                paths[i] = "";
            }


        }


        private void realGameButton_Click(object sender, EventArgs e)
        {
            OpenRealGameForm();
        }



        /// <summary>
        /// Запустить форму симуляции
        /// </summary>
        public void OpenRealGameForm()
        {
            byte havePlayers = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i] != null)
                {
                    havePlayers++;
                }
            }

            //Если хотя бы два игрока (пользователь или бот)
            if (havePlayers > 1)
            {
                Form1 realGameForm = new Form1(this, gbpseudo);
                this.Hide();
                realGameForm.Show();
            }
        }


        private void savedGameButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Serialized file | *.dat; *.json; *.gz";
            ofd.InitialDirectory = Directory.GetCurrentDirectory();


            if (ofd.ShowDialog() == DialogResult.OK)
            {
                List<GameBoard> gbstates;
                try
                {
                    gbstates = GetGameBoardStatesFromFile(ofd.FileName);
                }
                catch (Exception er)
                {
                    MessageBox.Show("Не удалось загрузить Визуализацию: " + er.Message);
                    return;
                }
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
            string[] splitedFile = psource.Split('.');
            string[] splitPath = psource.Split('\\');
            string fileExtension = splitedFile[splitedFile.Length - 1];
            string filePath = splitPath[splitPath.Length - 1];

            List<GameBoard> gameBoardStates = new List<GameBoard>();
            //  MessageBox.Show("psource " + psource);

            switch (fileExtension)
            {
                case "dat":
                    using (StreamReader sr = new StreamReader(psource))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        gameBoardStates = (List<GameBoard>)formatter.Deserialize(sr.BaseStream);
                    }
                    break;
                case "json":
                    using (StreamReader sr = new StreamReader(psource))
                    {
                        string json = sr.ReadToEnd();
                        Helper.LOG("C:\\log.txt", json);
                        gameBoardStates = JsonConvert.DeserializeObject<List<GameBoard>>(json);
                    }
                    break;

                case "gz":
                    string jsonPath = psource.Replace(".gz", ".dat");
                    Helper.Decompress(psource, jsonPath);
                    gameBoardStates = GetGameBoardStatesFromFile(jsonPath);
                    break;
            }

            return gameBoardStates;
        }


        private int[,] GetGameboardFromFile(string psource)
        {
            string[] splitedFile = psource.Split('.');
            string[] splitPath = psource.Split('\\');
            string fileExtension = splitedFile[splitedFile.Length - 1];
            string filePath = splitPath[splitPath.Length - 1];

            int[,] gameboardpseudo = new int[15, 15];
            //  MessageBox.Show("psource " + psource);

            using (StreamReader sr = new StreamReader(psource))
            {
                for (int i = 0; i < gameboardpseudo.GetLength(0); i++)
                {
                    string line = sr.ReadLine();
                    string[] linesplit = line.Split();

                    if (linesplit.Length != gameboardpseudo.GetLength(1))
                    {
                        throw new Exception();
                    }

                    for (int j = 0; j < linesplit.Length; j++)
                    {
                        int t = 0;
                        if (!int.TryParse(linesplit[j], out t))
                        {
                            throw new Exception();
                        }
                        gameboardpseudo[i, j] = t;
                    }
                }
                return gameboardpseudo;
            }          
        }





        /// <summary>
        /// Загрузка файла исходного кода стратегии
        /// </summary>
        /// <param name="i"></param>
        /// <returns>Имя файла стратегии</returns>
        private string AddBotsFiles(int i)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Исходный код C# | *.cs";


            if (ofd.ShowDialog() == DialogResult.OK)
            {
                paths[i] = ofd.FileName;
                return ofd.FileName;
            }
            return "";
        }


        /// <summary>
        /// Заменить тип игрока 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="btn"></param>
        /// <param name="label"></param>
        private void ChangeBotUser(int i, Button btn, Label label, CheckBox check)
        {
            labels[0] = path1_lab.Text;
            labels[1] = path2_lab.Text;
            labels[2] = path3_lab.Text;
            labels[3] = path4_lab.Text;

            if (check.Checked == false && labels[i] == "")
            {
                paths[i] = null;
            }
            else if (check.Checked == false && labels[i] != "")
            {
                paths[i] = labels[i];
            }
            else
            {
                paths[i] = "";
            }

            btn.Enabled = !btn.Enabled;
            label.Enabled = !label.Enabled;
        }


        private void path1_btn_Click(object sender, EventArgs e)
        {
            path1_lab.Text = AddBotsFiles(0);
            ToolTip t = new ToolTip();
            t.SetToolTip(path1_lab, path1_lab.Text);
        }

        private void path2_btn_Click(object sender, EventArgs e)
        {
            path2_lab.Text = AddBotsFiles(1);
            ToolTip t = new ToolTip();
            t.SetToolTip(path2_lab, path2_lab.Text);
        }

        private void path3_btn_Click(object sender, EventArgs e)
        {
            path3_lab.Text = AddBotsFiles(2);
            ToolTip t = new ToolTip();
            t.SetToolTip(path3_lab, path3_lab.Text);
        }

        private void path4_btn_Click(object sender, EventArgs e)
        {
            path4_lab.Text = AddBotsFiles(3);
            ToolTip t = new ToolTip();
            t.SetToolTip(path4_lab, path4_lab.Text);
        }

        private void checkBot1_CheckedChanged(object sender, EventArgs e)
        {
            ChangeBotUser(0, path1_btn, path1_lab, checkBot1);
        }

        private void checkBot2_CheckedChanged(object sender, EventArgs e)
        {
            ChangeBotUser(1, path2_btn, path2_lab, checkBot2);
        }

        private void checkBot3_CheckedChanged(object sender, EventArgs e)
        {
            ChangeBotUser(2, path3_btn, path3_lab, checkBot3);
        }

        private void checkBot4_CheckedChanged(object sender, EventArgs e)
        {
            ChangeBotUser(3, path4_btn, path4_lab, checkBot4);
        }



        private void load_custom_map_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Txt file | *.txt";
            ofd.InitialDirectory = Directory.GetCurrentDirectory();
            int[,] tempgbpseudo;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    tempgbpseudo = GetGameboardFromFile(ofd.FileName);
                }
                catch (Exception er)
                {
                    MessageBox.Show("Ошибка при работе с загружаемой схемой поля: " + er.Message);
                    return;
                }
                gbpseudo = tempgbpseudo;               
            }
        }
    }
}

