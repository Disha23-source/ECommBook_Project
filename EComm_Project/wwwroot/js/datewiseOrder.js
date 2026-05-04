var dataTable;

$(document).ready(function () {
    // 1. Initial table state: hidden (if not already handled in CSS)
    $('#tableContainer').hide();

    // 2. Disable past dates in 'To Date' based on 'From Date' selection
    $('#fromDate').on('change', function () {
        var fromDateValue = $(this).val();

        // Update the 'min' attribute of the 'To Date' input
        // This makes all dates before the selected From Date unclickable
        $('#toDate').attr('min', fromDateValue);

        // Logic check: If 'To Date' is now earlier than the new 'From Date', reset it
        var currentToDate = $('#toDate').val();
        if (currentToDate && currentToDate < fromDateValue) {
            $('#toDate').val('');
        }
    });
});
function loadDataTable(start = '', end = '', status = 'All') {
    if ($.fn.DataTable.isDataTable('#datewiseTable')) {
        $('#datewiseTable').DataTable().destroy();
    }

    dataTable = $('#datewiseTable').DataTable({
        "ajax": {
            "url": "/Admin/OrderManagement/DatewiseOrders",
            "data": {
                start: start, end: end, status: status
            }
        },
        "columns": [
            { "data": "id", "width": "10%" },
            { "data": "orderDate", "width": "20%" },
            { "data": "name", "width": "25%" },
            { "data": "orderStatus", "width": "10%" },
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<a href="/Admin/OrderManagement/Details/${data}" class="btn btn-info">View Detail</a>`;
                }
            }
        ]
    });
}
function filterByDate() {
    var fromDate = $('#fromDate').val();
    var toDate = $('#toDate').val();
    var status = $('#status').val();

    //if (fromDate && toDate) {
    //    if (new Date(toDate) < new Date(fromDate)) {
    //        alert("The 'To Date' cannot be earlier than the 'From Date'.");
    //        return;
    //    }
    //}

    //$('#tableContainer').show();
    loadDataTable(fromDate, toDate, status);
}