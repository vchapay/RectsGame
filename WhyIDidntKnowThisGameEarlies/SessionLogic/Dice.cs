using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    /// <summary>
    /// Класс игральной кости
    /// </summary>
    public class Dice
    {
        /// <summary>
        /// Последнее сгенерированное значение
        /// </summary>
        public Size LastValue { get; private set; }

        private Random random;

        public Dice() 
        {
            random = new Random();
        }

        /// <summary>
        /// Генерирует два новых случайных числа от 1 до 6
        /// </summary>
        /// <returns> Структура Size, содержащая два сгенерированных числа </returns>
        public Size Generate()
        {
            Size result = new Size();

            result.Width = random.Next(1, 7);
            result.Height = random.Next(1, 7);

            LastValue = result;
            return result;
        }
    }
}
