using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sapper
{
    public enum TypeCell//Вид ячейки
    {
        CloseCell, FlagCell, QuestionCell, TheBlowUpCell,             //Закрытая ячейка, Ячейка с флагом, ячейка с вопросом, Взорванная ячейка,
        CloseTheBlowUpCell, NumericCell, EmptyCell, KeyDownWhellMouse,//Закрытая ячейка при пройгрыше, ячейка с цифрой, Пустая ячейка, Ячейка при нажатии на неё средней кнопкой
        KeyDownCell, WrongFlagCell, CellMoveMouse                     //Ячейка при удерживании ЛКМ, ячейка при неправильной постановке флага
    };

    public partial class frmGame : Form
    {

        static public ushort N = 10, M = 10, K = 10;//ширина, высота игрового поля и число мин
        bool EndGame = false;//флаг, означающий окончена игра или нет
        int Iprev, Jprev, Xprev, Yprev;
        TypeCell PrevType;
        bool DownLeftButton;
        bool Cheat;//флаг использования чита
        int TimeCheat;//время просмотра бомб

        //Для записи в таблицу результатов (Данные победителя)
        static public bool NewWinner; //флаг, который показывает, надо ли добавлять нового победителя в таблицу результатов
        static public ushort TypeGame = 0; //0 - Новичок, 1 - Адепт, 2 - Мастер, 3 - Особый
        static public string NameWinner; //Имя победителя данной игры
        static public int TimeWinner; //время победителя данной игры
        static public int WidthWinner;   //ширина поля (если игра специальная)
        static public int HeightWinner; //высота поля
        static public int MinesWinner; //число мин
        static public DateTime DateWinner;//время победы


/*================================================= TSAPPER =============================================================*/
//************************************************************************************************************************
        public class TSapper//класс, инкапсулирующий алгоритмы игры
        {
            //------------------------- ПОЛЯ ------------------------------------------
            public struct FieldCell//ячейка игрового поля
            {
                public bool Open;// открыта или закрыта
                public bool Mined;// заминирована или нет
                public ushort FlagOrQuestion; //содержит 0-ничего, 1-флаг, 2-вопрос
                public ushort Numeric;//содержит число мин вокруг ячейки
                public Color NumericColor;//цвет цифры
                public bool Empty;//является пустой
                public bool BlewOnIt;//является ячейкой, на которой подорвался игрок
                public bool RePaint;//флаг перерисовки ячейки
                public TypeCell ViewCell;//вид ячейки (как она отрисовывается на поле)
            }

            public FieldCell[,] Fields;// игровое поле
            public ushort N, M, //размеры поля (ширина, высота) 
                   QuantityMine,// число мин 
                   SizeCell = 20;// размер ячейки пиксель
            public bool EndLoose, EndWin; //флаги - Игра окончена пройрышем, выйгрышем
            public int FindedMine = 0;//число найденных мин, отмеченных флагами
            public int CheckMine = 0;//число клеток, отмеченных флагами
            public int Time = 0;//время игры
            public int CloseCells;//число закрытых ячеек без флагов

//---------------------------------------- МЕТОДЫ ---------------------------------------------------------
            //-------------------------- Конструктор класса --------------------------------------
            public TSapper(ushort n, ushort m, ushort NumMine)
            {
                Fields = new FieldCell[n + 2, m + 2];//создание игрового поля ("+2" для упрощения алгоритма анализа соседних к данной ячеек)
                int i, j, 
                    Im, Jm; //координаты мины
                Random RndMineI = new Random();//рандом для генерации мин


                N = n;
                M = m;
                QuantityMine = NumMine;
                EndLoose = false;
                EndWin = false;
                CloseCells = N * M;
                FindedMine = CheckMine = Time = 0;                
                
                //заполняем поле пустыми закрытыми ячейками
                for (i = 0; i < n + 2; i++)
                    for (j = 0; j < m + 2; j++)
                    {
                        Fields[i, j].Open = false;
                        Fields[i, j].Empty = true;
                        Fields[i, j].Mined = false;
                        Fields[i, j].Numeric = 0;
                        Fields[i, j].FlagOrQuestion = 0;
                        Fields[i, j].Empty = true;
                        Fields[i, j].BlewOnIt = false;
                        Fields[i, j].RePaint = true;
                        Fields[i, j].ViewCell = TypeCell.CloseCell;
                    }

                //расставляем мины
                 for (i = 1; i <= NumMine; i++)
                 {
                     Im = RndMineI.Next(1, N + 1);
                     Jm = RndMineI.Next(1, M + 1);
                     while (Fields[Im, Jm].Mined)//генерируем до тех пор, пока не попадем в ячейку без мины
                      {
                       Im = RndMineI.Next(1, N + 1);
                       Jm= RndMineI.Next(1, M + 1);
                      }

                     Fields[Im, Jm].Mined = true;
                     Fields[Im, Jm].Empty = false;
                   }


                //расставляем числа в нужные ячейки
                for(i = 1; i <= n; i++)
                    for (j = 1; j <= m; j++)
                    {
                        if (Fields[i,j].Mined) continue;

                        ushort S = 0;
                        //анализируем 8 соседних к данной ячеек
                         for(int k = i - 1; k <= i + 1; k++)
                             for (int l = j - 1; l <= j + 1; l++)
                             {
                                 if ((k == i) && (l == j)) continue;//пропускаем ячейку, вокруг которой ищем мины
                                 if (Fields[k, l].Mined) S++;
                             }
                         Fields[i, j].Numeric = S;
                         if (S != 0)
                         {
                             Fields[i, j].Empty = false;
                             switch (S)//в зависимости от цифры в ячейки, задаем для неё определенный цвет
                             {//от зеленого к красному
                                 case 1: 
                                     Fields[i, j].NumericColor = new Color();
                                     Fields[i, j].NumericColor = Color.FromArgb(0, 255, 0);
                                     break;
                                 case 2:
                                     Fields[i, j].NumericColor = new Color();
                                     Fields[i, j].NumericColor = Color.FromArgb(0,128,0);
                                     break;
                                 case 3:
                                     Fields[i, j].NumericColor = new Color();
                                     Fields[i, j].NumericColor = Color.FromArgb(0,0,255);
                                     break;
                                 case 4:
                                     Fields[i, j].NumericColor = new Color();
                                     Fields[i, j].NumericColor = Color.FromArgb(0,64,128);
                                     break;
                                 case 5:
                                     Fields[i, j].NumericColor = new Color();
                                     Fields[i, j].NumericColor = Color.FromArgb(128,0,128);
                                     break;
                                 case 6:
                                     Fields[i, j].NumericColor = new Color();
                                     Fields[i, j].NumericColor = Color.FromArgb(255,128,0);
                                     break;
                                 case 7:
                                     Fields[i, j].NumericColor = new Color();
                                     Fields[i, j].NumericColor = Color.FromArgb(128,0,0);
                                     break;
                                 case 8:
                                     Fields[i, j].NumericColor = new Color();
                                     Fields[i, j].NumericColor = Color.DarkRed;
                                     break;

                             }
                         }
                    }
            }
            //----------------------------- End TSapper Create ---------------------------------------------


            //----------------- рекурсивный метод открытия пустых яечеек (в том числе граничных цифр) -------
            void OpenAllNextEmptyCells(int I, int J)//I,J - индексы анализируемой ячейки
            {
                if ((I == 0) || (J == 0) || (I == N + 1) || (J == M + 1))
                    return;//пропуск фиктивных ячеек

                //анализируем 8 соседних ячеек вокруг I, J
                for(int i = I - 1; i <= I + 1; i++)
                    for (int j = J - 1; j <= J + 1; j++)
                    {
                        if ((Fields[i, j].Empty) && (!Fields[i, j].Open))
                        { //если данная ячейка закрыта и пуста
                          //то открываем её и вызываем для неё рекурсивно этот метод                    
                            Fields[i, j].Open = true;
                            Fields[i, j].RePaint = true;
                            Fields[i, j].ViewCell = TypeCell.EmptyCell;
                            OpenAllNextEmptyCells(i, j);
                        }

                        if ((Fields[i, j].Numeric != 0) && (!Fields[i, j].Open))
                        {
                            //если в данной ячейке число, то просто открываем её
                            Fields[i, j].Open = true;
                            CloseCells--;
                            Fields[i, j].RePaint = true;
                            Fields[i, j].ViewCell = TypeCell.NumericCell;
                        }
                    }
                CloseCells--;
            }
            //---------------------------- End OpenAllNextEmptyCells ---------------------------------


            //----------------------------- Метод получений экранных координат левого ----------------------
            //----------------------------- верхнего угла и индексов ячейки, в которую --------------------
            //----------------------------- попали указателем мыши ----------------------------------------
            public void CellHit(int Xmouse, int Ymouse, out int X, out int Y, out int Imouse, out int Jmouse)
            {
                int i, j,
                    Icell, Jcell;//координаты левого верхнего угла данной ячейки

                X = 0; Y = 0;//экранный координаты чейки, в которую попали
                Imouse = 1; Jmouse = 1; //индексы ячейки, в которую попали

                //определяем в какую ячейку попал игрок
                for (i = 1; i <= N; i++)
                    for (j = 1; j <= M; j++)
                    {
                        Icell = (i - 1) * (SizeCell + 1);
                        Jcell = (j - 1) * (SizeCell + 1);

                        //если точка попадания внутри данной ячейки
                        if (((Xmouse >= Icell) && (Xmouse <= Icell + SizeCell)) &&
                             ((Ymouse >= Jcell) && (Ymouse <= Jcell + SizeCell)))
                        {
                            X = Icell;
                            Y = Jcell;
                            Imouse = i;
                            Jmouse = j;
                            break;
                        }
                    }
            }

            //---------------- Метод анализа хода игрока. Xmouse, Ymouse - координаты клика, ---------------
            //---------------- Btn - кнопка мыши, X, Y - координаты левого верхнего угла ячейки ---------
            public int CourseOfThePlayer(int Xmouse, int Ymouse, MouseButtons Btn, out int X, out int Y)
            {
                int i, j,
                    Imouse, Jmouse;

                CellHit(Xmouse, Ymouse, out X, out Y, out Imouse, out Jmouse);//определяем куда попал игрок

                switch (Btn)//в зависимости от того, какой кнопкой он щелкал правим массив Fields
                {

                    case MouseButtons.Left://если произошел щелчок ЛКМ
                        {
                            //если ячейка закрыта и без флага
                            if ((!Fields[Imouse, Jmouse].Open) && (Fields[Imouse, Jmouse].FlagOrQuestion != 1))
                            {
                               
                                // Ячейка заминирована
                                if (Fields[Imouse, Jmouse].Mined)
                                {
                                    EndLoose = true;//Игрок проиграл
                                    Fields[Imouse, Jmouse].BlewOnIt = true;//игрок подорвался на данной ячейке

                                    //необходимо изменить все игровое поле:
                                     //Изменить вид закрытых не взорванных ячеек
                                    // Открыть все ячейки с минами
                                    //изменить вид ячеки с флагом, если игрок ошибся с его постановкой
                                    //флаг RePaint в true для всех ячеек поля
                                    for (i = 1; i <= N; i++)
                                        for (j = 1; j <= M; j++)
                                        {
                                            //Меняем закрытые ячейки
                                            if (!Fields[i, j].Open)
                                            {
                                                if (Fields[i, j].FlagOrQuestion == 1)//если стоит флаг
                                                {
                                                    if (!Fields[i, j].Mined)//если флаг поставлен неверно меняем тип ячейки
                                                        Fields[i, j].ViewCell = TypeCell.WrongFlagCell;
                                                    Fields[i, j].RePaint = true;
                                                }
                                                else//если флага не стоит
                                                {
                                                    if (!Fields[i, j].Mined)//меняем вид закрытых не заминированных ячеек
                                                    {
                                                        Fields[i, j].ViewCell = TypeCell.CloseTheBlowUpCell;
                                                        Fields[i, j].RePaint = true;
                                                    }
                                                    else
                                                    {//открываем заминированную ячейку
                                                        Fields[i, j].Open = true;
                                                        Fields[i, j].ViewCell = TypeCell.TheBlowUpCell;
                                                        Fields[i, j].RePaint = true;
                                                    }
                                                }
                                            }
                                        }
                                }

                                //если мы попали на ячеку с числом то просто открываем его
                                if (Fields[Imouse, Jmouse].Numeric != 0)
                                {
                                    Fields[Imouse, Jmouse].Open = true;
                                    CloseCells--;
                                    Fields[Imouse, Jmouse].RePaint = true;
                                    Fields[Imouse, Jmouse].ViewCell = TypeCell.NumericCell;
                                }

                                //если попали в пустую ячеку, вызываем рекурсивный метод открытия соседних пустых ячеек
                                if (Fields[Imouse, Jmouse].Empty)
                                {
                                    Fields[Imouse, Jmouse].Open = true;
                                    Fields[Imouse, Jmouse].RePaint = true;
                                    Fields[Imouse, Jmouse].ViewCell = TypeCell.EmptyCell;
                                    OpenAllNextEmptyCells(Imouse, Jmouse);
                                    //перерисовываем всё поле
                                    for (i = 1; i <= N; i++)
                                        for (j = 1; j <= M; j++)
                                            Fields[i, j].RePaint = true;
                                }
                            }
                        }
                        break;
                    case MouseButtons.Right://если щелкнули ПКМ
                        {
                            if (!Fields[Imouse, Jmouse].Open)//ячейка закрыта
                            {
                                switch (Fields[Imouse, Jmouse].FlagOrQuestion)//проверяем, что стояло на ячейке до щелччка
                                {
                                    case 0://если ячека была просто закрыта, ставим на неё флаг
                                        Fields[Imouse, Jmouse].FlagOrQuestion = 1;
                                        Fields[Imouse, Jmouse].RePaint = true;
                                        Fields[Imouse, Jmouse].ViewCell = TypeCell.FlagCell;
                                        if (Fields[Imouse, Jmouse].Mined) FindedMine++;
                                        CheckMine++;
                                        CloseCells--;
                                        break;
                                    case 1://если на ней стоял флаг, меняем его на знак вопроса
                                        Fields[Imouse, Jmouse].FlagOrQuestion = 2;
                                        Fields[Imouse, Jmouse].RePaint = true;
                                        Fields[Imouse, Jmouse].ViewCell = TypeCell.QuestionCell;
                                        if (Fields[Imouse, Jmouse].Mined) FindedMine--;
                                        CheckMine--;
                                        CloseCells++;
                                        break;
                                    case 2://если там был знак вопроса то возвращаем ей вид закрытой ячейки
                                        Fields[Imouse, Jmouse].FlagOrQuestion = 0;
                                        Fields[Imouse, Jmouse].RePaint = true;
                                        Fields[Imouse, Jmouse].ViewCell = TypeCell.CloseCell;
                                        break;
                                }
                            }
                        }
                        break;
            }
                if (Fields[Imouse, Jmouse].RePaint)//если данную ячеку нужно перерисовывать, то
                                                  //проверяем, как это надо сделать
                {
                    if (Btn == MouseButtons.Right) return 1;              //необходимо перерисовать только эту ячейку
                    if (Fields[Imouse, Jmouse].Numeric != 0) return 1;    //если открыли число или щелкнули ПКМ
                                                                          //возвращаем 1

                    //если нужно перерисовать все поле, когда открыли пустую ячейку или подорвались, то -1
                    if ((Fields[Imouse, Jmouse].BlewOnIt) || (Fields[Imouse, Jmouse].Empty) || (Btn == MouseButtons.Middle)) return -1;

                }
                return 0;//если ничего не надо перерисовывать, вернём 0    

           }
      }
/*======================================== END TSAPPER =====================================================*/
//***********************************************************************************************************

        TSapper newGame;//Объект - новая игра


        public frmGame()
        {
            InitializeComponent();
        }

        //ВЫбор пункта меню с новой игрой
        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Stop();//останавливаем таймер

            linkLabel1.Text = "";//Инициализируем текстовые поля
            textBox1.Text = "";

            newGame = new TSapper(N, M, K);//создаем новую игру с необходимыми параметрами
            //настраиваем отображение компонентов
            picboxGameField.Width = N * newGame.SizeCell + N;
            picboxGameField.Height = M * newGame.SizeCell + M;
            frmGame.ActiveForm.Width = picboxGameField.Width + 35;
            frmGame.ActiveForm.Height = picboxGameField.Height + 190;

            textBox2.Text = newGame.QuantityMine.ToString();//отображаем число мин

            picboxGameField.Invalidate();//перерисовываем игровое поле
            picboxMan.Load("Resourse\\findmies.jpg");
            EndGame = false;
            Cheat = false;
            TimeCheat = 0;
        }



        //-------------- перерисовка игрового поля в зависимости от действия игрока ----------------------
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if((newGame != null))
            {
                if (!EndGame)//если с последним ходом игрока закончилась игра
                {
                    if (newGame.EndLoose)//закончилась поражением
                    {
                        EndGame = true;
                        linkLabel1.Text = "Lose! You killed them all";
                        picboxMan.Load("Resourse\\Loose.jpg");
                        timer1.Stop();
                    }
                    if (newGame.EndWin)
                    {
                        if (!Cheat)
                        {
                            EndGame = true;
                            timer1.Stop();
                            TimeWinner = newGame.Time;//сохраняем время выйгрыша
                            if (TypeGame != 0)
                            {
                                WidthWinner = N;//если игра особая то сохраняем её параметры
                                HeightWinner = M;
                                MinesWinner = K;
                            }
                            NewWinner = true;            //флаг наличия победителя
                            DateWinner = DateTime.Now;   //сохраняем текущие дату и время
                            frmNameWinner frm4 = new frmNameWinner();//вызываем форму ввода имени игрока
                            frm4.ShowDialog();
                            picboxMan.Load("Resourse\\Win.jpg");
                            switch (TypeGame)//в зывисимости от типа игры выводим сообщение
                            {
                                case 0:
                                    linkLabel1.Text = "Nice Job, " + NameWinner + "!";
                                    break;
                                case 1:
                                    linkLabel1.Text = "You're realy cool, " + NameWinner + "!";
                                    break;
                                case 2:
                                    linkLabel1.Text = "GODLIKE, " + NameWinner + "!";
                                    break;
                                case 3:
                                    double k = (double)MinesWinner / (HeightWinner * WidthWinner);//отношение числа мин к размеру поля
                                    if (k < 0.1)//введено для того, чтобы игрок с 1 миной на поле 10х10 не занимал строчку выше чем игрок
                                    {           //с полем 10х10 и числом мин 15 (к примеру)
                                        linkLabel1.Text = "A child can make it!";
                                        picboxMan.Load("Resourse\\badgame.jpg");
                                    }
                                    if ((k >= 0.1) && (k < 0.15625))//игра уровня сложности Новичок
                                        linkLabel1.Text = "Nice Job, " + NameWinner + "!";
                                    if ((k >= 0.15625) && (k < 0.2083))//игра уровня сложности Адепт
                                        linkLabel1.Text = "You're realy cool, " + NameWinner + "!";
                                    if ((k >= 0.2083) && (k <= 0.25))//игра уровня сложности Мастер
                                        linkLabel1.Text = "GODLIKE, " + NameWinner + "!";
                                    if (k > 0.25)//Невероятно сложная игра
                                        linkLabel1.Text = "IT'S INCREDIBLY, " + NameWinner + "!!!";
                                    break;
                            }
                        }
                        else
                        {
                            linkLabel1.Text = "You're a CHEATER!!!";
                            picboxMan.Load("Resourse\\Cheater.jpg");
                            timer1.Stop();
                            EndGame = true;
                        }
                    }
                }


             //проходим по всему массиву Игрового поля, и перерисовываем нужные ячейки
             for(int i = 1; i <= newGame.N; i++)
                 for (int j = 1; j <= newGame.M; j++)
                 {
                     if (newGame.Fields[i, j].RePaint)
                     {
                         //прямоугольник, в котором находится данная ячейка
                         Rectangle rect = new Rectangle(((i - 1) * (newGame.SizeCell + 1)), ((j - 1) * (newGame.SizeCell + 1)), newGame.SizeCell - 1, newGame.SizeCell - 1);

                         switch (newGame.Fields[i, j].ViewCell)
                         {//отрисовываем ячейку, в зависимости от её типа
                             case TypeCell.CloseCell:
                                 e.Graphics.DrawIcon(new System.Drawing.Icon("Resourse\\CloseCell.ico"), rect);
                                 newGame.Fields[i, j].RePaint = false;
                                 break;
                             case TypeCell.TheBlowUpCell:
                                 e.Graphics.DrawIcon(new System.Drawing.Icon("Resourse\\TheBlowUpCell.ico"), rect);
                                 newGame.Fields[i, j].RePaint = false;
                                 break;
                             case TypeCell.CloseTheBlowUpCell:
                                 e.Graphics.DrawIcon(new System.Drawing.Icon("Resourse\\CloseTheBlowUpCell.ico"), rect);
                                 newGame.Fields[i, j].RePaint = false;
                                 break;
                             case TypeCell.NumericCell:
                                 e.Graphics.DrawString(newGame.Fields[i, j].Numeric.ToString(), new Font("Times New Roman", 12, FontStyle.Bold), new SolidBrush(newGame.Fields[i, j].NumericColor), rect);
                                 break;
                             case TypeCell.EmptyCell:
                                 e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.White)), rect);
                                 newGame.Fields[i, j].RePaint = false;
                                 break;
                             case TypeCell.FlagCell:
                                 e.Graphics.DrawIcon(new System.Drawing.Icon("Resourse\\Flag.ico"), rect);
                                 newGame.Fields[i, j].RePaint = false;
                                 break;
                             case TypeCell.QuestionCell:
                                 e.Graphics.DrawIcon(new System.Drawing.Icon("Resourse\\Question.ico"), rect);
                                 newGame.Fields[i, j].RePaint = false;
                                 break;
                             case TypeCell.KeyDownCell:
                                 e.Graphics.DrawIcon(new System.Drawing.Icon("Resourse\\KeyDownCell.ico"), rect);
                                 newGame.Fields[i, j].RePaint = false;
                                 break;
                             case TypeCell.WrongFlagCell:
                                 e.Graphics.DrawIcon(new System.Drawing.Icon("Resourse\\WrongFlag.ico"), rect);
                                 newGame.Fields[i, j].RePaint = false;
                                 break;
                             case TypeCell.KeyDownWhellMouse:
                                 e.Graphics.DrawIcon(new System.Drawing.Icon("Resourse\\MouseDownWheelCell.ico"), rect);
                                 newGame.Fields[i, j].RePaint = false;
                                 break;

                         }
                     }
                 }
             }
        }

        //При нажатии и удерживании ЛКМ
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { DownLeftButton = true; Iprev = -1; }
            if (!EndGame)
            {
                int X, Y, Imouse, Jmouse;
                newGame.CellHit(e.X, e.Y, out X, out Y, out Imouse, out Jmouse);
                if ((newGame.Fields[Imouse, Jmouse].ViewCell == TypeCell.CloseCell) && (e.Button == MouseButtons.Left))
                {
                    picboxMan.Load("Resourse\\Wait, Wait.jpg");
                }
            }

        }

        //Щелчок на игровое поле, в зависимости от области попадания правим массив игрового поля
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            int X = 1, Y = 1;
            if ((newGame != null) && (!newGame.EndLoose) && (!newGame.EndWin))
            {
                int a = newGame.CourseOfThePlayer(e.X, e.Y, e.Button, out X, out Y);

                if (!timer1.Enabled)//если секундомер не запущен, запускаем его
                {
                    timer1.Start();
                }

                if (a == 1)//надо перерисовать одну ячейку
                {
                    picboxGameField.Invalidate(new Rectangle(X, Y, 20, 20));//задаем для области перерисовки только её координаты
                    textBox2.Text = (newGame.QuantityMine - newGame.CheckMine).ToString();
                }

                if (a == -1)//перерисовываем все игровое поле
                    picboxGameField.Invalidate();

                //если число закрытых ячеек без флагов плюс число ячеек с флагами становится равно числу мин (при этом число поставленных флагов равно числу флагов на минах) то констатируем победу игрока
                if (((newGame.CloseCells + newGame.FindedMine == newGame.QuantityMine) || (newGame.FindedMine == newGame.QuantityMine)) && (newGame.CheckMine == newGame.FindedMine))
                    newGame.EndWin = true;
            }
        }

        //выбор пункта меню с опциями, вызываем форму с настройками
        private void optionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmOption frm = new frmOption();
            frm.ShowDialog();
        }

        //выход из игры
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //при первом отображении главной формы с игрой
        private void Form1_Shown(object sender, EventArgs e)
        {
            newGame = new TSapper(N, M, K);
            picboxGameField.Width = N * newGame.SizeCell + N;
            picboxGameField.Height = M * newGame.SizeCell + M;
            frmGame.ActiveForm.Width = picboxGameField.Width + 35;
            frmGame.ActiveForm.Height = picboxGameField.Height + 190;
            textBox2.Text = newGame.QuantityMine.ToString();
            picboxMan.Load("Resourse\\findmies.jpg");
            NewWinner = false;
            picboxGameField.Invalidate();
            Cheat = false;
            TimeCheat = 0;
        }


        //Один тик таймера (по прошесвию секунды)
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (newGame != null)
            {
                newGame.Time++;
                textBox1.Text = newGame.Time.ToString();
            }
            if (Cheat)//если игрок хочет просмотреть расположение бомб
            {
                if (TimeCheat == 4)
                {//когда проходит 4 секунды, скрываем бомбы
                    for (int i = 1; i <= newGame.N; i++)
                        for (int j = 1; j <= newGame.M; j++)
                        {
                            if (newGame.Fields[i, j].Mined)
                            {
                                switch (newGame.Fields[i, j].FlagOrQuestion)
                                {
                                    case 0:
                                        newGame.Fields[i, j].ViewCell = TypeCell.CloseCell;
                                        break;
                                    case 1:
                                        newGame.Fields[i, j].ViewCell = TypeCell.FlagCell;
                                        break;
                                    case 2:
                                        newGame.Fields[i, j].ViewCell = TypeCell.QuestionCell;
                                        break;
                                }
                            }
                            newGame.Fields[i, j].RePaint = true;
                        }
                    TimeCheat = 0;
                    picboxGameField.Invalidate();
                }

                TimeCheat++;
            }
        }

        //убираем фокусы ввода с текстовых полей
        private void textBox1_Click(object sender, EventArgs e)
        {
            menuStrip1.Select();
        }


        private void textBox2_Click(object sender, EventArgs e)
        {
            menuStrip1.Select();
        }

        //при отпускании ЛКМ меняем изображение в picboxMan
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (DownLeftButton)
            {
                DownLeftButton = false;
            }
            if(!EndGame)
                picboxMan.Load("Resourse\\findmies.jpg");
        }

        //перерисовываем всё игровое поле при перемещении, сворачивании, перемещение за границу экрана формы с игрой
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 1; i <= newGame.N; i++)
                for (int j = 1; j <= newGame.M; j++)
                    newGame.Fields[i, j].RePaint = true;
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmResults frm3 = new frmResults();
            frm3.ShowDialog();
        }


        //если пользователь при нажатии ЛКМ начинает перемещать курсор
        private void picboxGameField_MouseMove(object sender, MouseEventArgs e)
        {
            if ((DownLeftButton) && !EndGame)
            {
                int ICell, JCell, Xcell, Ycell;
                newGame.CellHit(e.X, e.Y, out Xcell, out Ycell, out ICell, out JCell);
                //перерисовываем ячейку куда перемещается курсор, а данну. ячейку возвращаем в предыдущее положение
                if (!((newGame.Fields[ICell, JCell].Open) || (newGame.Fields[ICell, JCell].FlagOrQuestion != 0)))
                {
                    if (Iprev < 0)
                    {
                        Iprev = ICell;
                        Jprev = JCell;
                        Xprev = Xcell;
                        Yprev = Ycell;
                        PrevType = newGame.Fields[ICell, JCell].ViewCell;
                        newGame.Fields[ICell, JCell].ViewCell = TypeCell.KeyDownCell;
                        newGame.Fields[ICell, JCell].RePaint = true;
                        picboxGameField.Invalidate(new Rectangle(Xcell, Ycell, 20, 20));
                    }
                    else
                        if (!((ICell == Iprev) && (JCell == Jprev)))
                        {
                            newGame.Fields[Iprev, Jprev].ViewCell = PrevType;
                            newGame.Fields[Iprev, Jprev].RePaint = true;
                            picboxGameField.Invalidate(new Rectangle(Xprev, Yprev, 20, 20));
                            Iprev = ICell;
                            Jprev = JCell;
                            Xprev = Xcell;
                            Yprev = Ycell;
                            PrevType = newGame.Fields[ICell, JCell].ViewCell;
                            newGame.Fields[ICell, JCell].ViewCell = TypeCell.KeyDownCell;
                            newGame.Fields[ICell, JCell].RePaint = true;
                            picboxGameField.Invalidate(new Rectangle(Xcell, Ycell, 20, 20));

                        }
                }
                else
                {
                    if (Iprev > 0)
                    {
                        newGame.Fields[Iprev, Jprev].ViewCell = PrevType;
                        newGame.Fields[Iprev, Jprev].RePaint = true;
                        picboxGameField.Invalidate(new Rectangle(Xprev, Yprev, 20, 20));
                    }
                }
            }

        }

        //если указатель покинул игровую область, то перерисовываем ячейку, из которой он вышел за границы
        private void picboxGameField_MouseLeave(object sender, EventArgs e)
        {
            if ((Iprev > 0) && DownLeftButton)
            {
                newGame.Fields[Iprev, Jprev].ViewCell = PrevType;
                newGame.Fields[Iprev, Jprev].RePaint = true;
                picboxGameField.Invalidate(new Rectangle(Xprev, Yprev, 20, 20));
            }
            if (!newGame.Fields[1, 1].Open && !EndGame)
            {
                newGame.Fields[1, 1].ViewCell = TypeCell.CloseCell;
                newGame.Fields[1, 1].RePaint = true;
                picboxGameField.Invalidate(new Rectangle(0, 0, 20, 20));
            }
        }


        //показать бомбы
        private void showBombsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EndGame)
            {
                Cheat = true;
                for (int i = 1; i <= newGame.N; i++)
                    for (int j = 1; j <= newGame.M; j++)
                    {
                        if (newGame.Fields[i, j].Mined)
                        {//отображаем все бомбы на экране
                            if(newGame.Fields[i,j].FlagOrQuestion == 0)
                                newGame.Fields[i, j].ViewCell = TypeCell.TheBlowUpCell;
                        }
                        newGame.Fields[i, j].RePaint = true;
                    }
                if (!timer1.Enabled)
                {
                    timer1.Start();
                }
                picboxGameField.Invalidate();
            }
        }





    }
}
