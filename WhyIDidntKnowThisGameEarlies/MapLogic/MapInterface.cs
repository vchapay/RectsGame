using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhyIDidntKnowThisGameEarlier.SessionLogic;

namespace WhyIDidntKnowThisGameEarlier.MapLogic
{
    /// <summary>
    /// Перечисляет все возможные значения ячеек на карте
    /// </summary>
    public enum CellValues
    {
        /// <summary>
        /// Пустая ячейка
        /// </summary>
        Empty = 0,

        /// <summary>
        /// Ячейка первого игрока
        /// </summary>
        Player1 = 1,

        /// <summary>
        /// Ячейка второго игрока
        /// </summary>
        Player2 = 2
    }

    /// <summary>
    /// Представляет собой объект карты только для чтения
    /// </summary>
    public interface IReadOnlyMap
    {
        /// <summary>
        /// Ширина карты (в ячейках)
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Высота карты (в ячейках)
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Размеры карты (в ячейках)
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Общая площадь, занятая первым игроком (создателем сессии)
        /// </summary>
        int SpacePlayer1 { get; }

        /// <summary>
        /// Общая площадь, занятая вторым игроком (клиентом сессии)
        /// </summary>
        int SpacePlayer2 { get; }

        /// <summary>
        /// Готова ли карта к использованию в сессии
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Возвращает значение перечислимого типа,
        /// описывающее текущее состояние заданной ячейки
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        CellValues this[int x, int y] { get; }

        /// <summary>
        /// Возвращает стартовый прямоугольник для первого игрока (создателя сессии)
        /// либо пустую структуру Rectangle, если стартовый прямоугольник еще не был установлен
        /// </summary>
        Rectangle CreatorStartRectangle { get; }

        /// <summary>
        /// Возвращает стартовый прямоугольник для второго игрока (клиента сессии)
        /// либо пустую структуру Rectangle, если стартовый прямоугольник еще не был установлен
        /// </summary>
        Rectangle ClientStartRectangle { get; }
    }

    public class Map
    {
        public const int DefaultSize = 20;

        private int[,] map;
        private const int minSize = 10;
        private const int maxSize = 70;

        /// <summary>
        /// Создает карту заданного размера
        /// </summary>
        /// <param name="size"></param>
        public Map(Size size)
        {
            size = AdjustSize(size);
            map = new int[size.Width, size.Height];
        }

        /// <summary>
        /// Создает карту заданного размера
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Map(int width, int height) : this(new Size(width, height))
        {
        }

        /// <summary>
        /// Возвращает значение перечислимого типа,
        /// описывающее текущее состояние заданной ячейки
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CellValues this[int x, int y]
        {
            get
            {
                return (CellValues)map[x, y];
            }

            set
            {
                map[x, y] = (int)value;
            }
        }

        /// <summary>
        /// Ширина карты (в ячейках)
        /// </summary>
        public int Width => map.GetLength(0);

        /// <summary>
        /// Высота карты (в ячейках)
        /// </summary>
        public int Height => map.GetLength(1);

        /// <summary>
        /// Определяет, пустые ли все ячейки на карте
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                foreach (int x in map)
                    if (x != 0)
                        return false;

