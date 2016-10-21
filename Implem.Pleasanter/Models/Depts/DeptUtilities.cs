﻿using Implem.DefinitionAccessor;
using Implem.Libraries.Classes;
using Implem.Libraries.DataSources.SqlServer;
using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.Converts;
using Implem.Pleasanter.Libraries.DataSources;
using Implem.Pleasanter.Libraries.DataTypes;
using Implem.Pleasanter.Libraries.General;
using Implem.Pleasanter.Libraries.Html;
using Implem.Pleasanter.Libraries.HtmlParts;
using Implem.Pleasanter.Libraries.Models;
using Implem.Pleasanter.Libraries.Requests;
using Implem.Pleasanter.Libraries.Responses;
using Implem.Pleasanter.Libraries.Security;
using Implem.Pleasanter.Libraries.Server;
using Implem.Pleasanter.Libraries.Settings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Implem.Pleasanter.Models
{
    public static class DeptUtilities
    {
        /// <summary>
        /// Fixed:
        /// </summary>
        public static string Index(SiteSettings ss, Permissions.Types pt)
        {
            var hb = new HtmlBuilder();
            var formData = DataViewFilters.SessionFormData();
            var deptCollection = DeptCollection(ss, pt, formData);
            return hb.Template(
                pt: pt,
                verType: Versions.VerTypes.Latest,
                methodType: BaseModel.MethodTypes.Index,
                allowAccess: Sessions.User().TenantAdmin,
                referenceType: "Depts",
                title: Displays.Depts() + " - " + Displays.List(),
                action: () =>
                {
                    hb
                        .Form(
                            attributes: new HtmlAttributes()
                                .Id("DeptForm")
                                .Class("main-form")
                                .Action(Navigations.Action("Depts")),
                            action: () => hb
                                .Aggregations(
                                    ss: ss,
                                    aggregations: deptCollection.Aggregations)
                                .Div(id: "DataViewContainer", action: () => hb
                                    .Grid(
                                        deptCollection: deptCollection,
                                        pt: pt,
                                        ss: ss,
                                        formData: formData))
                                .MainCommands(
                                    siteId: ss.SiteId,
                                    pt: pt,
                                    verType: Versions.VerTypes.Latest)
                                .Div(css: "margin-bottom")
                                .Hidden(controlId: "TableName", value: "Depts")
                                .Hidden(controlId: "BaseUrl", value: Navigations.BaseUrl())
                                .Hidden(
                                    controlId: "GridOffset",
                                    value: Parameters.General.GridPageSize.ToString()))
                        .Div(attributes: new HtmlAttributes()
                            .Id("ImportSettingsDialog")
                            .Class("dialog")
                            .Title(Displays.Import()))
                        .Div(attributes: new HtmlAttributes()
                            .Id("ExportSettingsDialog")
                            .Class("dialog")
                            .Title(Displays.ExportSettings()));
                }).ToString();
        }

        private static string DataViewTemplate(
            this HtmlBuilder hb,
            SiteSettings ss,
            Permissions.Types pt,
            DeptCollection deptCollection,
            FormData formData,
            string dataViewName,
            Action dataViewBody)
        {
            return hb.Template(
                pt: pt,
                verType: Versions.VerTypes.Latest,
                methodType: BaseModel.MethodTypes.Index,
                allowAccess: pt.CanRead(),
                siteId: ss.SiteId,
                parentId: ss.ParentId,
                referenceType: "Depts",
                script: Libraries.Scripts.JavaScripts.DataView(
                    ss: ss,
                    pt: pt,
                    formData: formData,
                    dataViewName: dataViewName),
                userScript: ss.GridScript,
                userStyle: ss.GridStyle,
                action: () => hb
                    .Form(
                        attributes: new HtmlAttributes()
                            .Id("DeptsForm")
                            .Class("main-form")
                            .Action(Navigations.ItemAction(ss.SiteId)),
                        action: () => hb
                            .DataViewFilters(ss: ss)
                            .Aggregations(
                                ss: ss,
                                aggregations: deptCollection.Aggregations)
                            .Div(id: "DataViewContainer", action: () => dataViewBody())
                            .MainCommands(
                                siteId: ss.SiteId,
                                pt: pt,
                                verType: Versions.VerTypes.Latest,
                                bulkMoveButton: true,
                                bulkDeleteButton: true,
                                importButton: true,
                                exportButton: true)
                            .Div(css: "margin-bottom")
                            .Hidden(controlId: "TableName", value: "Depts")
                            .Hidden(controlId: "BaseUrl", value: Navigations.BaseUrl()))
                    .MoveDialog(bulk: true)
                    .Div(attributes: new HtmlAttributes()
                        .Id("ExportSettingsDialog")
                        .Class("dialog")
                        .Title(Displays.ExportSettings())))
                    .ToString();
        }

        public static string IndexJson(SiteSettings ss, Permissions.Types pt)
        {
            var formData = DataViewFilters.SessionFormData();
            var deptCollection = DeptCollection(ss, pt, formData);
            return new ResponseCollection()
                .Html("#DataViewContainer", new HtmlBuilder().Grid(
                    ss: ss,
                    deptCollection: deptCollection,
                    pt: pt,
                    formData: formData))
                .DataViewFilters(ss: ss)
                .ReplaceAll("#Aggregations", new HtmlBuilder().Aggregations(
                    ss: ss,
                    aggregations: deptCollection.Aggregations))
                .ToJson();
        }

        private static DeptCollection DeptCollection(
            SiteSettings ss,
            Permissions.Types pt,
            FormData formData,
            int offset = 0)
        {
            return new DeptCollection(
                ss: ss,
                pt: pt,
                column: GridSqlColumnCollection(ss),
                where: DataViewFilters.Get(
                    ss: ss,
                    tableName: "Depts",
                    formData: formData,
                    where: Rds.DeptsWhere().TenantId(Sessions.TenantId())),
                orderBy: GridSorters.Get(
                    formData, Rds.DeptsOrderBy().UpdatedTime(SqlOrderBy.Types.desc)),
                offset: offset,
                pageSize: ss.GridPageSize.ToInt(),
                countRecord: true,
                aggregationCollection: ss.AggregationCollection);
        }

        private static HtmlBuilder Grid(
            this HtmlBuilder hb,
            SiteSettings ss,
            Permissions.Types pt,
            DeptCollection deptCollection,
            FormData formData)
        {
            return hb
                .Table(
                    attributes: new HtmlAttributes()
                        .Id("Grid")
                        .Class("grid")
                        .DataAction("GridRows")
                        .DataMethod("post"),
                    action: () => hb
                        .GridRows(
                            ss: ss,
                            deptCollection: deptCollection,
                            formData: formData))
                .Hidden(
                    controlId: "GridOffset",
                    value: ss.GridPageSize == deptCollection.Count()
                        ? ss.GridPageSize.ToString()
                        : "-1");
        }

        public static string GridRows(
            SiteSettings ss,
            Permissions.Types pt,
            ResponseCollection res = null,
            int offset = 0,
            bool clearCheck = false,
            Message message = null)
        {
            var formData = DataViewFilters.SessionFormData();
            var deptCollection = DeptCollection(ss, pt, formData, offset);
            return (res ?? new ResponseCollection())
                .Remove(".grid tr", _using: offset == 0)
                .ClearFormData("GridCheckAll", _using: clearCheck)
                .ClearFormData("GridUnCheckedItems", _using: clearCheck)
                .ClearFormData("GridCheckedItems", _using: clearCheck)
                .Message(message)
                .Append("#Grid", new HtmlBuilder().GridRows(
                    ss: ss,
                    deptCollection: deptCollection,
                    formData: formData,
                    addHeader: offset == 0,
                    clearCheck: clearCheck))
                .Val("#GridOffset", ss.NextPageOffset(offset, deptCollection.Count()))
                .ToJson();
        }

        private static HtmlBuilder GridRows(
            this HtmlBuilder hb,
            SiteSettings ss,
            DeptCollection deptCollection,
            FormData formData,
            bool addHeader = true,
            bool clearCheck = false)
        {
            var checkAll = clearCheck ? false : Forms.Bool("GridCheckAll");
            var columns = ss.GridColumnCollection();
            return hb
                .THead(
                    _using: addHeader,
                    action: () => hb
                        .GridHeader(
                            columnCollection: columns, 
                            formData: formData,
                            checkAll: checkAll))
                .TBody(action: () => deptCollection
                    .ForEach(deptModel => hb
                        .Tr(
                            attributes: new HtmlAttributes()
                                .Class("grid-row")
                                .DataId(deptModel.DeptId.ToString()),
                            action: () =>
                            {
                                hb.Td(action: () => hb
                                    .CheckBox(
                                        controlCss: "grid-check",
                                        _checked: checkAll,
                                        dataId: deptModel.DeptId.ToString()));
                                columns
                                    .ForEach(column => hb
                                        .TdValue(
                                            column: column,
                                            deptModel: deptModel));
                            })));
        }

        private static SqlColumnCollection GridSqlColumnCollection(SiteSettings ss)
        {
            var sqlColumnCollection = Rds.DeptsColumn();
            new List<string> { "DeptId", "Creator", "Updator" }
                .Concat(ss.GridColumns)
                .Concat(ss.TitleColumns)
                    .Distinct().ForEach(column =>
                        sqlColumnCollection.DeptsColumn(column));
            return sqlColumnCollection;
        }

        public static HtmlBuilder TdValue(
            this HtmlBuilder hb, Column column, DeptModel deptModel)
        {
            switch (column.ColumnName)
            {
                case "DeptId": return hb.Td(column: column, value: deptModel.DeptId);
                case "Ver": return hb.Td(column: column, value: deptModel.Ver);
                case "DeptCode": return hb.Td(column: column, value: deptModel.DeptCode);
                case "Dept": return hb.Td(column: column, value: deptModel.Dept);
                case "Body": return hb.Td(column: column, value: deptModel.Body);
                case "Comments": return hb.Td(column: column, value: deptModel.Comments);
                case "Creator": return hb.Td(column: column, value: deptModel.Creator);
                case "Updator": return hb.Td(column: column, value: deptModel.Updator);
                case "CreatedTime": return hb.Td(column: column, value: deptModel.CreatedTime);
                case "UpdatedTime": return hb.Td(column: column, value: deptModel.UpdatedTime);
                default: return hb;
            }
        }

        public static string EditorNew()
        {
            return Editor(new DeptModel(
                SiteSettingsUtility.DeptsSiteSettings(),
                methodType: BaseModel.MethodTypes.New));
        }

        public static string Editor(int deptId, bool clearSessions)
        {
            var deptModel = new DeptModel(
                SiteSettingsUtility.DeptsSiteSettings(),
                deptId: deptId,
                clearSessions: clearSessions,
                methodType: BaseModel.MethodTypes.Edit);
            deptModel.SwitchTargets = GetSwitchTargets(
                SiteSettingsUtility.DeptsSiteSettings(), deptModel.DeptId);
            return Editor(deptModel);
        }

        public static string Editor(DeptModel deptModel)
        {
            var hb = new HtmlBuilder();
            var pt = Permissions.Admins();
            return hb.Template(
                pt: pt,
                verType: deptModel.VerType,
                methodType: deptModel.MethodType,
                allowAccess:
                    pt.CanEditTenant() &&
                    deptModel.AccessStatus != Databases.AccessStatuses.NotFound,
                referenceType: "Depts",
                title: deptModel.MethodType == BaseModel.MethodTypes.New
                    ? Displays.Depts() + " - " + Displays.New()
                    : deptModel.Title.Value,
                action: () =>
                {
                    hb
                        .Editor(
                            ss: deptModel.SiteSettings,
                            pt: pt,
                            deptModel: deptModel)
                        .Hidden(controlId: "TableName", value: "Depts")
                        .Hidden(controlId: "Id", value: deptModel.DeptId.ToString());
                }).ToString();
        }

        private static HtmlBuilder Editor(
            this HtmlBuilder hb,
            SiteSettings ss,
            Permissions.Types pt,
            DeptModel deptModel)
        {
            return hb.Div(id: "Editor", action: () => hb
                .Form(
                    attributes: new HtmlAttributes()
                        .Id("DeptForm")
                        .Class("main-form")
                        .Action(deptModel.DeptId != 0
                            ? Navigations.Action("Depts", deptModel.DeptId)
                            : Navigations.Action("Depts")),
                    action: () => hb
                        .RecordHeader(
                            pt: pt,
                            baseModel: deptModel,
                            tableName: "Depts")
                        .Div(id: "EditorComments", action: () => hb
                            .Comments(
                                comments: deptModel.Comments,
                                verType: deptModel.VerType))
                        .Div(id: "EditorTabsContainer", action: () => hb
                            .EditorTabs(deptModel: deptModel)
                            .FieldSetGeneral(
                                ss: ss,
                                pt: pt,
                                deptModel: deptModel)
                            .FieldSet(
                                attributes: new HtmlAttributes()
                                    .Id("FieldSetHistories")
                                    .DataAction("Histories")
                                    .DataMethod("get"),
                                _using: deptModel.MethodType != BaseModel.MethodTypes.New)
                            .MainCommands(
                                siteId: 0,
                                pt: pt,
                                verType: deptModel.VerType,
                                referenceType: "Depts",
                                referenceId: deptModel.DeptId,
                                updateButton: true,
                                mailButton: true,
                                deleteButton: true,
                                extensions: () => hb
                                    .MainCommandExtensions(
                                        deptModel: deptModel,
                                        ss: ss)))
                        .Hidden(controlId: "BaseUrl", value: Navigations.BaseUrl())
                        .Hidden(
                            controlId: "MethodType",
                            value: deptModel.MethodType.ToString().ToLower())
                        .Hidden(
                            controlId: "Depts_Timestamp",
                            css: "must-transport",
                            value: deptModel.Timestamp)
                        .Hidden(
                            controlId: "SwitchTargets",
                            css: "must-transport",
                            value: deptModel.SwitchTargets?.Join(),
                            _using: !Request.IsAjax()))
                .OutgoingMailsForm("Depts", deptModel.DeptId, deptModel.Ver)
                .CopyDialog("Depts", deptModel.DeptId)
                .OutgoingMailDialog()
                .EditorExtensions(deptModel: deptModel, ss: ss));
        }

        private static HtmlBuilder EditorTabs(this HtmlBuilder hb, DeptModel deptModel)
        {
            return hb.Ul(id: "EditorTabs", action: () => hb
                .Li(action: () => hb
                    .A(
                        href: "#FieldSetGeneral", 
                        text: Displays.Basic()))
                .Li(
                    _using: deptModel.MethodType != BaseModel.MethodTypes.New,
                    action: () => hb
                        .A(
                            href: "#FieldSetHistories",
                            text: Displays.Histories())));
        }

        private static HtmlBuilder FieldSetGeneral(
            this HtmlBuilder hb,
            SiteSettings ss,
            Permissions.Types pt,
            DeptModel deptModel)
        {
            return hb.FieldSet(id: "FieldSetGeneral", action: () =>
            {
                ss.EditorColumnCollection().ForEach(column =>
                {
                    switch (column.ColumnName)
                    {
                        case "TenantId": hb.Field(ss, column, deptModel.MethodType, deptModel.TenantId.ToControl(column, pt), column.ColumnPermissionType(pt)); break;
                        case "DeptId": hb.Field(ss, column, deptModel.MethodType, deptModel.DeptId.ToControl(column, pt), column.ColumnPermissionType(pt)); break;
                        case "Ver": hb.Field(ss, column, deptModel.MethodType, deptModel.Ver.ToControl(column, pt), column.ColumnPermissionType(pt)); break;
                        case "DeptCode": hb.Field(ss, column, deptModel.MethodType, deptModel.DeptCode.ToControl(column, pt), column.ColumnPermissionType(pt)); break;
                        case "DeptName": hb.Field(ss, column, deptModel.MethodType, deptModel.DeptName.ToControl(column, pt), column.ColumnPermissionType(pt)); break;
                        case "Body": hb.Field(ss, column, deptModel.MethodType, deptModel.Body.ToControl(column, pt), column.ColumnPermissionType(pt)); break;
                    }
                });
                hb.VerUpCheckBox(deptModel);
            });
        }

        private static HtmlBuilder MainCommandExtensions(
            this HtmlBuilder hb,
            SiteSettings ss,
            DeptModel deptModel)
        {
            return hb;
        }

        private static HtmlBuilder EditorExtensions(
            this HtmlBuilder hb,
            DeptModel deptModel,
            SiteSettings ss)
        {
            return hb;
        }

        public static string EditorJson(
            SiteSettings ss, Permissions.Types pt, int deptId)
        {
            return EditorResponse(new DeptModel(ss, deptId))
                .ToJson();
        }

        private static ResponseCollection EditorResponse(
            DeptModel deptModel, Message message = null, string switchTargets = null)
        {
            deptModel.MethodType = BaseModel.MethodTypes.Edit;
            return new DeptsResponseCollection(deptModel)
                .Invoke("clearDialogs")
                .ReplaceAll("#MainContainer", Editor(deptModel))
                .Val("#SwitchTargets", switchTargets, _using: switchTargets != null)
                .Invoke("setCurrentIndex")
                .Invoke("validateDepts")
                .Message(message)
                .ClearFormData();
        }

        public static List<int> GetSwitchTargets(
            SiteSettings ss, int deptId)
        {
            var formData = DataViewFilters.SessionFormData();
            var switchTargets = Rds.ExecuteTable(
                transactional: false,
                statements: Rds.SelectDepts(
                    column: Rds.DeptsColumn().DeptId(),
                    where: DataViewFilters.Get(
                        ss: ss,
                        tableName: "Depts",
                        formData: formData,
                        where: Rds.DeptsWhere().TenantId(Sessions.TenantId())),
                    orderBy: GridSorters.Get(
                        formData, Rds.DeptsOrderBy().UpdatedTime(SqlOrderBy.Types.desc))))
                            .AsEnumerable()
                            .Select(o => o["DeptId"].ToInt())
                            .ToList();
            if (!switchTargets.Contains(deptId))
            {
                switchTargets.Add(deptId);
            }
            return switchTargets;
        }

        public static ResponseCollection FormResponse(
            this ResponseCollection res,
            Permissions.Types pt,
            DeptModel deptModel)
        {
            Forms.All().Keys.ForEach(key =>
            {
                switch (key)
                {
                    default: break;
                }
            });
            return res;
        }

        public static string Create(SiteSettings ss, Permissions.Types pt)
        {
            var deptModel = new DeptModel(ss, 0, setByForm: true);
            var invalid = DeptValidators.OnCreating(ss, pt, deptModel);
            switch (invalid)
            {
                case Error.Types.None: break;
                default: return invalid.MessageJson();
            }
            var error = deptModel.Create();
            if (error.Has())
            {
                return error.MessageJson();
            }
            else
            {
                return EditorResponse(
                    deptModel,
                    Messages.Created(deptModel.Title.Value),
                    GetSwitchTargets(ss, deptModel.DeptId).Join()).ToJson();
            }
        }

        public static string Update(
            SiteSettings ss, Permissions.Types pt, int deptId)
        {
            var deptModel = new DeptModel(ss, deptId, setByForm: true);
            var invalid = DeptValidators.OnUpdating(ss, pt, deptModel);
            switch (invalid)
            {
                case Error.Types.None: break;
                default: return new ResponseCollection().Message(invalid.Message()).ToJson();
            }
            if (deptModel.AccessStatus != Databases.AccessStatuses.Selected)
            {
                return Messages.ResponseDeleteConflicts().ToJson();
            }
            var error = deptModel.Update();
            if (error.Has())
            {
                return error == Error.Types.UpdateConflicts
                    ? Messages.ResponseUpdateConflicts(deptModel.Updator.FullName()).ToJson()
                    : new ResponseCollection().Message(error.Message()).ToJson();
            }
            else
            {
                var res = new DeptsResponseCollection(deptModel);
                return ResponseByUpdate(pt, deptModel, res)
                    .PrependComment(deptModel.Comments, deptModel.VerType)
                    .ToJson();
            }
        }

        private static ResponseCollection ResponseByUpdate(
            Permissions.Types pt,
            DeptModel deptModel,
            DeptsResponseCollection res)
        {
            return res
                .Ver()
                .Timestamp()
                .Val("#VerUp", false)
                .FormResponse(pt, deptModel)
                .Disabled("#VerUp", false)
                .Html("#HeaderTitle", deptModel.Title.Value)
                .Html("#RecordInfo", new HtmlBuilder().RecordInfo(
                    baseModel: deptModel, tableName: "Depts"))
                .Message(Messages.Updated(deptModel.Title.ToString()))
                .RemoveComment(deptModel.DeleteCommentId, _using: deptModel.DeleteCommentId != 0)
                .ClearFormData();
        }

        public static string Delete(
            SiteSettings ss, Permissions.Types pt, int deptId)
        {
            var deptModel = new DeptModel(ss, deptId);
            var invalid = DeptValidators.OnDeleting(ss, pt, deptModel);
            switch (invalid)
            {
                case Error.Types.None: break;
                default: return invalid.MessageJson();
            }
            var error = deptModel.Delete();
            if (error.Has())
            {
                return error.MessageJson();
            }
            else
            {
                Sessions.Set("Message", Messages.Deleted(deptModel.Title.Value).Html);
                var res = new DeptsResponseCollection(deptModel);
                res.Href(Navigations.Index("Depts"));
                return res.ToJson();
            }
        }

        public static string Restore(int deptId)
        {
            var deptModel = new DeptModel();
            var invalid = DeptValidators.OnRestoring();
            switch (invalid)
            {
                case Error.Types.None: break;
                default: return invalid.MessageJson();
            }
            var error = deptModel.Restore(deptId);
            if (error.Has())
            {
                return error.MessageJson();
            }
            else
            {
                var res = new DeptsResponseCollection(deptModel);
                return res.ToJson();
            }
        }

        public static string Histories(
            SiteSettings ss, Permissions.Types pt, int deptId)
        {
            var deptModel = new DeptModel(ss, deptId);
            var columns = ss.HistoryColumnCollection();
            var hb = new HtmlBuilder();
            hb.Table(
                attributes: new HtmlAttributes().Class("grid"),
                action: () => hb
                    .THead(action: () => hb
                        .GridHeader(
                            columnCollection: columns,
                            sort: false,
                            checkRow: false))
                    .TBody(action: () =>
                        new DeptCollection(
                            ss: ss,
                            pt: pt,
                            where: Rds.DeptsWhere().DeptId(deptModel.DeptId),
                            orderBy: Rds.DeptsOrderBy().Ver(SqlOrderBy.Types.desc),
                            tableType: Sqls.TableTypes.NormalAndHistory)
                                .ForEach(deptModelHistory => hb
                                    .Tr(
                                        attributes: new HtmlAttributes()
                                            .Class("grid-row history not-link")
                                            .DataAction("History")
                                            .DataMethod("post")
                                            .DataVer(deptModelHistory.Ver)
                                            .DataLatest(1, _using:
                                                deptModelHistory.Ver == deptModel.Ver),
                                        action: () => columns
                                            .ForEach(column => hb
                                                .TdValue(column, deptModelHistory))))));
            return new DeptsResponseCollection(deptModel)
                .Html("#FieldSetHistories", hb).ToJson();
        }

        public static string History(
            SiteSettings ss, Permissions.Types pt, int deptId)
        {
            var deptModel = new DeptModel(ss, deptId);
            deptModel.Get(
                where: Rds.DeptsWhere()
                    .DeptId(deptModel.DeptId)
                    .Ver(Forms.Int("Ver")),
                tableType: Sqls.TableTypes.NormalAndHistory);
            deptModel.VerType =  Forms.Bool("Latest")
                ? Versions.VerTypes.Latest
                : Versions.VerTypes.History;
            return EditorResponse(deptModel).ToJson();
        }

        /// <summary>
        /// Fixed:
        /// </summary>
        public static string GridRows()
        {
            return GridRows(
                SiteSettingsUtility.DeptsSiteSettings(),
                Permissions.Admins(),
                offset: DataViewGrid.Offset());
        }
    }
}
