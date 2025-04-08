using System;
using System.Drawing;
using System.Reflection;
using WhyIDidntKnowThisGameEarlies.ConverterClass;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    /// <summary>
    /// Объект, описывающий ход сессии
    /// </summary>
    public class Move
    {
        /// <summary>
        /// Создает новый объект, хранящий данные о ходе
        /// </summary>
        /// <param name="movedPlayer"> Игрок, сделавший ход </param>
        /// <param name="timeSpent"> Время, затраченное на ход </param>
        /// <param name="isSkipped"> Был ли ход пропущен </param>
        /// <param name="suppliedRectangle"> Прямоугольник, который был добавлен </param>
        public Move(int index, Participants movedPlayer, int timeSpent, Rectangle suppliedRectangle,
            bool isSkipped, int skippedMovesCounter, int creatorPoints, int clientPoints)
        {
            Index = index;
            MovedPlayer = movedPlayer;
            TimeSpent = timeSpent;
            IsSkipped = isSkipped;
            CreatorPoints = creatorPoints;
            ClientPoints = clientPoints;
            SuppliedRectangle = suppliedRectangle;
            SkippedMovesCounter = skippedMovesCounter;
        }

        /// <summary>
        /// Возвращает объект Move, описывающий пустой ход (например, когда еще не был сделан
        /// ход ни одним из игроков)
        /// </summary>
        public static Move Empty => new Move(0, 0, 0, Rectangle.Empty, false, 0, 0, 0);

        /// <summary>
        /// Количество очков создателя сессии на момент текущего хода
        /// </summary>
        public int CreatorPoints { get; }

        /// <summary>
        /// Количество очков клиента сесиии на момент текущего хода
        /// </summary>
        public int ClientPoints { get; }

        /// <summary>
        /// Возвращает порядковый номер хода
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Игрок, сделавший ход
        /// </summary>
        public Participants MovedPlayer { get; }

        /// <summary>
        /// Время, затраченное на ход
        /// </summary>
        public int TimeSpent { get; }

        /// <summary>
        /// Был ли ход пропущен
        /// </summary>
        public bool IsSkipped { get; }

        /// <summary>
        /// Возвращает прямоугольник, который был добавлен, либо пустую структуру Rectangle,
        /// если ход был пропущен
        /// </summary>
        public Rectangle SuppliedRectangle { get; }

        /// <summary>
        /// Значение специального счетчика пропущенных ходов на момент хода
        /// </summary>
        public int SkippedMovesCounter { get; }

        /// <summary>
        /// Возвращает строку, полно представляющую информацию о ходе
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string value;

            if (IsSkipped)
                value = $"{Index}. {MovedPlayer}: +0 {{({SuppliedRectangle.Width}, {SuppliedRectangle.Height}) => (-)}}";
            else
                value = $"{Index}. {MovedPlayer}: +{SuppliedRectangle.Width * SuppliedRectangle.Height} {{({SuppliedRectangle.Width}, {SuppliedRectangle.Height}) => ({SuppliedRectangle.Location.X}, {SuppliedRectangle.Location.Y})}}";

            return value;
        }

        /// <summary>
        /// Возвращает данные этого объекта в виде строки для передачи его, например, в файловый поток
        /// </summary>
        /// <returns> Строка, содержащая данные в формате: 
        /// порядковый номер, походивший игрок, затраченное время, был ли ход пропущен,
        /// прямоугольник, очки создателя, очки клиента, счетчик пропущенных ходов. Запятая
        /// является разделителем данных.
        /// </returns>
        public string ToFullString()
        {
            return $"{Index}; {(int)MovedPlayer}; {TimeSpent}; {Convert.ToInt32(IsSkipped)}; {SuppliedRectangle}; {CreatorPoints}; {ClientPoints}; {SkippedMovesCounter}";
        }

        /// <summary>
        /// Пробует конвертировать строку в объект хода
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Move move)
        {
            value = value.Remove(' ', '\0');
            string[] info = value.Split(';');

            if (info.Length != 8)
            {
                move = Empty;
                return false;
            }

            int intVal;
            int index = int.TryParse(info[0], out intVal) ? intVal : 0;
            int player = int.TryParse(info[1], out intVal) ? intVal : 0;
            int time = int.TryParse(info[2], out intVal) ? intVal : 60;
            bool skipped = int.TryParse(info[3], out intVal) && Convert.ToBoolean(intVal);
            Rectangle rect = StringConverter.ToRectangle(info[4]);
            int cr = int.TryParse(info[5], out intVal) ? intVal : 0;
            int cl = int.TryParse(info[6], out intVal) ? intVal : 0;
            int counter = int.TryParse(info[7], out intVal) ? intVal : 0;

            move = new Move(index, (Participants)player, time, rect, skipped, counter, cr, cl);
            return true;
        }

        public static bool operator ==(Move operand1, Move operand2)
        {
            return operand1.Equals(operand2);
        }

        public static bool operator !=(Move operand1, Move operand2)
        {
            return !operand1.Equals(operand2);
        }

        /// <summary>
        /// Сравнивает данные ходов и определяет, есть ли полное совпадение
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Move))
                return false;

            Move o = obj as Move;

            return Index == o.Index && MovedPlayer == o.MovedPlayer &&
                SuppliedRectangle == o.SuppliedRectangle && TimeSpent == o.TimeSpent &&
                IsSkipped == o.IsSkipped && CreatorPoints == o.CreatorPoints &&
                ClientPoints == o.ClientPoints && SkippedMovesCounter == o.SkippedMovesCounter;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}