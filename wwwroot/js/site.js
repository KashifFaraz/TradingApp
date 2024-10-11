// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


const DocumentStatus = {
    Draft: 0,
    Finalized: 1,
    FinalizedPartialReconciled: 2,
    FinalizedReconciled: 3,

    getName: function (value) {
        switch (value) {
            case this.Draft:
                return "Draft";
            case this.Finalized:
                return "Finalized";
            case this.FinalizedPartialReconciled:
                return "Partial Reconciled";
            case this.FinalizedReconciled:
                return "Reconciled";
            default:
                return "Unknown";
        }
    }
};




//Table



function makeColumnsResizable(table) {
    let pressed = false;
    let start = undefined;
    let startX, startWidth;

    $(table).find('th').mousedown(function (e) {
        start = $(this);
        pressed = true;
        startX = e.pageX;
        startWidth = $(this).width();

        // Prevent text selection during resize
        $('body').css({
            'user-select': 'none',  // Disable text selection in modern browsers
            '-webkit-user-select': 'none',  // Disable in Safari
            '-moz-user-select': 'none',  // Disable in older Firefox
            '-ms-user-select': 'none'  // Disable in older versions of IE
        });
    });

    $(document).mousemove(function (e) {
        if (pressed) {
            let width = startWidth + (e.pageX - startX);
            start.width(width);
        }
    });

    $(document).mouseup(function () {
        if (pressed) {
            pressed = false;

            // Re-enable text selection after resize
            $('body').css({
                'user-select': '',
                '-webkit-user-select': '',
                '-moz-user-select': '',
                '-ms-user-select': ''
            });
        }
    });
}



$(document).ready(function () {

    const table = document.getElementsByClassName('ShowHideColumnTable');
    makeColumnsResizable(table[0]);
});


//Table Column Show And Hide
$(document).ready(function () {
    $('.toggle-column').change(function () {
        var column = $(this).data('column');
        var isChecked = $(this).is(':checked');

        $('.ShowHideColumnTable tr').each(function () {
            if (isChecked) {
                $(this).find('td:eq(' + column + '),th:eq(' + column + ')').show();
            } else {
                $(this).find('td:eq(' + column + '),th:eq(' + column + ')').hide();
            }
        });
    });
});

document.querySelectorAll('.toggle-column').forEach(checkbox => {
    checkbox.addEventListener('change', function () {
        const columnIndex = this.getAttribute('data-column');
        const column = document.querySelectorAll('table th, table td')[columnIndex];

        if (this.checked) {
            column.style.display = ''; // Show column
        } else {
            column.style.display = 'none'; // Hide column
        }
    });
});


//Table

