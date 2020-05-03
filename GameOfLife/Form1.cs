using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    //--------------------------------------------------------------
    // Игра «Жизнь» (англ. Conway's Game of Life) — клеточный автомат, 
    // придуманный английским математиком Джоном Конвеем в 1970 году.
    //--------------------------------------------------------------
    //--------------------Правила игры------------------------------
    // Место действия игры -"вселенная", имеющая размеченная на клетки поверхность.
    // Каждая клетка может быть "живой" либо "мертвой"
    // Игра начинается с нулевого поколения.
    // Каждый шаг игры - это зарождение нового поколения.
    // Каждая клетка имеет вокруг себя 8 соседей.
    // Поколение появляется следующим образом:
    //      1. в "мертвой" клетке, рядом с которой ровно 3 живые клетки - зарождается жизнь
    //      2. если у живой клетки есть две или три живые соседки, то эта клетка продолжает жить; 
    //      3. в противном случае, если соседей меньше двух или больше трёх, клетка умирает 
    //      («от одиночества» или «от перенаселённости»)
    public partial class Form1 : Form
    {
        Graphics g; // для рисования клеточного поля
        static ushort lifeSize = 32; // кол-во клеток в поле
        static ushort pointSize = 10; // размер одной клетки
        Generation gen = new Generation(lifeSize); // дял реализации бизнес-логики
        SolidBrush blackBrush, greenBrush; // для закраски клеток
        Rectangle[,] Cells = new Rectangle[lifeSize, lifeSize]; // массив из прямоугольников для отрисовки поля
        Boolean isAlive = false; // для рисования только "живых" клеток
        Boolean isDead = false; // для рисования только "мертвых" клеток

        public Form1()
        {
            InitializeComponent();
            InitializeTimer();
            pictureBox_GameField.Image = 
                new Bitmap(pictureBox_GameField.Width, pictureBox_GameField.Height);
            g = Graphics.FromImage(pictureBox_GameField.Image);
            blackBrush = new SolidBrush(Color.Black);
            greenBrush = new SolidBrush(Color.Green);
            ToolTip t = new ToolTip();
            t.SetToolTip(pictureBox_Fill, "Сгенировать поколение случайным образом");
            t.SetToolTip(pictureBox_Step, "Перейти к следующему поколению");
            t.SetToolTip(pictureBox_Play, "Автоматическая генерация поколений");
            t.SetToolTip(pictureBox_Clear, "Очистить поле");
        }

        // Кнопка "Play" которая активирует таймер
        private void pictureBox_Play_Click(object sender, EventArgs e)
        {

            pictureBox_Play.Visible = false;
            pictureBox_Pause.Visible = true;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void InitializeTimer()
        {
            timer1.Interval = 100;
            timer1.Tick += new EventHandler(timer1_Tick);
        }

            /// <summary>
            /// Отрисовать клеточное поле
            /// </summary>
        private void PaintField()
        {
            g = Graphics.FromHwnd(pictureBox_GameField.Handle);
            g.FillRectangle(blackBrush, 0, 0,
                pictureBox_GameField.Width, pictureBox_GameField.Height);
            for (int i = 0; i < lifeSize; i++)
            {
                for (int j = 0; j < lifeSize; j++)
                {
                    if (gen.getNextGeneration(i, j))
                    {
                        g.FillRectangle(greenBrush, i * pointSize, j * pointSize,
                            pointSize-1, pointSize-1);
                    }
                }
            }
        }

        // Кнопка Clear очищает булевский массив и поле
        private void pictureBox_Clear_Click(object sender, EventArgs e)
        {
            g = Graphics.FromHwnd(pictureBox_GameField.Handle);
            g.FillRectangle(blackBrush, 0, 0,
                pictureBox_GameField.Width, pictureBox_GameField.Height);
            gen.Clear();
        }

        // Кнопка Step отвечает за генерацию одного поколения и отрисовки его на поле
        private void pictureBox_Step_Click(object sender, EventArgs e)
        {
            gen.NextGeneration();
            PaintField();
        }

        // Каждый заданный интервал времени вызывается событие генерирующее одно поколение
        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox_Step_Click(pictureBox_Step, null);
        }

        private void pictureBox_Pause_Click(object sender, EventArgs e)
        {
            pictureBox_Play.Visible = true;
            pictureBox_Pause.Visible = false;
            timer1.Enabled = false;
            timer1.Stop();
        }

        /// <summary>
        /// Начальная отрисовка клеточного поля.
        /// Инициализация массива из прямоугольников.
        /// </summary>
        private void pictureBox_GameField_Paint(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(Color.Black);
            for (int i = 0; i < lifeSize; i++)
            {
                for (int j = 0; j < lifeSize; j++)
                {
                    Cells[i, j] = new Rectangle(
                        i * pictureBox_GameField.Width / (lifeSize),
                        j * pictureBox_GameField.Height / (lifeSize), pointSize-1, pointSize-1);
                    g.FillRectangle(blackBrush, Cells[i, j]);
                }
            }
        }

        // Событие, позволяющее закрашивать множество клеток проведением мыши
        private void pictureBox_GameField_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < lifeSize; i++)
                {
                    for (int j = 0; j < lifeSize; j++)
                    {
                        if ((e.Y >= (Cells[i, j].Location.Y)) &&
                        (e.Y <= (Cells[i, j].Location.Y + pointSize)) &&
                        (e.X >= Cells[i, j].Location.X) &&
                        (e.X <= (Cells[i, j].Location.X + pointSize)))
                        {
                            if (isDead)
                            {   
                                if (gen.getLifeGeneration(i, j))
                                {
                                    // будем закрашивать черным "живые" клетки
                                    gen.ChangeColorCell(i, j);
                                    g.FillRectangle(blackBrush, Cells[i, j]);
                                }
                            }
                            else
                            {
                                if (isAlive)
                                if (!gen.getLifeGeneration(i, j))
                                {
                                    // будем закрашивать зеленым "мертвые" клетки
                                    gen.ChangeColorCell(i, j);
                                    g.FillRectangle(greenBrush, Cells[i, j]);
                                }
                            }
                                
                        }
                    }
                }
            }
        }

        // Событие, позволяющее закарсить только одну клетку, по которой кликнули мышкой
        private void pictureBox_GameField_MouseDown(object sender, MouseEventArgs e)
        {
            g = Graphics.FromHwnd(pictureBox_GameField.Handle);
            var location = e.Location;
            int x = location.X;
            int y = location.Y;
            for (int i = 0; i < lifeSize; i++)
            {
                for (int j = 0; j < lifeSize; j++)
                {
                    if (Cells[i, j].Contains(x, y))
                    {
                        if (gen.getLifeGeneration(i, j))
                        {
                            gen.ChangeColorCell(i, j);
                            isDead = true; // значит в событии mouse_move будем закрашивать ТОЛЬКО "живые" клетки
                            isAlive = false;
                            g.FillRectangle(blackBrush, Cells[i, j]);
                        }
                        else
                        {
                            gen.ChangeColorCell(i, j);
                            isAlive = true; // значит в событии mouse_move будем закрашивать ТОЛЬКО "мертвые" клетки
                            isDead = false;
                            g.FillRectangle(greenBrush, Cells[i, j]);
                        }
                        break;
                    }
                }
            }
        }

        // Рандомная генерация клеток и их отрисовка
        private void pictureBox_Fill_Click(object sender, EventArgs e)
        {
            gen.RandomGeneration();
            PaintField();
        }
    }
}
