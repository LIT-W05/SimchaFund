$(function () {
    $("#new-contributor").on('click', function () {
        $(".new-contrib").modal();
    });

    $(".deposit-button").on('click', function () {
        const contribId = $(this).data('contribid');
        $('[name="contributorId"]').val(contribId);

        const tr = $(this).parent().parent();
        const name = tr.find('td:eq(1)').text();
        $("#deposit-name").text(name);

        $(".deposit").modal();
    });

    $("#search").on('keyup', function () {
        const text = $(this).val();
        $("table tr:gt(0)").each(function () {
            const tr = $(this);
            const name = tr.find('td:eq(1)').text();
            if (name.toLowerCase().indexOf(text.toLowerCase()) !== -1) {
                tr.show();
            } else {
                tr.hide();
            }
        });
    });

    $("#clear").on('click', function () {
        $("#search").val('');
        $("tr").show();
    });

    $(".edit-contrib").on('click', function () {
        const id = $(this).data('id');
        const firstName = $(this).data('firstName');
        const lastName = $(this).data('lastName');
        const cell = $(this).data('cell');
        const alwaysInclude = $(this).data('alwaysInclude');
        const date = $(this).data('date');

        const form = $(".new-contrib form");
        const hidden = $(`<input type='hidden' name='id' value='${id}' />`);
        form.append(hidden);

        $("#initialDepositDiv").hide();
        
        $("#contributor_first_name").val(firstName);
        $("#contributor_last_name").val(lastName);
        $("#contributor_cell_number").val(cell);
        $("#contributor_always_include").prop('checked', alwaysInclude === "True");
        $("#contributor_created_at").val(date);
        $(".new-contrib").modal();
        form.attr('action', '/contributors/edit');
    });
});
