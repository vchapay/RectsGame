using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;
using WhyIDidntKnowThisGameEarlier.SessionLogic;

namespace WhyIDidntKnowThisGameEarlier.MapLogic
{
    /// <summary>
    /// Инкапсулирует логику сетки для карты
    /// </summary>
    public class Grid
    {
        public const int MinCellSize = 5;
        public const int MaxCellSize = 150;

        private Map map;
        private MapDrawer mapGraphicInterface;
        private int cellSize;
        private Pen pen;

        private const int defaultCellSize = 25;

        public Grid(MapDrawer mapGraphicInterface)
        {
            this.mapGraphicInterface = mapGraphicInterface;
            map = mapGraphicInterface.Map;
            pen = new Pen(Color.LightGray);
            cellSize = defaultCellSize;
        }

        /// <summary>
        /// Размер игровой ячейки в пикселях
        /// </summary>
        public int CellSize
        {
            get { return cellSize; }
            set
            {
                if (cellSize == value || value < MinCellSize || value > MaxCellSize)
                    return;

                cellSize = value;
            }
        }

        /// <summary>
        /// Цвет сетки
        /// </summary>
        public Color Color
        {
            get { return pen.Color; }
            set { pen.Color = value; }
        }

        /// <summary>
        /// Отображает сетку
        /// </summary>
        /// <param name="g"> Графический объект, использующийся для отображения </param>
        public void Draw(Graphics g)
        {
            DrawVerticalLines(g);
            DrawHorizontalLines(g);

            Size sizeMap = new Size(map.Width * CellSize, map.Height * CellSize);
            g.DrawRectangle(Pens.Black, new Rectangle(mapGraphicInterface.ImagePosition, sizeMap));
        }

        private void DrawHorizontalLines(Graphics g)
        {
            for (int y = 0; y < map.Height; y++)
            {
                int X = map.Width * CellSize + mapGraphicInterface.ImagePosition.X;
                int Y = mapGraphicInterface.ImagePosition.Y + y * CellSize;

                g.DrawLine(pen, new Point(mapGraphicInterface.ImagePosition.X, Y), new Point(X, Y));
            }
        }

        private void DrawVerticalLines(Graphics g)
        {
            for (int x = 0; x < map.Width; x++)
            {
                int X = mapGraphicInterface.ImagePosition.X + x * CellSize;
                int Y = map.Height * CellSize + mapGraphicInterface.ImagePosition.Y;

                g.DrawLine(pen, new Point(X, mapGraphicInterface.ImagePosition.Y), new Point(X, Y));
            }
        }
    }
}
