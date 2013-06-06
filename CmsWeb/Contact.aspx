<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs"
    Inherits="CmsWeb.Contact" Title="Person" EnableEventValidation="false" %>

<%@ Register Assembly="CustomControls" Namespace="CustomControls" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">


    <asp:Button ID="RefreshGrids" runat="server" style="display:none" OnClick="RefreshGrids_Click" />      
    <h1>
        <a href="/ContactSearch">Contacts</a></h1>
    <table>
        <tr>
            <th class="right">
                Contact Date:
            </th>
            <td>
                <cc1:DisplayOrEditDate ID="ContactDate" CssClass="datepicker" runat="server" BindingMember="ContactDate"
                    BindingMode="TwoWay" BindingSource="contact">
                </cc1:DisplayOrEditDate>
            </td>
        </tr>
        <tr runat="server" id="TaskRow">
            <th class="right">
                Completed Task:
            </th>
            <td>
                <asp:HyperLink ID="TaskLink" runat="server"></asp:HyperLink>
            </td>
        </tr>
        <tr>
            <th class="right">
                Ministry:
            </th>
            <td>
                <cc1:DisplayOrEditDropDown ID="MinistryId" runat="server" BindingMember="MinistryId"
                    BindingMode="TwoWay" BindingSource="contact" DataTextField="Value" DataValueField="Id"
                    MakeDefault0="True" Width="250px" DataSourceID="ODSMinistryId" AppendDataBoundItems="false"
                    ChangedStatus="False">
                </cc1:DisplayOrEditDropDown>
            </td>
        </tr>
        <tr>
            <th class="right">
                Type:
            </th>
            <td>
                <cc1:DisplayOrEditDropDown ID="ContactTypeId" runat="server" BindingMember="ContactTypeId"
                    BindingMode="TwoWay" BindingSource="contact" DataTextField="Value" DataValueField="Id"
                    MakeDefault0="False" Width="250px" DataSourceID="ODSContactTypeId">
                </cc1:DisplayOrEditDropDown>
            </td>
        </tr>
        <tr>
            <th class="right">
                Reason:
            </th>
            <td>
                <cc1:DisplayOrEditDropDown ID="ContactReasonId" runat="server" BindingMember="ContactReasonId"
                    BindingMode="TwoWay" BindingSource="contact" DataTextField="Value" DataValueField="Id"
                    MakeDefault0="False" Width="250px" DataSourceID="ODSContactReasonId">
                </cc1:DisplayOrEditDropDown>
            </td>
        </tr>
    </table>
    <table>
        <tr>
            <td>
                <table>
                    <tr>
                        <td>
                            <cc1:DisplayOrEditCheckbox ID="NotAtHome" runat="server" BindingSource="contact"
                                Text="Attempted/Not Available" BindingMode="TwoWay" /><br />
                            <cc1:DisplayOrEditCheckbox ID="LeftDoorHanger" runat="server" BindingSource="contact"
                                Text="Left Note Card" BindingMode="TwoWay" /><br />
                            <cc1:DisplayOrEditCheckbox ID="LeftMessage" runat="server" BindingSource="contact"
                                Text="Left Message" BindingMode="TwoWay" /><br />
                        </td>
                        <td>
                            <cc1:DisplayOrEditCheckbox ID="ContactMade" runat="server" BindingSource="contact"
                                Text="Contact Made" BindingMode="TwoWay" /><br />
                            <cc1:DisplayOrEditCheckbox ID="GospelShared" runat="server" BindingSource="contact"
                                Text="Gospel Shared" BindingMode="TwoWay" /><br />
                            <br />
                        </td>
                        <td>
                            <cc1:DisplayOrEditCheckbox ID="PrayerRequest" runat="server" BindingSource="contact"
                                Text="Prayer Request Received" BindingMode="TwoWay" /><br />
                            <cc1:DisplayOrEditCheckbox ID="GiftBagGiven" runat="server" BindingSource="contact"
                                Text="Gift Bag Given" BindingMode="TwoWay" /><br />
                            <br />
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>
                <table class="Design2">
                    <tr>
                        <td height="100%" width="100%">
                            <span id="CommentsSection" runat="server"><strong>Comments:</strong>
                                <br />
                                <cc1:DisplayOrEditText ID="Comments" runat="server" BindingSource="contact" BindingMember="Comments"
                                    BindingMode="TwoWay" TextMode="MultiLine" Height="150" Width="500"></cc1:DisplayOrEditText>
                            </span>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td>
                <br />
                <cc1:EditUpdateButton ID="EditUpdateButton1" runat="server" CheckRole="false" OnClick="EditUpdateButton1_Click" />
            </td>
            <td>
                <cc1:LinkButtonConfirm ID="DeleteButton" Text="Delete" Confirm="Are you sure you want to delete this contact?"
                    runat="server" CommandName="Delete" OnClick="DeleteButton_Click">
                </cc1:LinkButtonConfirm>
            </td>
        </tr>
    </table>
    <br />
    <table>
        <tr>
            <td style="vertical-align: top">
                <strong>Ministry Team:</strong>
                <table style="vertical-align: top">
                    <tr style="vertical-align: top">
                        <td style="vertical-align: top">
                            <asp:GridView ID="ContactorGrid" CssClass="aspgrid" runat="server" AllowPaging="True" AllowSorting="True"
                                AutoGenerateColumns="False" CellPadding="3" EmptyDataText="No Contactors Found"
                                PageSize="10" DataSourceID="ContactorList" DataKeyNames="ContactId, PeopleId"
        ForeColor="#333333" GridLines="None" >
        <PagerSettings Position="TopAndBottom" />
        <FooterStyle BackColor="#4296CC" Font-Bold="True" ForeColor="White" />
        <RowStyle BackColor="#EFF3FB" />
        <PagerStyle CssClass="pagerstyle" />
        <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
        <HeaderStyle BackColor="#4296CC" Font-Bold="True" ForeColor="White" />
        <EditRowStyle BackColor="#EFF3FB"/>
        <AlternatingRowStyle BackColor="White" />
                                <Columns>
                                    <asp:BoundField DataField="PeopleId" HeaderText="PeopleId" SortExpression="Id" Visible="False">
                                        <ItemStyle HorizontalAlign="Right" />
                                    </asp:BoundField>
                                    <asp:TemplateField HeaderText="Name">
                                        <ItemTemplate>
                                            <asp:HyperLink ID="namelink" runat="server" NavigateUrl='<%# Eval("PeopleId", "~/Person/Index/{0}") %>'
                                                Text='<%# Eval("Name") %>'></asp:HyperLink>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Left" VerticalAlign="Middle" />
                                        <ItemStyle HorizontalAlign="Left" VerticalAlign="Middle" />
                                    </asp:TemplateField>
                                    <asp:CommandField CausesValidation="False" ShowEditButton="True" Visible="False" />
                                    <asp:TemplateField ShowHeader="False">
                                        <ItemTemplate>
                                            <cc1:LinkButtonConfirm ID="btnRemoveContactor" Text="Remove" Confirm="Are you sure you want to remove contactor from the list?"
                                                runat="server" CommandName="Delete">
                                            </cc1:LinkButtonConfirm>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; border-top-style: solid">
                            <a id="AddContactorLink" href="/SearchAdd/Index/<%=contact.ContactId %>?type=contactor" title="AddContactor">Add Contactor</a>
                            <p>
                                <asp:LinkButton ID="AddTeamContact" runat="server" OnClick="AddTeamContact_Click">Add another Contact for this team</asp:LinkButton>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
            <td>
            </td>
            <td style="vertical-align: top">
                <strong>Persons ministered to:</strong>
                <table style="vertical-align: top">
                    <tr style="vertical-align: top">
                        <td style="vertical-align: top">
                            <asp:GridView ID="ContacteeGrid" CssClass="aspgrid" runat="server" AllowPaging="True" AllowSorting="True"
                                AutoGenerateColumns="False" CellPadding="3" EmptyDataText="No Conactees Found"
                                PageSize="10" DataSourceID="ContacteeList" DataKeyNames="ContactId,PeopleId"
                                OnRowCreated="ContacteeGrid_RowCreated"
        ForeColor="#333333" GridLines="None" >
        <PagerSettings Position="TopAndBottom" />
        <FooterStyle BackColor="#4296CC" Font-Bold="True" ForeColor="White" />
        <RowStyle BackColor="#EFF3FB" />
        <PagerStyle CssClass="pagerstyle" />
        <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />
        <HeaderStyle BackColor="#4296CC" Font-Bold="True" ForeColor="White" />
        <EditRowStyle BackColor="#EFF3FB"/>
        <AlternatingRowStyle BackColor="White" />
                                <Columns>
                                    <asp:TemplateField ShowHeader="False">
                                        <EditItemTemplate>
                                            <asp:LinkButton ID="UpdateLinkButton" runat="server" CausesValidation="True" CommandName="Update"
                                                Text="Update"></asp:LinkButton>
                                            &nbsp;<asp:LinkButton ID="CancelLinkButton" runat="server" CausesValidation="False"
                                                CommandName="Cancel" Text="Cancel"></asp:LinkButton>
                                        </EditItemTemplate>
                                        <ItemTemplate>
                                            <asp:LinkButton ID="EditLinkButton" runat="server" CausesValidation="False" CommandName="Edit"
                                                Text="Edit" Enabled='<%# CanEdit() %>'></asp:LinkButton>
                                        </ItemTemplate>
                                        <ItemStyle ForeColor="Black" />
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="PeopleId" HeaderText="PeopleId" SortExpression="Id" Visible="False">
                                        <ItemStyle HorizontalAlign="Right" />
                                    </asp:BoundField>
                                    <asp:TemplateField HeaderText="Name">
                                        <ItemTemplate>
                                            <asp:HyperLink ID="namelink" runat="server" NavigateUrl='<%# Eval("PeopleId", "~/Person/Index/{0}") %>'
                                                Text='<%# Eval("Name") %>'></asp:HyperLink>
                                        </ItemTemplate>
                                        <HeaderStyle HorizontalAlign="Left" VerticalAlign="Middle" />
                                        <ItemStyle HorizontalAlign="Left" VerticalAlign="Middle" />
                                    </asp:TemplateField>
                                    <asp:CheckBoxField DataField="ProfessionOfFaith" HeaderText="Profession Of Faith"
                                        SortExpression="ProfessionOfFaith">
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:CheckBoxField>
                                    <asp:CheckBoxField DataField="PrayedForPerson" HeaderText="Prayed For Person" SortExpression="PrayedForPerson">
                                        <ItemStyle HorizontalAlign="Center" />
                                    </asp:CheckBoxField>
                                    <asp:CommandField CausesValidation="False" ShowEditButton="True" Visible="False" />
                                    <asp:TemplateField ShowHeader="False">
                                        <ItemTemplate>
                                            <cc1:LinkButtonConfirm ID="btnRemoveContactee" Text="Remove" Confirm="Are you sure you want to remove contactee from the list?"
                                                runat="server" CommandName="Delete">
                                            </cc1:LinkButtonConfirm>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="AddTask" ShowHeader="False">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="AddTask" runat="server" CommandName="addtask" OnCommand="AddTask_Command"
                                                Visible='<%# Eval("NoTask") %>'>Add task</asp:LinkButton>
                                            <asp:HyperLink ID="GoTask" runat="server" Visible='<%# Eval("HasTask") %>' NavigateUrl='<%# Eval("TaskId", "/Task/List/{0}") %>'>Go task</asp:HyperLink>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; border-top-style: solid">
                          <a id="AddContacteeLink" href="/SearchAdd/Index/<%=contact.ContactId %>?type=contactee" title="Add Contactee">Add Contactee</a>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
    <asp:ObjectDataSource ID="ContacteeList" runat="server" EnablePaging="True" SelectCountMethod="ContacteeCount"
        SelectMethod="ContacteeList" SortParameterName="sortExpression" TypeName="CMSPresenter.ContactController"
        DeleteMethod="DeleteContactee" UpdateMethod="UpdateContactee">
        <DeleteParameters>
            <asp:QueryStringParameter Name="ContactId" QueryStringField="id" Type="Int32" />
            <asp:Parameter Name="PeopleId" Type="Int32" />
        </DeleteParameters>
        <UpdateParameters>
            <asp:QueryStringParameter Name="ContactId" QueryStringField="id" Type="Int32" />
            <asp:Parameter Name="PeopleId" Type="Int32" />
            <asp:Parameter Name="ProfessionOfFaith" Type="Boolean" />
            <asp:Parameter Name="PrayedForPerson" Type="Boolean" />
        </UpdateParameters>
        <SelectParameters>
            <asp:QueryStringParameter Name="cid" QueryStringField="id" Type="Int32" />
            <asp:Parameter Name="sortExpression" Type="String" DefaultValue="Date DESC" />
            <asp:Parameter Name="maximumRows" Type="Int32" />
            <asp:Parameter Name="startRowIndex" Type="Int32" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="ContactorList" runat="server" EnablePaging="True" SelectCountMethod="ContactorCount"
        SelectMethod="ContactorList" SortParameterName="sortExpression" TypeName="CMSPresenter.ContactController"
        DeleteMethod="DeleteContactor">
        <DeleteParameters>
            <asp:QueryStringParameter Name="ContactId" QueryStringField="id" Type="Int32" />
            <asp:Parameter Name="PeopleId" Type="Int32" />
        </DeleteParameters>
        <SelectParameters>
            <asp:QueryStringParameter Name="cid" QueryStringField="id" Type="Int32" />
            <asp:Parameter Name="sortExpression" Type="String" DefaultValue="Date DESC" />
            <asp:Parameter Name="maximumRows" Type="Int32" />
            <asp:Parameter Name="startRowIndex" Type="Int32" />
        </SelectParameters>
    </asp:ObjectDataSource>
    <asp:ObjectDataSource ID="ODSMinistryId" runat="server" SelectMethod="Ministries0"
        TypeName="CmsWeb.Code.CodeValueModel"></asp:ObjectDataSource>
    <asp:ObjectDataSource ID="ODSContactTypeId" runat="server" SelectMethod="ContactTypeCodes"
        TypeName="CmsWeb.Code.CodeValueModel"></asp:ObjectDataSource>
    <asp:ObjectDataSource ID="ODSContactReasonId" runat="server" SelectMethod="ContactReasonCodes"
        TypeName="CmsWeb.Code.CodeValueModel"></asp:ObjectDataSource>
    <asp:LinkButton ID="HiddenDoNothingButton" runat="server" Style="display: none"></asp:LinkButton>
    <div id="contactDialog" style="display:none">
    <iframe style="width:100%;height:99%;border-width:0px;"></iframe>
    </div>
    <script type="text/javascript">
        $(function () {
            $('#contactDialog').dialog({
                autoOpen: false,
                width: 750,
                height: 700,
                modal: true,
                overlay: {
                    opacity: 0.5,
                    background: "black"
                }, close: function () {
                    $('iframe', this).attr("src", "");
                }
            });
            $('body').on("click", '#AddContacteeLink,#AddContactorLink', function (e) {
                e.preventDefault();
                var d = $('#contactDialog');
                $('iframe', d).attr("src", this.href);
                d.dialog("option", "title", this.title);
                d.dialog("open");
            });
            $("input.datepicker").datepicker();
        });
       function AddSelected() {
           var d = $('#contactDialog');
           d.dialog("close");
           $('#<%= RefreshGrids.ClientID %>').click();
        }
    </script>
</asp:Content>