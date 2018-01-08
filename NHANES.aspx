<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NHANES.aspx.cs" Inherits="HealthData2.NHANES" %>
<%--<%@ Register TagPrefix="fb" TagName="FileBrowser" Src="~/FileBrowser.ascx" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">   
    <script src="Scripts/jquery.cookie.js"></script>
    <script type="text/javascript">
        function pageLoad(sender, args) {
            $('#li_nhanesdata').addClass('active');
            $('#li_nhanes').addClass('active');           
           
        }

        $(document).ready(function () {
            $('#ct101').submit(function () {
                blockUIForDownload();
            });
        });

        var fileDownloadCheckTimer;
        function blockUIForDownload() {
            var token = new Date().getTime(); //use the current timestamp as the token value
            $('#MainContent_download_token_value_id').val(token);
            $('#MainContent_btnSubmit').prop("disabled", true).text("downloading...");
            $(".pdsa-submit-progress").removeClass("hidden");
            fileDownloadCheckTimer = window.setInterval(function () {
                var cookieValue = $.cookie('fileDownloadToken');
                if (cookieValue == token)
                    finishDownload();
            }, 1000);
        }

        function finishDownload() {
            window.clearInterval(fileDownloadCheckTimer);
            $.cookie('fileDownloadToken', null); //clears this cookie value
            $(".pdsa-submit-progress").addClass("hidden");
            $('#MainContent_btnSubmit').prop("disabled", false);

        }

    </script>
    <%--<script src="Scripts/common.js"></script>--%>
</asp:Content>
 
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
                <div class="col-lg-12">
                    <h3 class="page-header">NHANES Data</h3>
                </div>
                <!-- /.col-lg-12 -->
            </div>
            <!-- /.row -->           
            <div class="row">
                <input type="hidden" id="download_token_value_id" runat="server"/>
                <div class="col-lg-12">
                    <div class="panel panel-default">
                        <div class="panel-body">                           
                       
                            <div class="table-responsive">
                                <asp:GridView ID="GridViewStudy" runat="server" AutoGenerateColumns="False" 
                                    class="table table-bordered table-hover"
                                    OnPreRender="GridViewStudy_PreRender"
                                   >
                                    <Columns>
                                        <asp:TemplateField HeaderText="Id" Visible="false">
                                            <ItemTemplate>
                                                <asp:Label ID="lblId" runat="server" Text='<%# Bind("Id") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>                                        
                                        <asp:TemplateField HeaderText="GroupName">
                                            <ItemTemplate>
                                                <asp:Label ID="lblGroupName" runat="server" Text='<%# Bind("GroupName") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="GroupShortName" Visible="False">
                                            <ItemTemplate>
                                                <asp:Label ID="lblGroupShortName" runat="server" Text='<%# Bind("GroupShortName") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="Name">
                                            <ItemTemplate>
                                                <asp:Label ID="lblName" runat="server" Text='<%# Bind("Name") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="1999-2000">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow1999" runat="server"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2001-2002">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2001" runat="server"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2003-2004">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2003" runat="server"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2005-2006">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2005" runat="server"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2007-2008">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2007" runat="server"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2009-2010">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2009" runat="server"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2011-2012">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2011" runat="server"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="FileExists" Visible="false">
                                            <ItemTemplate>
                                                <asp:Label ID="lblFileExists" runat="server" Text='<%# Bind("FileExists") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                </asp:GridView>
                            </div>

                            <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" OnClientClick="blockUIForDownload()" class="btn btn-primary" UseSubmitBehavior="False"/>

                        </div> 
                    </div> 
                </div> 
            </div> 
    
     <div class="pdsa-submit-progress hidden">
        <i class="fa fa-2x fa-spinner fa-spin"></i>
        <label>Please wait while Downloading...</label>
      </div>
    
</asp:Content>
