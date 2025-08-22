using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB.Architecture;
using System.Windows;

namespace RevitTest2.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly CreateWallsEventHandler _eventHandler;
        private readonly ExternalEvent _externalEvent;

        public ObservableCollection<RoomItem> AllRooms { get; } = new ObservableCollection<RoomItem>();

        private bool _selectAll;
        public bool SelectAll
        {
            get => _selectAll;
            set
            {
                if (SetField(ref _selectAll, value))
                {
                    // Уведомляем об изменении, но не меняем элементы здесь
                    // Это предотвратит циклическое обновление
                    OnPropertyChanged(nameof(SelectAll));
                }
            }
        }

        public RelayCommand SelectAllCommand { get; }
        public RelayCommand DeselectAllCommand { get; }
        public RelayCommand CreateWallsCommand { get; }

        public MainViewModel(UIDocument uiDoc, CreateWallsEventHandler eventHandler, ExternalEvent externalEvent)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _eventHandler = eventHandler;
            _externalEvent = externalEvent;

            // Загружаем все помещения при инициализации
            LoadAllRooms();

            // Подписываемся на изменения выбора
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

            SelectAllCommand = new RelayCommand(OnSelectAll);
            DeselectAllCommand = new RelayCommand(OnDeselectAll);
            CreateWallsCommand = new RelayCommand(OnCreateWalls, CanCreateWalls);
        }

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

        private void OnSelectAll(object parameter)
        {
            foreach (var roomItem in AllRooms)
            {
                roomItem.IsSelected = true;
            }
            SelectAll = true;
        }

        private void OnDeselectAll(object parameter)
        {
            foreach (var roomItem in AllRooms)
            {
                roomItem.IsSelected = false;
            }
            SelectAll = false;
        }
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

        private void OnCreateWalls(object parameter)
        {
            // Получаем выбранные помещения
            var selectedRooms = AllRooms
                .Where(item => item.IsSelected)
                .Select(item => item.Room)
                .ToList();

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

        private bool CanCreateWalls(object parameter)
        {
            // Разрешаем создание стен, если есть выбранные помещения
            return AllRooms.Any(item => item.IsSelected);
        }
    }
}