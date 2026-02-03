window.initProjectTableDragDrop = function (dotNetHelper) {
    try {
        var table = $('#projectsTable');

        // Guard: If table doesn't exist, exit silently (or log debug)
        if (!table.length) return;

        // Guard: Check if the table body has actual data rows with 6 columns
        // This prevents initialization on 'Loading...' or 'No projects' rows which use colspan
        var firstRowCells = table.find('tbody tr:first td');
        if (firstRowCells.length !== 6) {
            console.log("DragDrop: Skipping initialization, table row has " + firstRowCells.length + " cells (expected 6).");
            return;
        }

        // If DataTable already exists, destroy so we can re-init (handles updates)
        if ($.fn.DataTable.isDataTable(table)) {
            table.DataTable().destroy();
        }

        var dt = table.DataTable({
            destroy: true, // Explicitly allow re-initialization
            rowReorder: {
                dataSrc: 0
            },
            columnDefs: [
                { targets: 0, orderable: true, className: 'reorder' }, // Index column
                { targets: '_all', orderable: false }
            ],
            // Explicitly define null columns to match the 6 header columns
            columns: [
                null, null, null, null, null, null
            ],
            order: [[0, 'asc']], // Sort by Index (Col 0)
            paging: false,
            searching: false,
            info: false,
            ordering: true
        });

        // Handle reorder
        dt.on('row-reorder', function (e, diff, edit) {
            // Wait for the DOM to settle
            setTimeout(function () {
                var order = [];
                // Iterate over the rows in their current visual order
                dt.rows({ order: 'applied' }).every(function () {
                    var node = this.node();
                    var id = $(node).attr('data-project-id');
                    if (id) {
                        order.push(parseInt(id));
                    }
                });

                console.log('DragDrop: New Order Captured', order);
                // Send to C#
                dotNetHelper.invokeMethodAsync('UpdateUserProjectOrder', order);
            }, 50);
        });

        console.log('DragDrop: Project Table Initialized');
    } catch (err) {
        console.error("DragDrop Error:", err);
    }
};
