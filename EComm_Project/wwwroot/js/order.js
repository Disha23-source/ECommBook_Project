var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $("#tblData").DataTable({
        "ajax": {
            "url": "/Admin/OrderManagement/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "id", "width": "10%", "title": "Order ID",
                render: function (data) {
                    return `<span class="badge bg-secondary">#${data}</span>`;
                }
            },
            {
                "data": "orderDate", "width": "15%", "title": "Date",
                render: function (data) {
                    return new Date(data).toLocaleDateString('en-IN', {
                        day: '2-digit', month: 'short', year: 'numeric'
                    });
                }
            },
            { "data": "name", "width": "20%", "title": "Customer Name" },
            {
                "data": "orderTotal", "width": "20%", "title": "Total Amount",
                render: function (data) {
                    return `<strong>₹${parseFloat(data).toFixed(2)}</strong>`;
                }
            },
            {
                "data": "id", "width": "15%", "title": "Action",
                render: function (data) {
                    return `
                        <div class="text-center">
                            <a href="/Admin/OrderManagement/Details?orderId=${data}" 
                               class="btn btn-sm btn-outline-primary">
                                <i class="bi bi-eye-fill"></i> View Details
                            </a>
                        </div>`;
                },
                orderable: false
            }
        ],
        "order": [[0, "desc"]],
        "pageLength": 10,
        "lengthMenu": [5, 10, 25, 50, 100],
        "language": {
            search: "🔍 Search:",
            lengthMenu: "Show _MENU_ orders per page",
            zeroRecords: "No orders found",
            info: "Showing _START_ to _END_ of _TOTAL_ orders",
            infoEmpty: "No orders available",
            infoFiltered: "(filtered from _MAX_ total orders)",
            paginate: { first: "«", last: "»", next: "›", previous: "‹" }
        },
        "footerCallback": function (row, data, start, end, display) {
            var api = this.api();

            var grandTotal = api
                .column(3)
                .data()
                .reduce(function (acc, val) {
                    return acc + parseFloat(val || 0);
                }, 0);

            $(api.column(3).footer()).html(
                `<div>Grand Total: <strong class="text-primary">₹${grandTotal.toFixed(2)}</strong></div>`
            );
        }
    });
}