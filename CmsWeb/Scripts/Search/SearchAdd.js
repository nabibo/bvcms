$(function () {
    $("a.searchadd").live("click", function (ev) {
        ev.preventDefault();
        $("<div id='search-add' class='modal fade hide' data-width='600' data-keyboard='false' data-backdrop='static' />")
            .load($(this).attr("href"), {}, function () {
                $(this).modal("show");
                $(this).on('hidden', function () {
                    $(this).remove();
                });
            });
    });
    $("#search-add a.clear").live('click', function (ev) {
        ev.preventDefault();
        $("#name").val('');
        $("#phone").val('');
        $("#address").val('');
        $("#dob").val('');
        return false;
    });

    $("form.ajax tbody > tr a.reveal").live("click", function (e) {
        e.stopPropagation();
    });
    $.NotReveal = function (ev) {
        if ($(ev.target).is("a"))
            if (!$(ev.target).is('.reveal'))
                return true;
        return false;
    };
    $("form.ajax tr.section.notshown").live("click", function (ev) {
        if ($.NotReveal(ev)) return;
        ev.preventDefault();
        ev.stopPropagation();
        $(this).removeClass("notshown").addClass("shown");
        $(this).nextUntil("tr.section").find("div.collapse")
            .off("hidden")
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse('show');
    });
    $("form.ajax tr.section.shown").live("click", function (ev) {
        if ($.NotReveal(ev)) return;
        ev.preventDefault();
        $(this).nextUntil("tr.section").find("div.collapse")
            .off("hidden")
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse('hide');
        $(this).removeClass("shown").addClass("notshown");
    });
    $('form.ajax a[rel="reveal"]').live("click", function (ev) {
        ev.preventDefault();
        $(this).parents("tr").next("tr").find("div.collapse")
            .off('hidden')
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse("toggle");
    });
    $("form.ajax tr.master").live("click", function (ev) {
        if ($.NotReveal(ev)) return;
        ev.preventDefault();
        $(this).next("tr").find("div.collapse")
            .off('hidden')
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse("toggle");
    });
    $("form.ajax tr.details").live("click", function (ev) {
        if ($.NotReveal(ev)) return;
        ev.preventDefault();
        ev.stopPropagation();
        $(this).find("div.collapse")
            .off("hidden")
            .on("hidden", function (e) { e.stopPropagation(); })
            .collapse('hide');
    });
});