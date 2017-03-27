﻿using Implem.Libraries.DataSources.SqlServer;
using Implem.Libraries.Utilities;
using Implem.Pleasanter.Libraries.DataSources;
using Implem.Pleasanter.Libraries.Html;
using Implem.Pleasanter.Libraries.Responses;
using Implem.Pleasanter.Libraries.Server;
using Implem.Pleasanter.Libraries.Settings;
using Implem.Pleasanter.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Implem.Pleasanter.Libraries.HtmlParts
{
    public static class HtmlLinks
    {
        public static HtmlBuilder Links(this HtmlBuilder hb, SiteSettings ss, long id)
        {
            var targets = Targets(id);
            var dataSet = DataSet(targets);
            return Contains(dataSet)
                ? hb.FieldSet(
                    css: " enclosed",
                    legendText: Displays.Links(),
                    action: () => hb
                        .Links(
                            targets: targets,
                            ss: ss,
                            dataSet: dataSet))
                : hb;
        }

        private static EnumerableRowCollection<DataRow> Targets(long linkId)
        {
            return Rds.ExecuteTable(statements: new SqlStatement[]
            {
                Rds.SelectLinks(
                    column: Rds.LinksColumn()
                        .SourceId(_as: "Id")
                        .Add("'Source' as [Direction]"),
                    where: Rds.LinksWhere().DestinationId(linkId)),
                Rds.SelectLinks(
                    unionType: Sqls.UnionTypes.UnionAll,
                    column: Rds.LinksColumn()
                        .DestinationId(_as: "Id")
                        .Add("'Destination' as [Direction]"),
                    where: Rds.LinksWhere().SourceId(linkId))
            }).AsEnumerable();
        }

        private static DataSet DataSet(EnumerableRowCollection<DataRow> targets)
        {
            return Rds.ExecuteDataSet(statements: new SqlStatement[]
            {
                SelectIssues(targets, "Destination"), SelectIssues(targets, "Source"),
                SelectResults(targets, "Destination"), SelectResults(targets, "Source")
            });
        }

        private static SqlStatement SelectIssues(
            EnumerableRowCollection<DataRow> targets, string direction)
        {
            return Rds.SelectIssues(
                dataTableName: "Issues" + "_" + direction,
                column: Rds
                    .IssuesDefaultColumns()
                    .Add("'" + direction + "' as Direction"),
                join: Rds.IssuesJoinDefault(),
                where: Rds.IssuesWhere().IssueId_In(targets
                    .Where(o => o["Direction"].ToString() == direction)
                    .Select(o => o["Id"].ToLong())));
        }

        private static SqlStatement SelectResults(
            EnumerableRowCollection<DataRow> targets, string direction)
        {
            return Rds.SelectResults(
                dataTableName: "Results" + "_" + direction,
                column: Rds
                    .ResultsDefaultColumns()
                    .Add("'" + direction + "' as Direction"),
                join: Rds.ResultsJoinDefault(),
                where: Rds.ResultsWhere().ResultId_In(targets
                    .Where(o => o["Direction"].ToString() == direction)
                    .Select(o => o["Id"].ToLong())));
        }

        private static bool Contains(DataSet dataSet)
        {
            for (var i = 0; i < dataSet.Tables.Count; i++)
            {
                if (dataSet.Tables[i].Rows.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static HtmlBuilder Links(
            this HtmlBuilder hb,
            EnumerableRowCollection<DataRow> targets,
            SiteSettings ss,
            DataSet dataSet)
        {
            return hb.Div(action: () => hb
                .LinkTables(
                    ssList: ss.Destinations,
                    dataSet: dataSet,
                    direction: "Destination",
                    caption: Displays.LinkDestinations())
                .LinkTables(
                    ssList: ss.Sources,
                    dataSet: dataSet,
                    direction: "Source",
                    caption: Displays.LinkSources()));
        }

        private static HtmlBuilder LinkTables(
            this HtmlBuilder hb,
            IEnumerable<SiteSettings> ssList,
            DataSet dataSet,
            string direction,
            string caption)
        {
            ssList.ForEach(ss => hb.Table(css: "grid", action: () =>
            {
                var dataRows = dataSet.Tables[ss.ReferenceType + "_" + direction]?
                    .AsEnumerable()
                    .Where(o => o["SiteId"].ToLong() == ss.SiteId);
                if (dataRows != null && dataRows.Any())
                {
                    ss.SetColumnAccessControls();
                    var columns = ss.GetLinkColumns(checkPermission: true);
                    switch (ss.ReferenceType)
                    {
                        case "Issues":
                            hb
                                .Caption(caption: "{0} : {1} - {2} {3}".Params(
                                    caption,
                                    SiteInfo.SiteMenu.Breadcrumb(ss.SiteId)
                                        .Select(o => o.Title)
                                        .Join(" > "),
                                    Displays.Quantity(),
                                    dataRows.Count()))
                                .THead(action: () => hb
                                    .GridHeader(columns: columns, sort: false, checkRow: false))
                                .TBody(action: () => dataRows
                                    .ForEach(dataRow =>
                                    {
                                        var issueModel = new IssueModel(ss, dataRow);
                                        ss.SetColumnAccessControls(issueModel.Mine());
                                        hb.Tr(
                                            attributes: new HtmlAttributes()
                                                .Class("grid-row")
                                                .DataId(issueModel.IssueId.ToString()),
                                            action: () => columns
                                                .ForEach(column => hb
                                                    .TdValue(
                                                        ss: ss,
                                                        column: column,
                                                        issueModel: issueModel)));
                                    }));
                            break;
                        case "Results":
                            hb
                                .Caption(caption: "{0} : {1} - {2} {3}".Params(
                                    caption,
                                    SiteInfo.SiteMenu.Breadcrumb(ss.SiteId)
                                        .Select(o => o.Title)
                                        .Join(" > "),
                                    Displays.Quantity(),
                                    dataRows.Count()))
                                .THead(action: () => hb
                                    .GridHeader(columns: columns, sort: false, checkRow: false))
                                .TBody(action: () => dataRows
                                    .ForEach(dataRow =>
                                    {
                                        var resultModel = new ResultModel(ss, dataRow);
                                        ss.SetColumnAccessControls(resultModel.Mine());
                                        hb.Tr(
                                            attributes: new HtmlAttributes()
                                                .Class("grid-row")
                                                .DataId(resultModel.ResultId.ToString()),
                                            action: () => columns
                                                .ForEach(column => hb
                                                    .TdValue(
                                                        ss: ss,
                                                        column: column,
                                                        resultModel: resultModel)));
                                    }));
                            break;
                    }
                }
            }));
            return hb;
        }
    }
}
