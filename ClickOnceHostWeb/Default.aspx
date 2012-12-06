<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="ClickOnceHostWeb._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
	<h2>
        ClickOnce Download And Install
    </h2>
    <%--<p>
        To learn more about ASP.NET visit <a href="http://www.asp.net" title="ASP.NET Website">www.asp.net</a>.
    </p>
    <p>
        You can also find <a href="http://go.microsoft.com/fwlink/?LinkID=152368&amp;clcid=0x409"
            title="MSDN ASP.NET Docs">documentation on ASP.NET at MSDN</a>.
    </p>--%>
	<p>
		First-time install: <a href="/Setup.exe">Setup.exe</a><br />
		Latest version: <a href="/SourceLog.application">SourceLog.application</a>
	</p>
	<p>
		<a href="https://github.com/tomhunter-gh/SourceLog">GitHub project page</a>
	</p>
</asp:Content>
