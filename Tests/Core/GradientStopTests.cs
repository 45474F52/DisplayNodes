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
using Color = DisplayNodes.Core.Color;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class GradientStopTests
{
    [Test]
    public void Constructor_StoresValues()
    {
        var stop = new GradientStop(new Color(255, 0, 0), 50);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(stop.Color.R, Is.EqualTo(255));
            Assert.That(stop.Color.G, Is.EqualTo(0));
            Assert.That(stop.Offset.Value, Is.EqualTo(Percent.Half.Value));
        }
    }

    [Test]
    public void Constructor_ImplicitPercentConversion()
    {
        // 50 → Percent(50)
        var stop = new GradientStop(Color.Red, 50);
        Assert.That(stop.Offset.Value, Is.EqualTo(50));
    }

    [Test]
    public void Constructor_OutOfRangeOffset_Throws()
        => _ = Assert.Throws<ArgumentOutOfRangeException>(() => new GradientStop(Color.Red, 101));
}