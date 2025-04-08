using System;
using System.CodeDom;
using System.Drawing;
using WhyIDidntKnowThisGameEarlier.MapLogic;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    /// <summary>
    /// Представляет базовую инфоромацию, необходимую для конструирования реплея
    /// </summary>
    public class ReplayBase
    {
        MapInterface mapInterface;

        public ReplayBase(string id, DateTime endingTime) 
        {
            ID = id;
            EndingTime = endingTime;
        }

        public string ID { get; }

        public DateTime EndingTime { get; }

        public bool IsStartPositionSet => mapInterface != null && mapInterface.IsReady;

        public string Name { get; set; }

        /// <summary>
        /// Устанавливает стартовое положение в реплее
        /// </summary>
        /// <param name="startCreatorRect"></param>
        /// <param name="startClientRect"></param>
        /// <param name="mapSize"></param>
        public void SetStartPosition(Size mapSize, Rectangle startCreatorRect, Rectangle startClientRect)
        {
            mapInterface = new MapInterface(mapSize);
            mapInterface.SetStartPosition(startClientRect.Size, startCreatorRect.Size);
        }

        /// <summary>
        /// Возвращает объект MapInterface, представляющий карту со стартовым положением
        /// </summary>
        /// <returns></returns>
        public MapInterface GetStartPosition()
        {
            if (!IsStartPositionSet)
                throw new InvalidOperationException("Стартовое положение еще не было установлено");

            return mapInterface.Clone();
        }
    }
}