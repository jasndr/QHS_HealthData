<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NHIS.aspx.cs" Inherits="HealthData2.NHIS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-lg-12">
            <h3 class="page-header">NHIS Data</h3>
        </div>
    </div>
    
    <div id="divErrorMsg">
        <span></span>
    </div>

    <div class="panel-body">
        <div class="table-responsive">
            <asp:GridView ID="GridViewStudy" runat="server" AutoGenerateColumns="False"
                class="table table-bordered table-hover" UseAccessibleHeader="True"
                OnPreRender="GridViewStudy_PreRender">
                <HeaderStyle CssClass="table-header" />
                <Columns>
                    <asp:TemplateField HeaderText="Id" Visible="False">
                        <ItemTemplate>
                            <asp:Label ID="lblId" runat="server" Text='<%# Bind("Id") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="File Name">
                        <ItemTemplate>
                            <asp:Label ID="lblGroupName" runat="server" Text='<%# Bind("Name") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2004">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2004" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2005">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2005" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2006">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2006" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2007">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2007" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2008">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2008" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2009">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2009" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2010">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2010" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2011">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2011" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2012">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2012" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2013">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2013" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2014">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2014" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="2015">
                        <ItemTemplate>
                            <asp:CheckBox ID="chkRow2015" runat="server" CssClass="checkbox js-check-file"></asp:CheckBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                   <%-- <asp:TemplateField HeaderText="FileExists" Visible="false">
                        <ItemTemplate>
                            <asp:Label ID="lblFileExists" runat="server" Text='<%# Bind("FileExists") %>'></asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>--%>
                </Columns>
            </asp:GridView>
        </div>
        <div class="row">
            <div class='col-md-11'></div>
            <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" OnClientClick="blockUIForDownload()" class="btn btn-primary" UseSubmitBehavior="False" />
        </div>
    </div>
    <div class="pdsa-submit-progress hidden">
        <i class="fa fa-2x fa-spinner fa-spin"></i>
        <label>Please wait while Downloading...</label>
    </div>

    <div id="moduleListTitle" class="pdsa-column-display hidden">
        <span id = "fileId" class="hidden"></span>
        <strong><span id = "yearId"></span></strong>
        <strong><span id = "groupId"></span></strong>
        <a href="https://www.cdc.gov/nchs/nhis" id="nhisUrl" target="_blank" title="Data Dictionary">National Health Interview Survey</a>
        <br />
        <input type="checkbox" id="checkAll" /> Select All 
        <ul id="eachList">
        </ul>
        <div class="margin-bottom">
            <input type="button" class="js-button-close btn btn-primary col-lg-offset-9" value="Close" />
        </div>
        
    </div>
    
    <script id="each" type="text/html">     
          <ul class="columnlist" id="listColumn">
               {{each ColumnName}}<li><input type="checkbox" name="field" /><label>${$value}</label></li>{{/each}}
          <ul>      
    </script>
    
    <div>
        <asp:HiddenField runat="server" id="selectedFiles"/>
        <input type="hidden" id="download_token_value_id" runat="server"/>
    </div>
    
    <script src="Scripts/jquery.cookie.js"></script>
    <script src="Scripts/jquery.tmpl.min.js"></script>
    <%--<script src="Scripts/knockout-3.4.2.js"></script>--%>
    <%--<script src="Scripts/NHIS_data.js"></script>--%>

    <script type="text/javascript">
        $(function () {
            $(".js-button-close").click(function () {
                var id = $("#fileId").text();
                if ($("[id=listColumn] input:checked").length > 0) {
                    var columns = "";
                    $("[id=listColumn] li").each(function (i, li) {
                        if ($(li).find('input[type="checkbox"]').prop('checked'))
                            //$("#divSelected span").append($(li).text() + ",");
                            columns += $(li).text() + ",";
                    });
                    
                    $('#' + id).attr('columnname', columns);
                } else {
                    $('#' + id + ' input').prop('checked', false);
                }

                $(".pdsa-column-display").addClass("hidden");
            });

            $('ul').delegate('li', 'click', function () {
                var item = $.tmplItem(this);
                //console.log(item.data.FileName);
                if ($("[id=listColumn] input:checked").length == $("[id=listColumn] input").length) {
                    $("[id=checkAll]").prop('checked', true);
                } else {
                    $("[id=checkAll]").prop('checked', false);
                }
            });
            
            $("[id=checkAll]").bind("click", function () {
                if ($(this).is(":checked")) {
                    //console.log("check all2");
                    $("[id=listColumn] input").prop('checked', true);
                } else {
                    $("[id=listColumn] input").prop('checked', false);
                }
            });
            $("[id=listColumn] input").bind("click", function () {
                console.log("check item");
                if ($("[id=listColumn] input:checked").length == $("[id=listColumn] input").length) {
                    $("[id=checkAll]").prop('checked', true);
                } else {
                    $("[id=checkAll]").prop('checked', false);
                }
            });

            $(".js-check-file").click(function (e) {
                var checkbox = $(e.target);
                if ($(checkbox).context.checked) {
                    var id = $(checkbox).parent().attr("id");
                    var yearId = $(checkbox).parent().attr("yearfrom");
                    $("#yearId").text(yearId);
                    $("#groupId").text($(checkbox).parent().attr("groupname"));
                    $('#nhisUrl').attr('href', 'https://www.cdc.gov/nchs/nhis/nhis_' + yearId + '_data_release.htm');
                    $('#nhisUrl').text('NHIS ' + yearId + ' Data Release');
                   
                    var uri = getBaseUrl() + 'api/NHIS/' + id;
                    $.getJSON(uri)
                        .done(function (data) {
                            $("[id=checkAll]").prop('checked', false);
                            $("#fileId").text(id);
                            $("#eachList").html("");
                            $('#each').tmpl(data).appendTo('#eachList');
                        })
                        .fail(function (jqXHR, textStatus, err) {
                            $("#divErrorMsg span").text('Error: ' + err);
                        });

                    $(".pdsa-column-display").removeClass("hidden");
                    $(".pdsa-column-display").scrollTop(0);
                }
                else {
                    $(".pdsa-column-display").scrollTop(0);
                    $(".pdsa-column-display").addClass("hidden");
                }
            });
        });

        var fileDownloadCheckTimer;
        function blockUIForDownload() {
            saveSelectedFiles();

            var token = new Date().getTime(); //use the current timestamp as the token value
            $('#MainContent_download_token_value_id').val(token);
            $('#MainContent_btnSubmit').prop("disabled", true).text("downloading...");
            $(".pdsa-submit-progress").removeClass("hidden");
            var fileDownloadCheckTimer = window.setInterval(function () {
                var cookieValue = $.cookie('fileDownloadToken');
                if (cookieValue == token)
                    finishDownload();
            }, 3000);
        }

        function finishDownload() {
            window.clearInterval(fileDownloadCheckTimer);
            $.cookie('fileDownloadToken', null); //clears this cookie value
            $(".pdsa-submit-progress").addClass("hidden");
            $('#MainContent_btnSubmit').prop("disabled", false);

        }

        function saveSelectedFiles() {
            var str ="";
            $("[id=MainContent_GridViewStudy] input[type='checkbox']:checked").each(function (i, chkbox) {
                var obj = {
                    Id: $(chkbox).parent().attr('id'),
                    GroupName: $(chkbox).parent().attr('groupname'),
                    FolderName: $(chkbox).parent().attr('foldername'),
                    FileName: $(chkbox).parent().attr('filename'),
                    Columns: $(chkbox).parent().attr('columnname')
                }

                str += JSON.stringify(obj) + ';';
                //console.log(JSON.stringify(obj));
                $(chkbox).prop('checked', false);
            });

            $('#MainContent_selectedFiles').val(str);
        }

        function getBaseUrl() {
            var re = new RegExp(/^.*\//);
            return re.exec(window.location.href);
        }

    </script>

</asp:Content>
