<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChurchAttendanceRpt.aspx.cs"
    MasterPageFile="~/Report/Reports.Master" Inherits="CmsWeb.Reports.ChurchAttendanceRpt" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style type="text/css">
        body
        {
            font-size: 110%;
        }
        .totalrow td
        {
            border-top: 2px solid black;
            font-weight: bold;
        }
        .headerrow td, th
        {
            border-bottom: 2px solid black;
        }
    </style>
    <%: Helper.IncludeJs() %>
    <script type="text/javascript">
        $(function () {
            $("input.datepicker").datepicker();
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div style="text-align:center">
        <h1>
            Church Attendance</h1>
        Sunday Date:
        <asp:TextBox ID="SundayDate" CssClass="datepicker" runat="server" AutoPostBack="True" Width="100" Style="font-size: 110%"></asp:TextBox>
        <hr />
        <table class="center">
            <tr style="vertical-align: top">
                <td>
                    <asp:ListView ID="Decisions" runat="server" DataSourceID="dsDecisions">
                        <LayoutTemplate>
                            <table id="itemPlaceholderContainer" runat="server" border="0" cellspacing="0" cellpadding="2"
                                style="width: 250px">
                                <tr class="headerrow">
                                    <th colspan="2">
                                        Decisions
                                    </th>
                                </tr>
                                <tr id="itemPlaceholder" runat="server">
                                </tr>
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr <%# (string)Eval("Name") == "Total" ? "class='totalrow'" : "" %> style='background-color: <%# (Container.DataItemIndex % 2 == 0)?"#eee":"#fff" %>'>
                                <td class="left">
                                    <asp:Label ID="lblSource" runat="server" Text='<%# Eval("Name")%>'></asp:Label>
                                </td>
                                <td class="right">
                                    <asp:Label ID="lblCount" runat="server" Text='<%# Eval("Count","{0:#,0}")%>'></asp:Label>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <EmptyDataTemplate>
                            <table id="Table1" runat="server" style="">
                                <tr>
                                    <td>
                                        No data was returned.
                                    </td>
                                </tr>
                            </table>
                        </EmptyDataTemplate>
                    </asp:ListView>
                </td>
                <td>
                    &nbsp; &nbsp;
                </td>
                <td>
                    <asp:ListView ID="Baptisms" runat="server" DataSourceID="dsBaptisms">
                        <LayoutTemplate>
                            <table id="itemPlaceholderContainer" runat="server" border="0" cellspacing="0" cellpadding="2"
                                style="width: 250px">
                                <tr class="headerrow">
                                    <th colspan="2">
                                        Baptisms
                                    </th>
                                </tr>
                                <tr id="itemPlaceholder" runat="server">
                                </tr>
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr <%# (string)Eval("Name") == "Total" ? "class='totalrow'" : "" %> style='background-color: <%# (Container.DataItemIndex % 2 == 0)?"#eee":"#fff" %>'>
                                <td class="left">
                                    <asp:Label ID="lblSource" runat="server" Text='<%# Eval("Name")%>'></asp:Label>
                                </td>
                                <td class="right">
                                    <asp:Label ID="lblCount" runat="server" Text='<%# Eval("Count","{0:#,0}")%>'></asp:Label>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <EmptyDataTemplate>
                            <table id="Table1" runat="server" style="">
                                <tr>
                                    <td>
                                        No data was returned.
                                    </td>
                                </tr>
                            </table>
                        </EmptyDataTemplate>
                    </asp:ListView>
                </td>
            </tr>
        </table>
    </div>
    <!-- Data Source Objects -->
    <asp:ObjectDataSource ID="dsBaptisms" runat="server" SelectMethod="Baptisms" TypeName="CMSPresenter.ChurchAttendanceController" onobjectcreating="ODS_ObjectCreating">
        <SelectParameters>
            <asp:ControlParameter ControlID="SundayDate" Name="sunday" Type="DateTime" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="dsDecisions" runat="server" SelectMethod="Decisions" TypeName="CMSPresenter.ChurchAttendanceController" onobjectcreating="ODS_ObjectCreating">
        <SelectParameters>
            <asp:ControlParameter ControlID="SundayDate" Name="sunday" Type="DateTime" />
        </SelectParameters>
    </asp:ObjectDataSource>
</asp:Content>
