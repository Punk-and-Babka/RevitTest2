using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitTest2
{
    /// <summary>
    /// Фильтр выбора элементов, позволяющий выбирать только помещения (Room)
    /// </summary>
    public class RoomFilter : ISelectionFilter
    {
        #region Методы ISelectionFilter

        /// <summary>
        /// Определяет, можно ли выбрать элемент
        /// </summary>
        /// <param name="elem">Проверяемый элемент</param>
        /// <returns>True, если элемент является помещением (Room), иначе False</returns>
        public bool AllowElement(Element elem)
        {
            return elem is Room;
        }

        /// <summary>
        /// Определяет, можно ли выбрать ссылку на элемент
        /// </summary>
        /// <param name="reference">Ссылка на элемент</param>
        /// <param name="position">Позиция выбора</param>
        /// <returns>Всегда возвращает False, так как разрешен только выбор элементов, а не ссылок</returns>
        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }

        #endregion
    }
}