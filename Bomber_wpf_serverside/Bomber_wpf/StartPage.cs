﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ClassLibrary_CGC;
using System.IO;
using System.Net;
using System.Diagnostics;

using Newtonsoft.Json;


namespace Bomber_wpf
{
    public partial class StartPage : Form
    {
        public string[] paths;
        public string[] labels;
        int[,] gbpseudo = null;
        string UserMapPath = "";
        int playersOnBoardCount = 0;
        int playersAddedCount = 0;
        string githubLink = "";


        public StartPage()
        {
            InitializeComponent();
            paths = new string[4];
            labels = new string[4];
            labels[0] = null;
            labels[1] = null;
            labels[2] = null;
            labels[3] = null;
           
            

            for (int i = 1; i < paths.Length; i++)
            {
                paths[i] = "";
            }

            comboBox1.SelectedIndex = 3;
            comboBox2.SelectedIndex = 3;
            comboBox3.SelectedIndex = 3;
            comboBox4.SelectedIndex = 3;

            SetVersions();
        }


        public void SetVersions()
        {
            try
            {
                string[] githubInfo = Helper.GetLastVersion();

                version_label.Text += "Последняя версия: " + githubInfo[0] + " ";
                githubLink = githubInfo[1];
            }
            catch
            {
                version_label.Text += "Не удалось узнать последнюю версию";
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
            playersAddedCount = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                switch (paths[i])
                {
                    case null:
                        break;
                    case "":
                        playersAddedCount++;
                        break;
                    case "needLoad":
                        MessageBox.Show("Не выбран файл стратегии для Игрока " + (i + 1));
                        return;
                    default:
                        playersAddedCount++;
                        break;
                }
            }

            //Если хотя бы два игрока (пользователь или бот)
            if (playersAddedCount > playersOnBoardCount && playersOnBoardCount != 0)
            {
                MessageBox.Show("Количество добавленных Ботов должно быть меньше или равно количеству Ботов на указанной карте");
                return;
            }

            if (playersAddedCount > 1)
            {
                Compiler cpm = new Compiler();
                Random rn = new Random();
                if (string.IsNullOrWhiteSpace(UserMapPath))
                {
                    try
                    {
                        DirectoryInfo di = new DirectoryInfo(Compiler.mapsPath);
                        var files = di.GetFiles("*.txt");
                        if (files.Length < 1)
                        {
                            throw new Exception($"Не удалось найти ни одной карты в папке {Compiler.mapsPath}");
                        }
                        UserMapPath = files[rn.Next(0, files.Length)].FullName;
                    }
                    catch (Exception er)
                    {
                        Helper.LOG(Compiler.LogPath, $"Не удалось загрузить карту из стандартных карт: {er.Message}");

                    }
                }


                Form1 realGameForm = new Form1(this, UserMapPath);
                this.Hide();
                realGameForm.Show();
            }
            else
            {
                MessageBox.Show("Необходимо добавить не менее двух игроков");
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
                        gameBoardStates = JsonConvert.DeserializeObject<List<GameBoard>>(json);
                        List<Bonus> bonu = new List<Bonus>();
                        for (int k = 0; k < gameBoardStates.Count; k++)
                        {
                            for (int i = 0; i < gameBoardStates[k].Bonuses.Count; i++)
                            {
                                if (gameBoardStates[k].Bonuses[i].Visible != true)
                                {
                                    gameBoardStates[k].Bonuses.RemoveAt(i);
                                    i--;
                                }
                            }
                        }


                    }
                    break;

                case "gz":
                    string jsonPath = psource.Replace(".gz", ".json");
                    Helper.Decompress(psource, jsonPath);
                    gameBoardStates = GetGameBoardStatesFromFile(jsonPath);
                    break;
            }

