﻿#region Disclaimer/Info
///////////////////////////////////////////////////////////////////////////////////////////////////
// Subtext WebLog
// 
// Subtext is an open source weblog system that is a fork of the .TEXT
// weblog system.
//
// For updated news and information please visit http://subtextproject.com/
// Subtext is hosted at Google Code at http://code.google.com/p/subtext/
// The development mailing list is at subtext-devs@lists.sourceforge.net 
//
// This project is licensed under the BSD license.  See the License.txt file for more information.
///////////////////////////////////////////////////////////////////////////////////////////////////
#endregion

using System;
using System.Linq;
using Subtext.Extensibility.Interfaces;
using Subtext.Framework.Configuration;
using Subtext.Framework.Providers;
using Subtext.Framework.Web.HttpModules;

namespace Subtext.Framework.Services
{
    public class BlogLookupService : IBlogLookupService
    {
        public BlogLookupService(ObjectProvider repository, HostInfo host)
        {
            Repository = repository;
            _host = host;
        }

        protected ObjectProvider Repository
        {
            get;
            private set;
        }

        protected HostInfo Host
        {
            get
            {
                if (_host == null)
                {
                    _host = HostInfo.LoadHost(true);
                }
                return _host;
            }
        }
        HostInfo _host;

        public BlogLookupResult Lookup(BlogRequest blogRequest)
        {
            if (Host == null)
            {
                return new BlogLookupResult(null, null);
            }

            string host = blogRequest.Host;
            Blog blog = Repository.GetBlog(host, blogRequest.Subfolder, true /* strict */);
            if (blog != null)
            {
                return new BlogLookupResult(blog, null);
            }

            string alternateHost = GetAlternateHostAlias(host);
            blog = Repository.GetBlog(alternateHost, blogRequest.Subfolder, true /* strict */);
            if (blog != null)
            {
                Uri alternateUrl = ReplaceHost(blogRequest.RawUrl, alternateHost).Uri;
                return new BlogLookupResult(null, alternateUrl);
            }

            blog = Repository.GetBlogByDomainAlias(host, blogRequest.Subfolder, true /* strict */);
            if (blog != null)
            {
                UriBuilder alternateUrl = ReplaceHost(blogRequest.RawUrl, blog.Host);
                alternateUrl = ReplaceSubfolder(alternateUrl, blogRequest, blog.Subfolder);
                return new BlogLookupResult(null, alternateUrl.Uri);
            }

            IPagedCollection<Blog> pagedBlogs = Repository.GetPagedBlogs(null, 0, 10, ConfigurationFlags.None);
            int totalBlogCount = pagedBlogs.MaxItems;
            if (Host.BlogAggregationEnabled && totalBlogCount > 0)
            {
                return new BlogLookupResult(Host.AggregateBlog, null);
            }

            if (totalBlogCount == 1)
            {
                Blog onlyBlog = pagedBlogs.First();
                if (onlyBlog.Host == blogRequest.Host)
                {
                    Uri onlyBlogUrl = ReplaceSubfolder(new UriBuilder(blogRequest.RawUrl), blogRequest, onlyBlog.Subfolder).Uri;
                    return new BlogLookupResult(null, onlyBlogUrl);
                }

                //Extra special case to deal with a common deployment problem where dev uses "localhost" on 
                //dev machine. But deploys to real domain.
                if (OnlyBlogIsLocalHostNotCurrentHost(host, onlyBlog))
                {
                    onlyBlog.Host = host;
                    Repository.UpdateBlog(onlyBlog);

                    if (onlyBlog.Subfolder != blogRequest.Subfolder)
                    {
                        Uri onlyBlogUrl = ReplaceSubfolder(new UriBuilder(blogRequest.RawUrl), blogRequest, onlyBlog.Subfolder).Uri;
                        return new BlogLookupResult(null, onlyBlogUrl);
                    }
                    return new BlogLookupResult(onlyBlog, null);
                }

                //return new BlogLookupResult(blog, null);
            }

            return null;
        }

        private static bool OnlyBlogIsLocalHostNotCurrentHost(string host, Blog onlyBlog)
        {
            return (
                !String.Equals("localhost", host, StringComparison.OrdinalIgnoreCase)
                && String.Equals("localhost", onlyBlog.Host, StringComparison.OrdinalIgnoreCase)
            )
            || (
               !String.Equals("127.0.0.1", host, StringComparison.OrdinalIgnoreCase)
               && String.Equals("127.0.0.1", onlyBlog.Host, StringComparison.OrdinalIgnoreCase)
            );
        }

        private int GetBlogCount()
        {
            IPagedCollection pagedBlogs = Repository.GetPagedBlogs(null, 0, 10, ConfigurationFlags.None);
            return pagedBlogs.MaxItems;
        }

        private UriBuilder ReplaceHost(Uri originalUrl, string newHost)
        {
            UriBuilder builder = new UriBuilder(originalUrl);
            builder.Host = newHost;
            return builder;
        }

        private UriBuilder ReplaceSubfolder(UriBuilder originalUrl, BlogRequest blogRequest, string newSubfolder)
        {
            if (!String.Equals(blogRequest.Subfolder, newSubfolder, StringComparison.OrdinalIgnoreCase))
            {
                string appPath = blogRequest.ApplicationPath;
                if (!appPath.EndsWith("/"))
                {
                    appPath += "/";
                }

                int indexAfterAppPath = appPath.Length;
                if (!String.IsNullOrEmpty(blogRequest.Subfolder))
                {
                    originalUrl.Path = originalUrl.Path.Remove(indexAfterAppPath, blogRequest.Subfolder.Length + 1);
                }
                if (!String.IsNullOrEmpty(newSubfolder))
                {
                    originalUrl.Path = originalUrl.Path.Substring(0, indexAfterAppPath) + newSubfolder + "/" + originalUrl.Path.Substring(indexAfterAppPath);
                }
            }
            return originalUrl;
        }

        /// <summary>
        /// If the host starts with www., gets the host without the www. If it 
        /// doesn't start with www., returns the host with www.
        /// </summary>
        /// <param name="host">Host.</param>
        /// <returns></returns>
        private string GetAlternateHostAlias(string host)
        {
            if (String.IsNullOrEmpty(host))
                throw new ArgumentNullException("host");

            if (host.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                return host.Substring(4);
            else
                return "www." + host;
        }
    }
}