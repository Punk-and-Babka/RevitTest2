using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTest2
{
    public class CreateWallsEventHandler : IExternalEventHandler
    {
        private List<Room> _rooms;
        private Document _doc;
        private string _statusMessage;

        public void SetData(Document doc, List<Room> rooms)
        {
            _doc = doc;
            _rooms = rooms;
        }

        public void Execute(UIApplication app)
        {
            try
            {
                using (Transaction transaction = new Transaction(_doc, "Создание стен по помещениям"))
                {
                    transaction.Start();

                    WallType wallType = new FilteredElementCollector(_doc)
                        .OfClass(typeof(WallType))
                        .FirstElement() as WallType;

                    if (wallType == null)
                    {
                        TaskDialog.Show("Ошибка", "В проекте нет типов стен.");
                        transaction.RollBack();
                        return;
                    }

                    double wallHalfThickness = wallType.Width / 2.0;
                    int wallsCreated = 0;

                    foreach (Room room in _rooms)
                    {
                        Level level = _doc.GetElement(room.LevelId) as Level;
                        SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
                        IList<IList<BoundarySegment>> boundarySegmentsList = room.GetBoundarySegments(options);

                        if (boundarySegmentsList == null || boundarySegmentsList.Count == 0)
                            continue;

                        foreach (IList<BoundarySegment> boundarySegments in boundarySegmentsList)
                        {
                            foreach (BoundarySegment segment in boundarySegments)
                            {
                                Curve curve = segment.GetCurve();
                                Curve offsetCurve = OffsetCurveInward(curve, wallHalfThickness, room);

                                if (offsetCurve != null)
                                {
                                    Wall wall = Wall.Create(_doc, offsetCurve, wallType.Id, level.Id, 3.0, 0, false, false);
                                    if (wall != null)
                                        wallsCreated++;
                                }
                            }
                        }
                    }

                    transaction.Commit();
                    TaskDialog.Show("Успех", $"Успешно создано {wallsCreated} стен");
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", $"Ошибка при создании стен: {ex.Message}");
            }
        }

        public string GetName()
        {
            return "Create Walls Event Handler";
        }

        private Curve OffsetCurveInward(Curve curve, double offsetDistance, Room room)
        {
            if (!(curve is Line line))
                return null;

            try
            {
                XYZ direction = line.Direction.Normalize();
                XYZ perpendicular = new XYZ(-direction.Y, direction.X, 0).Normalize();

                BoundingBoxXYZ bbox = room.get_BoundingBox(null);
                XYZ roomCenter = (bbox.Min + bbox.Max) / 2.0;

                XYZ curveMidPoint = curve.Evaluate(0.5, true);
                XYZ toCenter = (roomCenter - curveMidPoint).Normalize();

                double dotProduct = perpendicular.DotProduct(toCenter);
                XYZ offsetDirection = (dotProduct > 0) ? perpendicular : -perpendicular;

                return curve.CreateOffset(offsetDistance, offsetDirection);
            }
            catch
            {
                return null;
            }
        }
    }
}