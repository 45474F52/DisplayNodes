///////////////////////////////////////////////////////////////////////////
//
// Copyright 2026 AES
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
///////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace DisplayNodes.Core
{
    /// <summary>
    /// Bounding box трансформированного прямоугольника.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("BBox ({MinX}:{MinY}) ({MaxX}:{MaxY)")]
    internal readonly struct BoundingBox
    {
        public readonly float MinX;
        public readonly float MinY;
        public readonly float MaxX;
        public readonly float MaxY;

        public BoundingBox(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public float Width => MaxX - MinX;
        public float Height => MaxY - MinY;
    }
}
