﻿using Implem.DefinitionAccessor;
using Implem.Pleasanter.Libraries.Html;
using Implem.Pleasanter.Libraries.Responses;
namespace Implem.Pleasanter.Libraries.HtmlParts
{
    public static class HtmlCopies
    {
        public static HtmlBuilder CopyDialog(
            this HtmlBuilder hb, string referenceType, long id)
        {
            return hb.Div(
                attributes: new HtmlAttributes()
                    .Id("CopyDialog")
                    .Class("dialog")
                    .Title(Displays.CopySettings()),
                action: () => hb
                    .Form(
                        attributes: new HtmlAttributes()
                            .Action(Navigations.Action(referenceType, id)),
                        action: () => hb
                            .FieldCheckBox(
                                controlId: "CopyWithComments",
                                labelText: Displays.CopyWithComments(),
                                _checked: true,
                                fieldCss: "field-wide",
                                controlContainerCss: "m-l50",
                                controlCss: " must-transport",
                                labelPositionIsRight: true)
                            .P(css: "message-dialog")
                            .Div(css: "command-center", action: () => hb
                                .Button(
                                    text: Displays.Copy(),
                                    controlCss: "button-copy",
                                    onClick: "$p.sendByDialog($(this));",
                                    action: "Copy",
                                    method: "post")
                                .Button(
                                    text: Displays.Cancel(),
                                    controlCss: "button-cancel",
                                    onClick: "$p.closeDialog($(this));"))));
        }
    }
}