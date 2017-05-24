﻿// Copyright (c) 2017 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using test.EfHelpers;

namespace Test.Chapter09Listings.EfCode
{
    public static class RawSqlHelpers
    {
        public const string FilterOnReviewRank = "FilterOnReviewRank";
        public const string UdfAverageVotes = "udf_AverageVotes";

        public static void AddUpdateSqlProcs(this DbContext context)
        {

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    context.Database.ExecuteSqlCommand(
                        $"IF OBJECT_ID('dbo.{UdfAverageVotes}', N'FN') IS NOT NULL " +
                        $"DROP PROC dbo.{UdfAverageVotes}");

                    context.Database.ExecuteSqlCommand(
                        $"CREATE FUNCTION {UdfAverageVotes} (@bookId int)" +
                        @"  RETURNS decimal
  AS
  BEGIN
  DECLARE @result AS decimal
  SELECT @result = AVG(NumStars) FROM dbo.Review AS r
       WHERE @bookId = r.BookId
  IF (@result IS NULL)
     SET @result = -1
  RETURN @result
  END
  GO");

                    context.Database.ExecuteSqlCommand(
                        $"IF OBJECT_ID('dbo.{FilterOnReviewRank}') IS NOT NULL " +
                        $"DROP PROC dbo.{FilterOnReviewRank}");

                    context.Database.ExecuteSqlCommand(
                        $"CREATE PROC dbo.{FilterOnReviewRank}" +
                        @"(  @RankFilter int )
AS

SELECT * FROM dbo.Books
WHERE udf_AverageVotes(BookId) > @RankFilter
");



                    transaction.Commit();
                }
                catch (Exception)
                {
                    //Do nothing
                }
            }
        }

        public static bool EnsureSqlProcsSet(this DbContext context)
        {
            var connection = context.Database.GetDbConnection().ConnectionString;
            return connection.ExecuteRowCount("sysobjects", $"WHERE type='P' AND name='{FilterOnReviewRank}'") == 1
                   && connection.ExecuteRowCount("sys.objects",
                       $"WHERE object_id = OBJECT_ID(N'[dbo].[{UdfAverageVotes}]')" +
                       " AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' )") == 1;
        }
    }
}