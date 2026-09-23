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

using DisplayNodes.Core;
using DisplayNodes.Core.Rendering;
using DisplayNodes.Fluent;
using Point = DisplayNodes.Core.Point;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Fluent;

[TestFixture]
public class TransformNodeApplyTests
{
    [Test]
    public void Apply_TransformNode_ThrowsNotSupported()
    {
        var transformNode = UI.Transform(2f, 2f, 45f)
            .Add(new FixedNode(100, 50));

        _ = Assert.Throws<NotSupportedException>(() =>
        {
            transformNode.Apply(new StubComponent(), Point.Empty, new Size(200, 200));
        });
    }

#pragma warning disable CS8618
    private sealed class StubComponent : IRenderComponent
    {
        public IRenderComponent Parent { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public bool Visible { get; set; } = true;
    }
#pragma warning restore CS8618
}