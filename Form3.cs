using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Sapper
{
    public partial class frmResults : Form
    {
        //структура для записи в файл, содержащая данные об игроке
        public struct Player
        {
            public string Name;//имя
            public int Time; //время, за которое игрок нашел все мины
            public DateTime DateWin;//дата победы
            public int Width;//если игра специальная то ширина поля
            public int Height;//высота поля
            public int Mines;//число мин
        }

        double KoefSpecialGame;//отношение числа мин к размеру поля
        int PositionWin = 0;//позиция победителя в массиве
        //колличество записей в файлах
        int NumRecN;//новичков
        int NumRecA;//любителей
        int NumRecM;//профессионалов
        int NumRecS;//специальных игр
        public Player[] NeophyteGame;//массив для записи в файл результатов новичков
        public Player[] AdeptGame;//массив для записи в файл результатов любителей
        public Player[] MasterGame;//массив для записи в файл результатов профессионалов
        public Player[] SpecialGame;
        //пути к файлам результатов
        string pathN = "Results\\NeophyteResults.nres";//новичков
        string pathA = "Results\\AdeptResults.ares";//любителей
        string pathM = "Results\\MasterResults.mres";//профи
        string pathS = "Results\\SpecialResults.sres";//спец.

        //--------------- Метод добавления нового игрока в таблицу результатов -------------------------
        /*
         * Входные параметры:
         * NumRec - колличество записей, TimeWin, NameWin, WidthWin, HeightWin, MinesWin - параметры победителя
         * TypeGame - тип игры (нов., люб, маст., спец.); 
         * Выходные данные
         * Mass - массив игроков, Position - позиция победителя
         */
        void AddPlayer(ref int NumRec, int TimeWin, string NameWin, DateTime DateWin, int WidthWin, 
                       int HeightWin, int MinesWin, int TypeGame, ref Player[] Mass, ref int Position)
        {
            Position = NumRec;//положим в начале позицией игрока последнее место
            switch (TypeGame)
            {
                //Игры типа Новичок, Любитель, Специалист
                case 0:
                case 1:
                case 2:
                    if (Mass != null)
                    {
                        for (int i = 0; i < NumRec; i++)
                        {
                            if (TimeWin <= Mass[i].Time)//Если время пбедителя меньше очередного игрока то
                            {
                                Position = i;//победитель занимает его позицию
                                break;
                            }
                        }
                        Array.Resize(ref Mass, NumRec + 1);//увеличиваем кол-во мест в таблице на 1
                            for (int i = NumRec - 1; i >= Position; i--)
                                Mass[i + 1] = Mass[i];//смещаем всех от позиции победителя
                        Mass[Position].Time = TimeWin;//победитель занимает данную позицию
                        Mass[Position].Name = NameWin;
                        Mass[Position].DateWin = DateWin;
                        NumRec++;
                    }
                    else
                    {//если победитель первый в таблице результатов (после очистки), то ставим его на 1 место
                        Mass = new Player[1];
                        Mass[0].Time = TimeWin;
                        Mass[0].Name = NameWin;
                        Mass[Position].DateWin = DateWin;
                        NumRec = 1;
                    }
                    break;
                case 3://Специальная игра
                    //сравнение ведется по коэффициенту (число мин делить на размер поля) и по времени
                    double KoefWin = (double)MinesWin/(WidthWin*HeightWin);
                    if (Mass != null)
                    {
                        double koef;
                        for (int i = 0; i < NumRec; i++)
                        {
                            koef = (double)Mass[i].Mines / (Mass[i].Width * Mass[i].Height);
                            if (KoefWin > koef)
                            {
                                Position = i;
                                break;
                            }
                            else
                                if ((TimeWin <= Mass[i].Time) && (KoefWin == koef))
                                {
                                    Position = i;
                                    break;
                                }
                        }
                        Array.Resize(ref Mass, NumRec + 1);
                            for (int i = NumRec - 1; i >= Position; i--)
                                Mass[i + 1] = Mass[i];
                        //в таком случае, надо записать больше параметров в таблицу результатов
                        Mass[Position].Time = TimeWin;
                        Mass[Position].Name = NameWin;
                        Mass[Position].DateWin = DateWin;
                        Mass[Position].Width = WidthWin;
                        Mass[Position].Height = HeightWin;
                        Mass[Position].Mines = MinesWin;
                        NumRec++;
                    }
                    else
                    {
                        Mass = new Player[1];
                        Mass[0].Time = TimeWin;
                        Mass[0].Name = NameWin;
                        Mass[Position].DateWin = DateWin;
                        Mass[0].Width = WidthWin;
                        Mass[0].Height = HeightWin;
                        Mass[0].Mines = MinesWin;
                        NumRec = 1;
                    }
                    break;
            }
        }


        // -------------------------- Открытие файла результатов для чтения ---------------------------------
        /*
         * Входные данные:
         * path - путь к файлу результатов
         * TypeGame - тип игры
         * Выходные данные:
         * Mass - массив для хранения таблицы результатов, NumRec - число записей в файйле
         */
        void OpenAndReadFileRes(string path,int TypeGame, ref Player[] Mass, ref int NumRec)
        {
            using (StreamReader sr = File.OpenText(path))
            {
                NumRec = Convert.ToInt32(sr.ReadLine());
                if (NumRec != 0)
                {//если таблица результатов не пуста то считываем данные
                    Mass = new Player[NumRec];
                    for (int i = 0; i < NumRec; i++)
                    {
                        switch (TypeGame)
                        {
                            case 0:
                            case 1:
                            case 2:
                                Mass[i].Name = sr.ReadLine();
                                Mass[i].Time = Convert.ToInt32(sr.ReadLine());
                                Mass[i].DateWin = Convert.ToDateTime(sr.ReadLine());
                                break;
                            case 3:
                                Mass[i].Name = sr.ReadLine();
                                Mass[i].Time = Convert.ToInt32(sr.ReadLine());
                                Mass[i].DateWin = Convert.ToDateTime(sr.ReadLine());
                                Mass[i].Width = Convert.ToInt32(sr.ReadLine());
                                Mass[i].Height = Convert.ToInt32(sr.ReadLine());
                                Mass[i].Mines = Convert.ToInt32(sr.ReadLine());
                                break;
                        }
                    }
                }
            }
        }

        //---------------------- Запись результатов в файл результатов ---------------------
        /*
         * Входные данные:
         * path - путь к файлу результатов
         * TypeGame - тип игры
         * Mass - массив для хранения таблицы результатов, 
         * NumRec - число записей в файле
         */
        void OpenAndWriteFileRes(string path, int NumRec, int TypeGame, Player[] Mass)
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                if (Mass != null)
                {
                    sw.WriteLine(NumRec);
                    for (int i = 0; i < NumRec; i++)
                    {
                        switch (TypeGame)
                        {
                            case 0:
                            case 1:
                            case 2:
                                sw.WriteLine(Mass[i].Name);
                                sw.WriteLine(Mass[i].Time);
                                sw.WriteLine(Mass[i].DateWin);
                                break;
                            case 3:
                                sw.WriteLine(Mass[i].Name);
                                sw.WriteLine(Mass[i].Time);
                                sw.WriteLine(Mass[i].DateWin);
                                sw.WriteLine(Mass[i].Width);
                                sw.WriteLine(Mass[i].Height);
                                sw.WriteLine(Mass[i].Mines);
                                break;
                        }

                    }
                }
            }
        }



        //------------------- Вывод результатов в таблицы ----------------------------------
        /*
         * Входные данные:
         * TypeGame - тип игры
         * Mass - массив для хранения таблицы результатов, 
         * NumRec - число записей в файле
         * DataGrid - таблица для вывода типа DataGridViewer
         */
        void RecordingInTheTable(Player[] Mass, int NumRec, int TypeGame, DataGridView DataGrid)
        {
            if (NumRec != 0)
            {
                DataGrid.RowCount = NumRec;
                for (int i = 0; i < NumRec; i++)
                {
                    switch (TypeGame)
                    {
                        case 0:
                        case 1:
                        case 2:
                            DataGrid[0, i].Value = i + 1;
                            DataGrid[1, i].Value = Mass[i].Time;
                            DataGrid[2, i].Value = Mass[i].Name;
                            DataGrid[3, i].Value = Mass[i].DateWin.ToString("g");
                            break;
                        case 3:
                            DataGrid[0, i].Value = i + 1;
                            DataGrid[1, i].Value = Mass[i].Width.ToString() + " x " + Mass[i].Height.ToString();
                            DataGrid[2, i].Value = Mass[i].Mines;
                            DataGrid[3, i].Value = Mass[i].Time;
                            DataGrid[4, i].Value = Mass[i].Name;
                            DataGrid[5, i].Value = Mass[i].DateWin.ToString("g");
                            break;
                    }
                }
            }
            else
            {
                DataGrid.RowCount = 1;
            }
        }


        void ClearDataGrid(DataGridView DT)
        {
            DT.RowCount = 1;
            DT.Rows.Clear();

        }


        public frmResults()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }


        //при активации формы начинаем работать с данными
        private void frmResults_Activated(object sender, EventArgs e)
        {
            
            if (frmGame.NewWinner)//если вызов формы осуществился при победе очередного игрока
            {//загружаем результаты из файла, добавляем победителя в таблицу и 
             //отображаем их в таблицах (показываем таблицу игры, в которой выйграл игрок)

                //читаем файлы результатов
                OpenAndReadFileRes(pathN, 0, ref NeophyteGame, ref NumRecN);
                OpenAndReadFileRes(pathA, 1, ref AdeptGame, ref NumRecA);
                OpenAndReadFileRes(pathM, 2, ref MasterGame, ref NumRecM);
                OpenAndReadFileRes(pathS, 3, ref SpecialGame, ref NumRecS);

                //добавляем игрока (в зависимости от типа игры, в кот. играл игрок
                switch (frmGame.TypeGame)
                {
                    case 0:
                        //добавляем игрока
                        AddPlayer(ref NumRecN, frmGame.TimeWinner, frmGame.NameWinner,frmGame.DateWinner, frmGame.WidthWinner,
                                  frmGame.HeightWinner, frmGame.MinesWinner, 0, ref NeophyteGame, ref PositionWin);
                        //правим файл
                        OpenAndWriteFileRes(pathN, NumRecN, 0, NeophyteGame);
                        break;
                    case 1:
                        AddPlayer(ref NumRecA, frmGame.TimeWinner, frmGame.NameWinner, frmGame.DateWinner, frmGame.WidthWinner,
                                  frmGame.HeightWinner, frmGame.MinesWinner, 1, ref AdeptGame, ref PositionWin);
                        OpenAndWriteFileRes(pathA, NumRecA, 1, AdeptGame);
                        break;
                    case 2:
                        AddPlayer(ref NumRecM, frmGame.TimeWinner, frmGame.NameWinner, frmGame.DateWinner, frmGame.WidthWinner,
                                  frmGame.HeightWinner, frmGame.MinesWinner, 2, ref MasterGame, ref PositionWin);
                        OpenAndWriteFileRes(pathM, NumRecM, 2, MasterGame);
                        break;
                    case 3:
                        AddPlayer(ref NumRecS, frmGame.TimeWinner, frmGame.NameWinner, frmGame.DateWinner, frmGame.WidthWinner,
                                  frmGame.HeightWinner, frmGame.MinesWinner, 3, ref SpecialGame, ref PositionWin);
                        OpenAndWriteFileRes(pathS, NumRecS, 3, SpecialGame);
                        break;
                }


                //отображаем данные в таблицах
                RecordingInTheTable(NeophyteGame, NumRecN, 0, strgridNeophyte);
                RecordingInTheTable(AdeptGame, NumRecA, 1, strgridAdept);
                RecordingInTheTable(MasterGame, NumRecM, 2, strgridMaster);
                RecordingInTheTable(SpecialGame, NumRecS, 3, strgridSpecial);

                //отображаем нужную вкладку и выделяем нового победителя
                switch (frmGame.TypeGame)
                {
                    case 0:
                        tabControl1.SelectedTab = tabPage1;
                        strgridNeophyte.CurrentCell = strgridNeophyte.Rows[PositionWin].Cells[2];
                        strgridNeophyte.Rows[PositionWin].Selected = true;
                        break;
                    case 1:
                        tabControl1.SelectedTab = tabPage2;
                        strgridAdept.CurrentCell = strgridAdept.Rows[PositionWin].Cells[2];
                        strgridAdept.Rows[PositionWin].Selected = true;
                        break;
                    case 2:
                        tabControl1.SelectedTab = tabPage3;
                        strgridMaster.CurrentCell = strgridMaster.Rows[PositionWin].Cells[2];
                        strgridMaster.Rows[PositionWin].Selected = true;
                        break;
                    case 3:
                        tabControl1.SelectedTab = tabPage4;
                        strgridSpecial.CurrentCell = strgridSpecial.Rows[PositionWin].Cells[2];
                        strgridSpecial.Rows[PositionWin].Selected = true;
                        break;
                }
                frmGame.NewWinner = false;

            }
            else //показываем таблицы результатов
            {
                OpenAndReadFileRes(pathN, 0, ref NeophyteGame, ref NumRecN);
                OpenAndReadFileRes(pathA, 1, ref AdeptGame, ref NumRecA);
                OpenAndReadFileRes(pathM, 2, ref MasterGame, ref NumRecM);
                OpenAndReadFileRes(pathS, 3, ref SpecialGame, ref NumRecS);
                RecordingInTheTable(NeophyteGame, NumRecN, 0, strgridNeophyte);
                RecordingInTheTable(AdeptGame, NumRecA, 1, strgridAdept);
                RecordingInTheTable(MasterGame, NumRecM, 2, strgridMaster);
                RecordingInTheTable(SpecialGame, NumRecS, 3, strgridSpecial);

            }
            
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            string[] Zero = new String[1];
            Zero[0] = "0";
            File.WriteAllLines(pathN,Zero);
            File.WriteAllLines(pathA, Zero);
            File.WriteAllLines(pathM, Zero);
            File.WriteAllLines(pathS, Zero);
            ClearDataGrid(strgridNeophyte);
            ClearDataGrid(strgridAdept);
            ClearDataGrid(strgridMaster);
            ClearDataGrid(strgridSpecial);
        }
    }
}
