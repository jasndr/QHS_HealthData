<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="WebScrapper.aspx.cs" Inherits="HealthData.WebScrapper" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="Scripts/common.js"></script>
    <script type="text/javascript">
        function pageLoad(sender, args) {
            $('#li_nhanesdata').addClass('active');
            $('#li_nhanes').addClass('active');
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="row">
        <div class="col-lg-12">
            <h1 class="page-header">
                <small>Web Scrapper</small>
            </h1>
            <ol class="breadcrumb">
                <li>
                    <i class="fa fa-dashboard"></i><a href="Default">Main</a>
                </li>
                <li class="active">
                    <i class="fa fa-file"></i>Web Scrapper
                </li>
            </ol>

        </div>
    </div>

    <div class="row">
        <div class="col-lg-4">
            <label>Page URL</label>
            <asp:TextBox ID="txtUrl" runat="server" class="form-control"></asp:TextBox>
        </div>
    </div>
    <br />
    <div class="row">
        <div class="col-lg-4">
            <div class="form-group">
                <label>Download Folder</label>
                <asp:TextBox ID="txtFolder" runat="server" class="form-control"></asp:TextBox>
            </div>
        </div>
    </div>

    <div class="row">
        <div class="col-lg-6">
            <asp:Button ID="btnScrap" runat="server" Text="Download" OnClientClick="DisplayProgressMessage()" OnClick="btnScrap_Click" class="btn btn-primary" />
        </div>
    </div>
    <br />
    <div class="row">
        <div class="col-lg-6">
            <asp:TextBox ID="TextBox1" runat="server" class="form-control" TextMode="MultiLine" Rows="50"></asp:TextBox>
        </div>
    </div>
    <%-- </div>--%>
    <!-- /.container-fluid -->

    <%--</div>--%>
    <div class="pdsa-submit-progress hidden">
        <i class="fa fa-2x fa-spinner fa-spin"></i>
        <label>Please wait while Downloading...</label>
    </div>
</asp:Content>

