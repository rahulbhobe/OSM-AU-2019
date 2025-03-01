﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Osm.Revit.Models;
using OsmSharp;
using OsmSharp.Streams;

namespace Osm.Revit.Services
{
    class MapStreamOnDemandService : IMapStreamService
    {
        private static string OsmDetailsFile => "osmdetails";
        private static bool WaitForInput()
        {
            int idx = 0;
            while (true)
            {
                char ch = Convert.ToChar(Console.Read());
                if (ch == '\n')
                    return true; // found lf.
                else if (ch == '\x3')
                    return false; // error, cancelled.
                if (idx >= 16)
                    return false; // failed, after retries.
                idx++;
            }
        }

        public List<OsmGeo> GetOsmGeoList(MapBounds mapbounds)
        {
            string suffix = $"map?bbox={mapbounds.Left}%2C{mapbounds.Bottom}%2C{mapbounds.Right}%2C{mapbounds.Top}";
            string filename = $"{OsmDetailsFile}_{MapBounds.Serialize(mapbounds).GetHashCode()}.xml";

            Console.WriteLine($"!ACESAPI:acesHttpOperation(osmParam,{suffix},,,file://{filename})");

            if (!WaitForInput())
                throw new Exception($"Error in getting {filename}");

            Console.WriteLine($"Downloaded osm for: {MapBounds.Serialize(mapbounds)}");

            using (var stream = new XmlOsmStreamSource(File.Open(filename, FileMode.Open)))
            {
                return stream.ToList();
            }
        }
    }
}
