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
public class ShadowTests
{
    [Test]
    public void Constructor_StoresValues()
    {
        var shadow = new Shadow(2, 4, 8, new Color(0, 0, 0, 128));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(shadow.OffsetX, Is.EqualTo(2));
            Assert.That(shadow.OffsetY, Is.EqualTo(4));
            Assert.That(shadow.BlurRadius, Is.EqualTo(8));
            Assert.That(shadow.Color.A, Is.EqualTo(128));
        }
    }

    [Test]
    public void Constructor_DefaultColor_SemiTransparentBlack()
    {
        var shadow = new Shadow(1, 1, 4);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(shadow.Color.R, Is.EqualTo(0));
            Assert.That(shadow.Color.G, Is.EqualTo(0));
            Assert.That(shadow.Color.B, Is.EqualTo(0));
            Assert.That(shadow.Color.A, Is.EqualTo(80));
        }
    }

    [Test]
    public void Constructor_NegativeBlur_Throws()
        => _ = Assert.Throws<ArgumentOutOfRangeException>(() => new Shadow(0, 0, -1));

    [Test]
    public void ToString_Formats()
    {
        var shadow = new Shadow(2, 4, 8);
        Assert.That(shadow.ToString(), Does.Contain("2").And.Contain("4").And.Contain("8"));
    }
}