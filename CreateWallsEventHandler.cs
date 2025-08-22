using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitTest2
{
    /// <summary>
    /// Обработчик внешних событий для создания стен по границам помещений
    /// </summary>
    public class CreateWallsEventHandler : IExternalEventHandler
    {
        #region Поля

        private List<Room> _rooms;
        private Document _doc;

        #endregion

        #region Методы IExternalEventHandler

        /// <summary>
        /// Устанавливает данные для обработки
        /// </summary>
        /// <param name="doc">Документ Revit</param>
        /// <param name="rooms">Список помещений для обработки</param>
        public void SetData(Document doc, List<Room> rooms)
        {
            _doc = doc;
            _rooms = rooms;
        }

        /// <summary>
        /// Выполняет основную логику создания стен
        /// </summary>
        /// <param name="app">UI-приложение Revit</param>
        public void Execute(UIApplication app)
        {
            try
            {
                CreateWallsForSelectedRooms();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", $"Ошибка при создании стен: {ex.Message}");
            }
        }

        /// <summary>
        /// Возвращает имя обработчика событий
        /// </summary>
        /// <returns>Имя обработчика</returns>
        public string GetName()
        {
            return "Create Walls Event Handler";
        }

        #endregion

        #region Основная логика создания стен

        /// <summary>
        /// Создает стены для выбранных помещений
        /// </summary>
        private void CreateWallsForSelectedRooms()
        {
            using (Transaction transaction = new Transaction(_doc, "Создание стен по помещениям"))
            {
                transaction.Start();

                // Получение типа стены по умолчанию
                WallType wallType = GetDefaultWallType();
                if (wallType == null)
                {
                    TaskDialog.Show("Ошибка", "В проекте нет типов стен.");
                    transaction.RollBack();
                    return;
                }

                // Создание стен для каждого помещения
                int wallsCreated = CreateWallsForRooms(wallType);

                transaction.Commit();

                // Показать результат
                TaskDialog.Show("Успех", $"Успешно создано {wallsCreated} стен");
            }
        }

        /// <summary>
        /// Получает тип стены по умолчанию
        /// </summary>
        /// <returns>Тип стены или null, если не найден</returns>
        private WallType GetDefaultWallType()
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(WallType))
                .FirstElement() as WallType;
        }

        /// <summary>
        /// Создает стены для всех выбранных помещений
        /// </summary>
        /// <param name="wallType">Тип создаваемых стен</param>
        /// <returns>Количество созданных стен</returns>
        private int CreateWallsForRooms(WallType wallType)
        {
            double wallHalfThickness = wallType.Width / 2.0;
            int wallsCreated = 0;

            foreach (Room room in _rooms)
            {
                wallsCreated += CreateWallsForRoom(room, wallType, wallHalfThickness);
            }

            return wallsCreated;
        }

        /// <summary>
        /// Создает стены для конкретного помещения
        /// </summary>
        /// <param name="room">Помещение для обработки</param>
        /// <param name="wallType">Тип создаваемых стен</param>
        /// <param name="wallHalfThickness">Половина толщины стены</param>
        /// <returns>Количество созданных стен для помещения</returns>
        private int CreateWallsForRoom(Room room, WallType wallType, double wallHalfThickness)
        {
            int wallsCreated = 0;

            // Получение уровня помещения
            Level level = _doc.GetElement(room.LevelId) as Level;
            if (level == null) return 0;

            // Получение границ помещения
            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> boundarySegmentsList = room.GetBoundarySegments(options);

            if (boundarySegmentsList == null || boundarySegmentsList.Count == 0)
                return 0;

            // Обработка каждого сегмента границы
            foreach (IList<BoundarySegment> boundarySegments in boundarySegmentsList)
            {
                foreach (BoundarySegment segment in boundarySegments)
                {
                    Curve curve = segment.GetCurve();
                    Curve offsetCurve = OffsetCurveInward(curve, wallHalfThickness, room);

                    if (offsetCurve != null)
                    {
                        // Создание стены по смещенной кривой
                        Wall wall = Wall.Create(_doc, offsetCurve, wallType.Id, level.Id, 3.0, 0, false, false);
                        if (wall != null)
                            wallsCreated++;
                    }
                }
            }

            return wallsCreated;
        }

        #endregion

        #region Геометрические операции

        /// <summary>
        /// Смещает кривую внутрь помещения на указанное расстояние
        /// </summary>
        /// <param name="curve">Исходная кривая</param>
        /// <param name="offsetDistance">Расстояние смещения</param>
        /// <param name="room">Помещение для определения направления смещения</param>
        /// <returns>Смещенная кривая или null в случае ошибки</returns>
        private Curve OffsetCurveInward(Curve curve, double offsetDistance, Room room)
        {
            // Обрабатываем только линейные сегменты
            if (!(curve is Line line))
                return null;

            try
            {
                // Вычисление направления смещения
                XYZ direction = line.Direction.Normalize();
                XYZ perpendicular = new XYZ(-direction.Y, direction.X, 0).Normalize();

                // Определение центра помещения
                BoundingBoxXYZ bbox = room.get_BoundingBox(null);
                XYZ roomCenter = (bbox.Min + bbox.Max) / 2.0;

                // Определение направления внутрь помещения
                XYZ curveMidPoint = curve.Evaluate(0.5, true);
                XYZ toCenter = (roomCenter - curveMidPoint).Normalize();

                // Определение конечного направления смещения
                double dotProduct = perpendicular.DotProduct(toCenter);
                XYZ offsetDirection = (dotProduct > 0) ? perpendicular : -perpendicular;

                // Создание смещенной кривой
                return curve.CreateOffset(offsetDistance, offsetDirection);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}