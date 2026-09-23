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

using DisplayNodes.Gdi;

using Color = DisplayNodes.Core.Color;

namespace DisplayNodes.Tests.Gdi;

[TestFixture]
public class GdiBrushFactoryTests
{
	[Test]
	public void CreateSolidBrush_CreatesBrushWithCorrectColor()
	{
		var factory = new GdiBrushFactory();
		var color = new Color(100, 150, 200, 128);

		var brush = factory.CreateSolidBrush(color);

		Assert.That(brush, Is.InstanceOf<GdiBrush>());
		var gdiBrush = ((GdiBrush)brush).Inner;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(gdiBrush.Color.R, Is.EqualTo(100));
			Assert.That(gdiBrush.Color.G, Is.EqualTo(150));
			Assert.That(gdiBrush.Color.B, Is.EqualTo(200));
			Assert.That(gdiBrush.Color.A, Is.EqualTo(128));
		}
	}

	[Test]
	public void CreateSolidBrush_CreatesNewInstanceEachCall()
	{
		var factory = new GdiBrushFactory();
		var color = Color.Red;

		var brush1 = factory.CreateSolidBrush(color);
		var brush2 = factory.CreateSolidBrush(color);

		Assert.That(brush1, Is.Not.SameAs(brush2));
	}
}