                return true;
            }
        }

        /// <summary>
        /// Очищает карту
        /// </summary>
        public void Clear()
        {
            map = new int[Width, Height];
        }

        /// <summary>
        /// Создает и возвращает точную копию данного объекта карты
        /// </summary>
        /// <returns></returns>
        public Map Clone()
        {
            Map newMap = new Map(Width, Height)
            {
                map = (int[,])map.Clone()
            };

            return newMap;
        }

        private Size AdjustSize(Size size)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                size.Width = DefaultSize;
                size.Height = DefaultSize;
            }

            if (size.Width > maxSize || size.Width < minSize) size.Width = DefaultSize;
            if (size.Height > maxSize || size.Height < minSize) size.Height = DefaultSize;
            return size;
        }
    }


    /// <summary>
    /// Предоставляет оболочку для карты, необходимую для ее использования в сессии
    /// </summary>
    public class MapInterface : IReadOnlyMap
    {
        private readonly Map map;

        /// <summary>
        /// Создает карту заданного размера (в ячейках) и оболочку для нее
        /// </summary>
        /// <param name="size"> Размеры карты, представляемые
        /// количеством ячеек в каждом измерении</param>
        public MapInterface(Size size)
        {
            CreatorStartRectangle = Rectangle.Empty;
            ClientStartRectangle = Rectangle.Empty;
            map = new Map(size);
        }

        /// <summary>
        /// Создает карту заданного размера (в ячейках) и оболочку для нее
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public MapInterface(int width, int height) : this(new Size(width, height))
        {
        }

        /// <summary>
        /// Создает оболочку над заданной картой
        /// </summary>
        /// <param name="map"></param>
        public MapInterface(MapInterface mapInterface)
        {
            CreatorStartRectangle = mapInterface.CreatorStartRectangle;
            ClientStartRectangle = mapInterface.ClientStartRectangle;

            map = mapInterface.map.Clone();
            IsReady = mapInterface.IsReady;

            SpacePlayer1 = mapInterface.SpacePlayer1;
            SpacePlayer2 = mapInterface.SpacePlayer2;
        }

        public MapInterface(Map map)
        {
            if (map == null)
                throw new ArgumentNullException();

            ClientStartRectangle = Rectangle.Empty;
            CreatorStartRectangle = Rectangle.Empty;

            this.map = map;
        }

        /// <summary>
        /// Ширина карты (в ячейках)
        /// </summary>
        public int Width => map.Width;

        /// <summary>
        /// Высота карты (в ячейках)
        /// </summary>
        public int Height => map.Height;

        public Size Size => new Size(Width, Height);

        /// <summary>
        /// Общая площадь, занятая первым игроком (создателем сессии)
        /// </summary>
        public int SpacePlayer1 { get; private set; }

        /// <summary>
        /// Общая площадь, занятая вторым игроком (клиентом сессии)
        /// </summary>
        public int SpacePlayer2 { get; private set; }

        /// <summary>
        /// Готова ли карта к использованию в сессии
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Возвращает значение перечислимого типа,
        /// описывающее текущее состояние заданной ячейки
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CellValues this[int x, int y]
        {
            get
            {
                return map[x, y];
            }
        }

        /// <summary>
        /// Возвращает стартовый прямоугольник для первого игрока (создателя сессии)
        /// либо пустую структуру Rectangle, если стартовый прямоугольник еще не был установлен
        /// </summary>
        public Rectangle CreatorStartRectangle { get; private set; }

        /// <summary>
        /// Возвращает стартовый прямоугольник для второго игрока (клиента сессии)
        /// либо пустую структуру Rectangle, если стартовый прямоугольник еще не был установлен
        /// </summary>
        public Rectangle ClientStartRectangle { get; private set; }

        /// <summary>
        /// Устанавливает стартовые прямоугольники на карте, если они еще не были установлены. 
        /// Данный метод необходимо вызвать перед использованием карты. 
        /// </summary>
        /// <param name="clientRectSize"> Размер прямоугольника первого игрока (создателя сессии) </param>
        /// <param name="creatorRectSize"> Размер прямоугольника второго игрока (клиента сессии) </param>
        /// <exception cref="ArgumentException"></exception>
        public void SetStartPosition(Size clientRectSize, Size creatorRectSize)
        {
            if (IsReady) return;

            if (clientRectSize.Equals(Size.Empty) || creatorRectSize.Equals(Size.Empty))
                throw new ArgumentException("Получены нулевые значения размеров прямоугольников для " +
                    "задания стартовой позиции");

            ClientStartRectangle = new Rectangle(new Point(), clientRectSize);
            CreatorStartRectangle = new Rectangle(
                new Point(Width - creatorRectSize.Width, Height - creatorRectSize.Height), 
                creatorRectSize);

            AddRectangle(Participants.Creator, CreatorStartRectangle);
            AddRectangle(Participants.Client, ClientStartRectangle);

            IsReady = true;
        }

        /// <summary>
        /// Пробует добавить заданный прямоугольник на карту.
        /// </summary>
        /// <param name="player"> Игрок, добавляющий прямоугольник </param>
        /// <param name="rect"> Целевой прямоугольник для добавления на карту.
        /// Единицы измерения - ячейки </param>
        /// <returns> True, если операция удалась. Иначе false.</returns>
        public bool TryAddRectangle(Participants player, Rectangle rect)
        {
            if (!IsReady)
                return false;

            if (!IsPossibleToAdd(player, rect))
                return false;

            AddRectangle(player, rect);
            return true;
        }

        /// <summary>
        /// Создает копию текущего объекта MapInterface
        /// </summary>
        /// <returns></returns>
        public MapInterface Clone()
        {
            return new MapInterface(this);
        }

        private bool IsPossibleToAdd(in Participants player, in Rectangle rect)
        {
            if (!IsOnMap(rect))
                return false;

            if (!IsFitIn(rect))
                return false;

            if (!IsPlayerCellsNearby(player, rect))
                return false;

            return true;
        }

        private bool IsPlayerCellsNearby(in Participants player, in Rectangle rect)
        {
            return IsPlayersSpaceLeftOrRight(player, rect) || IsPlayersSpaceOverOrBelow(player, rect);
        }

        private bool IsPlayersSpaceOverOrBelow(in Participants player, in Rectangle rect)
        {
            for (int x = 0; x < rect.Width; x++)
            {
                int X = x + rect.X;

                // есть ли территория игрока сверху
                if (rect.Y - 1 >= 0)
                    if ((int)map[X, rect.Y - 1] == (int)player)
                        return true;

                // есть ли территория игрока снизу
                if (rect.Y + rect.Height < Height)
                    if ((int)map[X, rect.Y + rect.Height] == (int)player)
                        return true;
            }

            return false;
        }

        private bool IsPlayersSpaceLeftOrRight(in Participants player, in Rectangle rect)
        {
            for (int y = 0; y < rect.Height; y++)
            {
                int Y = y + rect.Y;

                // есть ли территория игрока слева
                if (rect.X - 1 >= 0)
                    if ((int)map[rect.X - 1, Y] == (int)player)
                        return true;

                // есть ли территория игрока справа
                if (rect.X + rect.Width < Width)
                    if ((int)map[rect.X + rect.Width, Y] == (int)player)
                        return true;
            }

            return false;
        }

        private bool IsFitIn(in Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0) 
                return false;

            for (int x = 0; x < rect.Width; x++)
            {
                for (int y = 0; y < rect.Height; y++)
                {
                    int X = x + rect.X;
                    int Y = y + rect.Y;

                    if (map[X, Y] != (int)CellValues.Empty)
                        return false;
                }
            }

            return true;
        }

        private bool IsOnMap(in Rectangle rect)
        {
            bool isPositive = rect.X >= 0 && rect.Y >= 0;

            bool isFitIn = (rect.X <= Width - rect.Width) && 
                (rect.Y <= Height - rect.Height);

            return isPositive && isFitIn;
        }

        private void AddRectangle(in Participants player, in Rectangle rect)
        {
            for (int x = 0; x < rect.Width; x++)
            {
                for (int y = 0; y < rect.Height; y++)
                {
                    int X = x + rect.X;
                    int Y = y + rect.Y;

                    map[X, Y] = (CellValues)(int)player;
                    IncreasePlayersSpace(player);
                }
            }
        }

        private void IncreasePlayersSpace(Participants player)
        {
            switch (player)
            {
                case Participants.Creator:
                    SpacePlayer1++;
                    break;
                case Participants.Client:
                    SpacePlayer2++;
                    break;
            }
        }
    }
}
