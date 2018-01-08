<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NHANES2.aspx.cs" Inherits="HealthData2.NHANES2" %>
<%--<%@ Register TagPrefix="fb" TagName="FileBrowser" Src="~/FileBrowser.ascx" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">   
    <script src="Scripts/jquery.cookie.js"></script>
    <script type="text/javascript">
        var Grid = null;
        var UpperBound = 0;
        var LowerBound = 2;
        var CollapseImage = 'images/collapse.gif';
        var ExpandImage = 'images/expand.gif';
        var DemoIsExpanded = true;
        var DietIsExpanded = true;
        var ExamIsExpanded = true;
        var LabIsExpanded = true;
        var QuesIsExpanded = true;
        var IsExpanded = true;
        var Rows = null;
        var n = 1;
        var TimeSpan = 0;

        var dict = new Object();

        function pageLoad(sender, args) {
            $('#li_nhanesdata').addClass('active');
            $('#li_nhanes').addClass('active');           
           
            Grid = document.getElementById('<%= this.GridViewStudy.ClientID %>');
            <%--UpperBound = parseInt('<%= this.GridViewStudy.Rows.Count %>');--%>
            UpperBound = 2;
            Rows = Grid.getElementsByTagName('tr');
        }

        function Toggle(Image) {            
            ToggleImage(Image);   
            ToggleRows();
        }

        function ToggleImage(Image) {
            var group = document.getElementById(Image.id).nextElementSibling;
            var groupName = group.innerText || group.textContent;
            //alert(groupName);
            switch (groupName) {
                case "Demographics":
                    LowerBound = 2;
                    UpperBound = 2;
                    IsExpanded = DemoIsExpanded;
                    DemoIsExpanded = !DemoIsExpanded;
                    break;
                case "Dietary":
                    LowerBound = 4;
                    UpperBound = 28;
                    IsExpanded = DietIsExpanded;
                    DietIsExpanded = !DietIsExpanded;
                    break;
                case "Examination":
                    LowerBound = 30;
                    UpperBound = 63;
                    IsExpanded = ExamIsExpanded;
                    ExamIsExpanded = !ExamIsExpanded;
                    break;
                case "Laboratory":
                    LowerBound = 65;
                    UpperBound = 252;
                    //UpperBound = 74;
                    IsExpanded = LabIsExpanded;
                    LabIsExpanded = !LabIsExpanded;
                    break;
                case "Questionnaire":
                    LowerBound = 254;
                    UpperBound = 321;
                    //LowerBound = 76;
                    //UpperBound = 77;
                    IsExpanded = QuesIsExpanded;
                    QuesIsExpanded = !QuesIsExpanded;
                    break;
            }

            //alert(IsExpanded);
            if (IsExpanded) {
                Image.src = ExpandImage;
                Image.title = 'Expand';
                Grid.rules = 'none';
                n = LowerBound;

                //IsExpanded = false;
            }
            else {
                Image.src = CollapseImage;
                Image.title = 'Collapse';
                Grid.rules = 'cols';
                n = UpperBound;

                //IsExpanded = true;
            }

            //alert(LowerBound + ' ' + UpperBound + ' ' + n);
        }

        function ToggleRows() {
            if (n < LowerBound || n > UpperBound) return;

            Rows[n].style.display = Rows[n].style.display == '' ? 'none' : '';
            if (!IsExpanded) n--; else n++;
            setTimeout("ToggleRows()", TimeSpan);
        }

        $(document).ready(function () {
            $('#ct101').submit(function () {
                blockUIForDownload();
            });
        });

        function ShowColumns(CheckBox) {
            if (CheckBox.checked) {
                //alert($(CheckBox).parent().attr('columnname'));
                $(".pdsa-column-display").removeClass("hidden");

                var columns = $(CheckBox).parent().attr('columnname').split(',');
                //var studyName = $(CheckBox).closest('td').prev('td').text();
                
                var id = $(CheckBox).parent().attr('id');
                var yearfrom = $(CheckBox).parent().attr('yearfrom');
                var yearto = parseInt(yearfrom) + 1;
                var groupname = $(CheckBox).parent().attr('groupname');
                var foldername = $(CheckBox).parent().attr('foldername');
                var codebook = $(CheckBox).parent().attr('codebook');
                var studyname = yearfrom + '-' + yearto + '/' + groupname + '/' + foldername;
                var selectedList = [];
                var cookieId = yearfrom + id;

                //var html = "<label title='wwwn.cdc.gov/Nchs/Nhanes/2011-2012/CBC_G.htm' for='" + studyname + "'>" + studyname + "</label><br/>";
                var html = "<a href='http://" + codebook + "' color='white' target='_blank' title='code book'" + "'>" + studyname + "</a>"; 
                html += "<span id='closeDiv' onclick='closeWindow()'>x</span><br />&nbsp;";
                html += "<div class='row'>";
                for (var i = 0; i < columns.length-1; i++) {
                    var columnName = columns[i];
                    html += "<div class='col-md-4'><input type='checkbox' name='columns' id='" + columnName + "' class='module' />";
                    html += "<label for='" + columnName + "'>" + columnName + "</label></div>";
                    if ((i + 1) % 3 == 0) {
                        html += "</div>"
                        html += "<div class='row'>";
                    }
                }

                html += "<div class='row'><br />&nbsp;</div>"
                html += "&emsp;<div class='col-md-4'><input type='button' id='btnClose' class='btn btn-primary' value='Close' onclick='closeWindow()' /><br /></div>&emsp;";
                html += "</div>"
                
                
                $("#moduleListTitle").html(html);
                $('#moduleListTitle').parent().trigger('create');
                
                $('input:checkbox.module').on('change', function () {
                    //console.log(this.tagName);
                    //alert(this.id);
                    //console.log(this.id);
                    //console.log("-----");
                    //selectedList.push({ id: id, value: this.id + ','});                    
                    //SetCookie(studyname, this.id);
                    if ($(this).is(':checked')) {
                        if (dict[cookieId]) {
                            dict[cookieId] += this.id + ',';
                        }
                        else {                            
                            ClearCookie(cookieId);
                            dict[cookieId] = this.id + ',';
                        }                            
                    }
                    else {
                        dict[cookieId] = dict[cookieId].replace(this.id + ',', '');
                    }
                       
                   
                });

                //if (selectedList.length > 0) {
                //    var columns;
                //    for (i = 0; i < selectedList.length; i++) {
                //        columns += selectedList[i].value;
                //    }

                //    SetCookie(id, columns);
                //}

            }
            else {
                $(".pdsa-column-display").addClass("hidden");
            }
        }

        function ClearCookie(name) {
            document.cookie = name + '=;expires=Thu, 01 Jan 2000 00:00:01 GMT;';
        }

        function SetCookie(name, value)
        {
            document.cookie = name + "=" + escape(value);
        }

        function closeWindow() {
            //alert("test");
            for (var key in dict) {
                SetCookie(key, dict[key]);
            }

            $(".pdsa-column-display").addClass("hidden");
        }

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
                                    OnRowDataBound="GridViewStudy_DataBound"
                                    OnRowCreated="GridViewStudy_RowCreated"
                                    OnRowCommand="GridViewStudy_RowCommand"
                                   >
                                    <HeaderStyle CssClass="ColumnHeaderStyle" />
                                    <Columns>
                                        <asp:TemplateField HeaderText="DataSet">
                                            <ItemTemplate>
                                                <%--<asp:ImageButton ID="imgExpand" runat="server" Visible="false"
                                                    ImageUrl="images/expand.gif" Height="16px"
                                                    CommandName="Expand" />--%>
                                                <asp:Image ID="imgCollapse" runat="server" onclick="javascript:Toggle(this);"
                                                    Visible="true" ImageUrl="~/images/collapse.gif"
                                                    Height="17px" Width="14px"  />
                                                <asp:Label ID="lblGroupName" runat="server"
                                                    Text='<%#Eval("GroupName")%>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                       <%-- <asp:TemplateField>
                                            <HeaderStyle Width="25px" />
                                            <ItemStyle Width="25px" BackColor="White" />
                                            <HeaderTemplate>
                                                <asp:Image ID="imgTab" onclick="javascript:Toggle(this);" runat="server" ImageUrl="~/images/minus.gif"
                                                    ToolTip="Collapse" />
                                            </HeaderTemplate>
                                        </asp:TemplateField>--%>
                                        <asp:TemplateField HeaderText="Id" Visible="false">
                                            <ItemTemplate>
                                                <asp:Label ID="lblId" runat="server" Text='<%# Bind("Id") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>                                        
                                        <%--<asp:TemplateField HeaderText="GroupName">
                                            <ItemTemplate>
                                                <asp:Label ID="lblGroupName" runat="server" Text='<%# Bind("GroupName") %>'></asp:Label>
                                            </ItemTemplate>
                                        </asp:TemplateField>--%>
                                        
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
                                                <asp:CheckBox ID="chkRow1999" runat="server" onclick="javascript:ShowColumns(this)"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2001-2002">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2001" runat="server" onclick="javascript:ShowColumns(this)"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2003-2004">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2003" runat="server" onclick="javascript:ShowColumns(this)"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2005-2006">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2005" runat="server" onclick="javascript:ShowColumns(this)"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2007-2008">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2007" runat="server" onclick="javascript:ShowColumns(this)"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2009-2010">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2009" runat="server" onclick="javascript:ShowColumns(this)"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2011-2012">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2011" runat="server" onclick="javascript:ShowColumns(this)"></asp:CheckBox>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:TemplateField HeaderText="2013-2014">
                                            <ItemTemplate>
                                                <asp:CheckBox ID="chkRow2013" runat="server" onclick="javascript:ShowColumns(this)"></asp:CheckBox>
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
                            <div class="row">
                                <div class='col-md-11'></div>
                                <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" OnClientClick="blockUIForDownload()" class="btn btn-primary" UseSubmitBehavior="False"/>
                            </div>
                        </div> 
                    </div> 
                </div> 
            </div> 
    
     <div class="pdsa-submit-progress hidden">
        <i class="fa fa-2x fa-spinner fa-spin"></i>
        <label>Please wait while Downloading...</label>
      </div>

    <div id="moduleListTitle" class="pdsa-column-display hidden">
        <%--<asp:Button id="b1" Text="Close" runat="server" OnClientClick="close()" />--%>
    </div>
    
</asp:Content>
