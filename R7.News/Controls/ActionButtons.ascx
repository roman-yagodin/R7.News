﻿<%@ Control Language="C#" AutoEventWireup="false" CodeBehind="ActionButtons.ascx.cs" Inherits="R7.News.Controls.ActionButtons" %>
<ul runat="server" class="<%# CssClass %>">
    <asp:ListView id="listActionButtons" runat="server" ItemType="R7.News.Controls.ViewModels.NewsEntryAction">
        <LayoutTemplate>
            <li runat="server" id="itemPlaceholder"></li>
        </LayoutTemplate>
        <ItemTemplate>
            <li>
				<asp:LinkButton runat="server" role="button" 
					Enabled="<%# Item.Enabled %>"
				    aria-disabled="<%# (!Item.Enabled).ToString ().ToLowerInvariant () %>"
					CssClass='<%# "btn btn-sm btn-default" + (Item.Enabled? string.Empty : " disabled") %>'
					Text="<%# LocalizeString (Item.ActionKey) %>"
					CommandName="<%# Item.ActionKey %>" CommandArgument="<%# Item.EntryId %>" OnCommand="linkActionButton_Command" />
            </li>
        </ItemTemplate>
    </asp:ListView>
</ul>