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

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class PercentTests
{
    [Test]
    public void Constructor_ValidRange_Succeeds()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(new Percent(0).Value, Is.EqualTo(0));
            Assert.That(new Percent(50).Value, Is.EqualTo(50));
            Assert.That(new Percent(100).Value, Is.EqualTo(100));
        }
    }

    [Test]
    public void Constructor_BelowZero_Throws()
        => _ = Assert.Throws<ArgumentOutOfRangeException>(() => new Percent(-1));

    [Test]
    public void Constructor_AboveHundred_Throws()
        => _ = Assert.Throws<ArgumentOutOfRangeException>(() => new Percent(101));

    [Test]
    public void Zero_And_Hundred_Constants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Percent.Zero.Value, Is.EqualTo(0));
            Assert.That(Percent.Hundred.Value, Is.EqualTo(100));
        }
    }

    [Test]
    public void Quarter_And_Half_Constants()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Percent.Quarter.Value, Is.EqualTo(25));
            Assert.That(Percent.Half.Value, Is.EqualTo(50));
        }
    }

    [Test]
    public void ImplicitConversion_IntToPercent()
    {
        Percent p = 42;
        Assert.That(p.Value, Is.EqualTo(42));
    }

    [Test]
    public void ImplicitConversion_PercentToInt()
    {
        var p = new Percent(75);
        int value = p;
        Assert.That(value, Is.EqualTo(75));
    }

    [Test]
    public void Equality_Works()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Percent.Half, Is.EqualTo(new Percent(50)));
            Assert.That(Percent.Half, Is.Not.EqualTo(new Percent(51)));
        }
    }

    [Test]
    public void ToString_Formats()
        => Assert.That(new Percent(75).ToString(), Is.EqualTo("75%"));
}