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
public class GridDefinitionTests
{
	[Test]
	public void GridLength_Pixel_CreatedCorrectly()
	{
		var length = GridLength.Pixels(100);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(length.Value, Is.EqualTo(100));
			Assert.That(length.IsAbsolute, Is.True);
			Assert.That(length.IsAuto, Is.False);
			Assert.That(length.IsStar, Is.False);
		}
	}

	[Test]
	public void GridLength_Auto_CreatedCorrectly()
	{
		var length = GridLength.Auto;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(length.Value, Is.EqualTo(0));
			Assert.That(length.IsAuto, Is.True);
			Assert.That(length.IsAbsolute, Is.False);
		}
	}

	[Test]
	public void GridLength_Star_CreatedCorrectly()
	{
		var length = GridLength.Star(2);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(length.Value, Is.EqualTo(2));
			Assert.That(length.IsStar, Is.True);
			Assert.That(length.IsAuto, Is.False);
		}
	}

	[Test]
	public void GridLength_Star_DefaultValueIsOne()
	{
		var length = GridLength.Star();
		Assert.That(length.Value, Is.EqualTo(1));
	}

	[Test]
	public void GridLength_NegativeValue_ThrowsException()
		=> _ = Assert.Throws<ArgumentOutOfRangeException>(() => GridLength.Pixels(-1));

	[Test]
	public void RowDefinition_DefaultIsStar()
	{
		var row = new RowDefinition(GridLength.Auto);
		Assert.That(row.Height.IsAuto, Is.True);
	}

	[Test]
	public void ColumnDefinition_DefaultIsStar()
	{
		var col = new ColumnDefinition(GridLength.Pixels(100));
		using (Assert.EnterMultipleScope())
		{
			Assert.That(col.Width.IsAbsolute, Is.True);
			Assert.That(col.Width.Value, Is.EqualTo(100));
		}
	}
}