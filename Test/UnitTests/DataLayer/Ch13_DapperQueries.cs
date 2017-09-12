﻿// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;
using DataLayer.EfClasses;
using DataLayer.EfCode;
using DataLayer.SqlCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using ServiceLayer.BookServices;
using ServiceLayer.BookServices.DapperQueries;
using test.EfHelpers;
using test.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace test.UnitTests.DataLayer
{
    public class Ch13_DapperQueries
    {
        private readonly DbContextOptions<EfCoreContext> _options;
        private readonly ITestOutputHelper _output;

        private readonly string _sqlScriptFilepath = Path.Combine(TestFileHelpers.GetSolutionDirectory(),
            @"EfCoreInAction\wwwroot\",
            UdfDefinitions.SqlScriptName);

        public Ch13_DapperQueries(ITestOutputHelper output)
        {
            _output = output;
            var connectionString = this.GetUniqueDatabaseConnectionString();
            var optionsBuilder =
                new DbContextOptionsBuilder<EfCoreContext>();

            optionsBuilder.UseSqlServer(connectionString);
            _options = optionsBuilder.Options;
            using (var context = new EfCoreContext(_options))
            {
                if (context.Database.EnsureCreated())
                {
                    context.ExecuteScriptFileInTransaction(_sqlScriptFilepath);
                    context.SeedDatabaseFourBooks();
                }
            }
        }

        [Fact]
        public void DapperBookListQuery()
        {
            //SETUP 
            using (var context = new EfCoreContext(_options))
            {
                //ATTEMPT
                var options = new SortFilterPageOptions();
                var books = context.BookListQuery(options).ToList();

                //VERIFY
                books.Count.ShouldEqual(4);
            }
        }

        [Fact]
        public void DapperBookListQuerySmallPage()
        {
            //SETUP 
            using (var context = new EfCoreContext(_options))
            {
                //ATTEMPT
                var options = new SortFilterPageOptions {PageSize = 2};
                var books = context.BookListQuery(options).ToList();

                //VERIFY
                books.Count.ShouldEqual(2);
            }
        }

        [Fact]
        public void DapperBookListQuerySmallPageSkip()
        {
            //SETUP 
            using (var context = new EfCoreContext(_options))
            {
                //ATTEMPT
                var options = new SortFilterPageOptions { PageSize = 3, PageNum = 2};
                var books = context.BookListQuery(options).ToList();

                //VERIFY
                books.Count.ShouldEqual(1);
                books.First().Title.ShouldEqual("Refactoring");
            }
        }

    }
}