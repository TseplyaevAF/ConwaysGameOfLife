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
    // Каждая клетка может быть "живой" (зеленая) либо "мертвой" (черная)
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
        Graphics g;
        static ushort lifeSize = 32; // кол-во жизненных клеток в PictureBox
        static ushort pointRadius = 10; // радиус одной жизненной клетки
        Generation gen = new Generation(lifeSize);
        SolidBrush blackBrush, greenBrush;
        Rectangle[,] Cells = new Rectangle[lifeSize, lifeSize];

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
            pictureBox_Play.Enabled = false;
        }

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
            /// Нарисовать поле
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
                        g.FillRectangle(greenBrush, i * pointRadius, j * pointRadius,
                            pointRadius, pointRadius);
                    }
                }
            }
        }

        private void pictureBox_Clear_Click(object sender, EventArgs e)
        {
            g = Graphics.FromHwnd(pictureBox_GameField.Handle);
            g.FillRectangle(blackBrush, 0, 0,
                pictureBox_GameField.Width, pictureBox_GameField.Height);
            gen.Clear();
        }

        private void pictureBox_Step_Click(object sender, EventArgs e)
        {
            gen.NextGeneration();
            PaintField();
        }

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

        private void pictureBox_GameField_Paint(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(Color.Black);
            for (int i = 0; i < lifeSize; i++)
            {
                for (int j = 0; j < lifeSize; j++)
                {
                    Cells[i, j] = new Rectangle(
                        i * pictureBox_GameField.Width / (lifeSize),
                        j * pictureBox_GameField.Height / (lifeSize), pointRadius, pointRadius);
                    g.FillRectangle(blackBrush, Cells[i, j]);
                }
            }
        }

        private void pictureBox_GameField_MouseClick(object sender, MouseEventArgs e)
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
                        Cells[i, j] = new Rectangle(
                        i * pictureBox_GameField.Width / (lifeSize),
                        j * pictureBox_GameField.Height / (lifeSize), pointRadius, pointRadius);
                        gen.ChangeColorCell(i, j);
                        if (gen.getLifeGeneration(i, j))
                            g.FillRectangle(greenBrush, Cells[i, j]);
                        else g.FillRectangle(blackBrush, Cells[i, j]);
                        break;
                    }
                }
            }
        }

        private void pictureBox_GameField_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < lifeSize; i++)
                {
                    for (int j = 0; j < lifeSize; j++)
                    {
                        if ((e.Y >= (Cells[i, j].Location.Y)) &&
                        (e.Y <= (Cells[i, j].Location.Y + pointRadius)) &&
                        (e.X >= Cells[i, j].Location.X) &&
                        (e.X <= (Cells[i, j].Location.X + pointRadius)))
                        {

                            gen.ChangeColorCell(i, j);
                            if (gen.getLifeGeneration(i, j))
                                g.FillRectangle(greenBrush, Cells[i, j]);
                            else g.FillRectangle(blackBrush, Cells[i, j]);
                        }
                    }
                }
            }
        }

        private void pictureBox_Fill_Click(object sender, EventArgs e)
        {
            pictureBox_Play.Enabled = true;
            gen.RandomGeneration();
            PaintField();
        }
    }
}
