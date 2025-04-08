using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhyIDidntKnowThisGameEarlier.MapLogic;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    /// <summary>
    /// Реализует графическое представление игровой карты
    /// </summary>
    public class MapDrawer
    {
        /// <summary>
        /// Объект карты, оболочкой над которым является текущий объект
        /// </summary>
        public readonly Map Map;

        /// <summary>
        /// Сетка для карты данного объекта
        /// </summary>
        public readonly Grid Grid;

        private readonly SolidBrush brush;
        private Color player1Color;
        private Color player2Color;
        private Color emptyCellColor;
        
        /// <summary>
        /// Создает объект MapDrawer для заданной карты
        /// </summary>
        /// <param name="map"> Объект отображаемой карты </param>
        public MapDrawer(Map map) 
        {
            Map = map;
            Grid = new Grid(this);

            player1Color = Color.LightBlue;
            player2Color = Color.IndianRed;
            emptyCellColor = Color.Transparent;

            brush = new SolidBrush(emptyCellColor);
        }

        /// <summary>
        /// Левый верхний угол изображения карты на контроле
        /// </summary>
        public Point ImagePosition { get; private set; }

        /// <summary>
        /// Цвет территории игрока 1
        /// </summary>
        public Color Player1Color
        {
            get { return player1Color; }
            set { player1Color = value; }
        }

        /// <summary>
        /// Цвет территории игрока 2
        /// </summary>
        public Color Player2Color
        {
            get { return player2Color; }
            set { player2Color = value; }
        }

        /// <summary>
        /// Цвет пустой территории
        /// </summary>
        public Color EmptyCellColor
        {
            get { return emptyCellColor; }
            set { emptyCellColor = value; }
        }

        /// <summary>
        /// Рисует карту
        /// </summary>
        /// <param name="g"></param>
        public void Draw(Graphics g)
        {
            Grid.Draw(g);

            for (int x = 0; x < Map.Width; x++)
            {
                for (int y = 0; y < Map.Height; y++)
                {
                    if (Map[x, y] == CellValues.Empty)
                        continue;

                    DrawCell(g, x, y);
                }
            }
        }

        /// <summary>
        /// Центрирует изображение карты в соответствии с переданными размерами контейнера
        /// </summary>
        /// <param name="surfaceSize"> Размер поверхности отображения </param>
        public void CenterIn(Size surfaceSize)
        {
            bool isSizeCorrect = surfaceSize.Width > 0 && surfaceSize.Height > 0;
            bool isImageFit = surfaceSize.Width > Map.Width * Grid.CellSize && surfaceSize.Height > Map.Height * Grid.CellSize;

            if (!isSizeCorrect || !isImageFit)
                return;

            ImagePosition = new Point
            {
                X = (surfaceSize.Width - Map.Width * Grid.CellSize) / 2,
                Y = (surfaceSize.Height - Map.Height * Grid.CellSize) / 2
            };
        }

        /// <summary>
        /// Пробует изменить размеры ячеек так, 
        /// чтобы карта полностью поместилась в указанном контейнере, 
        /// если при текущих размерах карта не влезает. Если карта изначально влезает, 
        /// подбирает оптимальный размер для ячеек
        /// </summary>
        /// <returns> True, если карта уже влезает или подобраны новые возможные размеры ячеек.
        /// False, если размер ячеек оказывается слишком мал </returns>
        public bool FitIn(Size size)
        {
            if (size.Width <= 0 || size.Height <= 0)
                return false;

            int newSize = FindTargetSize(size);

            if (Grid.MinCellSize < newSize)
            {
                Grid.CellSize = newSize;
                return true;
            }

            return false;
        }

        private int FindTargetSize(Size size)
        {
            int targetWidth = (int)(size.Width * 0.9);
            int targetHeight = (int)(size.Height * 0.9);

            bool isBiggerWidth = Map.Width > Map.Height;

            int targetSize = isBiggerWidth ? Map.Width : Map.Height;
            int newSize;

            if (isBiggerWidth)
                newSize = targetWidth / targetSize;
            else
                newSize = targetHeight / targetSize;
            return newSize;
        }

        private void DrawCell(Graphics g, int x, int y)
        {
            brush.Color = GetColor(Map[x, y]);

            float X = ImagePosition.X + x * Grid.CellSize + 1;
            float Y = ImagePosition.Y + y * Grid.CellSize + 1;
            RectangleF rect = new RectangleF(new PointF(X, Y), new SizeF(Grid.CellSize - 2, Grid.CellSize - 2));
            g.FillRectangle(brush, rect);
        }

        private Color GetColor(CellValues value)
        {
            switch (value)
            {
                case CellValues.Player1:
                    return player1Color;
                case CellValues.Player2:
                    return player2Color;
                default:
                    return emptyCellColor;
            }
        }
    }
}
