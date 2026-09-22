using DisplayNodes.Core;
using Size = DisplayNodes.Core.Size;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class GridNodeTests
{
	[Test]
	public void Grid_AutoColumns_MeasuresByContent()
	{
		var grid = new GridNode();
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
		grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

		_ = grid.Add(new FixedNode(50, 30), 0, 0);
		_ = grid.Add(new FixedNode(80, 40), 0, 1);

		var size = grid.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(130));  // 50 + 80
			Assert.That(size.Height, Is.EqualTo(40));  // max(30, 40)
		}
	}

	[Test]
	public void Grid_StarColumns_DistributesSpaceProportionally()
	{
		var grid = new GridNode();
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(2)));
		grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

		_ = grid.Add(new FixedNode(10, 10), 0, 0);
		_ = grid.Add(new FixedNode(10, 10), 0, 1);

		_ = grid.Measure(new Size(300, 100));
		grid.Arrange(new Rect(0, 0, 300, 100));

		using (Assert.EnterMultipleScope())
		{
			// 300 / 3 = 100, 200
			Assert.That(grid.Children[0].Bounds.Width, Is.EqualTo(100));
			Assert.That(grid.Children[1].Bounds.Width, Is.EqualTo(200));
		}
	}

	[Test]
	public void Grid_PixelColumn_FixedWidth()
	{
		var grid = new GridNode();
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Pixels(100)));
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star(1)));
		grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

		_ = grid.Add(new FixedNode(10, 10), 0, 0);
		_ = grid.Add(new FixedNode(10, 10), 0, 1);

		_ = grid.Measure(new Size(300, 100));
		grid.Arrange(new Rect(0, 0, 300, 100));

		using (Assert.EnterMultipleScope())
		{
			Assert.That(grid.Children[0].Bounds.Width, Is.EqualTo(100));
			Assert.That(grid.Children[1].Bounds.Width, Is.EqualTo(200)); // 300 - 100
		}
	}

	[Test]
	public void Grid_Add_ThrowsOnInvalidIndices()
	{
		var grid = new GridNode();
		grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

		_ = Assert.Throws<ArgumentOutOfRangeException>(() => grid.Add(new FixedNode(10, 10), -1, 0));
		_ = Assert.Throws<ArgumentOutOfRangeException>(() => grid.Add(new FixedNode(10, 10), 0, -1));
		_ = Assert.Throws<ArgumentOutOfRangeException>(() => grid.Add(new FixedNode(10, 10), 1, 0)); // Out of range
	}

	[Test]
	public void Grid_Padding_IncludedInMeasure()
	{
		var grid = new GridNode { Padding = new Thickness(10, 20) };
		grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Pixels(100)));
		grid.RowDefinitions.Add(new RowDefinition(GridLength.Pixels(50)));
		_ = grid.Add(new FixedNode(10, 10), 0, 0);

		var size = grid.Measure(Size.Infinity);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(size.Width, Is.EqualTo(120));  // 100 + 20
			Assert.That(size.Height, Is.EqualTo(90));  // 50 + 40
		}
	}
}