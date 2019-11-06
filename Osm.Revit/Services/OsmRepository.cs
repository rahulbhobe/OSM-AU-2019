﻿using Osm.Revit.Models;
using Osm.Revit.Store;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Osm.Revit.Services
{
    public class OsmRepository
    {
        private readonly HttpClient client;
        private readonly string baseAddres = "https://api.openstreetmap.org/api/0.6/";
        private readonly OsmStore osmStore;

        public OsmRepository(OsmStore osmStore)
        {
            this.client = new HttpClient();
            this.osmStore = osmStore;
        }

        public XmlOsmStreamSource GetMapStream(MapBounds mapBounds)
        {
            var task = this.client
                .GetStreamAsync($"{baseAddres}/map?bbox={osmStore.MapLeft},{osmStore.MapBottom},{osmStore.MapRight},{osmStore.MapTop}");
            var source = new XmlOsmStreamSource(task.Result);
            return source;
        }

    }
}
