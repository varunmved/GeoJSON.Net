﻿using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoJSON.Net.Converters
{
    /// <summary>
    /// Converter to read and write the <see cref="MultiPolygon" /> type.
    /// </summary>
    public class MultiPolygonConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var polygons = value as List<Polygon>;
            writer.WriteStartArray();
            if (polygons != null && polygons.Count > 0)
            {
                foreach (var polygon in polygons)
                {
                    writer.WriteStartArray();

                    foreach (var lineString in polygon.Coordinates)
                    {
                        var coordinateElements = lineString.Coordinates as List<IPosition>;
                        if (coordinateElements != null && coordinateElements.Count > 0)
                        {
                            var coordinateArray = new JArray();
                            foreach (var position in coordinateElements)
                            {
                                var coordinates = (GeographicPosition)position;
                                var coordinateElement = new JArray(coordinates.Longitude, coordinates.Latitude);
                                if (coordinates.Altitude.HasValue)
                                    coordinateElement = new JArray(coordinates.Longitude, coordinates.Latitude, coordinates.Altitude);

                                coordinateArray.Add(coordinateElement);
                            }
                            writer.WriteRawValue(coordinateArray.ToString());
                        }
                    }
                    writer.WriteEndArray();

                }
                writer.WriteEndArray();
            }

        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param><param name="objectType">Type of the object.</param><param name="existingValue">The existing value of object being read.</param><param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
			var o = serializer.Deserialize<JArray>(reader);
			var polygonConverter = new PolygonConverter();
			var polygons = o.Select(polygonObject => polygonConverter.ReadJson(polygonObject.CreateReader(), typeof(Polygon), polygonObject, serializer) as List<LineString>).Select(lines => new Polygon(lines)).ToList();
			return polygons;
		}

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MultiPolygon);
        }
    }
}