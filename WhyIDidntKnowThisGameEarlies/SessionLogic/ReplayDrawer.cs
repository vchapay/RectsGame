using System;
using System.Collections.Generic;
using System.Drawing;
using WhyIDidntKnowThisGameEarlier.MapLogic;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    /// <summary>
    /// Является оболочкой-визуализатором реплеев
    /// </summary>
    public class ReplayDrawer
    {
        private MapDrawer mapDrawer;
        private Map map;
        private readonly List<Point> selectedCells;
        private Replay replay;
        private readonly SolidBrush selectedCellsBrush;
        private Color selectingColor;
        private readonly Pen selectedCellsPen;
        private readonly List<EventHandler> showingMoveChangingHandlers;

        /// <summary>
        /// Создает объект визуализатора для заданного реплея
        /// </summary>
        /// <param name="replay"></param>
        public ReplayDrawer(Replay replay) : this()
        {
            ChangeShowingReplay(replay);
        }

        /// <summary>
        /// Создает объект визуализатора, для которого не установлен целевой реплей
        /// </summary>
        public ReplayDrawer()
        {
            selectedCellsBrush = new SolidBrush(Color.Gold);
            selectedCellsPen = new Pen(Color.Gold);
            SelectingColor = Color.Gold;
            selectedCells = new List<Point>();
            showingMoveChangingHandlers = new List<EventHandler>();
        }

        /// <summary>
        /// Возвращает объект реплея, отображаемый текущим визуализатором
        /// </summary>
        public Replay Replay => replay;

        /// <summary>
        /// Задает или возвращает отображаемый полуход
        /// </summary>
        public int CurrentHalfMove
        {
            get => Replay.CurrentHalfMove;
            private set 
            {
                Replay.CurrentHalfMove = value;
                OnShowingMoveShanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Возвращает или устанавливает цвет, которым будут отображаться выделенные ячейки на карте
        /// </summary>
        public Color SelectingColor 
        { 
            get { return selectingColor; }
            set 
            {
                selectingColor = value;
                selectedCellsBrush.Color = FillingSelectingColor;
                selectedCellsPen.Color = OutlineSelectingColor;
            } 
        }

        /// <summary>
        /// Происходит, когда сменяется отображаемый ход
        /// </summary>
        public event EventHandler ShowingMoveChanged
        {
            add
            {
                showingMoveChangingHandlers.Add(value);
            }

            remove
            {
                if (!showingMoveChangingHandlers.Contains(value))
                    return;

                showingMoveChangingHandlers.Remove(value);
            }
        }

        private Color FillingSelectingColor => Color.FromArgb(20, SelectingColor);
        private Color OutlineSelectingColor => Color.FromArgb(120, SelectingColor);

        /// <summary>
        /// Изменяет отображаемый реплей
        /// </summary>
        /// <param name="replay"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ChangeShowingReplay(Replay replay)
        {
            this.replay = replay ?? throw new ArgumentNullException();

            map = new Map(replay.MapSize);
            mapDrawer = new MapDrawer(map);
        }

        /// <summary>
        /// Отрисовывает реплей
        /// </summary>
        /// <param name="g"></param>
        public void Draw(Graphics g)
        {
            g.Clear(Color.White);

            if (replay == null)
                return;

            mapDrawer.Draw(g);
            DrawSelectedCells(g);
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
            if (mapDrawer  == null)
                return false;

            return mapDrawer.FitIn(size);
        }

        /// <summary>
        /// Центрирует изображение карты в соответствии с переданными размерами контейнера
        /// </summary>
        /// <param name="surfaceSize"> Размер поверхности отображения </param>
        public void CenterIn(Size surfaceSize)
        {
            mapDrawer?.CenterIn(surfaceSize);
        }

        /// <summary>
        /// Устанавливает выделение на ячейке, если такая ячейка существует на карте
        /// </summary>
        public void Select(int x, int y)
        {
            if (Replay == null)
                return;

            if (!IsOnMap(x, y))
                return;

            Point cell = new Point(x, y);

            if (selectedCells.Contains(cell))
                return;

            selectedCells.Add(cell);
        }

        /// <summary>
        /// Устанавливает выделение на ячейке, если такая ячейка существует на карте
        /// </summary>
        public void Select(Point cell)
        {
            Select(cell.X, cell.Y);
        }

        /// <summary>
        /// Возвращает логическое значение, находится ли заданная ячейка на карте
        /// </summary>
        /// <returns> True, если ячейка на карте, иначе - false </returns>
        public bool IsOnMap(int x, int y)
        {
            if (x < 0 || y < 0)
                return false;

            if (x >= map.Width || y >= map.Height)
                return false;

            return true;
        }

        /// <summary>
        /// Возвращает логическое значение, находится ли заданная ячейка на карте
        /// </summary>
        /// <returns> True, если ячейка на карте, иначе - false </returns>
        public bool IsOnMap(Point cell)
        {
            return IsOnMap(cell.X, cell.Y);
        }

        /// <summary>
        /// Снимает выделение с заданной ячейки, если эта ячейка была выделена
        /// </summary>
        public void Deselect(int x, int y)
        {
            if (selectedCells.Contains(new Point(x, y)))
                selectedCells.Remove(new Point(x, y));
        }

        /// <summary>
        /// Снимает выделение с заданной ячейки, если эта ячейка была выделена
        /// </summary>
        public void Deselect(Point cell)
        {
            Deselect(cell.X, cell.Y);
        }

        /// <summary>
        /// Выделяет все ячейки в прямоугольнике, 
        /// заданном левым верхним и правым нижним углами (единицы измерения - ячейки)
        /// </summary>
        public void SelectArea(Point startCell, Point endCell)
        {
            ChangeSelectingForArea(Select, startCell, endCell);
        }

        /// <summary>
        /// Снимает выделение со всех ячеек в прямоугольнике, 
        /// заданном левым верхним и правым нижним углами (единицы измерения - ячейки)
        /// </summary>
        /// <param name="startCell"></param>
        /// <param name="endCell"></param>
        public void DeselectArea(Point startCell, Point endCell)
        {
            ChangeSelectingForArea(Deselect, startCell, endCell);
        }

        /// <summary>
        /// Снимает выделение со всех выделенных ячеек
        /// </summary>
        public void DeselectAll()
        {
            selectedCells.Clear();
        }

        /// <summary>
        /// Возвращает ячейку, которая содержит точку с заданными координатами
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Point GetCell(Point point)
        {
            int x = (point.X - mapDrawer.ImagePosition.X) / mapDrawer.Grid.CellSize;
            int y = (point.Y - mapDrawer.ImagePosition.Y) / mapDrawer.Grid.CellSize;
            return new Point(x, y);
        }

        /// <summary>
        /// Переходит к отображению заданного полухода по его порядковому номеру в списке ходов,
        /// если такой полуход существует 
        /// </summary>
        /// <param name="moveInd"></param>
        /// <param name="turn"></param>
        public void ShowMove(int moveInd, Participants turn)
        {
            int targetIndex = moveInd * 2;
            if (turn == Participants.Creator) 
                targetIndex -= 1;

            ShowMove(targetIndex);
        }

        /// <summary>
        /// Переходит к отображению заданного полухода по его порядковому номеру в списке ходов,
        /// если такой полуход существует
        /// </summary>
        /// <param name="ind"></param>
        public void ShowMove(int ind)
        {
            if (Replay == null)
                return;

            map.Clear();
            AddRect(Replay.CreatorStartRectangle, Participants.Creator);
            AddRect(Replay.ClientStartRectangle, Participants.Client);

            for (int moveNum = 0; moveNum <= ind; moveNum++)
            {
                Replay.GoToHalfMove(moveNum);
                if (CurrentHalfMove != moveNum)
                    return;

                Move move = Replay.GetMove();
                if (move.IsSkipped)
                    continue;

                AddRect(move.SuppliedRectangle, move.MovedPlayer);
            }
        }

        /// <summary>
        /// Переходит к отображению следующего хода
        /// </summary>
        public void Next()
        {
            if (Replay == null)
                return;

            CurrentHalfMove++;
            AddNextMoveRectangle();
            OnShowingMoveShanged(EventArgs.Empty);
        }

        /// <summary>
        /// Переходит к отображению предыдущего хода
        /// </summary>
        public void Previous()
        {
            if (Replay == null)
                return;

            ClearCurrentMoveRectangle();
            CurrentHalfMove--;
        }

        /// <summary>
        /// Переходит к отображению начального положения
        /// </summary>
        public void ToBeginning()
        {
            if (Replay == null)
                return;

            CurrentHalfMove = 0;

            map.Clear();
            AddRect(Replay.CreatorStartRectangle, Participants.Creator);
            AddRect(Replay.ClientStartRectangle, Participants.Client);
        }

        /// <summary>
        /// Переходит к отображению финального положения
        /// </summary>
        public void ToEnding()
        {
            if (Replay == null)
                return;

            CurrentHalfMove = Replay.Lenght - 1;
            ShowMove(CurrentHalfMove);
        }

        private void AddNextMoveRectangle()
        {
            Move move = Replay.GetMove();
            if (move.IsSkipped)
                return;
            Rectangle rect = move.SuppliedRectangle;
            AddRect(rect, move.MovedPlayer);
        }

        private void AddRect(Rectangle rect, Participants player)
        {
            for (int x = rect.X; x < rect.X + rect.Width; x++)
            {
                for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    map[x, y] = (CellValues)(int)player;
                }
            }
        }

        private void ClearCurrentMoveRectangle()
        {
            Move move = Replay.GetMove();
            if (move.IsSkipped)
                return;
            Rectangle rect = move.SuppliedRectangle;
            ClearRect(rect);
        }

        private void ClearRect(Rectangle rect)
        {
            for (int x = rect.X; x < rect.X + rect.Width; x++)
            {
                for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                {
                    map[x, y] = CellValues.Empty;
                }
            }
        }

        private void DrawSelectedCells(Graphics g)
        {
            foreach (var cell in selectedCells)
            {
                int cellSize = mapDrawer.Grid.CellSize;
                int x = cell.X * cellSize + mapDrawer.ImagePosition.X + 1;
                int y = cell.Y * cellSize + mapDrawer.ImagePosition.Y + 1;
                Rectangle rect = new Rectangle(new Point(x, y), new Size(cellSize - 2, cellSize - 2));
                g.FillRectangle(selectedCellsBrush, rect);
                g.DrawRectangle(selectedCellsPen, rect);
            }
        }

        private void ChangeSelectingForArea(MethodChangesSelecting changingMethod,
            Point startCell, Point endCell)
        {
            int width = Math.Abs(endCell.X - startCell.X);
            int height = Math.Abs(endCell.Y - startCell.Y);

            int signX = endCell.X > startCell.X ? 1 : -1;
            int signY = endCell.Y > startCell.Y ? 1 : -1;

            for (int x = 0; x <= width; x++)
            {
                for (int y = 0; y <= height; y++)
                {
                    int X = startCell.X + (x * signX);
                    int Y = startCell.Y + (y * signY);

                    changingMethod(X, Y);
                }
            }
        }

        private void OnShowingMoveShanged(EventArgs e)
        {
            foreach (var handler in showingMoveChangingHandlers)
            {
                handler.Invoke(this, e);
            }
        }

        delegate void MethodChangesSelecting(int x, int y);
    }
}
