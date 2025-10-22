// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// DataTable
function initFileDataTable(tableid) {
    new DataTable(tableid, {
        buttons: [
            {
                extend: 'excelHtml5',
                className: 'btn btn-sm',
                text: 'Excel',
                header: true,
                title: '', // disable Page title in export files
            },
            {
                extend: 'pdf',
                className: 'btn btn-sm',
                text: 'PDF',
            },
            {
                extend: 'colvis',
                className: 'btn btn-sm',
                text: 'Display',
            }
        ],
        layout: {
            topStart: 'pageLength',
            topEnd: ['buttons', 'search' ],
            bottomStart: 'info',
            bottomEnd: 'paging'
        },
        responsive: true,
        aaSorting: [],
        language: {
            searchPlaceholder: 'search...',
            sSearch: '',
            lengthMenu: "_MENU_"
        }
    });
}

