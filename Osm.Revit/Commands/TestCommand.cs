﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Osm.Revit.Models;
using Osm.Revit.Services;
using OsmSharp.Streams;
using System.IO;
using OsmSharp.Tags;
using Autodesk.Revit.Attributes;
using System.Linq;
using OsmSharp;
using System.Collections.Generic;

namespace Osm.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            var httpService = new HttpService();
            var mapBounds = new MapBounds
            {
                Left = -73.88025,
                Bottom = 40.71562,
                Right = -73.87901,
                Top = 40.71712,
            };

            var source = httpService.GetMapStream(mapBounds);

            var everything = source.FilterNodes(n => true).ToList();
            var buildings = everything.Where(n => n.Tags.IsTrue("building"));

            System.Windows.MessageBox.Show(buildings.Count().ToString());
        
            var coordService = new CoordinatesService();
            coordService.Geolocate(mapBounds.Bottom, mapBounds.Left);

            var levelId = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().FirstElementId();

            using (Transaction t = new Transaction(doc, "points"))
            {
                t.Start();
                var sketchPlane = SketchPlane.Create(doc, levelId);
                foreach (var building in buildings)
                {
                    if (building is Way way)
                    {
                        var points = new List<XYZ>();

                        foreach (var nodeId in way.Nodes)
                        {
                            var geometry = everything.FirstOrDefault(n => n.Id == nodeId);
                            if (geometry is Node node)
                            {
                                var coords = coordService.GetRevitCoords((double)node.Latitude, (double)node.Longitude);
                                points.Add(coords);
                            }
                        }

                        for (int i = 0; i < points.Count - 1; i++)
                        {
                            Line line = Line.CreateBound(points[i], points[i + 1]);
                            doc.Create.NewModelCurve(line, sketchPlane);
                        }
                    }
                }
                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
