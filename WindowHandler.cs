using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTest2
{
    public class WindowHandler
    {
        public void Show(Window window)
        {
            // Создаем новый поток для окна, чтобы не блокировать Revit
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                window.Show();

                // Запускаем диспетчер сообщений для окна
                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
        }
    }
}