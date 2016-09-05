﻿$p.openColumnPropertiesDialog = function ($control) {
    var error = $p.send($control, undefined, false);
    if (error === 0) {
        $('#ColumnPropertiesDialog').dialog({
            modal: true,
            width: '90%',
            height: 'auto',
            appendTo: '.main-form'
        });
    }
}

$p.uploadSiteImage = function ($control) {
    var data = new FormData();
    data.append('SiteImage', $('[id=\'SiteSettings,SiteImage\']').prop('files')[0]);
    $p.upload(
        $('.main-form').attr('action').replace('_action_', $control.attr('data-action')),
        $control.attr('data-method'),
        data);
}

$p.setAggregationDetails = function ($control) {
    var data = $p.getData();
    data.AggregationType = $('#AggregationType').val();
    data.AggregationTarget = $('#AggregationTarget').val();
    $('.ui-dialog-content').dialog('close');
    $p.send($control);
}

$p.addSummary = function ($control) {
    $p.setData($('#SummarySiteId'));
    $p.setData($('#SummaryDestinationColumn'));
    $p.setData($('#SummaryLinkColumn'));
    $p.setData($('#SummaryType'));
    $p.setData($('#SummarySourceColumn'));
    $p.send($control);
}