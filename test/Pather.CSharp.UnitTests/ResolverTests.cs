﻿using FluentAssertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Pather.CSharp.UnitTests
{
    public class ResolverTests
    {
        public ResolverTests()
        {
        }

        [Fact]
        public void SinglePropertyResolution_CorrectSetup_Success()
        {
            var value = "1";
            var r = new Resolver();
            var o = new { Property = value };
            var path = "Property";

            var result = r.Resolve(o, path);
            result.Should().Be(value);
        }

        [Fact]
        public void MultiplePropertyResolution_CorrectSetup_Success()
        {
            var value = "1";
            var r = new Resolver();
            var o = new { Property1 = new { Property2 = value } };
            var path = "Property1.Property2";

            var result = r.Resolve(o, path);
            result.Should().Be(value);
        }

        [Fact]
        public void DictionaryKeyResolutionWithProperty_CorrectSetup_Success()
        {
            var r = new Resolver();
            var dictionary = new Dictionary<string, string> { { "Key", "Value" } };
            var o = new { Dictionary = dictionary };
            var path = "Dictionary[Key]";

            var result = r.Resolve(o, path);
            result.Should().Be("Value");
        }

        [Fact]
        public void DictionaryKeyResolution_CorrectSetup_Success()
        {
            var r = new Resolver();
            var dictionary = new Dictionary<string, string> { { "Key", "Value" } };
            var path = "[Key]";

            var result = r.Resolve(dictionary, path);
            result.Should().Be("Value");
        }

        [Fact]
        public void MultipleDictionaryKeyResolution_CorrectSetup_Success()
        {
            var r = new Resolver();
            var dictionary = new Dictionary<string, Dictionary<string, string>> {
                { "Key", new Dictionary<string, string> { { "Key", "Value" } } }
            };
            var path = "[Key][Key]";

            var result = r.Resolve(dictionary, path);
            result.Should().Be("Value");
        }

        [Fact]
        public void ArrayIndexResolutionWithProperty_CorrectSetup_Success()
        {
            var r = new Resolver();
            var array = new[] { "1", "2" };
            var o = new { Array = array };
            var path = "Array[0]";

            var result = r.Resolve(o, path);
            result.Should().Be("1");
        }

        [Fact]
        public void ArrayIndexResolution_CorrectSetup_Success()
        {
            var r = new Resolver();
            var array = new[] { "1", "2" };
            var path = "[0]";

            var result = r.Resolve(array, path);
            result.Should().Be("1");
        }

        [Fact]
        public void MultipleArrayIndexResolution_CorrectSetup_Success()
        {
            var r = new Resolver();
            var array = new[] { new[] { "1", "2" } };
            var path = "[0][0]";

            var result = r.Resolve(array, path);
            result.Should().Be("1");
        }

        [Fact]
        public void SelectionResolution_CorrectSetup_Success()
        {
            var r = new Resolver();
            var array = new[] { 1, 2};
            var path = "[]";

            var result = r.Resolve(array, path);
            result.Should().BeOfType(typeof(Selection));
            result.ShouldBeEquivalentTo(new[] { 1, 2 });
        }

        [Fact]
        public void SelectionPropertyResolution_CorrectSetup_Success()
        {
            var r = new Resolver();
            var array = new[]
            {
                new { P1 = "1" },
                new { P1 = "2" }
            };
            var path = "[].P1";

            var result = r.Resolve(array, path) as IEnumerable;
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new[] { "1", "2" });
        }

        [Fact]
        public void SelectionDictionaryKeyResolution_CorrectSetup_Success()
        {
            var r = new Resolver();
            var array = new[]
            {
                new Dictionary<string, string> { { "Key", "1" } },
                new Dictionary<string, string> { { "Key", "2" } }
            };
            var path = "[][Key]";

            var result = r.Resolve(array, path) as IEnumerable;
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new[] { "1", "2" });
        }

        [Fact]
        public void SelectionArrayIndexResolution_CorrectSetup_Success()
        {
            var r = new Resolver();
            var array = new[]
            {
                new[] { "1", "2" },
                new[] { "3", "4" }
            };
            var path = "[][1]";

            var result = r.Resolve(array, path) as IEnumerable;
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new[] { "2", "4" });
        }

        [Fact]
        public void SelectionFlattening_CorrectSetup_Success()
        {
            var r = new Resolver();
            var array = new object[]
            {
                1,
                new[] { 2, 3 }
            };
            var path = "[][]";

            var result = r.Resolve(array, path) as IEnumerable;
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        }

        [Fact]
        public void SinglePropertyResolution_NoPathElementTypeForPath_FailWithNoApplicablePathElementType()
        {
            var r = new Resolver();
            var o = new { Property = "1" };
            var path = "Property^%#";

            r.Invoking(re => re.Resolve(o, path)).ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void SinglePropertyResolution_NonExistingProperty_FailWithPropertyCouldNotBeFound()
        {
            var r = new Resolver();
            var o = new { Property = "1" };
            var path = "NonExistingProperty";

            r.Invoking(re => re.Resolve(o, path)).ShouldThrow<ArgumentException>();
        }

        [Fact]
        public void ArrayIndexResolution_IndexHigher_FailWithIndexTooHigh()
        {
            var r = new Resolver();
            var array = new[] { "1", "2" };
            var path = "[3]";

            r.Invoking(re => re.Resolve(array, path)).ShouldThrow<IndexOutOfRangeException>();
        }

        [Fact]
        public void ArrayIndexResolution_IndexLower_FailWithNoApplicablePathElementType()
        {
            var r = new Resolver();
            var array = new[] { "1", "2" };
            var path = "[-2]";

            r.Invoking(re => re.Resolve(array, path)).ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void DictionaryKeyResolution_KeyNotExisting_FailWithKeyNotExisting()
        {
            var r = new Resolver();
            var dictionary = new Dictionary<string, string> { { "Key", "Value" } };
            var path = "[NonExistingKey]";

            r.Invoking(re => re.Resolve(dictionary, path)).ShouldThrow<ArgumentException>();
        }
    }
}
