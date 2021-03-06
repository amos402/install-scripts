﻿// Copyright (c) Microsoft. All rights reserved.

using Kusto.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonitoringFunctions.DataService.Kusto;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace MonitoringFunctions.Test
{
    /// <summary>
    /// Contains tests that make sure <see cref="DirectJsonMappingResolver"/> can correctly create
    /// <see cref="ColumnMapping"/>s for models.
    /// </summary>
    [TestClass]
    public class TestDirectJsonMappingResolver
    {
        private class BasicMappingModel : IKustoTableRow
        {
            public float someFloat { get; set; }

            public string someString { get; set; }
        }

        private class CommonMappingModel : IKustoTableRow
        {
            [JsonProperty("some_float")]
            public float someFloat { get; set; }

            public int someInteger { get; set; }

            private string somePrivateString { get; set; }
        }

        private class ComplexMappingModel : CommonMappingModel, IKustoTableRow
        {
            [JsonIgnore]
            public int someOtherInteger { get; set; }

            public string privateGetter { private get; set; }

            public int someField;
        }

        /// <summary>
        /// Tests if <see cref="DirectJsonMappingResolver"/> can correctly create column mappings
        /// for a simple type <see cref="BasicMappingModel"/>
        /// </summary>
        [TestMethod]
        public void TestBasicMappingAsync()
        {
            string[] expectedColumnNames = new string[] { "someFloat", "someString" };
            CompareMappings<BasicMappingModel>(expectedColumnNames);
        }

        /// <summary>
        /// Tests if <see cref="DirectJsonMappingResolver"/> can correctly create column mappings
        /// for a not-so-basic type <see cref="CommonMappingModel"/>
        /// </summary>
        [TestMethod]
        public void TestCommonMappingAsync()
        {
            string[] expectedColumnNames = new string[] { "some_float", "someInteger" };
            CompareMappings<CommonMappingModel>(expectedColumnNames);
        }

        /// <summary>
        /// Tests if <see cref="DirectJsonMappingResolver"/> can correctly create column mappings
        /// for a complicated type <see cref="ComplexMappingModel"/>
        /// </summary>
        [TestMethod]
        public void TestComplexMappingAsync()
        {
            string[] expectedColumnNames = new string[] { "some_float", "someInteger", "someField" };
            CompareMappings<ComplexMappingModel>(expectedColumnNames);
        }

        /// <summary>
        /// Compares the given column names with the column names generated by <see cref="DirectJsonMappingResolver"/>
        /// for the given type.
        /// </summary>
        /// <typeparam name="T">Data class whose column names will be resolved by <see cref="DirectJsonMappingResolver"/></typeparam>
        /// <param name="expectedColumnNames">Column names to compare. Order of the items is unimportant.</param>
        private void CompareMappings<T>(string[] expectedColumnNames) where T : IKustoTableRow
        {
            DirectJsonMappingResolver mappingResolver = new DirectJsonMappingResolver();
            IEnumerable<ColumnMapping> mappingCollection = mappingResolver.GetColumnMappings<T>();

            List<string> mappedColumnNames = mappingCollection.Select(m => m.ColumnName).ToList();

            Assert.IsTrue(!mappedColumnNames.Except(expectedColumnNames).Any(),
                $"The following columns shouldn't have been mapped, but they were: { string.Join(", ", mappedColumnNames.Except(expectedColumnNames))}");

            Assert.IsTrue(!expectedColumnNames.Except(mappedColumnNames).Any(),
                $"The following columns should have been mapped, but they weren't: { string.Join(", ", expectedColumnNames.Except(mappedColumnNames))}");
        }
    }
}
