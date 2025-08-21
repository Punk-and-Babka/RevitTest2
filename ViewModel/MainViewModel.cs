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

        public ObservableCollection<Room> SelectedRooms { get; } = new ObservableCollection<Room>();

        public RelayCommand SelectRoomsCommand { get; }
        public RelayCommand CreateWallsCommand { get; }

        public MainViewModel(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;

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
            // Здесь будет логика создания стен
            MessageBox.Show("позже");
        }

        private bool CanCreateWalls(object parameter)
        {
            // Разрешаем создание стен только если есть выбранные помещения
            return SelectedRooms.Any();
        }
    }
}