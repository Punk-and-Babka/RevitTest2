using Autodesk.Revit.DB.Architecture;
using RevitTest2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTest2
{
    public class RoomItem : BaseViewModel
    {
        private bool _isSelected;

        public Room Room { get; }
        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public RoomItem(Room room)
        {
            Room = room;
            IsSelected = false;
        }
    }
}