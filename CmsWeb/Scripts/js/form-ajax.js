$(function () {
    $.AttachFormElements = function (f) {
        $("input.ajax-typeahead", f).typeahead({
            minLength: 3,
            source: function (query, process) {
                return $.ajax({
                    url: $(this.$element[0]).data("link"),
                    type: 'post',
                    data: { query: query },
                    dataType: 'json',
                    success: function (jsonResult) {
                        return typeof jsonResult == 'undefined' ? false : process(jsonResult);
                    }
                });
            }
        });
        $("select", f).chosen();
        $(".date", f).datepicker();
    };
    $("div.modal form.ajax").live("submit", function (event) {
        var $form = $(this);
        var $target = $form.closest("div.modal");
        $.ajax({
            type: 'POST',
            url: $form.attr('action'),
            data: $form.serialize(),
            success: function (data, status) {
                //$target.removeClass("fade");
                $target.html(data);
                var top = ($(window).height() - $target.height()) / 2;
                if (top < 10)
                    top = 10;
                $target.css({ 'margin-top': top, 'top': '0' });
                $.AttachFormElements($form);
            }
        });
        event.preventDefault();
    });
    $("form.ajax a.ajax").live("click", function (event) {
        var $this = $(this);
        var $form = $this.closest("form.ajax");
        var $modal = $form.closest("div.modal");
        var url = $this[0].href;
        var data = $form.serialize();
        $.ajax({
            type: 'POST',
            url: url,
            data: data,
            success: function (data, status) {
                if ($modal.length > 0) {
                    //$modal.removeClass("fade");
                    $modal.html(data);
                    var top = ($(window).height() - $modal.height()) / 2;
                    if (top < 10)
                        top = 10;
                    $modal.css({ 'margin-top': top, 'top': '0' });
                } else {
                    $form.html(data);
                }
                $.AttachFormElements($form);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alert(xhr.status);
                alert(thrownError);
            }
        });
        event.preventDefault();
    });
    $("body").on({
        ajaxStart: function () {
           $("#loading-indicator").css({ 
               'position': 'absolute',
               'left': $(window).width() / 2, 
               'top': $(window).height() / 2,
               'z-index' : 2000
           }).show();
        },
        ajaxStop: function () {
            $("#loading-indicator").hide();
        }
    });
});
