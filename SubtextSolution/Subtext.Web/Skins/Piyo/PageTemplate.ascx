<%@ Control %>
<%@ Register TagPrefix="DT" Namespace="Subtext.Web.UI.WebControls" Assembly="Subtext.Web" %>
<%@ Register TagPrefix="uc1" TagName="Footer" Src="Controls/Footer.ascx" %>
<%@ Register TagPrefix="uc1" TagName="News" Src="Controls/News.ascx" %>
<%@ Register TagPrefix="uc1" TagName="SingleColumn" Src="Controls/SingleColumn.ascx" %>
<%@ Register TagPrefix="uc1" TagName="MyLinks" Src="Controls/MyLinks.ascx" %>
<%@ Register TagPrefix="uc1" TagName="Header" Src="Controls/Header.ascx" %>
<%@ Register TagPrefix="uc1" TagName="Calendar" Src="Controls/SubTextBlogCalendar.ascx" %>


<div id="main">

	<div id="header">
		<uc1:header id="Header1" runat="server" />		
	</div>
	<div id="contentHeadLeft"><div id="contentHeadRight"><div id="contentHeadCenter"></div></div></div>
		<div id="contentBodyLeft">
		<div id="contentBodyRight">
			<div id="contentBodyCenter">
				<div id="content">
				
					<div id="entries">
					<dt:contentregion id="MPMain" runat="server" />
					</div>
					<div id="column">
						
						<uc1:Calendar id="cal" runat="server" />
						<uc1:News id="news" runat="server" />
						<uc1:MyLinks id="links" runat="server" />
						<uc1:SingleColumn id="column" runat="server" />
						<div id="subtext">
							<p><asp:hyperlink imageurl="~/images/PoweredBySubtext85x33.png" alt="Powered By SubText" navigateurl="http://sourceforge.net/projects/subtext/" runat="server" id="Hyperlink2" name="Hyperlink1" title="Click here to visit the homepage of the SubText project"/></p>
						</div>
					</div>
				</div>
				<div class="clear">&nbsp;</div>
			</div>
		</div>
	</div>
	<div id="contentFootLeft"><div id="contentFootRight"><div id="contentFootCenter"></div></div></div>

	<div id="Footer">
		<uc1:footer id="Footer1" runat="server" />
	</div>
	
</div>