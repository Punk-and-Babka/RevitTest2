using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitTest2
{
    public class App : IExternalApplication
    {
        // Путь к сборке
        static string ExecutingAssemblyPath = Assembly.GetExecutingAssembly().Location;

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                // Создаем вкладку "Надстройки" если её нет
                string tabName = "Построение стен";
                try
                {
                    application.CreateRibbonTab(tabName);
                }
                catch (Exception)
                {
                    // Вкладка уже существует - это нормально
                }

                // Создаем панель на вкладке
                RibbonPanel panel = application.CreateRibbonPanel(tabName, "Работа с помещениями");

                // Создаем кнопку
                PushButtonData buttonData = new PushButtonData(
                    "CreateWallsButton", // Уникальное имя кнопки
                    "Построить стены",   // Текст на кнопке
                    ExecutingAssemblyPath, // Путь к сборке
                    "RevitTest2.HelloWorld" // Полное имя класса команды
                );

                // Настраиваем кнопку
                buttonData.ToolTip = "Построение стен по границам помещений со смещением внутрь";
                buttonData.LongDescription = "Плагин для автоматического создания стен по границам выбранных помещений со смещением внутрь на половину толщины стены";
                buttonData.Image = LoadImage("icon16.png"); // Иконка 16x16
                buttonData.LargeImage = LoadImage("icon32.png"); // Иконка 32x32

                // Добавляем кнопку на панель
                PushButton pushButton = panel.AddItem(buttonData) as PushButton;

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", $"Не удалось создать панель: {ex.Message}");
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // Ничего особенного не делаем при закрытии
            return Result.Succeeded;
        }

        // Метод для загрузки изображений из ресурсов
        private System.Windows.Media.ImageSource LoadImage(string imageName)
        {
            try
            {
                // Получаем текущую сборку
                Assembly assembly = Assembly.GetExecutingAssembly();

                // Формируем имя ресурса
                string resourceName = assembly.GetName().Name + ".Resources." + imageName;

                // Получаем поток ресурса
                Stream stream = assembly.GetManifestResourceStream(resourceName);

                if (stream != null)
                {
                    // Создаем изображение из потока
                    System.Windows.Media.Imaging.BitmapImage image = new System.Windows.Media.Imaging.BitmapImage();
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.EndInit();
                    return image;
                }
            }
            catch
            {
                // Если не удалось загрузить изображение, возвращаем null
            }

            return null;
        }
    }
}