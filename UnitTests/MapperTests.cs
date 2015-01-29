using System.Linq;
using DataMapper;
using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class MapperTests
	{
		private readonly IntersectionMapper _mapper = new IntersectionMapper();

		[Test]
		public void CloneTest()
		{
			var source = new TestClass {Id = 1, Name = "Test"};
			var destination = _mapper.Map<TestClass, TestClass>(source);

			Assert.That(destination.Id, Is.EqualTo(source.Id));
			Assert.That(destination.Name, Is.EqualTo(source.Name));
		}
		
		[Test]
		public void PlainTest()
		{
			var source = new PlainSourceClass { Id = 1, Name = "Test", SourceFieldOnly = "source"};
			var destination = _mapper.Map<PlainSourceClass, PlainDestinationClass>(source);

			Assert.That(destination.Id, Is.EqualTo(source.Id));
			Assert.That(destination.Name, Is.EqualTo(source.Name));
			Assert.That(destination.DestFieldOnly, Is.Null.Or.Empty);
		}

		[Test]
		public void ConversionTest()
		{
			var source = new PlainSourceClass { Id = 1, Name = "Test", SourceFieldOnly = "source"};
			var destination = _mapper.Map<PlainSourceClass, ConversionDestinationClass>(source);

			Assert.That(destination.Id, Is.EqualTo(source.Id.ToString()));
			Assert.That(destination.Name, Is.EqualTo(source.Name));
		}

		[Test]
		public void ComplexTest()
		{
			var source = new ComplexSourceClass {Item = new PlainSourceClass {Id = 1, Name = "Test", SourceFieldOnly = "source"}};
			var destination = _mapper.Map<ComplexSourceClass, ComplexDestinationClass>(source);

			Assert.That(destination.Item, Is.Not.Null);
			Assert.That(destination.Item.Id, Is.EqualTo(source.Item.Id));
			Assert.That(destination.Item.Name, Is.EqualTo(source.Item.Name));
		}

		[Test]
		public void CollectionTest()
		{
			var sourceItem = new PlainSourceClass {Id = 1, Name = "Test", SourceFieldOnly = "source"};
			var source = new CollectionSourceClass {Items = new [] {sourceItem}};
			var destination = _mapper.Map<CollectionSourceClass, CollectionDestinationClass>(source);

			Assert.That(destination.Items, Is.Not.Null);
			Assert.That(destination.Items.Count(), Is.EqualTo(source.Items.Count()));
			var destinationItem = destination.Items.First();
			Assert.That(destinationItem.Id, Is.EqualTo(sourceItem.Id));
			Assert.That(destinationItem.Name, Is.EqualTo(sourceItem.Name));
		}
	}
}