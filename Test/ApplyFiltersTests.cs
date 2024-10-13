using System;
using System.Collections.Generic;
using System.Linq;
using TradingApp.DTOs;
using TradingApp.Models.Extension;
using Xunit;

namespace TradingApp.Tests;

    public class ApplyFiltersTests
    {
        private class TestEntity
        {
            public int Id { get; set; }
            public DateTime? DocDate { get; set; }
            public string StakeholderId { get; set; }
        }

        [Fact]
        public void ApplyFilters_ShouldFilterByDateTime()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 1, DocDate = new DateTime(2024, 9, 27), StakeholderId = "S1" },
                new TestEntity { Id = 2, DocDate = new DateTime(2024, 9, 28), StakeholderId = "S2" },
                new TestEntity { Id = 3, DocDate = new DateTime(2024, 9, 29), StakeholderId = "S3" }
            }.AsQueryable();

            var filters = new PaginationFilter
            {
                filters = new List<FilterCriteria>
                {
                    new FilterCriteria
                    {
                        PropertyName = "DocDate",
                        Operation = "Equals",
                        DisplayText="",
                        Value = new DateTime(2024, 9, 27).ToString("yyyy-MM-dd")
                    }
                }
            };

            // Act
            var result = entities.ApplyFilters(filters).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public void ApplyFilters_ShouldFilterByNullableDateTime()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 1, DocDate = new DateTime(2024, 9, 27), StakeholderId = "S1" },
                new TestEntity { Id = 2, DocDate = new DateTime(2024, 9, 28), StakeholderId = "S2" },
                new TestEntity { Id = 3, DocDate = null, StakeholderId = "S3" }
            }.AsQueryable();

            var filters = new PaginationFilter
            {
                filters = new List<FilterCriteria>
                {
                    new FilterCriteria
                    {
                        PropertyName = "DocDate",
                        Operation = "Equals",   DisplayText="",
                        Value = new DateTime(2024, 9, 27).ToString("yyyy-MM-dd")
                    }
                }
            };

            // Act
            var result = entities.ApplyFilters(filters).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public void ApplyFilters_ShouldReturnAll_WhenNoFiltersApplied()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 1, DocDate = new DateTime(2024, 9, 27), StakeholderId = "S1" },
                new TestEntity { Id = 2, DocDate = new DateTime(2024, 9, 28), StakeholderId = "S2" },
                new TestEntity { Id = 3, DocDate = new DateTime(2024, 9, 29), StakeholderId = "S3" }
            }.AsQueryable();

            var filters = new PaginationFilter
            {
                filters = new List<FilterCriteria>()
            };

            // Act
            var result = entities.ApplyFilters(filters).ToList();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void ApplyFilters_ShouldHandleEmptyFilterValue()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { Id = 1, DocDate = new DateTime(2024, 9, 27), StakeholderId = "S1" },
                new TestEntity { Id = 2, DocDate = new DateTime(2024, 9, 28), StakeholderId = "S2" },
                new TestEntity { Id = 3, DocDate = new DateTime(2024, 9, 29), StakeholderId = "S3" }
            }.AsQueryable();

            var filters = new PaginationFilter
            {
                filters = new List<FilterCriteria>
                {
                    new FilterCriteria
                    {
                        PropertyName = "DocDate",
                        Operation = "Equals",   DisplayText="",
                        Value = ""
                    }
                }
            };

            // Act
            var result = entities.ApplyFilters(filters).ToList();

            // Assert
            Assert.Equal(3, result.Count);
        }
    }


