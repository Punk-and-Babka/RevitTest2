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

        public ObservableCollection<Room> SelectedRooms { get; } = new ObservableCollection<Room>();

        public RelayCommand SelectRoomsCommand { get; }
        public RelayCommand CreateWallsCommand { get; }

        public MainViewModel(UIDocument uiDoc, CreateWallsEventHandler eventHandler, ExternalEvent externalEvent)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _eventHandler = eventHandler;
            _externalEvent = externalEvent;

            SelectRoomsCommand = new RelayCommand(OnSelectRooms);
            CreateWallsCommand = new RelayCommand(OnCreateWalls, CanCreateWalls);
        }

        private void OnSelectRooms(object parameter)
        {
            SelectedRooms.Clear();

            try
            {
                var pickedRefs = _uiDoc.Selection.PickObjects(
                    ObjectType.Element,
                    new RoomFilter(),
                    "Выберите помещения"
                );

                foreach (Reference reference in pickedRefs)
                {
                    if (_doc.GetElement(reference) is Room room)
                    {
                        SelectedRooms.Add(room);
                    }
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Пользователь отменил операцию выбора
            }
        }

        private void OnCreateWalls(object parameter)
        {
            if (!SelectedRooms.Any())
            {
                MessageBox.Show("Не выбрано ни одного помещения");
                return;
            }

            try
            {
                // Устанавливаем данные для обработчика
                _eventHandler.SetData(_doc, SelectedRooms.ToList());

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
            return SelectedRooms.Any();
        }
    }
}