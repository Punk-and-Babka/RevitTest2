using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using RevitTest2.View;
using RevitTest2.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitTest2
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HelloWorld : IExternalCommand
    {
        private static CreateWallsEventHandler _eventHandler;
        private static ExternalEvent _externalEvent;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiDoc = commandData.Application.ActiveUIDocument;

                if (_eventHandler == null)
                {
                    _eventHandler = new CreateWallsEventHandler();
                    _externalEvent = ExternalEvent.Create(_eventHandler);
                }

                // Создаем и показываем наше WPF-окно
                var viewModel = new MainViewModel(uiDoc, _eventHandler, _externalEvent);
                var view = new MainWindow { DataContext = viewModel };

                view.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}