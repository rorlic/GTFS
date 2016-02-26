// The MIT License (MIT)

// Copyright (c) 2014 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using GTFS.IO;
using GTFS.IO.CSV;
using NUnit.Framework;
using System.IO;
using System.Linq;
using GTFS.Entities;

namespace GTFS.Test
{
    /// <summary>
    /// Contains tests for writing shape files.
    /// </summary>
    [TestFixture]
    public class WriteShapeTests
    {
        /// <summary>
        /// Tests writing shape files.
        /// </summary>
        [Test]
        public void WriteShapeFilesDoesNotUseExponentialNotation()
        {
            const string ShapesFile = "shapes.txt";

            var feed = new GTFSFeed();
            var shape = new Shape { Id = "something", Sequence = 1, Latitude = 1e-3, Longitude = -1e-5, DistanceTravelled = 1e-7 };
            feed.Shapes.Add(shape);

            using (var stream = File.OpenWrite(ShapesFile))
            {
                var writer = new GTFSWriter<GTFSFeed>();
                var targetFile = new GTFSTargetFileStream(stream, "shapes");
                writer.Write(feed, new IGTFSTargetFile[] { targetFile });
            }

            var bytes = File.ReadAllBytes(ShapesFile);

            using (var reader = new CSVStreamReader(new MemoryStream(bytes)))
            {
                reader.MoveNext();
                var headerLine = reader.Current;
                var indexedColumns = headerLine.Select((col, i) => new { col, i }).ToDictionary(c => c.col, c => c.i);

                reader.MoveNext();
                var shapeLine = reader.Current;

                var latitude = shapeLine[indexedColumns["shape_pt_lat"]];
                var longitude = shapeLine[indexedColumns["shape_pt_lon"]];
                var distance = shapeLine[indexedColumns["shape_dist_traveled"]];

                var expected = new[]
                {
                    "0.001000000", 
                    "-0.000010000", 
                    "0.000000100"
                };
                var actual = new[] { latitude, longitude, distance };
                CollectionAssert.AreEquivalent(expected, actual);
            }
        }
    }
}