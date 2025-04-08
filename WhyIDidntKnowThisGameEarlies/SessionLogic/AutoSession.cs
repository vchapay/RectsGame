using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WhyIDidntKnowThisGameEarlier.SessionLogic;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    /// <summary>
    /// Организует работу сессий для ботов
    /// </summary>
    public class AutoSession
    {
        private AIBot bot1;
        private AIBot bot2;
        private Session session;
        private Control _surface;

        public AutoSession(Control surface)
        {
            _surface = surface;
            bot1 = new AIBot();
            bot2 = new AIBot();
            bot1.MaxTimeToMove = 500;
            bot2.MaxTimeToMove = 500;
            _surface.Paint += new PaintEventHandler(DrawSession);
        }

        /// <summary>
        /// Текущая сессия
        /// </summary>
        public Session Session { get => session; }

        /// <summary>
        /// Создать и запустить новую сессию ботов
        /// </summary>
        public Session CreateNewSession()
        {
            PrepareBots();
            session.Ended += delegate
            {
                bot1.Disable();
                bot2.Disable();
            };
            session.TurnChanged += delegate 
            {
                _surface?.Invalidate(); 
            };
            StartSession();
            return session;
        }

        /// <summary>
        /// Устанавливает новую поверхность для отображения карты сессии
        /// </summary>
        /// <param name="newSurface"></param>
        public void SetNewSurface(Control newSurface)
        {
            if (newSurface == null)
                throw new ArgumentNullException();

            _surface.Paint -= new PaintEventHandler(DrawSession);
            _surface = newSurface;
            _surface.Paint += new PaintEventHandler(DrawSession);

            session.MapDrawer.FitIn(_surface.Size);
            session.MapDrawer.CenterIn(_surface.Size);
        }

        private void PrepareBots()
        {
            session = bot1.CreateSession();
            bot2.Join(session);
        }

        private void StartSession()
        {
            Task.Run(() =>
            {
                bot1.Activate();
                bot2.Activate();
                session.MapDrawer.FitIn(_surface.Size);
                session.MapDrawer.CenterIn(_surface.Size);
                session.Start();
            });
        }

        private void DrawSession(object sender, PaintEventArgs e)
        {
            _surface.Invoke((MethodInvoker)delegate
            {
                session?.Draw(e.Graphics);
            });
        }
    }
}
