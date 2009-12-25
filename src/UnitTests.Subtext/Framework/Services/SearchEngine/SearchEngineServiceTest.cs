﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Snowball;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Store;
using MbUnit.Framework;
using Subtext.Framework.Configuration;
using Subtext.Framework.Services.SearchEngine;

namespace UnitTests.Subtext.Framework.Services.SearchEngine
{
    [TestFixture]
    public class SearchEngineServiceTest
    {
        private SearchEngineService _service;


        [SetUp]
        public void CreateSearchEngine()
        {
            _service = new SearchEngineService(new RAMDirectory(), new SnowballAnalyzer("English", StopAnalyzer.ENGLISH_STOP_WORDS), new FullTextSearchEngineSettings());
        }

        [TearDown]
        public void DestroySearchEngine()
        {
            _service.Dispose();
        }

        [Test]
        public void SearchEngineService_WithEntry_AddsToIndex()
        {
            _service.AddPost(new SearchEngineEntry()
                                {
                                    EntryId = 1,
                                    Body = "This is a sample post",
                                    Title = "This is the title",
                                    Tags = "Title",
                                    BlogName = "MyTestBlog",
                                    IsPublished = true,
                                    PublishDate = DateTime.Now,
                                    EntryName = "this-is-the-title"
                                }
                );

            _service.AddPost(new SearchEngineEntry()
                    {
                        EntryId = 2,
                        Body = "This is another sample post",
                        Title = "This is another title",
                        Tags = "Title another",
                        BlogName = "MyTestBlog",
                        IsPublished = true,
                        PublishDate = DateTime.Now,
                        EntryName = "this-is-the-title"
                    }
            );

            List<SearchEngineResult> result = _service.Search("sample", 100,0) as List<SearchEngineResult>;
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void SearchEngineService_ConvertsToSearchResult()
        {
            _service.AddPost(new SearchEngineEntry()
                {
                    EntryId = 1,
                    Body = "This is a sample post",
                    Title = "This is the title",
                    Tags = "Title",
                    BlogName = "MyTestBlog",
                    IsPublished = true,
                    PublishDate = DateTime.Now,
                    EntryName = "this-is-the-title"
                }
            );

            List<SearchEngineResult> result = _service.Search("sample", 100,0) as List<SearchEngineResult>;

            Assert.AreEqual("This is the title", result[0].Title);
            Assert.AreEqual("MyTestBlog", result[0].BlogName);
            Assert.AreEqual(1, result[0].EntryId);
        }

        [Test]
        public void SearchEngineService_WhenAddingToItemWithSamePostId_UpdatesOriginalEntry()
        {
            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 1,
                Body = "This is a sample post",
                Title = "This is the title",
                Tags = "Title",
                BlogName = "MyTestBlog",
                IsPublished = true,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            }
            );

            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 1,
                Body = "This is a post",
                Title = "This is the title",
                Tags = "Title",
                BlogName = "MyTestBlog",
                IsPublished = true,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            }
            );

            List<SearchEngineResult> result = _service.Search("sample", 100,0) as List<SearchEngineResult>;
            Assert.AreEqual(0, result.Count);

            result = _service.Search("post", 100,0) as List<SearchEngineResult>;
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1, result[0].EntryId);
        }

        [Test]
        public void SearchEngineService_DeletesEntry()
        {
            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 1,
                Body = "This is a sample post",
                Title = "This is the title",
                Tags = "Title",
                BlogName = "MyTestBlog",
                IsPublished = true,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            }
            );

            _service.RemovePost(1);

            List<SearchEngineResult> result = _service.Search("sample", 100,0) as List<SearchEngineResult>;
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void SearchEngineService_ReturnsCorrectTotalNumber()
        {
            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 1,
                BlogId = 1,
                Body = "This is a sample post",
                Title = "This is the title",
                Tags = "Title",
                BlogName = "MyTestBlog",
                IsPublished = true,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            }
                );

            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 2,
                BlogId = 2,
                Body = "This is another sample post",
                Title = "This is another title",
                Tags = "Title another",
                BlogName = "MyTestBlog",
                IsPublished = true,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            }
            );

            int totNumber = _service.GetTotalIndexedEntryCount();
            Assert.AreEqual(2,totNumber);
        }

        [Test]
        public void SearchEngineService_ReturnsCorrectNumberOfPostsByBlog()
        {
            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 1,
                BlogId = 1,
                Body = "This is a sample post",
                Title = "This is the title",
                Tags = "Title",
                BlogName = "MyTestBlog",
                IsPublished = true,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            }
                );

            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 2,
                BlogId = 2,
                Body = "This is another sample post",
                Title = "This is another title",
                Tags = "Title another",
                BlogName = "MyTestBlog",
                IsPublished = true,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            }
            );

            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 3,
                BlogId = 2,
                Body = "This is another sample post",
                Title = "This is another title",
                Tags = "Title another",
                BlogName = "MyTestBlog",
                IsPublished = true,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            }
            );

            int postCountBlog1 = _service.GetIndexedEntryCount(1);
            int postCountBlog2 = _service.GetIndexedEntryCount(2);
            Assert.AreEqual(1, postCountBlog1);
            Assert.AreEqual(2, postCountBlog2);
        }

        [Test]
        public void SearchEngineService_PerformsMoreLikeThisSearch()
        {
            for (int i = 1; i <= 10; i++)
            {
                _service.AddPost(new SearchEngineEntry()
                                {
                                    EntryId = i,
                                    Body = "This is a sample post",
                                    Title = "This is the title of the post",
                                    Tags = ".net, mvc, post",
                                    BlogName = "MyTestBlog",
                                    IsPublished = true,
                                    PublishDate = DateTime.Now,
                                    EntryName = "this-is-the-title"
                                }
                );
            }


            List<SearchEngineResult> result = _service.RelatedContents(1, 100,0) as List<SearchEngineResult>;
            Assert.IsTrue(result.Count>0);
        }

        [Test]
        public void SearchEngineService_MoreLikeThisSearch_FiltersOriginalDocOut()
        {
            for (int i = 1; i <= 10; i++)
            {
                _service.AddPost(new SearchEngineEntry()
                {
                    EntryId = i,
                    Body = "This is a sample post",
                    Title = "This is the title of the post",
                    Tags = ".net, mvc, post",
                    BlogName = "MyTestBlog",
                    IsPublished = true,
                    PublishDate = DateTime.Now,
                    EntryName = "this-is-the-title"
                }
                );
            }

            List<SearchEngineResult> result = _service.RelatedContents(1, 100,0) as List<SearchEngineResult>;
            Assert.AreEqual(0, result.Count(r => r.EntryId==1));
        }

        [Test]
        public void SearchEngineService_MoreLikeThisSearch_WithMinDocumentSet_ReturnsEmptySet()
        {
            _service.Dispose();
            _service = new SearchEngineService(new RAMDirectory(), new SnowballAnalyzer("English", StopAnalyzer.ENGLISH_STOP_WORDS), new FullTextSearchEngineSettings() { Parameters = new TuningParameters() { MinimumDocumentFrequency = 20 } });

            for (int i = 1; i <= 10; i++)
            {
                _service.AddPost(new SearchEngineEntry()
                {
                    EntryId = i,
                    Body = "This is a sample post",
                    Title = "This is the title of the post",
                    Tags = ".net, mvc, post",
                    BlogName = "MyTestBlog",
                    IsPublished = true,
                    PublishDate = DateTime.Now,
                    EntryName = "this-is-the-title"
                }
                );
            }
            

            List<SearchEngineResult> result = _service.RelatedContents(1, 100, 0) as List<SearchEngineResult>;
            Assert.AreEqual(0, result.Count());

        }

        [Test]
        public void SearchEngineService_Search_DoesntIncludeNotActiveEntries()
        {
            for (int i = 1; i <= 10; i++)
            {
                _service.AddPost(new SearchEngineEntry()
                {
                    EntryId = i,
                    Body = "This is a sample post",
                    Title = "This is the title of the post",
                    Tags = ".net, mvc, post",
                    BlogName = "MyTestBlog",
                    IsPublished = true,
                    PublishDate = DateTime.Now,
                    EntryName = "this-is-the-title"
                }
                );
            }

            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 20,
                Body = "This is a sample post",
                Title = "This is the title of the post",
                Tags = ".net, mvc, post",
                BlogName = "MyTestBlog",
                IsPublished = false,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            });


            List<SearchEngineResult> result = _service.RelatedContents(1, 100,0) as List<SearchEngineResult>;
            Assert.AreEqual(0, result.Count(r => r.EntryId == 20));
        }

        [Test]
        public void SearchEngineService_Search_DoesntIncludeFuturePosts()
        {
            for (int i = 1; i <= 10; i++)
            {
                _service.AddPost(new SearchEngineEntry()
                {
                    EntryId = i,
                    Body = "This is a sample post",
                    Title = "This is the title of the post",
                    Tags = ".net, mvc, post",
                    BlogName = "MyTestBlog",
                    IsPublished = true,
                    PublishDate = DateTime.Now,
                    EntryName = "this-is-the-title"
                }
                );
            }

            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 20,
                Body = "This is a sample post",
                Title = "This is the title of the post",
                Tags = ".net, mvc, post",
                BlogName = "MyTestBlog",
                IsPublished = true,
                PublishDate = DateTime.Now.AddDays(1),
                EntryName = "this-is-the-title"
            });


            List<SearchEngineResult> result = _service.RelatedContents(1,100,0) as List<SearchEngineResult>;
            Assert.AreEqual(0, result.Count(r => r.EntryId == 20));
        }

        [Test]
        public void SearchEngineService_Search_DoesntIncludePostsFromOtherBlogs()
        {
            for (int i = 1; i <= 10; i++)
            {
                _service.AddPost(new SearchEngineEntry()
                {
                    EntryId = i,
                    Body = "This is a sample post",
                    Title = "This is the title of the post",
                    Tags = ".net, mvc, post",
                    BlogName = "MyTestBlog",
                    BlogId = 1,
                    IsPublished = true,
                    PublishDate = DateTime.Now,
                    EntryName = "this-is-the-title"
                }
                );
            }

            _service.AddPost(new SearchEngineEntry()
            {
                EntryId = 20,
                Body = "This is a sample post",
                Title = "This is the title of the post",
                Tags = ".net, mvc, post",
                BlogName = "MyTestBlog",
                BlogId = 2,
                IsPublished = true,
                PublishDate = DateTime.Now,
                EntryName = "this-is-the-title"
            });


            List<SearchEngineResult> result = _service.RelatedContents(1, 100, 1) as List<SearchEngineResult>;
            Assert.AreEqual(0, result.Count(r => r.EntryId == 20));
        }

        [Test]
        public void SearchEngineService_Search_WhenAllTheSame_ReturnsCorrectNumberOfHits()
        {
            for (int i = 1; i <= 10; i++)
            {
                _service.AddPost(new SearchEngineEntry()
                {
                    EntryId = i,
                    Body = "This is a sample post",
                    Title = "This is the title of the post",
                    Tags = ".net, mvc, post",
                    BlogName = "MyTestBlog",
                    BlogId = 1,
                    IsPublished = true,
                    PublishDate = DateTime.Now,
                    EntryName = "this-is-the-title"
                }
                );
            }

            List<SearchEngineResult> result = _service.RelatedContents(1, 10, 1) as List<SearchEngineResult>;
            Assert.AreEqual(9, result.Count);
        }

    }
}