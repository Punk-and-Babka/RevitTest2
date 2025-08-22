using Autodesk.Revit.DB.Architecture;
using RevitTest2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTest2
{
    /// <summary>
    /// Класс-обертка для помещения (Room) с поддержкой выбора
    /// </summary>
    public class RoomItem : BaseViewModel
    {
        #region Поля

        private bool _isSelected;

        #endregion

        #region Свойства

        /// <summary>
        /// Помещение Revit
        /// </summary>
        public Room Room { get; }

        /// <summary>
        /// Флаг, указывающий выбрано ли помещение
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        #endregion

        #region Конструктор

        /// <summary>
        /// Инициализирует новый экземпляр RoomItem
        /// </summary>
        /// <param name="room">Помещение Revit</param>
        public RoomItem(Room room)
        {
            Room = room;
            IsSelected = false;
        }

        #endregion
    }
}