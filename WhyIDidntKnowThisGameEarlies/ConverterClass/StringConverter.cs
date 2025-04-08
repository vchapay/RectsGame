using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WhyIDidntKnowThisGameEarlier.SessionLogic;
using static OfficeOpenXml.ExcelErrorValue;

namespace WhyIDidntKnowThisGameEarlies.ConverterClass
{
    /// <summary>
    /// Предоставляет методы преобразования строк в сложные объекты
    /// </summary>
    public static class StringConverter
    {

        /// <summary>
        /// Преобразует строку в объект перечисления Participants.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Participants ToParticipant(string value)
        {
            Participants gameMode;

            try
            {
                gameMode = (Participants)int.Parse(value);
                if (gameMode == 0)
                    gameMode = Participants.Creator;

                return gameMode;
            }

            catch
            {
                switch (value)
                {
                    case "Client":
                        return Participants.Client;
                    default:
                        return Participants.Creator;
                }
            }
        }

        /// <summary>
        /// Преобразует строку в объект перечисления GameModes.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static GameModes ToGameMode(string value)
        {
            GameModes gameMode;

            try
            {
                gameMode = (GameModes)int.Parse(value);
                if (gameMode == 0)
                    gameMode = GameModes.StreakRule;

                return gameMode;
            }

            catch 
            {
                switch (value)
                {
                    case "AccumulationRule":
                        return GameModes.AccumulationRule;
                    default:
                        return GameModes.StreakRule;
                }
            }
        }

        /// <summary>
        /// Пробует преобразовать заданную строку в логическое значение
        /// </summary>
        /// <param name="value"></param>
        /// <returns> True или false, если преобразование произошло успешно, в противном
        /// случае false</returns>
        public static bool ToBoolean(string value)
        {
            if (value == null)
                return false;

            value = PrepareString(value);
            value = value.ToUpper();

            if (value.Equals("ИСТИНА"))
                return true;

            return false;
        }

        /// <summary>
        /// Пробует преобразовать заданную строку в структуру Size
        /// </summary>
        /// <param name="value"></param>
        /// <returns> Полученная после преобразования структура. 
        /// В противном случае пустая структура </returns>
        public static Size ToSize(string value)
        {
            if (value == null)
                return Size.Empty;

            value = PrepareString(value);

            try
            {
                int[] results = IntParse(value, 2);
                return new Size(results[0], results[1]);
            }

            catch { return Size.Empty; }
        }

        /// <summary>
        /// Пробует преобразовать заданную строку в структуру SizeF
        /// </summary>
        /// <param name="value"></param>
        /// <returns> Полученная после преобразования структура. 
        /// В противном случае пустая структура </returns>
        public static SizeF ToSizeF(string value)
        {
            if (value == null)
                return SizeF.Empty;

            value = PrepareString(value);

            try
            {
                float[] results = Parse(value, 2);
                return new SizeF(results[0], results[1]);
            }

            catch { return SizeF.Empty; }
        }

        /// <summary>
        /// Пробует преобразовать заданную строку в структуру RectangleF
        /// </summary>
        /// <param name="value"></param>
        /// <returns> Полученная после преобразования структура. 
        /// В противном случае пустая структура </returns>
        public static RectangleF ToRectangleF(string value)
        {
            if (value == null)
                return RectangleF.Empty;

            value = PrepareString(value);

            try
            {
                float[] results = Parse(value, 4);
                return new RectangleF(results[0], results[1], results[2], results[3]);
            }

            catch { return RectangleF.Empty; }
        }

        /// <summary>
        /// Пробует преобразовать заданную строку в структуру Rectangle
        /// </summary>
        /// <param name="value"></param>
        /// <returns> Полученная после преобразования структура. 
        /// В противном случае пустая структура </returns>
        public static Rectangle ToRectangle(string value)
        {
            if (value == null)
                return Rectangle.Empty;
            value = PrepareString(value);

            try
            {
                int[] results = IntParse(value, 4);
                return new Rectangle(results[0], results[1], results[2], results[3]);
            }

            catch { return Rectangle.Empty; }
        }

        private static string PrepareString(string value)
        {
            value = value.Replace('{', '\0');
            value = value.Replace('}', '\0');
            value = value.Replace(' ', '\0');
            value = value.Trim();
            return value;
        }

        private static float[] Parse(string value, int count)
        {
            float[] results = new float[count];

            for (int i = 0; i < count; i++)
            {
                results[i] = ParsePart(ref value);
            }

            return results;
        }

        private static int[] IntParse(string value, int count)
        {
            int[] results = new int[count];

            for (int i = 0; i < count; i++)
            {
                results[i] = (int)ParsePart(ref value);
            }

            return results;
        }

        private static float ParsePart(ref string value)
        {
            string strPartOfValue;
            int firstInd = value.IndexOf('=');
            int endingInd = value.IndexOf(',');

            if (firstInd == -1)
            {
                return 0;
            }

            firstInd++;
            if (endingInd == -1)
            {
                strPartOfValue = value.Substring(firstInd);
                value = string.Empty;
                return float.Parse(strPartOfValue);
            }

            int lenght = endingInd - firstInd;

            if (lenght < 1)
            {
                return 0;
            }

            strPartOfValue = value.Substring(firstInd, lenght);
            value = value.Substring(endingInd + 1);
            return float.Parse(strPartOfValue);
        }
    }
}
