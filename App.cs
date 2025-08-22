using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Reflection;

namespace RevitTest2
{
    /// <summary>
    /// Внешнее приложение Revit для создания пользовательского интерфейса
    /// </summary>
    public class App : IExternalApplication
    {
        #region Поля и константы

        /// <summary>
        /// Путь к текущей сборке
        /// </summary>
        private static readonly string ExecutingAssemblyPath = Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// Название вкладки на ленте Revit
        /// </summary>
        private const string TabName = "Построение стен";

        /// <summary>
        /// Название панели на вкладке
        /// </summary>
        private const string PanelName = "Работа с помещениями";

        #endregion

        #region Методы IExternalApplication

        /// <summary>
        /// Вызывается при запуске Revit
        /// </summary>
        /// <param name="application">UI-контролируемое приложение Revit</param>
        /// <returns>Результат выполнения операции</returns>
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                CreateRibbonInterface(application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", $"Не удалось создать панель: {ex.Message}");
                return Result.Failed;
            }
        }

        /// <summary>
        /// Вызывается при завершении работы Revit
        /// </summary>
        /// <param name="application">UI-контролируемое приложение Revit</param>
        /// <returns>Результат выполнения операции</returns>
        public Result OnShutdown(UIControlledApplication application)
        {
            // Очистка ресурсов при необходимости
            return Result.Succeeded;
        }

        #endregion

        #region Методы создания интерфейса

        /// <summary>
        /// Создает пользовательский интерфейс на ленте Revit
        /// </summary>
        /// <param name="application">UI-контролируемое приложение Revit</param>
        private void CreateRibbonInterface(UIControlledApplication application)
        {
            // Создаем или получаем вкладку на ленте
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (Exception)
            {
                // Вкладка уже существует - это нормально
            }

            // Создаем панель на вкладке
            RibbonPanel panel = application.CreateRibbonPanel(TabName, PanelName);

            // Создаем и настраиваем кнопку
            PushButtonData buttonData = CreateButtonData();

            // Добавляем кнопку на панель
            PushButton pushButton = panel.AddItem(buttonData) as PushButton;
        }

        /// <summary>
        /// Создает и настраивает данные для кнопки
        /// </summary>
        /// <returns>Данные для создания кнопки</returns>
        private PushButtonData CreateButtonData()
        {
            PushButtonData buttonData = new PushButtonData(
                "CreateWallsButton", // Уникальное имя кнопки
                "Построить стены",   // Текст на кнопке
                ExecutingAssemblyPath, // Путь к сборке
                "RevitTest2.HelloWorld" // Полное имя класса команды
            );

            // Настраиваем кнопку
            buttonData.ToolTip = "Построение стен по границам помещений со смещением внутрь";
            buttonData.LongDescription = "Плагин для автоматического создания стен по границам выбранных помещений со смещением внутрь на половину толщины стены";

            // Загружаем иконки
            buttonData.Image = LoadImage("icon16.png"); // Иконка 16x16
            buttonData.LargeImage = LoadImage("icon32.png"); // Иконка 32x32

            return buttonData;
        }

        #endregion

        #region Вспомогательные методы

        /// <summary>
        /// Загружает изображение из ресурсов сборки
        /// </summary>
        /// <param name="imageName">Имя файла изображения</param>
        /// <returns>Загруженное изображение или null в случае ошибки</returns>
        private System.Windows.Media.ImageSource LoadImage(string imageName)
        {
            try
            {
                // Получаем текущую сборку
                Assembly assembly = Assembly.GetExecutingAssembly();

                // Формируем имя ресурса
                string resourceName = $"{assembly.GetName().Name}.Resources.{imageName}";

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
                // В случае ошибки возвращаем null
            }

            return null;
        }

        #endregion
    }
}