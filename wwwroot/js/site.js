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



//Table Page Change

function changePageSize(selectElement) {
    const pageSize = selectElement.value;
    const currentUrl = new URL(window.location.href);

    // Set the 'pageSize' query parameter
    currentUrl.searchParams.set('pageSize', pageSize);
    currentUrl.searchParams.set('pageNumber', 1); // Reset to first page

    // Redirect to the new URL
    window.location.href = currentUrl;
}

//Table Page Change




let draggedRow = null;

function allowDrop(event) {
    event.preventDefault(); // Allow drop action
    const targetRow = event.target.closest('tr');
    if (targetRow && targetRow !== draggedRow) {
        targetRow.classList.add('drop-target');
    }
}

function drag(event) {
    draggedRow = event.target.closest('tr'); // Set dragged row reference
    event.dataTransfer.effectAllowed = 'move';
    event.dataTransfer.setData('text/plain', draggedRow.id); // Store dragged element ID
    draggedRow.classList.add('dragged-row'); // Visual indicator for dragging
}

function dragLeave(event) {
    const targetRow = event.target.closest('tr');
    if (targetRow && targetRow !== draggedRow) {
        targetRow.classList.remove('drop-target');
    }
}

function drop(event) {
    event.preventDefault(); // Allow drop by preventing default behavior
    const targetRow = event.target.closest('tr');

    if (targetRow) {
        targetRow.classList.remove('drop-target');
    }
    if (draggedRow) {
        draggedRow.classList.remove('dragged-row');
    }

    // If a valid drop occurred, move the dragged row
    if (draggedRow && targetRow && draggedRow !== targetRow) {
        const tableBody = draggedRow.parentNode;
        const rows = Array.from(tableBody.children);
        const draggedIndex = rows.indexOf(draggedRow);
        const targetIndex = rows.indexOf(targetRow);

        if (draggedIndex < targetIndex) {
            tableBody.insertBefore(draggedRow, targetRow.nextSibling);
        } else {
            tableBody.insertBefore(draggedRow, targetRow);
        }

        updateRowNames();
        announcePositionChange(rows.indexOf(draggedRow), 'moved');
    }
}

function dragEnd() {
    if (draggedRow) {
        draggedRow.classList.remove('dragged-row');
        removeDropTargetStyles();
    }
}

function removeDropTargetStyles() {
    document.querySelectorAll('.drop-target').forEach((row) => {
        row.classList.remove('drop-target');
    });
}

function updateRowNames() {
    const rows = document.querySelectorAll("tr[id^='row-']");
    rows.forEach((row, newIndex) => {
        // Update the row's id
        row.id = `row-${newIndex}`;

        // Update all form elements within the row
        row.querySelectorAll('input, select, button, span').forEach((element) => {
            // Update 'name' attribute for input, select, button elements
            if (element.name) {
                element.name = element.name.replace(/\[\d+\]/, `[${newIndex}]`);
            }

            // Update 'data-valmsg-for' attribute for span elements (for validation messages)
            if (element.hasAttribute('data-valmsg-for')) {
                element.setAttribute(
                    'data-valmsg-for',
                    element.getAttribute('data-valmsg-for').replace(/\[\d+\]/, `[${newIndex}]`)
                );
            }

            // Update the 'data-index' attribute if present
            if (element.hasAttribute('data-index')) {
                element.setAttribute('data-index', newIndex);
            }
        });
    });
}


function announcePositionChange(rowIndex, direction) {
    const liveRegion = document.getElementById('live-region');
    liveRegion.textContent = `Row ${rowIndex + 1} moved ${direction}`;
}


//Drag and drop row 
