using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB.Architecture;

namespace RevitTest2.ViewModel
{
    /// <summary>
    /// ViewModel главного окна для управления выбором помещений и созданием стен
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        #region Поля

        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly CreateWallsEventHandler _eventHandler;
        private readonly ExternalEvent _externalEvent;
        private bool _selectAll;

        #endregion

        #region Свойства

        /// <summary>
        /// Коллекция всех помещений в проекте с флагами выбора
        /// </summary>
        public ObservableCollection<RoomItem> AllRooms { get; } = new ObservableCollection<RoomItem>();

        /// <summary>
        /// Флаг, указывающий выбраны ли все помещения
        /// </summary>
        public bool SelectAll
        {
            get => _selectAll;
            set
            {
                if (SetField(ref _selectAll, value))
                {
                    OnPropertyChanged(nameof(SelectAll));
                }
            }
        }

        #endregion

        #region Команды

        /// <summary>
        /// Команда для выбора всех помещений
        /// </summary>
        public RelayCommand SelectAllCommand { get; }

        /// <summary>
        /// Команда для снятия выделения со всех помещений
        /// </summary>
        public RelayCommand DeselectAllCommand { get; }

        /// <summary>
        /// Команда для создания стен по выбранным помещениям
        /// </summary>
        public RelayCommand CreateWallsCommand { get; }

        #endregion

        #region Конструктор

        /// <summary>
        /// Инициализирует новый экземпляр MainViewModel
        /// </summary>
        /// <param name="uiDoc">UI-документ Revit</param>
        /// <param name="eventHandler">Обработчик событий для создания стен</param>
        /// <param name="externalEvent">Внешнее событие для выполнения в основном потоке Revit</param>
        public MainViewModel(UIDocument uiDoc, CreateWallsEventHandler eventHandler, ExternalEvent externalEvent)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _eventHandler = eventHandler;
            _externalEvent = externalEvent;

            // Загружаем все помещения при инициализации
            LoadAllRooms();

            // Подписываемся на изменения выбора
            SubscribeToRoomSelectionChanges();

            // Инициализируем команды
            SelectAllCommand = new RelayCommand(OnSelectAll);
            DeselectAllCommand = new RelayCommand(OnDeselectAll);
            CreateWallsCommand = new RelayCommand(OnCreateWalls, CanCreateWalls);
        }

        #endregion

        #region Методы загрузки данных

        /// <summary>
        /// Загружает все помещения из документа Revit
        /// </summary>
        private void LoadAllRooms()
        {
            AllRooms.Clear();

            // Получаем все помещения в проекте
            var rooms = new FilteredElementCollector(_doc)
                .OfClass(typeof(SpatialElement))
                .OfType<Room>()
                .Where(room => room.Area > 0); // Исключаем помещения с нулевой площадью

            foreach (var room in rooms)
            {
                AllRooms.Add(new RoomItem(room));
            }
        }

        /// <summary>
        /// Подписывается на события изменения выбора для всех помещений
        /// </summary>
        private void SubscribeToRoomSelectionChanges()
        {
            foreach (var roomItem in AllRooms)
            {
                roomItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(RoomItem.IsSelected))
                    {
                        UpdateSelectAllProperty();
                    }
                };
            }
        }

        #endregion

        #region Методы управления выделением

        /// <summary>
        /// Выбирает все помещения
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется)</param>
        private void OnSelectAll(object parameter)
        {
            foreach (var roomItem in AllRooms)
            {
                roomItem.IsSelected = true;
            }
            SelectAll = true;
        }

        /// <summary>
        /// Снимает выделение со всех помещений
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется)</param>
        private void OnDeselectAll(object parameter)
        {
            foreach (var roomItem in AllRooms)
            {
                roomItem.IsSelected = false;
            }
            SelectAll = false;
        }

        /// <summary>
        /// Обновляет свойство SelectAll на основе текущего состояния выбора
        /// </summary>
        private void UpdateSelectAllProperty()
        {
            if (!AllRooms.Any())
            {
                SelectAll = false;
                return;
            }

            // Проверяем, все ли элементы выбраны
            bool allSelected = AllRooms.All(item => item.IsSelected);
            bool noneSelected = AllRooms.All(item => !item.IsSelected);

            // Обновляем свойство SelectAll
            if (allSelected)
                SelectAll = true;
            else if (noneSelected)
                SelectAll = false;
            // Если смешанное состояние, оставляем как есть
        }

        #endregion

        #region Методы создания стен

        /// <summary>
        /// Выполняет создание стен для выбранных помещений
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется)</param>
        private void OnCreateWalls(object parameter)
        {
            // Получаем выбранные помещения
            var selectedRooms = GetSelectedRooms();

            if (!selectedRooms.Any())
            {
                MessageBox.Show("Не выбрано ни одного помещения");
                return;
            }

            try
            {
                // Устанавливаем данные для обработчика
                _eventHandler.SetData(_doc, selectedRooms);

                // Запускаем выполнение в основном потоке Revit
                _externalEvent.Raise();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании стен: {ex.Message}");
            }
        }

        /// <summary>
        /// Получает список выбранных помещений
        /// </summary>
        /// <returns>Список выбранных помещений</returns>
        private List<Room> GetSelectedRooms()
        {
            return AllRooms
                .Where(item => item.IsSelected)
                .Select(item => item.Room)
                .ToList();
        }

        /// <summary>
        /// Определяет, можно ли выполнить команду создания стен
        /// </summary>
        /// <param name="parameter">Параметр команды (не используется)</param>
        /// <returns>True если есть выбранные помещения, иначе False</returns>
        private bool CanCreateWalls(object parameter)
        {
            // Разрешаем создание стен, если есть выбранные помещения
            return AllRooms.Any(item => item.IsSelected);
        }

        #endregion
    }
}