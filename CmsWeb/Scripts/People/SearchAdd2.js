$(function () {
    $("a.xsearchadd").click(function (ev) {
        ev.preventDefault();
        var href = $(this).attr("href");
        $('<div/>').dialog3({
            id: "search-add",
            content: href,
            type: "POST"
        });
    });
    $("a.searchadd").click(function (ev) {
        ev.preventDefault();
        $("<div id='search-add' class='modal fade hide' data-width='760' />")
            .load($(this).attr("href"), {}, function () {
            $(this).modal("show");
        });
    });
//    $("#search-add a.submit-post").live("click", function (ev) {
//        ev.preventDefault();
//        var f = $(this).closest("form");
//        var q = f.serialize();
//        $("#search-add").dialog2("close");
//        $.post($(this).attr("href"), q, function (ret) {
//            $(ret).dialog2({ id: "search-add" });
//        });
//    });
    $("#search-add a.commit").live("click", function (ev) {
        ev.preventDefault();
        var f = $(this).closest("form");
        var q = f.serialize();
        var loc = $(this).attr("href");
        $.block();
        $.post(loc, q, function (ret) {
            if (ret.close) {
                if (ret.message) {
                    alert(ret.message);
                }
                switch (ret.how) {
                    case 'rebindgrids':
                        if (self.parent.RebindMemberGrids)
                            self.parent.RebindMemberGrids();
                        break;
                    case 'addselected':
                        if (self.parent.AddSelected)
                            self.parent.AddSelected(ret);
                        break;
                    case 'addselected2':
                        if (self.parent.AddSelected2)
                            self.parent.AddSelected2(ret);
                        break;
                    case 'CloseAddDialog':
                        if (self.parent.CloseAddDialog)
                            self.parent.CloseAddDialog();
                        break;
                }
            }
            $.unblock();
        });
        return false;
    });
    $("a.clear").live('click', function (ev) {
        ev.preventDefault();
        $("#name").val('');
        $("#phone").val('');
        $("#address").val('');
        $("#dob").val('');
        return false;
    });
});

