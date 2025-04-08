using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WhyIDidntKnowThisGameEarlier.MapLogic;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    /// <summary>
    /// Представляет бота для сессии
    /// </summary>
    class AIBot
    {
        private Session _session;
        private Participants role;
        private bool isEnabled;

        private int timeToMove;
        private const int defaultMaxMoveTime = 500;
        private const int minPossibleMoveTime = 99;
        private int minTimeToMove;
        private int maxTimeToMove;

        private Grid grid;
        private Random random;
        private Task task;

        /// <summary>
        /// Создает бота с пустой ссылкой на сессию
        /// </summary>
        public AIBot()
        {
            random = new Random();
            minTimeToMove = minPossibleMoveTime;
            maxTimeToMove = defaultMaxMoveTime;
        }

        /// <summary>
        /// Возвращает роль бота в последней или текущей сессии
        /// </summary>
        public Participants Role => role;

        /// <summary>
        /// Максимальное время на ход (в миллисекундах)
        /// </summary>
        public int MaxTimeToMove
        {
            get { return maxTimeToMove; }
            set
            {
                if (value > _session?.MoveDuration * 1000)
                    return;

                if (value <= minTimeToMove) 
                    return;

                maxTimeToMove = value; 
            }
        }

        /// <summary>
        /// Минимальное время на ход (в миллисекундах)
        /// </summary>
        public int MinTimeToMove
        {  
            get { return minTimeToMove; } 
            set
            {
                if (value < minPossibleMoveTime)
                    return;

                if (value >= maxTimeToMove)
                    return;

                minTimeToMove = value;
            } 
        }

        /// <summary>
        /// Присоединяет бота к заданной существующей сессии.
        /// </summary>
        /// <param name="session"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Join(Session session)
        {
            if (session == null)
                throw new ArgumentNullException("Попытка присоединения к несуществующей сессии");

            JoinSession(session);

            role = Participants.Client;
        }

        /// <summary>
        /// Создает сессию и присоединяет бота как создателя
        /// </summary>
        /// <returns> Сессия со стандартным набором правил, готовая к запуску </returns>
        public Session CreateSession()
        {
            Session session = new Session();
            session.InitialzeMap();
            JoinSession(session);
            role = Participants.Creator;
            return session;
        }

        /// <summary>
        /// Включает бота
        /// </summary>
        public void Activate()
        {
            if (_session == null)
                throw new InvalidOperationException("Нельзя активировать бота, " +
                    "пока для него не определена сессия");

            if (isEnabled)
                return;

            isEnabled = true;
            Run();
        }

        /// <summary>
        /// Выключает бота
        /// </summary>
        public void Disable()
        {
            isEnabled = false;
        }

        private void Run()
        {
            task = Task.Run(() =>
            {
                while (isEnabled)
                {
                    if (_session.Turn != role)
                        continue;

                    ImitateTimeToMove();

                    Thread.Sleep(timeToMove);
                    TryMakeMove();
                }
            });
        }

        private void TryMakeMove()
        {
            for (int x = _session.MapInterface.Size.Width; x >= 0; x--)
            {
                for (int y = _session.MapInterface.Size.Height; y >= 0; y--)
                {
                    int X = x * grid.CellSize + _session.MapDrawer.ImagePosition.X;
                    int Y = y * grid.CellSize + _session.MapDrawer.ImagePosition.Y;

                    _session.ToMousePosition(new System.Drawing.Point(X, Y));

                    if (_session.TryMakeMove())
                        return;
                }
            }

            _session.SkipMove();
        }

        private void ImitateTimeToMove()
        {
            timeToMove = random.Next(minTimeToMove, maxTimeToMove);
        }

        private void JoinSession(Session session)
        {
            _session = session;
            if (MaxTimeToMove > _session.MoveDuration * 1000)
                MaxTimeToMove = _session.MoveDuration * 1000;

            grid = _session.MapDrawer.Grid;
        }
    }
}
