﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using MbUnit.Framework;
using Moq;
using Moq.Stub;
using Subtext.Framework;
using Subtext.Framework.Providers;
using Subtext.Framework.Routing;
using Subtext.Framework.Syndication;

namespace UnitTests.Subtext.Framework.Syndication
{
    [TestFixture]
    public class OpmlHandlerTests
    {
        [Test]
        public void OpmlHandler_WithRequest_SetsContentTypeToXml() { 
            //arrange
            var context = new Mock<ISubtextContext>();
            context.Stub(c => c.HttpContext.Response.ContentType);
            context.Setup(c => c.HttpContext.Response.Output).Returns(new StringWriter());
            context.SetupUrlHelper(new Mock<UrlHelper>());
            var writer = new Mock<OpmlWriter>();
            writer.Setup(w => w.Write(It.IsAny<IEnumerable<Blog>>(), It.IsAny<TextWriter>(), It.IsAny<UrlHelper>()));
            OpmlHandler handler = new OpmlHandler(context.Object, writer.Object);

            //act
            handler.ProcessRequest(new HostInfo());

            //assert
            Assert.AreEqual("text/xml", context.Object.HttpContext.Response.ContentType);
        }

        [Test]
        public void OpmlHandler_WithRequestForAggregateBlog_GetsGroupIdFromQueryString()
        {
            //arrange
            var queryString = new NameValueCollection();
            queryString.Add("GroupID", "310");
            
            var context = new Mock<ISubtextContext>();
            context.Stub(c => c.HttpContext.Response.ContentType);
            context.Setup(c => c.HttpContext.Response.Output).Returns(new StringWriter());
            context.Setup(c => c.HttpContext.Request.QueryString).Returns(queryString);
            context.SetupUrlHelper(new Mock<UrlHelper>());
            var repository = new Mock<ObjectProvider>();
            int? parsedGroupId = null;
            repository.Setup(r => r.GetBlogsByGroup(It.IsAny<string>(), It.IsAny<int?>())).Callback<string, int?>((host, groupId) => parsedGroupId = groupId);
            context.SetupRepository(repository);

            var writer = new Mock<OpmlWriter>();
            writer.Setup(w => w.Write(It.IsAny<IEnumerable<Blog>>(), It.IsAny<TextWriter>(), It.IsAny<UrlHelper>()));
            OpmlHandler handler = new OpmlHandler(context.Object, writer.Object);
            var hostInfo = new HostInfo();
            hostInfo.BlogAggregationEnabled = true;
            hostInfo.AggregateBlog = new Blog();

            //act
            handler.ProcessRequest(hostInfo);

            //assert
            Assert.AreEqual(310, parsedGroupId.Value);
        }
    }
}