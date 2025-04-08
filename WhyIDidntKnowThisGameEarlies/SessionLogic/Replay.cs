using OfficeOpenXml.FormulaParsing.Excel.Operators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WhyIDidntKnowThisGameEarlier.MapLogic;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    public class Replay
    {
        readonly List<Move> moves;
        int currentMove;

        public Replay(Session session)
        {
            ID = session.ID;
            Name = string.Empty;
            MapSize = session.MapInterface.Size;
            ClientStartRectangle = session.MapInterface.ClientStartRectangle;
            CreatorStartRectangle = session.MapInterface.CreatorStartRectangle;
            Rule = new Rule(session);
            currentMove = 0;

            moves = new List<Move>(1)
            {
                Move.Empty
            };
            moves.AddRange(session.Moves);
            EndingTime = session.EndingTime;
        }

        private Replay(ReplayBase repBase, Rule rule, List<Move> moves)
        {
            this.moves = moves;
            ID = repBase.ID;
            Name = repBase.Name;
            EndingTime = repBase.EndingTime;
            var map = repBase.GetStartPosition();
            MapSize = map.Size;
            ClientStartRectangle = map.ClientStartRectangle;
            CreatorStartRectangle = map.CreatorStartRectangle;
            Rule = rule;

            currentMove = 0;
        }

        /// <summary>
        /// Возвращает или задает отображающийся полуход
        /// </summary>
        public int CurrentHalfMove
        {
            get { return currentMove; }
            set { GoToHalfMove(value); }
        }

        /// <summary>
        /// Задает или возвращает имя реплея
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Количество полуходов, записанных в реплее
        /// </summary>
        public int Lenght => moves.Count;

        /// <summary>
        /// Общее число пропущенных ходов за сессию
        /// </summary>
        public int SkippedMoves => moves.Where(m => m.IsSkipped == true).Count();

        /// <summary>
        /// Время, когда записанная сессия завершилась
        /// </summary>
        public DateTime EndingTime { get; }

        /// <summary>
        /// Размер карты записанной сессии
        /// </summary>
        public Size MapSize { get; }

        /// <summary>
        /// Идентификатор записанной сессии
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Возвращает объект, содержащий все правила записанной сессии
        /// </summary>
        public Rule Rule { get; }

        /// <summary>
        /// Стартовый прямоугольник второго игрока (клиента)
        /// </summary>
        public Rectangle ClientStartRectangle { get; }

        /// <summary>
        /// Количество очков второго игрока (клиента) в стартовой позиции
        /// </summary>
        public int StartClientPoints => ClientStartRectangle.Width * ClientStartRectangle.Height;

        /// <summary>
        /// Стратовый прямогоульник первого игрока (создателя)
        /// </summary>
        public Rectangle CreatorStartRectangle { get; }

        /// <summary>
        /// Количество очков первого игрока (создателя) в стартовой позиции
        /// </summary>
        public int StartCreatorPoints => CreatorStartRectangle.Width * CreatorStartRectangle.Height;

        /// <summary>
        /// Определяет, записан ли в реплей хоть один ход
        /// </summary>
        public bool IsEmpty => moves.Count == 1;

        /// <summary>
        /// Переводит указатель к заданному полуходу, если такой полуход существует в списке ходов.
        /// Иначе переносит указатель к ближайшему крайнему полуходу в списке.
        /// </summary>
        /// <param name="moveInd"> Номер хода </param>
        /// <param name="turn"> Игрок, сделавший ход </param>
        public void GoToHalfMove(int moveInd, Participants turn)
        {
            int targetIndex = moveInd * 2;
            if (turn == Participants.Creator) targetIndex -= 1;
            GoToHalfMove(targetIndex);
        }

        /// <summary>
        /// Переводит указатель к заданному полуходу, если такой полуход существует в списке ходов.
        /// Иначе переносит указатель к ближайшему крайнему полуходу в списке.
        /// </summary>
        /// <param name="halfMoveInd"> Номер полухода </param>
        /// <exception cref="InvalidOperationException"></exception>
        public void GoToHalfMove(int halfMoveInd)
        {
            if (moves.Count == 1)
                return;

            if (halfMoveInd < 0)
                currentMove = 0;

            else if (halfMoveInd >= moves.Count)
                currentMove = moves.Count - 1;

            else currentMove = halfMoveInd;
        }

        /// <summary>
        /// Возвращает объект Move, описывающий ход, на котором в данный момент находится указатель
        /// </summary>
        /// <returns></returns>
        public Move GetMove() => moves[currentMove];

        /// <summary>
        /// Возвращает список записанных ходов
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<Move> GetMovesList() => moves;

        /// <summary>
        /// Конструирует объект реплея по переданной информации
        /// </summary>
        /// <param name="repBase"> Объект, описывающий общую информацию о реплее (идентификатор, имя и т.д.)</param>
        /// <param name="rule"> Объект, описывающий правила записанной сессии </param>
        /// <param name="moves"> Список записанных ходов </param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Replay Construct(ReplayBase repBase, Rule rule, List<Move> moves)
        {
            if (repBase == null)
                throw new ArgumentNullException(nameof(repBase));

            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (moves == null)
                throw new ArgumentNullException(nameof(moves));

            if (moves.Count == 0)
                throw new ArgumentException("Список ходов не может быть пуст");

            if (moves[0] != Move.Empty)
                moves.Insert(0, Move.Empty);

            if (!repBase.IsStartPositionSet)
                throw new InvalidOperationException("Начальное положение не было установлено");
            
            var map = repBase.GetStartPosition();
            foreach (var move in moves)
            {
                if (move == Move.Empty)
                    continue;

                if (!map.TryAddRectangle(move.MovedPlayer, move.SuppliedRectangle))
                    throw new InvalidOperationException("В списке ходов обнаружены невозможные ходы");
            }

            return new Replay(repBase, rule, moves);
        }
    }
}