            return gameBoardStates;
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
                labels[i] = ofd.FileName;
                return ofd.FileName;
            }
            else
            {
                paths[i] = "needLoad";
                labels[i] = null;
            }
            return "";
        }


        ///// <summary>
        ///// Заменить тип игрока 
        ///// </summary>
        ///// <param name="i"></param>
        ///// <param name="btn"></param>
        ///// <param name="label"></param>
        //private void ChangeBotUser(int i, Button btn, Label label, CheckBox check)
        //{
        //    labels[0] = path1_lab.Text;
        //    labels[1] = path2_lab.Text;
        //    labels[2] = path3_lab.Text;
        //    labels[3] = path4_lab.Text;

        //    if (check.Checked == false && labels[i] == "")
        //    {
        //        paths[i] = null;
        //    }
        //    else if (check.Checked == false && labels[i] != "")
        //    {
        //        paths[i] = labels[i];
        //    }
        //    else
        //    {
        //        paths[i] = "";
        //    }

        //    btn.Enabled = !btn.Enabled;
        //    label.Enabled = !label.Enabled;
        //}

        private void path1_btn_Click_1(object sender, EventArgs e)
        {
            path1_lab.Text = AddBotsFiles(0);
            ToolTip t = new ToolTip();
            t.SetToolTip(path1_lab, path1_lab.Text);
        }


        private void path2_btn_Click_1(object sender, EventArgs e)
        {
            path2_lab.Text = AddBotsFiles(1);
            ToolTip t = new ToolTip();
            t.SetToolTip(path2_lab, path2_lab.Text);
        }

        private void path2_btn_Click(object sender, EventArgs e)
        {

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

        //private void checkBot1_CheckedChanged(object sender, EventArgs e)
        //{
        //    ChangeBotUser(0, path1_btn, path1_lab, checkBot1);
        //}

        //private void checkBot2_CheckedChanged(object sender, EventArgs e)
        //{
        //    ChangeBotUser(1, path2_btn, path2_lab, checkBot2);
        //}

        //private void checkBot3_CheckedChanged(object sender, EventArgs e)
        //{
        //    ChangeBotUser(2, path3_btn, path3_lab, checkBot3);
        //}

        //private void checkBot4_CheckedChanged(object sender, EventArgs e)
        //{
        //    ChangeBotUser(3, path4_btn, path4_lab, checkBot4);
        //}



        private void load_custom_map_Click(object sender, EventArgs e)
        {
            mapPathLabel.Text = "";
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Txt file | *.txt";
            ofd.InitialDirectory = Directory.GetCurrentDirectory();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                UserMapPath = ofd.FileName;
            }

            if (String.IsNullOrWhiteSpace(UserMapPath))
            {
                return;
            }

            mapPathLabel.Text = Helper.SpliteEndPath(UserMapPath);

            int[,] tempgbppseudo = new int[1, 1];
            try
            {
                tempgbppseudo = Helper.GetGameboardFromFile(UserMapPath);
            }
            catch
            {
                return;
            }

            playersOnBoardCount = 0;
            for (int i = 0; i < tempgbppseudo.GetLength(0); i++)
            {
                for (int j = 0; j < tempgbppseudo.GetLength(1); j++)
                {
                    if (tempgbppseudo[i, j] == 5)
                    {
                        playersOnBoardCount++;
                    }
                }
            }

        }


        public void CheckComboBoxes()
        {
            if (comboBox1.SelectedIndex == 0 && comboBox2.SelectedIndex == 0 && comboBox3.SelectedIndex == 0 && comboBox4.SelectedIndex == 0)
            {
                realGameButton.Enabled = false;
                return;
            }

            realGameButton.Enabled = true;
        }


        private void ComboboxChange(ComboBox combo, Button btn, Label label, int i)
        {
            CheckComboBoxes();

            btn.Enabled = false;
            label.Enabled = false;

            switch (combo.SelectedIndex)
            {
                case 0:
                    paths[i] = null;
                    break;

                case 1:
                    paths[i] = "wait";
                    break;

                case 2:
                    if (labels[i] == null)
                    {
                        paths[i] = "needLoad";
                    }
                    else
                    {
                        paths[i] = labels[i];
                    }

                    btn.Enabled = true;
                    label.Enabled = true;
                    break;

                case 3:
                    paths[i] = "";
                    break;
                case 4:
                    paths[i] = "wait_bot";
                    break;
                case 5:
                    paths[i] = "right_left";
                    break;
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboboxChange(comboBox1, path1_btn, path1_lab, 0);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboboxChange(comboBox2, path2_btn, path2_lab, 1);
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboboxChange(comboBox3, path3_btn, path3_lab, 2);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboboxChange(comboBox4, path4_btn, path4_lab, 3);
        }

        private void github_link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(githubLink);
        }
    }
}

