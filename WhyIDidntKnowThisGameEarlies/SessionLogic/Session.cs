using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using WhyIDidntKnowThisGameEarlier.MapLogic;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    public enum GameModes : byte
    {
        /// <summary>
        /// К завершению игры приводит серия пропущенных ходов. 
        /// Критическое число указывается в соответствующем свойстве
        /// </summary>
        StreakRule,

        /// <summary>
        /// К завершению игры приводит достижение счетчиком пропущенных ходов определенного значения.
        /// Каждый сделанный ход уменьшает счетчик. Критическое число указывается в соответствующем свойстве
        /// </summary>
        AccumulationRule
    }

    /// <summary>
    /// Перечисляет участников сессии
    /// </summary>
    public enum Participants : byte
    {
        Creator = 1,
        Client = 2
    }

    /// <summary>
    /// Описывает игровую сессию
    /// </summary>
    public class Session
    {
        private MapInterface mapInterface;

        private int skippedMovesCounter;
        private const int defaultMoveDuration = 60;
        private const int defaultSkippedMovesLimit = 4;
        private int moveDuration;
        private bool isWorking;
        private readonly List<EventHandler<TurnChangedArgs>> turnChangingHandlers;
        private readonly List<EventHandler> endingHandlers;
        private Dice dice;
        private Timer timer;
        private Rectangle activeRectangle;
        private SolidBrush activeRectangleBrush;
        private int skippedMovesLimit;
        private GameModes gameMode;

        private Replay replay;
        private List<Move> moves;

        private static int sessions = 0;
        private bool isPossibleToRotate;
        private bool isSkipped;
        private int moveIndex;

        /// <summary>
        /// Создает новую игровую сессию для двух игроков
        /// </summary>
        public Session()
        {
            sessions++;
            ID = Guid.NewGuid().ToString() + "-" + sessions;

            isWorking = false;
            IsEnding = false;

            InitializeComponents();
            MoveTimeCounter = 0;

            MoveDuration = defaultMoveDuration;
            SkippedMovesLimit = defaultSkippedMovesLimit;
            GameMode = GameModes.StreakRule;

            moves = new List<Move>();
            turnChangingHandlers = new List<EventHandler<TurnChangedArgs>>();
            endingHandlers = new List<EventHandler>();
        }

        public string ID { get; }

        /// <summary>
        /// Возвращает время, когда сессия завершилась
        /// </summary>
        public DateTime EndingTime { get; private set; }

        /// <summary>
        /// Возвращает текущий ход игры
        /// </summary>
        public int Move { get; private set; }

        /// <summary>
        /// Возвращает коллекцию ходов, сделанных игроками в текущей сессии
        /// </summary>
        public IReadOnlyList<Move> Moves => moves;

        /// <summary>
        /// Возвращает очередь хода
        /// </summary>
        public Participants Turn { get; private set; }

        /// <summary>
        /// Возвращает игровую карту текущей сессии
        /// </summary>
        public IReadOnlyMap MapInterface { get { return mapInterface; } }

        /// <summary>
        /// Возвращает оболочку графического представления для игровой карты
        /// </summary>
        public MapDrawer MapDrawer { get; private set; }

        /// <summary>
        /// Определяет, создана ли карта для данной сессии
        /// </summary>
        public bool IsMapInitialized { get; private set; }

        /// <summary>
        /// Возвращает таймер текущего хода (в секундах)
        /// </summary>
        public int MoveTimeCounter { get; private set; }

        /// <summary>
        /// Возвращает или задает ограничение времени на ход (в секундах). Возможно изменить
        /// только перед запуском сессии
        /// </summary>
        public int MoveDuration 
        {
            get { return moveDuration; } 
            set 
            {
                if (isWorking || IsEnding)
                    return;

                if (moveDuration != value || moveDuration <= 0)
                    moveDuration = defaultMoveDuration;

                moveDuration = value; 
            } 
        }

        /// <summary>
        /// Возвращает общее число пропущенных ходов за сессию
        /// </summary>
        public int AllSkippedMoves { get; private set; }

        /// <summary>
        /// Возвращает или задает количество пропущенных ходов, при достижении которого в 
        /// соответствии с правилом игра заканчивается. Возможно изменить
        /// только перед запуском сессии
        /// </summary>
        public int SkippedMovesLimit
        {
            get { return skippedMovesLimit; }
            set
            {
                if (isWorking || IsEnding)
                    return;

                if (skippedMovesLimit != value || skippedMovesLimit <= 0)
                    skippedMovesLimit = defaultSkippedMovesLimit;

                skippedMovesLimit = value;
            }
        }

        /// <summary>
        /// Определяет, завершена ли текущая сессия
        /// </summary>
        public bool IsEnding { get; private set; }

        /// <summary>
        /// Происходит при смене очереди хода
        /// </summary>
        public event EventHandler<TurnChangedArgs> TurnChanged
        {
            add
            {
                turnChangingHandlers.Add(value);
            }

            remove
            {
                if (turnChangingHandlers.Contains(value))
                    turnChangingHandlers.Remove(value);
            }
        }

        /// <summary>
        /// Происходит каждую секунду
        /// </summary>
        public event EventHandler Tick
        {
            add
            {
                timer.Tick += value;
            }

            remove
            {
                timer.Tick -= value;
            }
        }

        /// <summary>
        /// Происходит, когда сессия завершается
        /// </summary>
        public event EventHandler Ended
        {
            add
            {
                endingHandlers.Add(value);
            }

            remove
            {
                if (endingHandlers.Contains(value))
                    endingHandlers.Remove(value);
            }
        }

        /// <summary>
        /// Возвращает или задает режим игры для текущей сесиии. 
        /// Возможно изменить только перед запуском сессии
        /// </summary>
        public GameModes GameMode
        {
            get { return gameMode; }
            set 
            {
                if (isWorking || IsEnding)
                    return;

                gameMode = value; 
            }
        }

        /// <summary>
        /// Определяет, можно ли поворачивать активный прямоугольник. Возможно
        /// изменить только перед запуском сессии
        /// </summary>
        public bool IsPossibleToRotate 
        { 
            get { return isPossibleToRotate; }
            set 
            { 
                if (isWorking || IsEnding) 
                    return;

                isPossibleToRotate = value;
            }
        }

        /// <summary>
        /// Счетчик пропущенных ходов, обновляющийся в соответствии с выбранным правилом
        /// </summary>
        public int SkippedMovesCounter { get => skippedMovesCounter; }

        /// <summary>
        /// Перемещает активный прямоугольник к позиции курсора
        /// </summary>
        /// <param name="position"></param>
        public void ToMousePosition(Point position)
        {
            if (IsEnding)
                return;

            activeRectangle.X = (position.X - MapDrawer.ImagePosition.X) / MapDrawer.Grid.CellSize;
            activeRectangle.Y = (position.Y - MapDrawer.ImagePosition.Y) / MapDrawer.Grid.CellSize;
        }

        /// <summary>
        /// Пробует сделать ход за активного в данный момент игрока
        /// </summary>
        /// <returns> True, если ход удался. Иначе false </returns>
        public bool TryMakeMove()
        {
            if (isWorking && !IsEnding)
            {
                bool isSuccessful = mapInterface.TryAddRectangle(Turn, activeRectangle);
                if (isSuccessful)
                {
                    switch (GameMode)
                    {
                        case GameModes.StreakRule:
                            skippedMovesCounter = 0;
                            break;
                        case GameModes.AccumulationRule:
                            if (skippedMovesCounter > 0)
                                skippedMovesCounter--;
                            break;
                    }
                    StartNewMove();
                }
                return isSuccessful;
            }

            return false;
        }

        /// <summary>
        /// Пропускает ход за текущего игрока
        /// </summary>
        public void SkipMove()
        {
            if (IsEnding)
                return;

            isSkipped = true;
            AllSkippedMoves++;
            skippedMovesCounter++;

            if (skippedMovesCounter == SkippedMovesLimit)
            {
                moveIndex++;
                AddMoveToList();
                End();
                return;
            }

            StartNewMove();
        }

        /// <summary>
        /// Запускает текущую игровую сессию
        /// </summary>
        public void Start()
        {
            if (isWorking)
                return;

            Move = 1;
            isWorking = true;

            Turn = Participants.Creator;
            timer.Start();
        }

        /// <summary>
        /// Завершает текущую сессию
        /// </summary>
        public void End()
        {
            if (!isWorking)
                return;

            isWorking = false;
            IsEnding = true;
            EndingTime = DateTime.Now;
            OnEnded(EventArgs.Empty);
            timer.Stop();
            timer.Tick -= new EventHandler(Update);
            timer.Dispose();
        }

        /// <summary>
        /// Создает карту с заданными размерами для текущей сессии
        /// </summary>
        /// <param name="size"> Размер карты </param>
        public void InitialzeMap(Size size)
        {
            if (IsMapInitialized)
                return;

            if (size.Width <= 0 && size.Height <= 0)
            {
                size = new Size(Map.DefaultSize, Map.DefaultSize);
            }

            PrepareMapDrawer(size);

            activeRectangle = Rectangle.Empty;
            UpdateActiveRectangle();

            IsMapInitialized = true;
        }

        /// <summary>
        /// Создает карту со стандартными размерами для текущей сессии
        /// </summary>
        public void InitialzeMap()
        {
            InitialzeMap(new Size(Map.DefaultSize, Map.DefaultSize));
        }

        /// <summary>
        /// Отрисовывает все необходимое текущей сессии
        /// </summary>
        /// <param name="g"></param>
        public void Draw(Graphics g)
        {
            g.Clear(Color.White);
            MapDrawer?.Draw(g);
        }

        /// <summary>
        /// Рисует активный в данный момент прямоугольник, которым игроку нужно сделать ход
        /// </summary>
        /// <param name="g"></param>
        public void DrawActiveRectangle(Graphics g)
        {
            if (!isWorking)
                return;

            Point targetRectPos = new Point(MapDrawer.ImagePosition.X + activeRectangle.Location.X * MapDrawer.Grid.CellSize,
                MapDrawer.ImagePosition.Y + activeRectangle.Location.Y * MapDrawer.Grid.CellSize);

            Size targetRectSize = new Size(activeRectangle.Width * MapDrawer.Grid.CellSize,
                activeRectangle.Height * MapDrawer.Grid.CellSize);

            Rectangle targetRect = new Rectangle(targetRectPos, targetRectSize);
            g.FillRectangle(activeRectangleBrush, targetRect);
        }

        /// <summary>
        /// Поворачивает активный в данный момент прямоугольник, если для сессии 
        /// установлено соответствующее правило
        /// </summary>
        public void RotateActiveRectangle()
        {
            if (!isWorking || !IsPossibleToRotate)
                return;

            activeRectangle.Size = new Size(activeRectangle.Height, activeRectangle.Width);
        }

        /// <summary>
        /// Создает реплей для текущей сессии
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Replay CreateReplay()
        {
            if (!IsEnding)
                throw new InvalidOperationException("Нельзя создать реплей незавершенной " +
                    "или не начатой сессии");

            return new Replay(this);
        }

        private void InitializeComponents()
        {
            dice = new Dice();
            timer = new Timer
            {
                Interval = 1000
            };

            activeRectangleBrush = new SolidBrush(Color.FromArgb(150, Color.DarkGray));
            timer.Tick += new EventHandler(Update);
        }

        private void Update(object sender, EventArgs e)
        {
            if (MoveTimeCounter == MoveDuration)
            {
                SkipMove();
                return;
            }

            MoveTimeCounter++;
        }

        private void PrepareMapDrawer(Size size)
        {
            Map map = new Map(size);
            mapInterface = new MapInterface(map);
            MapDrawer = new MapDrawer(map);

            Size size1 = dice.Generate();
            Size size2 = dice.Generate();
            mapInterface.SetStartPosition(size1, size2);
        }

        private void StartNewMove()
        {
            if (IsEnding)
                return;
            moveIndex++;
            AddMoveToList();
            OnTurnChanged(new TurnChangedArgs(Move, Turn, isSkipped));
            ChangeTurn();
            UpdateActiveRectangle();
            Move++;
            isSkipped = false;
            MoveTimeCounter = 0;
        }

        private void AddMoveToList()
        {
            try
            {
                moves.Add(new Move(moveIndex, Turn, MoveTimeCounter,
                    activeRectangle, isSkipped, SkippedMovesCounter,
                    mapInterface.SpacePlayer1, mapInterface.SpacePlayer2));
            }
            catch (Exception ex) 
            {
                string n = ex.Message;
            }
        }

        private void UpdateActiveRectangle()
        {
            Size size = dice.Generate();
            Point pos = new Point
            {
                X = activeRectangle.X,
                Y = activeRectangle.Y
            };

            activeRectangle = new Rectangle(pos, size);
        }

        private void ChangeTurn()
        {
            switch (Turn)
            {
                case Participants.Creator:
                    Turn = Participants.Client;
                    break;
                case Participants.Client:
                    Turn = Participants.Creator;
                    break;
            }
        }

        private void OnTurnChanged(TurnChangedArgs e)
        {
            foreach (var handler in turnChangingHandlers)
            {
                handler(this, e);
            }
        }

        private void OnEnded(EventArgs e)
        {
            foreach (var handler in endingHandlers)
            {
                handler(this, e);
            }
        }
    }
}

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    /// <summary>
    /// Предоставляет информацию о событии смены очереди хода
    /// </summary>
    public class TurnChangedArgs
    {
        /// <summary>
        /// Номер сделанного хода
        /// </summary>
        public int Move { get; }

        /// <summary>
        /// Игрок, сделавший ход
        /// </summary>
        public Participants MovedPlayer { get; }

        /// <summary>
        /// Был ли последний ход пропущен
        /// </summary>
        public bool IsSkipped { get; }

        /// <summary>
        /// Создает объект, содержащий информацию о событии смены очереди хода
        /// </summary>
        /// <param name="move">Номер сделанного хода</param>
        /// <param name="movedPlayer">Игрок, сделавший ход</param>
        /// <param name="isSkipped">Был ли последний ход пропущен</param>
        public TurnChangedArgs(int move, Participants movedPlayer, bool isSkipped)
        {
            Move = move;
            MovedPlayer = movedPlayer;
            IsSkipped = isSkipped;
        }
    }
}